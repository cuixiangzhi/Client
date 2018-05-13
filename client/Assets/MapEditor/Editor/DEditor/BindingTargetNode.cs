using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BindingTargetNode : BaseNode {
    private MapEditConfigData.TriggerRootNodeData _rootData;
    public MapEditConfigData.TriggerRootNodeData RootData
    {
        get
        {
            _rootData.node.parents.Clear();
            _rootData.node.children.Clear();
            _rootData.node.posx = Position.x;
            _rootData.node.posy = Position.y;
            _rootData.node.id = nodeID;
            _rootData.node.NodeType = (int)NodeType.Binding;

            for (int i = 0; i < children.Count; i++)
            {
                _rootData.node.children.Add(children[i].nodeID);
            }

            return _rootData;
        }

        set
        {
            _rootData = value;

            nodeID = _rootData.node.id;
            nodeType = NodeType.Binding;

            parents.Clear();
            children.Clear();
            for (int i = 0; i < _rootData.node.children.Count; i++)
            {
                BaseNode n = TriggerNodeWindow.GetNodeByID(_rootData.node.children[i]);
                if (n != null)
                    children.Add(n);
                else
                    Debug.LogError(string.Format("未在节点列表找到编号为[{0}]的子节点!", _rootData.node.children[i]));
            }

            for (int i = 0; i < _rootData.targets.Count; i++)
            {
                IEditEventSender[] srcs = GameObject.FindObjectsOfType<IEditEventSender>();
                foreach (var src in srcs)
                {
                    if (src.ID == _rootData.targets[i].id)
                        _rootData.targets[i].targetObj = src;
                }
                if (_rootData.targets[i].targetObj == null)
                {
                    Debug.LogError(string.Format("绑定对象ID[{0}]未在场景中找到!", _rootData.targets[i].id));
                }
            }

            ResetSize();
            SetPosition(new Vector2(_rootData.node.posx, _rootData.node.posy));
        }
    }

    List<int> m_vCommonEventIds = new List<int>();//绑定对象集合公共事件ID
    bool triggerBindObjectsFoldout;
    List<StageTriggerEventData.StageTriggerEventData> m_vStageEvents;

    public BindingTargetNode()
    {
        nodeType = NodeType.Binding;
        m_vStageEvents = DatasManager.Instance.GetDatas<StageTriggerEventData.StageTriggerEventData>();
        ItemWidth = 200;
        nodeName = "根节点";
    }

    public override void OnDrawGUI(Rect rect, Color color)
    {
        GUI.Box(rect, "", GUI.skin.window);

        GUI.color = Color.white;
        BeginNodeUI(rect);
        DrawTriggerPage(rect);
        EndNodeUI();
    }

    public override void ResetSize()
    {
        Vector2 center = Position;
        body_Rect = new Rect();
        body_Rect.width = ItemWidth;
        body_Rect.height = ItemHeight;
        Position = center;

        leftHand_Rect = new Rect();
        leftHand_Rect.width = HandWidth;
        leftHand_Rect.height = HandHeight;
        leftHand_Rect.center = new Vector2(body_Rect.xMin, Position.y);

        rightHand_Rect = new Rect();
        rightHand_Rect.width = HandWidth;
        rightHand_Rect.height = HandHeight;
        rightHand_Rect.center = new Vector2(body_Rect.xMax, Position.y);
    }

    //触发器界面
    void DrawTriggerPage(Rect rect)
    {
        Label(string.Format("Name: {0}", TriggerNodeWindow.new_trigger.name), labelStyle);
        Label(string.Format("ID: {0}", TriggerNodeWindow.new_trigger.id), labelStyle);

        UI_DOWN();
        if (_rootData.targets.Count == 0)
        {
            if(Button("+", TextAnchor.MiddleLeft, 0.15f))
            {
                OnPropertyChanged();
                _rootData.targets.Add(new MapEditConfigData.TriggerTargetInfo());
            }
        }
        else
        {
            for (int i = 0; i < _rootData.targets.Count; i++)
            {

                if (Button("-", TextAnchor.MiddleLeft, 0.15f))
                {
                    OnPropertyChanged();
                    _rootData.targets.RemoveAt(i);
                    break;
                }

                UI_UP();

                if (_rootData.targets[i].targetObj == null)
                    GUI.color = Color.yellow;
                _rootData.targets[i].targetObj = ObjectField("", _rootData.targets[i].targetObj, 0, 0.8f, typeof(IEditEventSender));
                GUI.color = Color.white;

                if (null != _rootData.targets[i].targetObj)
                {
                    IEditEventSender tar = _rootData.targets[i].targetObj as IEditEventSender;
                    _rootData.targets[i].id = tar.id;
                    _rootData.targets[i].type = (int)tar.ToTargetType();
                }
            }

            if (Button("+", TextAnchor.MiddleLeft, 0.15f))
            {
                OnPropertyChanged();
                _rootData.targets.Add(new MapEditConfigData.TriggerTargetInfo());
            }
        }

        UI_DOWN();
        TriggerNodeWindow.new_trigger.defaultEnabled = Toggle("默认开启状态", TriggerNodeWindow.new_trigger.defaultEnabled);
        UI_DOWN();

        ShowCommonEventEnums();
    }

    /// <summary>
    /// 展示公共可选事件ID
    /// </summary>
    void ShowCommonEventEnums()
    {
        int[] tmpCommonIds;//根据当前绑定对象计算的公共事件列表
        string[] eventNames;//相应的事件说明列表

        GetCommonEventIDList(TriggerNodeWindow.new_trigger, out tmpCommonIds, out eventNames);
        bool change = IsCommonEventChanged(tmpCommonIds);

        if (!change)
        {
            Label("事件ID:");
            _rootData.eventInfo.eventPopupIndex = Popup(_rootData.eventInfo.eventPopupIndex, eventNames);
            _rootData.eventInfo.eventID = m_vCommonEventIds[_rootData.eventInfo.eventPopupIndex];
        }
        else
        {
            if (tmpCommonIds == null || eventNames == null)
            {
                Rect r = GetControlRect();
                r = new Rect(r.xMin, r.yMin, r.width, r.height * 3);
                EditorGUI.HelpBox(r, "未找到共有事件ID,请确认监听对象是否有共同触发事件.", MessageType.Error);
                UI_DOWN(3);
            }
            else
            {
                m_vCommonEventIds.Clear();
                bool inCurCommonEvents = false;

                for (int i = 0; i < tmpCommonIds.Length; i++)
                {
                    m_vCommonEventIds.Add(tmpCommonIds[i]);
                    if (_rootData.eventInfo.eventID == tmpCommonIds[i])//判断当前触发器监听ID是否在公共事件ID列表中
                    {
                        _rootData.eventInfo.eventPopupIndex = i;
                        inCurCommonEvents = true;
                    }
                }

                if (inCurCommonEvents)
                {
                    Label("事件ID:");
                    _rootData.eventInfo.eventPopupIndex = Popup(_rootData.eventInfo.eventPopupIndex, eventNames);
                    _rootData.eventInfo.eventID = m_vCommonEventIds[_rootData.eventInfo.eventPopupIndex];
                }
                else
                {
                    _rootData.eventInfo.eventPopupIndex = 0;
                    _rootData.eventInfo.eventID = 0;
                    Label("事件ID:");
                    _rootData.eventInfo.eventPopupIndex = Popup(_rootData.eventInfo.eventPopupIndex, eventNames);
                }
            }
        }
    }

    /// <summary>
    /// 获取公共ID列表
    /// </summary>
    /// <param name="_targetData">_target data.</param>
    /// <param name="eventIds">Event identifiers.</param>
    /// <param name="eventNames">Event names.</param>
    void GetCommonEventIDList(MapEditConfigData.TriggerInfo _triggerInfo, out int[] eventIds, out string[] eventNames)
    {
        List<int> tmpCommonIds = new List<int>();//公共ID
        List<Object> obj_list = new List<Object>();//绑定对象组

        for (int i = 0; i < m_vStageEvents.Count; i++)
        {
            tmpCommonIds.Add(m_vStageEvents[i].ID);
        }
        for (int i = 0; i < _rootData.targets.Count; i++)
        {
            if (_rootData.targets[i].targetObj != null)
            {
                obj_list.Add(_rootData.targets[i].targetObj);
            }
        }

        #region 统计所有可用ID
        if (obj_list.Count == 0)
        {
            for (int i = 0; i < m_vStageEvents.Count; i++)
            {
                if (m_vStageEvents[i].EventTargetTypes.Count != 0)
                {
                    tmpCommonIds.RemoveAll((WTF) => { if (m_vStageEvents[i].ID == WTF)return true; return false; });
                }
            }
        }
        else
        {
            for (int i = 0; i < obj_list.Count; i++)
            {
                IEditEventSender tmp = obj_list[i] as IEditEventSender;
                for (int j = 0; j < m_vStageEvents.Count; j++)
                {
                    if (m_vStageEvents[j].EventTargetTypes.Find((shit) => { if (shit == (int)tmp.ToTargetType())return true; return false; }) == 0)
                    {
                        tmpCommonIds.RemoveAll((fuck) => { if (fuck == m_vStageEvents[j].ID)return true; return false; });
                    }
                }
            }
        }
        #endregion

        if (tmpCommonIds.Count > 0)
        {
            eventIds = new int[tmpCommonIds.Count];
            eventNames = new string[tmpCommonIds.Count];
            tmpCommonIds.CopyTo(eventIds);
            for (int i = 0; i < eventNames.Length; i++)
                eventNames[i] = m_vStageEvents.Find(
                    (killme) =>
                    {
                        if (killme.ID == tmpCommonIds[i])
                            return true;
                        return false;
                    }
                ).Des;
        }
        else
        {
            eventIds = null;
            eventNames = null;
        }
    }

    bool IsCommonEventChanged(int[] newEvents)
    {
        if (null == newEvents || m_vCommonEventIds == null)
            return true;

        if (newEvents.Length != m_vCommonEventIds.Count)
            return true;

        for (int i = 0; i < newEvents.Length; i++)
        {
            bool find = false;
            for (int j = 0; j < m_vCommonEventIds.Count; j++)
            {
                if (m_vCommonEventIds[j] == newEvents[i])
                {
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                return true;
            }
        }

        return false;
    }

    public override void OnPropertyChanged()
    {
        BindingTargetPropertyChangeUndo btcc = new BindingTargetPropertyChangeUndo(this);
        btcc.des = string.Format("Root Node[{0}] Property Changed", nodeID);
        NodeUndo.RecordUndo(btcc);
        TriggerNodeWindow.propChanged = true;
    }
}

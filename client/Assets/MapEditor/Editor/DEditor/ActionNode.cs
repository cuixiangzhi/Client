using MapEditConfigData;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ActionNode : BaseNode {
    const int MAX_PARAM_NUM = 6;
    private TriggerActionData _actionData;

    public TriggerActionData ActionData
    {
        get
        {
            if (_actionData == null)
            {
                _actionData = new TriggerActionData();
                _actionData.node = new TriggerNodeData();
                _actionData.actionInfo = new TriggerHandlerCellInfo();
            }
            _actionData.node.children.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                _actionData.node.children.Add(children[i].nodeID);
            }
            _actionData.node.parents.Clear();
            for (int i = 0; i < parents.Count; i++)
            {
                _actionData.node.parents.Add(parents[i].nodeID);
            }

            _actionData.node.posx = Position.x;
            _actionData.node.posy = Position.y;
            _actionData.node.id = nodeID;
            _actionData.node.NodeType = (int)nodeType;

            return _actionData;
        }

        set
        {
            _actionData = value;
            if (_actionData == null)
                _actionData = new TriggerActionData();
            if (_actionData.actionInfo == null)
                _actionData.actionInfo = new TriggerHandlerCellInfo();
            if (_actionData.node == null)
                _actionData.node = new TriggerNodeData();
            nodeID = _actionData.node.id;
            ResetSize();

            parents.Clear();
            children.Clear();

            Vector2 position = new Vector2(_actionData.node.posx, _actionData.node.posy);
            SetPosition(position);

            for (int i = 0; i < _actionData.node.children.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_actionData.node.children[i]);

                if (node != null)
                {
                    children.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的子节点!当前节点编号[{1}]", _actionData.node.children[i], nodeID));
                }
            }

            for (int i = 0; i < _actionData.node.parents.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_actionData.node.parents[i]);
                if (node != null)
                {
                    parents.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的父节点!当前节点编号[{1}]", _actionData.node.parents[i], nodeID));
                }
            }

            #region 初始化Action数据
            IEditEventSender[] objs = GameObject.FindObjectsOfType<IEditEventSender>();

            StageEventHandlerData.StageEventHandlerData data = DatasManager.Instance.GetData<StageEventHandlerData.StageEventHandlerData>(_actionData.actionInfo.handlerID);
            nodeName = data.Name;
            for (int i = 0; i < _actionData.actionInfo.handlerParams.Count; i++)
            {
                var handlerParam = _actionData.actionInfo.handlerParams[i];

                PropertyInfo paramSourceType = data.GetType().GetProperty(string.Format("Param{0}SourceType", i + 1));
                if (null == paramSourceType)
                {
                    continue;
                }
                ParamSourceType paramSourceTypeValue = (ParamSourceType)paramSourceType.GetValue(data, null);

                if (paramSourceTypeValue == ParamSourceType.UserSelect)
                {
                    foreach (var src in objs)
                    {
                        if (src.id == handlerParam.handlerTarget.id)
                        {
                            if ((int)src.ToTargetType() == handlerParam.handlerTarget.type)
                            {
                                handlerParam.handlerTarget.targetObj = src;
                                break;
                            }
                        }
                    }
                }
                else if (paramSourceTypeValue == ParamSourceType.Enum)
                {
                    List<AttrInfoData.AttrInfoData> infodatas = TriggerCondtionMappingTable.GetDatasWithEnumType(handlerParam.stringParam);
                    for (int tempIndex = 0; tempIndex < infodatas.Count; tempIndex++)
                    {
                        if (infodatas[tempIndex].Key == handlerParam.handlerIntParam)
                        {
                            handlerParam.tmpEnum = tempIndex;
                            break;
                        }
                    }
                }
            }
            #endregion
        }
    }

    public ActionNode()
    {
        nodeType = NodeType.Action;
    }

    public override void OnDrawGUI(Rect rect, Color color)
    {
        GUI.color = Color.black;
        GUI.Box(rect, "", GUI.skin.window);
        BeginNodeUI(rect);
        DrawHandlerCellData(ActionData.actionInfo);
        EndNodeUI();
    }

    //画每一条事件处理函数数据
    void DrawHandlerCellData(MapEditConfigData.TriggerHandlerCellInfo cell)
    {
        if (cell == null)
        {
            Debug.LogError("trigger handler cell info is null!");
            return;
        }

        GUI.color = Color.white;
        Label(string.Format("动作 ID: {0}", nodeID), labelStyle);

        StageEventHandlerData.StageEventHandlerData data = DatasManager.Instance.GetData<StageEventHandlerData.StageEventHandlerData>(_actionData.actionInfo.handlerID);
        if (data == null)
        {
            Debug.LogError(string.Format("StageEventHandlerData[{0}] is Null! NodeID:{1}", _actionData.actionInfo.handlerID, _actionData.node.id));
            return;
        }
        Label(new GUIContent(data.Name, data.Des), labelStyle);

        for (int i = 1; i <= MAX_PARAM_NUM; i++)
        {
            if (IsHasParam(i, data))
            {
                DoDrawHandleParam(i, data, cell);
            }
            else
            {
                break;
            }
        }
    }

    bool IsHasParam(int index, StageEventHandlerData.StageEventHandlerData data)
    {
        PropertyInfo[] infos = data.GetType().GetProperties();
        List<PropertyInfo> infoList = new List<PropertyInfo>(infos);

        PropertyInfo paramName = infoList.Find((x) => { return x.Name == string.Format("Param{0}SourceType", index); });
        if (null == paramName)
        {
            return false;
        }

        int val = (int)paramName.GetValue(data, null);
        return 0 != val;
    }

    void DoDrawHandleParam(int index, StageEventHandlerData.StageEventHandlerData data, MapEditConfigData.TriggerHandlerCellInfo cell)
    {
        PropertyInfo[] infos = data.GetType().GetProperties();
        List<PropertyInfo> infoList = new List<PropertyInfo>(infos);

        PropertyInfo paramName = infoList.Find((x) => { return x.Name == string.Format("Param{0}Name", index); });
        if (null == paramName)
        {
            return;
        }

        string val = (string)paramName.GetValue(data, null);
        Label(val);

        PropertyInfo paramEnumTag = infoList.Find((x) => { return x.Name == string.Format("Param{0}EnumTag", index); });
        if (null == paramEnumTag)
        {
            return;
        }

        if (cell.handlerParams.Count < index)
        {
            MapEditConfigData.TriggerHandlerParamInfo tmp = new MapEditConfigData.TriggerHandlerParamInfo();
            cell.handlerParams.Add(tmp);
        }

        PropertyInfo paramSourceType = infoList.Find((x) => { return x.Name == string.Format("Param{0}SourceType", index); });
        if (null == paramSourceType)
        {
            return;
        }
        int paramSourceTypeValue = (int)paramSourceType.GetValue(data, null);
        if (ParamSourceType.Enum == (ParamSourceType)paramSourceTypeValue)
        {
            string enumTag = (string)paramEnumTag.GetValue(data, null);
            cell.handlerParams[index - 1].stringParam = enumTag;
        }

        PropertyInfo paramSelectObjectType = infoList.Find((x) => { return x.Name == string.Format("Param{0}SelectObjectType", index); });
        if (null == paramSelectObjectType)
        {
            return;
        }

        if (cell.handlerParams.Count >= index)
        {
            List<int> paramSelectObjectTypeValue = (List<int>)paramSelectObjectType.GetValue(data, null);
            DrawHandlerParamData(data.ID, cell.handlerParams[index - 1], (ParamSourceType)paramSourceTypeValue, paramSelectObjectTypeValue);
        }
    }
    
    //画每条数据的参数项
    void DrawHandlerParamData(int handlerId, MapEditConfigData.TriggerHandlerParamInfo param, ParamSourceType srcType, List<int> selectObjectTypes)
    {
        #region
        if (srcType == ParamSourceType.ReadExcel)
        {
            Debug.LogError("handler param do not support get param from excel!");
            return;
        }
        switch (srcType)
        {
            case ParamSourceType.Enum:
                List<AttrInfoData.AttrInfoData> infodatas = TriggerCondtionMappingTable.GetDatasWithEnumType(param.stringParam);
                string[] attrDes = new string[infodatas.Count];
                for (int tempIndex = 0; tempIndex < infodatas.Count; tempIndex++)
                {
                    attrDes[tempIndex] = infodatas[tempIndex].Des;
                    if (infodatas[tempIndex].Key == param.handlerIntParam)
                        param.tmpEnum = tempIndex;
                }

                param.tmpEnum = Popup(param.tmpEnum, attrDes);
                param.handlerIntParam = infodatas[param.tmpEnum].Key;
                break;
            case ParamSourceType.UserSelect:
                if (null == param.handlerTarget)
                {
                    param.handlerTarget = new MapEditConfigData.TriggerTargetInfo();
                }
                param.handlerTarget.targetObj = ObjectField(param.handlerTarget.targetObj, typeof(IEditEventSender));
                GUI.color = Color.white;

                IEditEventSender sen = param.handlerTarget.targetObj as IEditEventSender;
                if (sen != null && selectObjectTypes.Contains((int)sen.ToTargetType()))
                {
                    param.handlerTarget.id = sen.id;
                    param.handlerTarget.type = (int)sen.ToTargetType();
                    param.handlerTarget.targetObj = sen;
                }
                else
                {
                    param.handlerTarget.targetObj = null;
                    param.handlerTarget.id = -1;
                }
                break;
            case ParamSourceType.UserInputInt:
                param.handlerIntParam = IntField(param.handlerIntParam);
                break;
            case ParamSourceType.UserInputFloat:
                param.handlerFloatParam = FloatField(param.handlerFloatParam);
                break;
            case ParamSourceType.UserInputString:
                param.stringParam = TextField(param.stringParam);
                if (string.IsNullOrEmpty(param.stringParam))
                {
                    param.stringParam = "";
                }
                break;

        }
        #endregion
    }

    public override string ToString()
    {
        return "Position: " + Position.ToString() + "  NodeID: " + nodeID + "  HandlerID:" + _actionData.actionInfo.handlerID + " item height = " + ItemHeight + " item width = " + ItemWidth;
    }

    public override void OnPropertyChanged()
    {
        ActionCellPropertyChangeUndo accc = new ActionCellPropertyChangeUndo(this);
        accc.des = string.Format("Action Node[{0}] Property Changed", nodeID);
        NodeUndo.RecordUndo(accc);
        TriggerNodeWindow.propChanged = true;
    }
}
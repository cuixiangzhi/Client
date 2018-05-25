using MapEditConfigData;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriggerNodeWindow : EditorWindow {
    public const int NODE_START_ID = 1;
    public static List<BaseNode> nodes = new List<BaseNode>();
    BaseNode currentNode = null;
    List<BaseNode> currentNodes = new List<BaseNode>();
    private int viewIndex = 0;

    public static TriggerInfo new_trigger;

    float zoomScale = 1f;
    float zoomTarget = 1f;
    Vector2 cameraPos = Vector2.zero;

    int screen_width;
    int screen_height;
    Rect windowMainViewRect;
    Rect windowConditionListRect;
    Rect windowActionListRect;

    private bool needExplore = true;

    Color color_item = new Color(0.6f, 0.6f, 0.6f, 0.8f);

    //条件定义列表
    List<ConditionTemplateItem> conditions = new List<ConditionTemplateItem>();
    Vector2 scrollposition_Condition = Vector2.zero;
    ConditionTemplateItem draggingConditionItem = null;
    float conditionViewRectPosY = 0;

    //动作定义列表
    Dictionary<string, List<ActionTemplateItem>> actions = new Dictionary<string, List<ActionTemplateItem>>();
    Vector2 scrollposition_Action = Vector2.zero;
    ActionTemplateItem DraggingActionItem = null;
    List<bool> foldout = new List<bool>();
    float actionViewRectPosY = 0;

    int labelWidth = 145;
    int labelHeight = 20;
    int tabHeight = 4;
    int listWindowWidth = 178;

    //读表数据
    public static List<StageEventHandlerData.StageEventHandlerData> m_vHandlers = new List<StageEventHandlerData.StageEventHandlerData>();
    public static List<ConditionAttrTypeData.ConditionAttrTypeData> m_vConditions = new List<ConditionAttrTypeData.ConditionAttrTypeData>();
    public static Dictionary<string, List<int>> triggerTargetTable = new Dictionary<string,List<int>>();

    Rect cuttingArea = new Rect();
    bool startCutting;
    Vector2 cutStartPos;
    bool isDragging = false;
    private List<Vector2> dragStartPos = new List<Vector2>();

    int caching = 0;

    public static bool isEditable
    {
        get
        {
            return !Application.isPlaying;
        }
    }
    public static bool propChanged = false;
    private bool excuteByStep;

    void OnEnable()
    {
        if(!EventManager.Instance.HasRegister<EventTriggerNodeExecBegin>(OnTriggerNodeExecBegin))
        {
            EventManager.Instance.AddListener<EventTriggerNodeExecBegin>(OnTriggerNodeExecBegin);
        }

        if (!EventManager.Instance.HasRegister<EventTriggerNodeExecFinish>(OnTriggerNodeExecFinish))
        {
            EventManager.Instance.AddListener<EventTriggerNodeExecFinish>(OnTriggerNodeExecFinish);
        }

        currentNode = null;
        currentNodes.Clear();
        draggingConditionItem = null;
        DraggingActionItem = null;
        scrollposition_Condition = Vector2.zero;
        scrollposition_Action = Vector2.zero;
        this.cameraPos = new Vector2(32368f, 32468f);
        needExplore = true;

        screen_width = Screen.width * 2;
        screen_height = Screen.height;

        conditions.Clear();
        actions.Clear();
        foldout.Clear();

        m_vHandlers = DatasManager.Instance.GetDatas<StageEventHandlerData.StageEventHandlerData>();
        m_vConditions = DatasManager.Instance.GetDatas<ConditionAttrTypeData.ConditionAttrTypeData>();
        if (m_vHandlers == null || m_vConditions == null)
        {
            DatasManager.Instance.LoadAllDatas();
            m_vHandlers = DatasManager.Instance.GetDatas<StageEventHandlerData.StageEventHandlerData>();
            m_vConditions = DatasManager.Instance.GetDatas<ConditionAttrTypeData.ConditionAttrTypeData>();
        }

        for (int i = 0; i < m_vHandlers.Count; i++)
        {
            ActionTemplateItem _acdata = new ActionTemplateItem();
            _acdata.pop_index = i;
            _acdata.handlerID = m_vHandlers[i].ID;
            _acdata.dataDesc = m_vHandlers[i].Des;
            _acdata.name = m_vHandlers[i].Name;
            _acdata.group = m_vHandlers[i].Group;
            _acdata.groupIndex = m_vHandlers[i].Index;
            if (!actions.ContainsKey(_acdata.group))
            {
                actions.Add(_acdata.group, new List<ActionTemplateItem>());
                foldout.Add(false);
            }
            actions[_acdata.group].Add(_acdata);
        }
        foreach (var key in actions.Keys)
        {
            actions[key].Sort((first, second) => { return first.groupIndex - second.groupIndex; });
        }


        for (int i = 0; i < m_vConditions.Count; i++)
        {
            ConditionTemplateItem _cddata = new ConditionTemplateItem();
            _cddata.desc = m_vConditions[i].Des;
            _cddata.tag = m_vConditions[i].Tag;
            _cddata.attrID = m_vConditions[i].AttrID;
            _cddata.attrType = m_vConditions[i].AttrType;
            _cddata.paremSource = m_vConditions[i].ParamSource;
            _cddata.readExcelParam = m_vConditions[i].ReadExcelFuncParam;
            _cddata.name = m_vConditions[i].Name;
            if (conditions.Find((con) => { return con.attrID == _cddata.attrID; }) == null)
            {
                conditions.Add(_cddata);
            }
            if (!triggerTargetTable.ContainsKey(_cddata.desc))
            {
                triggerTargetTable.Add(_cddata.desc, new List<int>());
                triggerTargetTable[_cddata.desc].Add(_cddata.tag);
            }
            else
            {
                triggerTargetTable[_cddata.desc].Add(_cddata.tag);
            }
        }
    }

    public void InitData(TriggerInfo _data)
    {
        //避免上一个触发器未保存而丢失数据
        SaveData();
        this.cameraPos = new Vector2(32368f, 32468f);
        NodeUndo.ClearRecords();
        propChanged = false;

        needExplore = true;
        new_trigger = _data;
        currentNode = null;
        currentNodes.Clear();
        draggingConditionItem = null;
        DraggingActionItem = null;
        scrollposition_Condition = Vector2.zero;
        scrollposition_Action = Vector2.zero;
        foreach (var _n in nodes)
        {
            if (_n != null)
                ScriptableObject.DestroyImmediate(_n);
        }
        nodes.Clear();

        #region 生成节点, 并为节点ID赋值, 方便节点之间通过ID相互查找父子
        //生成Empty数量相当的节点, 先给节点ID赋值
        for (int i = 0; i < new_trigger.nodes.emptyNodes.Count; i++)
        {
            BaseNode _node = ScriptableObject.CreateInstance<EmptyNode>();
            _node.nodeID = new_trigger.nodes.emptyNodes[i].id;
            nodes.Add(_node);
        }
        //生成Condition数量相当的节点, 先给节点ID赋值
        for (int i = 0; i < new_trigger.nodes.conditions.Count; i++)
        {
            BaseNode _node = ScriptableObject.CreateInstance<ConditionNode>();
            _node.nodeID = new_trigger.nodes.conditions[i].node.id;
            nodes.Add(_node);
        }
        //生成Action数量相当的节点, 先给节点ID赋值
        for (int i = 0; i < new_trigger.nodes.actions.Count; i++)
        {
            BaseNode _node = null;
            if (new_trigger.nodes.actions[i].actionInfo.handlerID == TRIGGER_HANDLER_CONSTANCE_ID.RANDOM_ACTION_BEGIN)
            {
                _node = ScriptableObject.CreateInstance<RandomGroupNode>();
            }
            else
            {
                _node = ScriptableObject.CreateInstance<ActionNode>();
            }
            _node.nodeID = new_trigger.nodes.actions[i].node.id;
            nodes.Add(_node);
        }
        #endregion

        //生成绑定对象根节点(这个是特殊的)
        Vector2 bindingNodePos = new Vector2(new_trigger.nodes.root.node.posx, new_trigger.nodes.root.node.posy);
        /*BaseNode binding = */AddNode(BaseNode.NodeType.Binding, bindingNodePos);

        #region 给各个节点数据赋值, 节点会根据上一步的ID赋值自动链接父子
        int nodeIndex = 0;
        //空节点 赋值
        for (int i = 0; i < new_trigger.nodes.emptyNodes.Count; i++, nodeIndex++)
        {
            (nodes[nodeIndex] as EmptyNode).EmptyData = new_trigger.nodes.emptyNodes[i];
        }
        //condition 赋值
        for (int i = 0; i < new_trigger.nodes.conditions.Count; i++, nodeIndex++)
        {
            (nodes[nodeIndex] as ConditionNode).ConditionData = new_trigger.nodes.conditions[i];
        }
        //action 赋值
        for (int i = 0; i < new_trigger.nodes.actions.Count; i++, nodeIndex++)
        {
            if (new_trigger.nodes.actions[i].actionInfo.handlerID == TRIGGER_HANDLER_CONSTANCE_ID.RANDOM_ACTION_BEGIN)
            {
                (nodes[nodeIndex] as RandomGroupNode).RandomData = new_trigger.nodes.actions[i];
            }
            else
            {
                (nodes[nodeIndex] as ActionNode).ActionData = new_trigger.nodes.actions[i];
            }
        }
        #endregion
    }

    void OnGUI()
    {
        if (new_trigger == null)
        {
            ShowNotification(new GUIContent("触发器数据丢失! 请选中场景中的触发器, 点击 打开节点编辑器! \n请勿在未保存地图数据时更新!"));
            return;
        }
        RemoveNotification();
        if (caching > 0)
        {
            caching--;
        }
        if (propChanged)
        {
            title = "*" + new_trigger.name;
        }
        else
        {
            title = new_trigger.name;
        }

        if (screen_height != Screen.height || screen_width != Screen.width)
        {
            screen_width = Screen.width;
            screen_height = Screen.height;

            windowConditionListRect = new Rect(0, 0, listWindowWidth, screen_height);
            windowActionListRect = new Rect(screen_width - listWindowWidth, 0, listWindowWidth, screen_height);
            windowMainViewRect = new Rect(listWindowWidth, 0, screen_width - listWindowWidth * 2, screen_height);
        }

        if (needExplore)
        {
            needExplore = false;
            ExploreTo();
        }

        BeginWindows();
        GUI.Window(2, windowConditionListRect, new GUI.WindowFunction(OnDrawConditionListWindow), "Conditions");
        GUI.Window(3, windowActionListRect, new GUI.WindowFunction(OnDrawActionListWindow), "Actions");
        EndWindows();

        Event e = Event.current;
        /*Rect windowArea = */ZoomArea.Begin(zoomScale, windowMainViewRect, cameraPos);
        {
            #region Events
            switch (e.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(e);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(e);
                    break;
                case EventType.MouseMove:
                    OnMouseMove(e);
                    break;
                case EventType.MouseUp:
                    OnMouseUP(e);
                    break;
                case EventType.KeyUp:
                    OnKeyUp(e);
                    break;
                case EventType.KeyDown:
                    OnKeyDown(e);
                    break;
                case EventType.ScrollWheel:
                    OnScrollWheel(e);
                    break;
            }
            #endregion

            #region DrawNode
            for (int i = 0; nodes != null && i < nodes.Count; i++)
            {
                if (currentNodes.Contains(nodes[i]))
                {
                    nodes[i].Draw(currentNodes, Color.cyan);
                }
                else
                {
                    nodes[i].Draw(currentNodes, color_item);
                }
            }
            #endregion

            if (startCutting)
            {
                Handles.color = Color.red;
                GUILines.DrawRect(cuttingArea, 5); 
            }
        }
        ZoomArea.End(zoomScale);
        SetZoom(Mathf.Lerp(zoomScale, zoomTarget, 0.1f));

        DoMainWindowUI();

        CheckDirtyItems();

        #region 拖拽创建Node
        if (e.type == EventType.Repaint)//这个必须得写在最外面,不能归到上面switch里.
        {
            if (draggingConditionItem != null)
            {
                Rect rect = new Rect();
                rect.width = labelWidth;
                rect.height = labelHeight;
                rect.center = e.mousePosition;
                GUI.Box(rect, "Condition: " + draggingConditionItem.desc);
            }
            else if (DraggingActionItem != null)
            {
                Rect rect = new Rect();
                rect.width = labelWidth;
                rect.height = labelHeight;
                rect.center = e.mousePosition;
                GUI.Box(rect, "Action: " + DraggingActionItem.name);
            }
        }
        #endregion

        if (GUI.changed)
        {
            SaveData();
        }
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            TriggerEditData ted = Selection.activeGameObject.GetComponent<TriggerEditData>();
            if (ted != null)
            {
                InitData(ted.Data);
                Repaint();
            }
        }
    }

    #region UI

    #region ConditionList
    void OnDrawConditionListWindow(int controlID)
    {
        Rect position = new Rect(0, 15, listWindowWidth, Screen.height - 38);
        Rect viewRect = new Rect(-10,0,1,1200);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Normal;
        viewRect.y = scrollposition_Condition.y;
        viewRect.height = Mathf.Max(conditionViewRectPosY, position.height);

        scrollposition_Condition = GUI.BeginScrollView(position, scrollposition_Condition, viewRect, false, true);
        Rect r = new Rect(0, tabHeight, labelWidth, labelHeight);
        for (int i = 0; i < conditions.Count; i++)
        {
            DrawButton(style, ref r, conditions[i]);
        }
        conditionViewRectPosY = r.y;
        GUI.EndScrollView();


        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
            case EventType.MouseDrag:
                Repaint();
                break;
            case EventType.MouseUp:
                if (windowConditionListRect.Contains(e.mousePosition))
                {
                    if (startCutting)
                    {
                        OnMouseUP(e);
                    }
                    else
                    {
                        Debug.Log("Cancel Create Condition!");
                        draggingConditionItem = null;
                        DraggingActionItem = null;
                        currentNode = null;
                        currentNodes.Clear();
                        Repaint();
                    }
                }
                break;
        }
    }
    public class ConditionTemplateItem 
    {
        public int tag;     //拖选类型判定
        /*
         * 0,
         * 1, //触发者
         * 2, //怪物
         * 3, //角色
         * 4, //计时器
         * 5, //触发器
         * 6, //区域检测
         * 7, //点
         * 8, //Bool关卡变量
         * 9, //Int关卡变量
         * 10,//塔
         * 11,//怪物组
         * 12,//Path    = 12,//路径
         * 13,//StarValue   = 13,//关卡变量
         */
        public int attrID;  //objectTargetType;
        /**
         * OTT_MonsterType      = 1,
         * OTT_CharacterType    = 2,
         * OTT_AliveState       = 3,
         * OTT_ExistState       = 4,
         * OTT_TimmerState      = 5,
         * OTT_TriggerState     = 6,
         * OTT_TriggerManIsSpecifiedId = 7,
         * OTT_HP               = 8,
         * OTT_BoolValue        = 9,
         * OTT_IntegerValue     = 10,
         * OTT_MonsterGroup     = 11,
         * OTT_SurveyedAreaTrapedState = 12,
        **/
        public string desc; //描述
        /*
         *  怪物类型
         *  生命值
         *  生死状态
         *  存在状态
         *  角色类型
         *  计时器运行状态
         *  触发器运行状态
         *  是否是指定的目标
         *  是否在区域检测器中
         *  Bool类型关卡变量
         *  整形关卡变量
         *  星星变量
         */
        public int attrType;
        public int paremSource;
        public int readExcelParam;
        public string name;
    }
    void DrawButton(GUIStyle style, ref Rect btnRect, ConditionTemplateItem data)
    {
        int remFontSize = style.fontSize;
        if (data == draggingConditionItem)
        {
            style.normal.textColor = Color.green;
            style.fontStyle = FontStyle.BoldAndItalic;
            style.fontSize = 16;
        }
        else
        { 
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 14;
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && isEditable)
        {
            if (btnRect.Contains(Event.current.mousePosition))
            {
                draggingConditionItem = data;
                DraggingActionItem = null;
                currentNode = null;
                currentNodes.Clear();
            }
            GUI.FocusControl(null);
        }
        GUI.Box(btnRect, new GUIContent("", data.desc));
        GUI.Label(btnRect, data.desc, style);
        style.fontSize = remFontSize;
        btnRect.y += btnRect.height + tabHeight;
    }
    #endregion

    #region ActionList
    void OnDrawActionListWindow(int controlID)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.MiddleCenter;
        style.wordWrap = false;
        style.fontSize = 10;

        Rect position = new Rect(0, 15, listWindowWidth, Screen.height - 38);
        Rect viewRect = new Rect(-10, 0, 1, 1200);
        viewRect.y = scrollposition_Action.y;

        viewRect.height = Mathf.Max(actionViewRectPosY, position.height);
        scrollposition_Action = GUI.BeginScrollView(position, scrollposition_Action, viewRect, false, true);
        Rect r = new Rect(0, tabHeight, labelWidth, labelHeight);

        Rect titleRect = new Rect(-9, 0, 176, labelHeight + tabHeight);
        int i = 0;
        foreach (var key in actions.Keys)
        {
            titleRect.y = r.y;
            GUI.color = Color.cyan;
            style.normal.textColor = Color.cyan;
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 16;
            GUI.Box(titleRect, key, style);
            GUI.color = Color.white;
            foldout[i] = EditorGUI.Foldout(titleRect, foldout[i], "", true);
            r.y += titleRect.height + tabHeight;
            if (foldout[i])
            { 
                foreach (var action in actions[key])
                {
                    DrawButton(style, ref r, action);
                }
            }
            i++;
        }
        actionViewRectPosY = r.y;
        GUI.EndScrollView();


        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
            case EventType.MouseDrag:
                Repaint();
                break;
            case EventType.MouseUp:
                if (windowConditionListRect.Contains(e.mousePosition))
                {
                    if (startCutting)
                    {
                        OnMouseUP(e);
                    }
                    else
                    {
                        Debug.Log("Cancel Create Action!");
                        draggingConditionItem = null;
                        DraggingActionItem = null;
                        currentNode = null;
                        currentNodes.Clear();
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
                break;
        }
    }
    class ActionTemplateItem
    {
        public int pop_index;
        public int handlerID;
        public string dataDesc;
        public string name;
        public string group;
        public int groupIndex;
    }
    void DrawButton(GUIStyle style, ref Rect btnRect, ActionTemplateItem data)
    { 
        if (data == DraggingActionItem)
        {
            style.normal.textColor = Color.green;
            style.fontStyle = FontStyle.BoldAndItalic;
            style.fontSize = 15;
        }
        else
        { 
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 13;
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && isEditable)
        {
            if (btnRect.Contains(Event.current.mousePosition))
            {
                GUI.FocusControl(null);
                draggingConditionItem = null;
                DraggingActionItem = data;
                currentNode = null;
                currentNodes.Clear();
            }
        }
        GUI.Box(btnRect, new GUIContent("", data.dataDesc + "\nHandlerID: " + data.handlerID.ToString()));
        GUI.Label(btnRect, data.name, style);
        btnRect.y += btnRect.height + tabHeight;
    }
    #endregion

    #region NodeViewUI
    void DoMainWindowUI()
    {
        if (isEditable)
            return;

        GUI.BeginGroup(windowMainViewRect);
        Rect r = new Rect(tabHeight, TabOffset + tabHeight, labelWidth, labelHeight);
        excuteByStep = GUI.Toggle(r, excuteByStep, "执行到节点暂停");

        if (excuteByStep)
        {
            r = new Rect((windowMainViewRect.width - labelWidth) * 0.5f, tabHeight + TabOffset, labelWidth, labelHeight);
            if (EditorApplication.isPaused && GUI.Button(r, "继续"))
            {
                EditorApplication.isPaused = false;
            }
        }
        GUI.EndGroup();
    }
    #endregion

    #endregion

    #region 缩放
    public float TabOffset = 22.0f;

    void OnScrollWheel(Event e)
    {
        zoomTarget = ClampZoom(zoomTarget * (1f - e.delta.y * 0.05f));
    }
    float ClampZoom(float in_zoom)
    {
        return Mathf.Clamp(in_zoom, 0.125f, 2.0f);
    }
    public void SetZoom(float setZoom)
    {
        if (zoomScale == zoomTarget)
            return;

        Vector2 vector = new Vector2(this.windowMainViewRect.width, this.windowMainViewRect.height) / this.zoomScale;
        this.zoomScale = this.ClampZoom(setZoom);
        Vector2 vector2 = new Vector2(this.windowMainViewRect.width, this.windowMainViewRect.height) / this.zoomScale;
        Vector2 vector3 = vector2 - vector;
        Vector2 vector4 = new Vector2(windowMainViewRect.width / 2.0f, windowMainViewRect.height / 2.0f); //Event.current.mousePosition - new Vector2(this.windowMainViewRect.xMin, TabOffset);
        vector4.x /= this.windowMainViewRect.width;
        vector4.y /= this.windowMainViewRect.height;
        this.cameraPos -= Vector2.Scale(vector3, vector4);

        if (Mathf.Abs(zoomTarget - zoomScale) < 1e-3f)
        {
            zoomScale = zoomTarget;
            SnapCamera();
        }

        Repaint();
    }
    private void SnapCamera()
    {
        Vector2 recCamPos = cameraPos;
        cameraPos = new Vector2(32368f, 32468f);
        Vector2 delta = cameraPos - recCamPos;

        foreach (var n in nodes)
        {
            n.Move(delta);
        }
    }

    public Vector2 ZoomSpaceToScreenSpace(Vector2 in_vec)
    {
        return (in_vec - this.cameraPos + windowMainViewRect.TopLeft()) * this.zoomScale + this.windowMainViewRect.TopLeft() + new Vector2(0, TabOffset * (this.zoomScale - 1f));
    }
    public Rect ZoomSpaceToScreenSpace(Rect in_rect)
    {
        Vector2 vector = this.ZoomSpaceToScreenSpace(in_rect.TopLeft());
        in_rect.x = vector.x;
        in_rect.y = vector.y;
        in_rect.width = in_rect.width / this.zoomScale;
        in_rect.height = in_rect.height / this.zoomScale;
        return in_rect;
    }
    public Vector2 ScreenSpaceToZoomSpace(Vector2 in_vec)
    {
        return (in_vec - this.windowMainViewRect.TopLeft()) / this.zoomScale - windowMainViewRect.TopLeft() + this.cameraPos + new Vector2(0, TabOffset * (this.zoomScale - 1f));
    }
    public Rect ScreenSpaceToZoomSpace(Rect in_rect)
    {
        Vector2 vector = this.ScreenSpaceToZoomSpace(in_rect.TopLeft());
        in_rect.x = vector.x;
        in_rect.y = vector.y;
        in_rect.width = in_rect.width * this.zoomScale;
        in_rect.height = in_rect.height * this.zoomScale;
        return in_rect;
    }
    #endregion

    void CheckDirtyItems()
    {
        bool needRepaint = false;

        nodes.Sort((f, s) => { return f.nodeID - s.nodeID; });

        nodes.RemoveAll((_node) =>
        {
            if (_node.readyToGC)
            {
                needRepaint = true;
                PastDelNode(_node);
                return true;
            }
            return false;
        });

        if (needRepaint)
        {
            SaveData();
            Repaint();
        }
    }

    #region 事件
    void OnMouseDown(Event e)
    {
        if (!isEditable)
            return;

        //这里逻辑需要好好的理理

        currentNode = CheckCurrentSelected(e);
        if (currentNode != null)
        {
            if (currentNodes.Count > 1 && currentNodes.Exists((node) => { return node.nodeID == currentNode.nodeID; }))
            {
                //nothing to do
            }
            else
            {
                currentNodes.Clear();
                currentNodes.Add(currentNode);
                currentNode.OnMouseDown(e.mousePosition);
                viewIndex = currentNode.nodeID;
            }
        }
        else if (currentNode == null)
        {
            if (e.button == 1)
            {
                ShowContext();
            }

            currentNodes.Clear();
            GUI.FocusControl(null);
        }

        Repaint();
    }

    void OnKeyUp(Event e)
    {
        if (!isEditable)
            return;

        switch (e.keyCode)
        {
            case KeyCode.Delete:
                if (e.shift)
                {
                    if (currentNode != null)
                    {
                        if (EditorUtility.DisplayDialog("Warning!", "确认移除当前分支?", "确定", "取消"))
                        {
                            if (currentNode.isRoot || currentNode.nodeType == BaseNode.NodeType.Binding)
                            {
                                EditorUtility.DisplayDialog("Warning", "当前节点不可删除!", "OK");
                                return;
                            }

                            NodeGroupDestroyUndo nfc = new NodeGroupDestroyUndo(currentNode.nodeID);
                            nfc.des = string.Format("移除节点[{0}]及其分支", currentNode.nodeID);
                            NodeUndo.RecordUndo(nfc);

                            currentNode.OnDelete(true);
                        }
                        this.Focus();
                    }
                }
                else
                {
                    if (currentNodes.Count > 0)
                    {
                        if (EditorUtility.DisplayDialog("Warning!", "确认移除当前所选项?", "确定", "取消"))
                        {
                            NodeGroupDestroyUndo ngdc = new NodeGroupDestroyUndo();
                            ngdc.des = "删除所选多个节点:";

                            bool delResult = false;//检测是否有Node被删除
                            foreach (var node in currentNodes)
                            {
                                if (node.isRoot || node.nodeType == BaseNode.NodeType.Binding)
                                {
                                    EditorUtility.DisplayDialog("Warning", "根节点不可删除!", "OK");
                                    continue;
                                }

                                delResult = true;
                                NodeCreateDestroyUndoUtil bnc = new NodeCreateDestroyUndoUtil(node, NodeOperatorType.DESTROY);
                                ngdc.AddUndo(bnc);
                                ngdc.des += string.Format("\n\t删除节点[{0}]", node.nodeID);

                                node.OnDelete();
                            }
                            if (delResult)
                            {
                                NodeUndo.RecordUndo(ngdc);
                            }
                        }
                        this.Focus();
                    }
                }
                break;
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                //OnStopCutting(e);
                break;
        }
    }

    void OnKeyDown(Event e)
    {
        if (!isEditable)
            return;

        switch (e.keyCode)
        {
            case KeyCode.Z:
                if (e.shift && e.control)
                {
                    if (caching == 0)
                    {
                        caching = 10;
                        NodeUndo.Undo();
                        SaveData();
                    }
                    else
                    {
                        Debug.Log("waiting!!!");
                    }
                }
                break;
            case KeyCode.Y:
                if (e.shift && e.control)
                {
                    if (caching == 0)
                    {
                        caching = 10;
                        NodeUndo.ReverseUndo();
                        SaveData();
                    }
                }
                break;
            case KeyCode.F1:
                {
                    BaseNode root = TriggerNodeWindow.GetRootNode();
                    if (root != null)
                    {
                        ExploreTo(root);
                        viewIndex = root.nodeID;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "未在节点列表中找到绑定对象根节点! 很不幸的告诉您, 这个触发器让您编废了...", "OK");
                    }
                    e.Use();
                }
                break;
            case KeyCode.LeftArrow:
                ExploreAuto(false);
                break;
            case KeyCode.RightArrow:
                ExploreAuto(true);
                break;
            case KeyCode.S:
                if (e.shift)
                { 
                    SaveData();
                }
                break;
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                //OnStartCutting(e);
                break;
        }
        Repaint();
    }

    void OnMouseUP(Event e)
    {
        if (!isEditable)
            return;

        if (startCutting)
        {
            OnStopCutting(e);
            return;
        }

        if (currentNodes.Count > 1)
        {
            if (isDragging)
            {
                isDragging = false;

                BaseNode root = GetRootNode();
                GroupMoveUndo gmc = new GroupMoveUndo();
                for(int i = 0; i < currentNodes.Count; i++)
                {
                    Vector2 local_org = dragStartPos[i] - root.Position;
                    Vector2 local_cur = currentNodes[i].Position - root.Position;

                    MoveUndo mc = new MoveUndo(local_org, local_cur, currentNodes[i].nodeID);
                    gmc.AddUndo(mc);
                }
                gmc.des = "移动所有已选节点";
                NodeUndo.RecordUndo(gmc);
            }
        }
        else if (currentNode != null)
        {
            BaseNode tmp = CheckCurrentSelected(e);
            if (tmp != null)
            {
                tmp.OnMouseUp(currentNode, e.mousePosition);
            }
            else
            {
                currentNode.OnMouseUp(tmp, e.mousePosition);
            }
            SaveData();
        }
        else if (draggingConditionItem != null)
        {
            currentNode = AddNode(BaseNode.NodeType.Condition, e.mousePosition);
            currentNodes.Clear();
            currentNodes.Add(currentNode);
            SaveData();
        }
        else if (DraggingActionItem != null)
        {
            if (DraggingActionItem.handlerID == TRIGGER_HANDLER_CONSTANCE_ID.RANDOM_ACTION_BEGIN)
            {
                currentNode = AddNode(BaseNode.NodeType.Random, e.mousePosition);
            }
            else
            {
                currentNode = AddNode(BaseNode.NodeType.Action, e.mousePosition);
            }
            currentNodes.Clear();
            currentNodes.Add(currentNode);
            SaveData();
        }

        startCutting = false;
        DraggingActionItem = null;
        draggingConditionItem = null;
        Repaint();
    }

    void OnMouseDrag(Event e)
    {
        if (!isEditable)
            return;

        if (e.alt)
        {
            OnStartCutting(e);
        }
        if (startCutting)
        {
            OnCuttingUpdate(e);
            return;
        }

        //还有这里的逻辑
        if (currentNodes.Count > 1)
        {
            if (!isDragging)
            {
                isDragging = true;
                dragStartPos.Clear();

                for (int i = 0; i < currentNodes.Count; i++)
                {
                    dragStartPos.Add(currentNodes[i].Position);
                }
            }

            foreach (var node in currentNodes)
            {
                node.Move(e.delta);
            }

            Repaint();
        }
        else if(currentNode != null)
        {
            BaseNode hoverNode = CheckCurrentSelected(e);
            currentNode.OnMouseDrag(e, hoverNode);
            Repaint();
        }
        else if (draggingConditionItem != null)
        {
            Rect rect = new Rect();
            rect.center = e.mousePosition;
            rect.width = labelWidth;
            rect.height = labelHeight;
            GUILines.DrawRect(rect);
            Repaint();
        }
        else if (DraggingActionItem != null)
        {
            Rect rect = new Rect();
            rect.center = e.mousePosition;
            rect.width = labelWidth;
            rect.height = labelHeight;
            GUILines.DrawRect(rect);
            Repaint();
        }
        else
        {
            float multiSpeed = 1;
            if (e.shift)
            {
                multiSpeed = 5;
            }
            else if (e.control)
            {
                multiSpeed = 2;
            }
            Vector2 delta = e.delta * multiSpeed;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Move(delta);
            }
            Repaint();
            SaveData();
        }
    }

    void OnMouseMove(Event e)
    {
        if (!isEditable)
            return;
    }

    void OnDestroy()
    {
        SaveData();
        foreach (var n in nodes)
        {
            if (n != null)
                ScriptableObject.DestroyImmediate(n);
        }
        nodes.Clear();
        new_trigger = null;

        EventManager.Instance.RemoveListener<EventTriggerNodeExecBegin>(OnTriggerNodeExecBegin);
        EventManager.Instance.RemoveListener<EventTriggerNodeExecFinish>(OnTriggerNodeExecFinish);
    }

    void OnLostFocus()
    {
        SaveData();
    }


    void OnStartCutting(Event e)
    {
        if (startCutting)
            return;

        currentNode = null;
        DraggingActionItem = null;
        draggingConditionItem = null;
        currentNodes.Clear();
        startCutting = true;
        cutStartPos = e.mousePosition;
        Repaint();
    }
    void OnCuttingUpdate(Event e)
    {
        cuttingArea = CalcCutArea(cutStartPos, e.mousePosition);

        currentNodes.Clear();

        //计算出裁切区域中的所有节点
        foreach (var node in nodes)
        {
            if (cuttingArea.Contains(node.Position))
            {
                currentNodes.Add(node);
            }
        }

        Repaint();
    }
    void OnStopCutting(Event e)
    {
        startCutting = false;
        //Vector2 stopCutPos = e.mousePosition;

        currentNodes.Clear();

        //计算出裁切区域中的所有节点
        foreach (var node in nodes)
        {
            if (cuttingArea.Contains(node.Position))
            {
                currentNodes.Add(node);
            }
        }

        Repaint();
    }


    void SaveData()
    {
        if (new_trigger != null)
        {
            new_trigger.nodes.actions.Clear();
            new_trigger.nodes.conditions.Clear();
            new_trigger.nodes.emptyNodes.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                switch (nodes[i].nodeType)
                {
                    case BaseNode.NodeType.Action:
                        if (nodes[i] is ActionNode)
                        {
                            new_trigger.nodes.actions.Add((nodes[i] as ActionNode).ActionData);
                        }
                        else
                        {
                            new_trigger.nodes.actions.Add((nodes[i] as RandomGroupNode).RandomData);
                        }
                        break;
                    case BaseNode.NodeType.Condition:
                        new_trigger.nodes.conditions.Add((nodes[i] as ConditionNode).ConditionData);
                        break;
                    case BaseNode.NodeType.Empty:
                        new_trigger.nodes.emptyNodes.Add((nodes[i] as EmptyNode).EmptyData);
                        break;
                    case BaseNode.NodeType.Binding:
                        new_trigger.nodes.root = (nodes[i] as BindingTargetNode).RootData;
                        break;
                }
            }
        }
        propChanged = false;
    }
    #endregion

    #region 右键菜单
    void ShowContext()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("创建空节点"), false, new GenericMenu.MenuFunction2(OnContextClick), new InData("create_empty", Event.current.mousePosition));
        genericMenu.AddItem(new GUIContent("帮助"), false, new GenericMenu.MenuFunction2(OnContextClick), new InData("help", Event.current.mousePosition));
        genericMenu.AddItem(new GUIContent("关于"), false, new GenericMenu.MenuFunction2(OnContextClick), new InData("about", Event.current.mousePosition));
        genericMenu.ShowAsContext();
        Event.current.Use();
    }
    class InData
    {
        public InData(string command, Vector2 position)
        {
            this.command = command;
            this.pos = position;
        }
        public string command;
        public Vector2 pos;
    }
    void OnContextClick(object o)
    {
        InData dat = o as InData;

        if (dat.command.Equals("create_empty"))
        {
            AddNode(BaseNode.NodeType.Empty, dat.pos);
        }
        else if (dat.command.Equals("about"))
        {
            NodeHelper.type = NodeHelper.NodeHelperWindowType.about;
            EditorWindow.GetWindowWithRect<NodeHelper>(new Rect(0, 0, 500, 200), true, "关于").Show();
        }
        else if (dat.command.Equals("help"))
        {
            NodeHelper.type = NodeHelper.NodeHelperWindowType.help;
            EditorWindow.GetWindowWithRect<NodeHelper>(new Rect(0, 0, 600, 300), true, "帮助文档").Show();
        }
    }
    #endregion

    private BaseNode AddNode(BaseNode.NodeType type, Vector2 nodePos)
    {
        BaseNode _node = null;
        switch (type)
        {
            case BaseNode.NodeType.Condition:
                if (draggingConditionItem != null)
                {
                    _node = ScriptableObject.CreateInstance<ConditionNode>();
                    (_node as ConditionNode).Init(draggingConditionItem);
                }
                else
                {
                    System.Exception up = new System.Exception("Tell me how you create this condition node! Create Error!");
                    throw up;
                }
                //添加ID
                _node.nodeID = GenNodeID();
                new_trigger.nodes.conditions.Add((_node as ConditionNode).ConditionData);
                break;
            case BaseNode.NodeType.Action:
                if (DraggingActionItem != null)
                {
                    _node = ScriptableObject.CreateInstance<ActionNode>();
                    (_node as ActionNode).ActionData.actionInfo.handlerID = DraggingActionItem.handlerID;
                    _node.nodeName = DraggingActionItem.name;
                }
                else
                {
                    System.Exception up = new System.Exception("Tell me how you create this action node! Create Error!");
                    throw up;
                }
                //添加ID
                _node.nodeID = GenNodeID();
                new_trigger.nodes.actions.Add((_node as ActionNode).ActionData);
                break;
            case BaseNode.NodeType.Empty:
                _node = ScriptableObject.CreateInstance<EmptyNode>();
                //添加ID
                _node.nodeID = GenNodeID();
                new_trigger.nodes.emptyNodes.Add((_node as EmptyNode).EmptyData);
                break;
            case BaseNode.NodeType.Random:
                _node = ScriptableObject.CreateInstance<RandomGroupNode>();
                //添加ID
                _node.nodeID = GenNodeID();
                new_trigger.nodes.actions.Add((_node as RandomGroupNode).RandomData);
                break;
            case BaseNode.NodeType.Binding:
                _node = ScriptableObject.CreateInstance<BindingTargetNode>();
                (_node as BindingTargetNode).RootData = new_trigger.nodes.root;
                break;
        }

        if (_node == null)
        {
            Debug.LogError("What The Fuck?! I dont know what ya creating!");
            return null;
        }

        _node.ResetSize();
        _node.SetPosition(nodePos);

        nodes.Add(_node);

        if (_node.nodeType != BaseNode.NodeType.Binding)
        {
            NodeCreateDestroyUndoUtil bnc = new NodeCreateDestroyUndoUtil(_node, NodeOperatorType.CREATE);
            NodeUndo.RecordUndo(bnc);
        }

        return _node;
    }
    private int GenNodeID()
    {
        int ID = NODE_START_ID;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (ID <= nodes[i].nodeID)
            {
                ID = nodes[i].nodeID + 1;
            }
        }
        return ID;
    }

    void PastDelNode(BaseNode _node)
    {
        switch (_node.nodeType)
        {
            case BaseNode.NodeType.Empty:
                {
                    EmptyNode node = _node as EmptyNode;
                    if (new_trigger.nodes.emptyNodes.Contains(node.EmptyData))
                    {
                        new_trigger.nodes.emptyNodes.Remove(node.EmptyData);
                    }
                    else
                    {
                        Debug.LogError("移除空节点,但是并未在原数据中找到相应节点数据!");
                    }
                }
                break;
            case BaseNode.NodeType.Action:
                {
                    if (_node is RandomGroupNode)
                    {
                        RandomGroupNode node = _node as RandomGroupNode;

                        if (new_trigger.nodes.actions.Contains(node.RandomData))
                        {
                            new_trigger.nodes.actions.Remove(node.RandomData);
                        }
                        else
                        {
                            Debug.LogError("移除随机组节点,但是并未在原数据中找到相应节点数据!");
                        }
                    }
                    else
                    {
                        ActionNode node = _node as ActionNode;

                        if (new_trigger.nodes.actions.Contains(node.ActionData))
                        {
                            new_trigger.nodes.actions.Remove(node.ActionData);
                        }
                        else
                        {
                            Debug.LogError("移除动作节点,但是并未在原数据中找到相应节点数据!");
                        }
                    }
                }
                break;
            case BaseNode.NodeType.Condition:
                {
                    ConditionNode node = _node as ConditionNode;
                    if (new_trigger.nodes.conditions.Contains(node.ConditionData))
                    {
                        new_trigger.nodes.conditions.Remove(node.ConditionData);
                    }
                    else
                    {
                        Debug.LogError("移除条件节点,但是并未在原数据中找到相应节点数据!");
                    }
                }
                break;
            case BaseNode.NodeType.Binding:
                Debug.LogError("不要开玩笑,这个你移除不了!!!");
                break;
        }

        ScriptableObject.DestroyImmediate(_node);
    }

    BaseNode CheckCurrentSelected(Event e)
    {
        if (nodes.Count == 0)
            return null;

        BaseNode tmp = null;
        List<BaseNode> selections = new List<BaseNode>();
        List<Rect> rects = new List<Rect>();

        for (int i = 0; i < nodes.Count; i++)
        {
            Rect r = new Rect();
            r.width = nodes[i].body_Rect.width + nodes[i].leftHand_Rect.width + nodes[i].rightHand_Rect.width;
            r.height = nodes[i].body_Rect.height + nodes[i].leftHand_Rect.height + nodes[i].rightHand_Rect.height;
            r.center = nodes[i].Position;
            rects.Add(r);
        }

        for (int i = 0; i < rects.Count; i++)
        {
            if (rects[i].Contains(e.mousePosition))
            {
                selections.Add(nodes[i]);
            }
        }

        if (selections.Count > 0)
        {
            float dis = 1000;
            float disTest;
            foreach (BaseNode t in selections)
            {
                disTest = Vector2.Distance(t.Position, e.mousePosition);
                if (dis > disTest)
                {
                    tmp = t;
                    dis = disTest;
                }
            }
        }
        return tmp;
    }

    void ExploreTo()
    {
        BaseNode root = TriggerNodeWindow.GetRootNode();
        Vector2 center = root.Position;
        Vector2 targetCenter = new Vector2(windowConditionListRect.width + root.ItemWidth * 0.5f + TabOffset, screen_height * 0.5f);
        targetCenter = ScreenSpaceToZoomSpace(targetCenter);
        Vector2 delta = targetCenter - center;
        foreach (BaseNode _node in nodes)
        {
            _node.Move(delta);
        }
        currentNode = root;
        currentNodes.Clear();
        currentNodes.Add(root);
        SaveData();
        Repaint();
    }

    void ExploreTo(BaseNode node)
    {
        if (node is BindingTargetNode)
        {
            ExploreTo();
            return;
        }

        Vector2 targetCenter = new Vector2(screen_width * 0.5f, screen_height * 0.5f);
        targetCenter = ScreenSpaceToZoomSpace(targetCenter);
        Vector2 delta = targetCenter - node.Position;

        currentNode = node;
        currentNodes.Clear();
        currentNodes.Add(node);
        draggingConditionItem = null;
        DraggingActionItem = null;

        foreach (BaseNode _node in nodes)
        {
            _node.Move(delta);
        }
        SaveData();
        Repaint();
    }

    void ExploreAuto(bool forward)
    {
        int min = 999;
        int max = -1;
        foreach (BaseNode _node in nodes)
        {
            if (min > _node.nodeID)
                min = _node.nodeID;
            if (max < _node.nodeID)
                max = _node.nodeID;
        }

        BaseNode _view_node = null;

        if (forward)
            viewIndex++;
        else
            viewIndex--;

        while (min <= max)
        {
            if (viewIndex > max)
                viewIndex = min;
            if (viewIndex < min)
                viewIndex = max;
            _view_node = GetNodeByID(viewIndex);

            if (_view_node != null)
            {
                ExploreTo(_view_node);
                break;
            }
            else
            {
                if (forward)
                    viewIndex++;
                else
                    viewIndex--;
            }
        } 
    }

    Rect CalcCutArea(Vector2 startPos, Vector2 endPos)
    {
        Rect cutRect = new Rect();

        if (endPos.x > startPos.x)
        {
            cutRect.xMin = startPos.x;
            cutRect.xMax = endPos.x;
        }
        else
        {
            cutRect.xMin = endPos.x;
            cutRect.xMax = startPos.x;
        }

        if (endPos.y > startPos.y)
        {
            cutRect.yMin = startPos.y;
            cutRect.yMax = endPos.y;
        }
        else
        {
            cutRect.yMax = startPos.y;
            cutRect.yMin = endPos.y;
        }
        return cutRect;
    }

    Vector2 GetNodesCenter()
    {
        Vector2 center = Vector2.zero;
        if (nodes.Count == 0)
            return center;

        foreach (BaseNode _node in nodes)
        {
            center += _node.Position;
        }
        return center / (float)nodes.Count;
    }

    public static BaseNode GetNodeByID(int nodeID)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].nodeID == nodeID)
                return nodes[i];
        }

        return null;
    }

    void OnTriggerNodeExecBegin(EventTriggerNodeExecBegin e)
    {
        if (new_trigger == null || e.TriggerId != new_trigger.id)
            return;

        BaseNode n = nodes.Find((_n) => { return _n.nodeID == e.NodeId; });

        if (n != null)
        {
            Rect test = n.body_Rect;
            test.center = ZoomSpaceToScreenSpace(test.center);
            test.width *= zoomScale;
            test.height *= zoomScale;

            if (windowMainViewRect.Incapsule(test))
            {
                currentNode = n;
                currentNodes.Clear();
                currentNodes.Add(n);
                DraggingActionItem = null;
                draggingConditionItem = null;
            }
            else
            {
                ExploreTo(n);
            }

            if (!isEditable && excuteByStep)
            {
                EditorApplication.isPaused = true;
            }

            this.Focus();
            this.Repaint();
        }
    }
    void OnTriggerNodeExecFinish(EventTriggerNodeExecFinish e)
    {
        if (new_trigger == null || e.TriggerId != new_trigger.id)
            return;

        currentNode = null;
        DraggingActionItem = null;
        draggingConditionItem = null;
        currentNodes.Clear();
        this.Focus();
        this.Repaint();
    }

    public static BaseNode GetRootNode()
    {
        foreach (var node in nodes)
        {
            if (node.nodeType == BaseNode.NodeType.Binding)
                return node;
        }

        System.Exception up = new System.Exception("未找到当前触发器的根节点!");
        throw up;
    }
}
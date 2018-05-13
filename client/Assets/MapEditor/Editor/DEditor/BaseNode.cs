using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BaseNode : ScriptableObject
{
    //大小信息
    public int ItemWidth = 120;
    public int ItemHeight = 150;
    public int HandWidth = 20;
    public int HandHeight = 15;
    public static int HandIndentPixel = 2;

    //位置信息
    public Rect body_Rect = new Rect();
    public Rect leftHand_Rect;
    public Rect rightHand_Rect;
    public Vector2 Position { get { return body_Rect.center; } set { SetPosition(value); } }

    //链接线按键大小
    public int buttonWidth = 18;
    public int buttonHeight = 18;
    public int tabHeight = 2;

    //删除节点标志
    public bool readyToGC = false;

    //节点以及关系数据
    public bool isRoot { get { return nodeType == NodeType.Binding; } }
    public List<BaseNode> children = new List<BaseNode>();
    public List<int> weights = new List<int>();
    public List<BaseNode> parents = new List<BaseNode>();
    public int nodeID = 0;//自身ID
    public enum NodeType
    {
        Empty = 0,
        Condition = 1,
        Action = 2,
        Security = 3,
        Random = 4,
        Binding = 5
    }
    public NodeType nodeType = NodeType.Empty;
    public string nodeName;

    protected BaseNode currentHoverNode;
    private bool isDragging;
    Vector2 dragBeginPos = Vector2.zero;

    public enum CurrentSelection
    {
        NONE = 0,       //无
        LeftHand = 1,   //左手
        RightHand = 2,  //右手
        ItemBody = 3    //主体
    }
    public CurrentSelection selectedType = CurrentSelection.NONE;

    public enum NodeGenericPopup
    {
        break_parent = 1,
        break_child = 2,
        break_all = 3,
        delete = 4,
        set_root = 5,
        log_info = 6
    }

    private Rect uiItemRect;
    protected GUIStyle labelStyle;

    public virtual void ResetSize()
    {
        Vector2 center = Position;
        body_Rect = new Rect();
        body_Rect.width = ItemWidth;
        body_Rect.height = ItemHeight;
        Position = center;

        leftHand_Rect = new Rect();
        leftHand_Rect.width = HandWidth;
        leftHand_Rect.height = HandHeight;
        if (isRoot)
        {
            leftHand_Rect.center = new Vector2(body_Rect.xMin, Position.y);
        }
        else
        {
            leftHand_Rect.center = new Vector2(Position.x + HandIndentPixel - (body_Rect.width + leftHand_Rect.width) * 0.5f, Position.y);
        }

        rightHand_Rect = new Rect();
        rightHand_Rect.width = HandWidth;
        rightHand_Rect.height = HandHeight;
        rightHand_Rect.center = new Vector2(Position.x - HandIndentPixel + (body_Rect.width + rightHand_Rect.width) * 0.5f, Position.y);
    }

    #region 右键点击
    public class PopupData
    {
        public NodeGenericPopup select;
        public int intParam;
        public float floatParam;
        public string strParam;
    }

    public virtual void ContextClick(object o)
    {
        PopupData data = o as PopupData;
        if (data == null)
            return;
        switch (data.select)
        {
            case NodeGenericPopup.break_all:
                ConnectionGroupCommand cgc = new ConnectionGroupCommand();
                cgc.des = string.Format("移除节点[{0}]所有链接:", nodeID);
                bool breakResult = false;
                foreach (BaseNode parent in parents)
                {
                    ConnectionUndo cc = new ConnectionUndo(parent.nodeID, nodeID, CennectionLinkState.BREAKING);
                    cgc.des += string.Format("\n\t断开与父节点[{0}]的链接", parent.nodeID);
                    cgc.AddUndo(cc);
                    breakResult = true;
                    parent.OnDelChild(this);
                }
                parents.Clear();

                foreach (BaseNode child in children)
                {
                    ConnectionUndo cc = new ConnectionUndo(nodeID, child.nodeID, CennectionLinkState.BREAKING);
                    cgc.des += string.Format("\n\t断开与子节点[{0}]的链接", child.nodeID);
                    cgc.AddUndo(cc);
                    breakResult = true;
                    child.OnDelParent(this);
                }
                children.Clear();
                if (breakResult)
                {
                    NodeUndo.RecordUndo(cgc);
                }
                break;
            case NodeGenericPopup.break_child:
                for (int i = 0; i < children.Count; i++)
                {
                    if (data.intParam == children[i].nodeID)
                    {
                        ConnectionUndo cc = new ConnectionUndo(nodeID, children[i].nodeID, CennectionLinkState.BREAKING);
                        cc.des = string.Format("断开节点[{0}]和子节点[{1}]的链接", nodeID, children[i].nodeID);
                        NodeUndo.RecordUndo(cc);

                        children[i].OnDelParent(this);
                        OnDelChild(children[i]);
                        break;
                    }
                }
                break;
            case NodeGenericPopup.break_parent:
                foreach (BaseNode _parent in parents)
                {
                    if (data.intParam == _parent.nodeID)
                    {
                        ConnectionUndo cc = new ConnectionUndo(_parent.nodeID, nodeID, CennectionLinkState.BREAKING);
                        cc.des = string.Format("断开父节点[{0}]和节点[{1}]的链接", _parent.nodeID, nodeID);
                        NodeUndo.RecordUndo(cc);

                        _parent.OnDelChild(this);
                        OnDelParent(_parent);
                        break;
                    }
                }
                break;
            case NodeGenericPopup.delete:
                NodeCreateDestroyUndoUtil bnc = new NodeCreateDestroyUndoUtil(this, NodeOperatorType.DESTROY);
                bnc.des = string.Format("删除节点[{0}]", nodeID);
                NodeUndo.RecordUndo(bnc);

                OnDelete(false);
                break;
            case NodeGenericPopup.log_info:
                Debug.Log(ToString());
                break;
        }
    }

    void PopLeftMenu()
    {
        if (nodeType == NodeType.Binding)
            return;
        GenericMenu genericMenu = new GenericMenu();
        foreach(BaseNode _parent in parents)
        {
            PopupData param = new PopupData();
            param.select = NodeGenericPopup.break_parent;
            param.intParam = _parent.nodeID;
            genericMenu.AddItem(new GUIContent(string.Format("断开[{0}, {1}]", _parent.nodeName, _parent.nodeID)), false, new GenericMenu.MenuFunction2(this.ContextClick), param);
        }
        genericMenu.ShowAsContext();
        Event.current.Use();
    }

    void PopRightMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        for(int i = 0; i < children.Count;i++)
        {
            PopupData param = new PopupData();
            param.select = NodeGenericPopup.break_child;
            param.intParam = children[i].nodeID;
            genericMenu.AddItem(new GUIContent(string.Format("断开[{0}, {1}]", children[i].nodeName, children[i].nodeID)), false, new GenericMenu.MenuFunction2(ContextClick), param);
        }
        genericMenu.ShowAsContext();
        Event.current.Use();
    }

    void PopItemMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        if (isRoot)
        {
            genericMenu.AddDisabledItem(new GUIContent("删除"));
        }
        else
        { 
            PopupData param0 = new PopupData();
            param0.select = NodeGenericPopup.delete;
            genericMenu.AddItem(new GUIContent("删除"), false, new GenericMenu.MenuFunction2(ContextClick), param0);
        }

        PopupData param1 = new PopupData();
        param1.select = NodeGenericPopup.break_all;
        genericMenu.AddItem(new GUIContent("移除所有链接"), false, new GenericMenu.MenuFunction2(ContextClick), param1);

        PopupData param3 = new PopupData();
        param3.select = NodeGenericPopup.log_info;
        genericMenu.AddItem(new GUIContent("打印节点信息"), false, new GenericMenu.MenuFunction2(ContextClick), param3);

        genericMenu.ShowAsContext();
        Event.current.Use();
    }
    #endregion

    #region 移动
    public void SetPosition(Vector2 pos)
    {
        Vector2 delta = pos - Position;

        Move(delta);
    }

    public void Move(Vector2 deltaPos)
    {
        body_Rect.center += deltaPos;
        leftHand_Rect.center += deltaPos;
        rightHand_Rect.center += deltaPos;
    }
    #endregion

    #region 事件
    public virtual void OnMouseDown(Vector2 mousePos)
    {
        selectedType = GetMouseSelection(mousePos);
        if (Event.current.button == 1)
        {
            switch (selectedType)
            {
                case CurrentSelection.LeftHand:
                    PopLeftMenu();
                    break;
                case CurrentSelection.RightHand:
                    PopRightMenu();
                    break;
                case CurrentSelection.ItemBody:
                    PopItemMenu();
                    break;
            }
            selectedType = CurrentSelection.NONE;
        }
        //Debug.Log("OnMouseDown: type = " + selectedType);
        currentHoverNode = null;
    }

    public virtual void OnMouseUp(BaseNode preSelected, Vector2 mousePos)
    {
        selectedType = GetMouseSelection(mousePos);
        if (preSelected != null)
        {
            switch (selectedType)
            {
                case CurrentSelection.LeftHand:
                    if (preSelected.selectedType == CurrentSelection.RightHand)
                    {
                        string log = NodeRuleUtil.Instance.CheckConnectlegal(preSelected, this);
                        if (string.IsNullOrEmpty(log))
                        {
                            OnAddParent(preSelected);
                            preSelected.OnAddChild(this);

                            ConnectionUndo cc = new ConnectionUndo(preSelected.nodeID, nodeID, CennectionLinkState.LINKING);
                            cc.des = string.Format("链接节点[{0}]和节点[{1}]", preSelected.nodeID, nodeID);
                            NodeUndo.RecordUndo(cc);
                        }
                    }
                    break;
                case CurrentSelection.RightHand:
                    if (preSelected.selectedType == CurrentSelection.LeftHand)
                    {
                        string log = NodeRuleUtil.Instance.CheckConnectlegal(this, preSelected);
                        if (string.IsNullOrEmpty(log))
                        {
                            preSelected.OnAddParent(this);
                            OnAddChild(preSelected);

                            ConnectionUndo cc = new ConnectionUndo(nodeID, preSelected.nodeID, CennectionLinkState.LINKING);
                            cc.des = string.Format("链接节点[{0}]和节点[{1}]", nodeID, preSelected.nodeID);
                            NodeUndo.RecordUndo(cc);
                        }
                    }
                    break;
                case CurrentSelection.ItemBody:
                    {
                        switch (preSelected.selectedType)
                        {
                            case CurrentSelection.LeftHand:
                                {
                                    string log = NodeRuleUtil.Instance.CheckConnectlegal(this, preSelected);
                                    if (string.IsNullOrEmpty(log))
                                    {
                                        preSelected.OnAddParent(this);
                                        OnAddChild(preSelected);

                                        ConnectionUndo cc = new ConnectionUndo(nodeID, preSelected.nodeID, CennectionLinkState.LINKING);
                                        cc.des = string.Format("链接节点[{0}]和节点[{1}]", nodeID, preSelected.nodeID);
                                        NodeUndo.RecordUndo(cc);
                                    }
                                }
                                break;
                            case CurrentSelection.RightHand:
                                {
                                    string log = NodeRuleUtil.Instance.CheckConnectlegal(preSelected, this);
                                    if (string.IsNullOrEmpty(log))
                                    {
                                        OnAddParent(preSelected);
                                        preSelected.OnAddChild(this);

                                        ConnectionUndo cc = new ConnectionUndo(preSelected.nodeID, nodeID, CennectionLinkState.LINKING);
                                        cc.des = string.Format("链接节点[{0}]和节点[{1}]", preSelected.nodeID, nodeID);
                                        NodeUndo.RecordUndo(cc);
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            preSelected.selectedType = CurrentSelection.NONE;
        }

        selectedType = CurrentSelection.NONE;
        currentHoverNode = null;

        if (isDragging)
        {
            Vector2 delta = Position - dragBeginPos;
            if (Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1)
            {
                BaseNode root = TriggerNodeWindow.GetRootNode();
                Vector2 local_org = dragBeginPos - root.Position;
                Vector2 local_cur = Position - root.Position;
                MoveUndo mc = new MoveUndo(local_org, local_cur, nodeID, string.Format("移动节点[{0}]", nodeID));

                NodeUndo.RecordUndo(mc);
            }
        }

        isDragging = false;
    }

    public virtual void OnMouseDrag(Event e, BaseNode hover)
    {
        currentHoverNode = hover;
        switch (selectedType)
        {
            case CurrentSelection.LeftHand:
                {
                    if (currentHoverNode != null && currentHoverNode != this)
                    {
                        string msg = NodeRuleUtil.Instance.CheckConnectlegal(currentHoverNode, this);
                        if (string.IsNullOrEmpty(msg))
                        {
                            GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, currentHoverNode, this, 80), Color.green, 2, true);
                        }
                        else
                        {
                            GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, currentHoverNode, this, 80), Color.red, 2, true);
                            Handles.Label(e.mousePosition, "   " + msg);
                        }
                    }
                    else
                    {
                        GUILines.DrawBezierConnection(leftHand_Rect.center, e.mousePosition, 0, Color.grey);
                    }
                }
                break;
            case CurrentSelection.RightHand:
                {
                    if (currentHoverNode != null && currentHoverNode != this)
                    {
                        string msg = NodeRuleUtil.Instance.CheckConnectlegal(this, currentHoverNode);
                        if (string.IsNullOrEmpty(msg))
                        {
                            GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, this, currentHoverNode, 80), Color.green, 2, true);
                        }
                        else
                        {
                            GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, this, currentHoverNode, 80), Color.red, 2, true);
                            Handles.Label(e.mousePosition, "   " + msg);
                        }
                    }
                    else
                    {
                        GUILines.DrawBezierConnection(e.mousePosition, rightHand_Rect.center, 0, Color.grey);
                    }
                }
                break;
            case CurrentSelection.ItemBody:
                {
                    if (!isDragging)
                    { 
                        dragBeginPos = Position;
                    }
                    isDragging = true;
                    Move(e.delta);
                }
                break;
        }
    }

    public virtual void OnAddParent(BaseNode _parent)
    {
        if (!parents.Contains(_parent))
        {
            this.parents.Add(_parent);
        }
        else
        {
            Debug.LogError(string.Format("Error: Node[{0}] is in Node[{1}] parents already!", _parent.nodeID, nodeID));
        }
    }

    public virtual void OnDelParent(BaseNode _parent)
    {
        if (parents.Contains(_parent))
        {
            this.parents.Remove(_parent);
        }
        else
        {
            Debug.LogError(string.Format("Node[{0}] Not Found Parent ID: {1}", nodeID, _parent.nodeID));
        }
    }

    public virtual void OnAddChild(BaseNode _child)
    {
        if (children.Contains(_child))
        {
            Debug.LogError(string.Format("Error: Node[{0}] is in Node[{1}] children already, no need reconnect!", _child.nodeID, nodeID));
        }
        else
        {
            children.Add(_child);
        }
    }

    public virtual void OnDelChild(BaseNode _child)
    {
        if (children.Contains(_child))
        {
            children.Remove(_child);
        }
        else
        {
            Debug.LogError(string.Format("Error: delete child failed! Node:{0} has no child of ID:{1}", nodeID, _child.nodeID));
        }
    }

    public virtual void OnDelete(bool recursively = false)
    {
        if (isRoot || nodeType == NodeType.Binding)
        {
            EditorUtility.DisplayDialog("Warning", "当前节点不可删除!", "OK");
            return;
        }

        foreach (BaseNode _parent in parents)
        {
            _parent.OnDelChild(this);
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].OnDelParent(this);
            if (recursively)
            {
                children[i].OnDelete(recursively);
            }
        }
        children.Clear();
        readyToGC = true;
    }

    public abstract void OnPropertyChanged();
    #endregion

    #region 线框骨架
    public virtual void Draw(List<BaseNode> selected, Color color)
    {
        if (selected.Count > 0 && selected.Exists((n) => { return n.nodeID == nodeID; }))
        {
            float width = 5;
            switch (nodeType)
            {
                case NodeType.Action:
                    GUILines.DrawHighLightRect(body_Rect, width, true, Color.cyan);
                    break;
                case NodeType.Condition:
                    GUILines.DrawHighLightRect(body_Rect, width, true, Color.cyan);
                    break;
                case NodeType.Empty:
                    GUILines.DrawHexagon(body_Rect, width, Color.cyan);
                    break;
                case NodeType.Binding:
                    GUILines.DrawHighLightRect(body_Rect, width, true, Color.cyan);
                    break;
            }
        }
        //链接线
        DrawConnection(selected, color);

        if (!isRoot)
        {
            //左手(接受连接父项)
            GUILines.DrawHandRect(leftHand_Rect, color);
        }
        //右手(接收连接子项)
        GUILines.DrawHandRect(rightHand_Rect, color);
        if (Event.current.type == EventType.Repaint)
        {
            OnMouseDrag(Event.current, currentHoverNode); //重复利用该函数
        }

        OnDrawGUI(body_Rect, color);
    }

    public virtual void OnDrawGUI(Rect rect, Color color)
    {
    }

    public virtual void DrawConnection(List<BaseNode> selected, Color color)
    {
        float width = (NodeType.Binding == nodeType) ? 4 : 2;

        for (int j = 0; j < children.Count; j++)
        {
            if (selected.Exists((node) => { return node.nodeID == children[j].nodeID; }))
            {
                GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, this, children[j], 20), Color.yellow, width, true);
            }
            else
            {
                GUILines.DrawLines(GUILines.ConnectionBezierOffsetArray(0, this, children[j], 20), color, width, true);
            }
        }
    }

    CurrentSelection GetMouseSelection(Vector2 pos)
    {
        if (leftHand_Rect.Contains(pos))
        {
            GUI.FocusControl(null);
            return CurrentSelection.LeftHand;
        }
        else if (rightHand_Rect.Contains(pos))
        {
            GUI.FocusControl(null);
            return CurrentSelection.RightHand;
        }
        else if (body_Rect.Contains(pos))
        {
            return CurrentSelection.ItemBody;
        }
        else
        {
            return CurrentSelection.NONE;
        }
    }
    #endregion

    #region UI
    protected void BeginNodeUI(Rect area)
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;
        }
        GUI.BeginGroup(area);
        uiItemRect = new Rect(ItemWidth * 0.1f, 0, ItemWidth * 0.8f, buttonHeight);
    }
    protected void EndNodeUI()
    {
        GUI.EndGroup();
        int calcHeight = (int)Mathf.Ceil(uiItemRect.y + uiItemRect.height);
        if (ItemHeight != calcHeight)
        {
            ItemHeight = calcHeight;
            ResetSize();
        }
    }

    protected void Label(string txt)
    {
        GUI.Label(uiItemRect, txt);
        UI_DOWN();
    }
    protected void Label(string label, GUIStyle style)
    {
        GUI.Label(uiItemRect, label, style);
        UI_DOWN();
    }
    protected void Label(GUIContent content, GUIStyle style)
    {
        GUI.Label(uiItemRect, content, style);
        UI_DOWN();
    }

    protected int IntField(string label, int value)
    {
        Rect label_rect = new Rect(uiItemRect.xMin, uiItemRect.yMin, ItemWidth * 0.4f, buttonHeight);
        Rect input_rect = new Rect(label_rect.xMin + label_rect.width, label_rect.yMin, label_rect.width, label_rect.height);

        GUI.Label(label_rect, label);
        int curValue = EditorGUI.IntField(input_rect, value);
        UI_DOWN();
        if (curValue != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return curValue;
        else
            return value;
    }
    protected int IntField(int value)
    {
        int curValue = EditorGUI.IntField(uiItemRect, value);
        UI_DOWN();
        if (curValue != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return curValue;
        else
            return value;
    }

    protected float FloatField(string label, float value)
    {
        Rect label_rect = new Rect(uiItemRect.xMin, uiItemRect.yMin, ItemWidth * 0.4f, buttonHeight);
        Rect input_rect = new Rect(label_rect.xMin + label_rect.width, label_rect.yMin, label_rect.width, label_rect.height);

        GUI.Label(label_rect, label);
        float curValue = EditorGUI.FloatField(input_rect, value);
        UI_DOWN();
        if (curValue != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return curValue;
        else
            return value;
    }
    protected float FloatField(float value)
    {
        float curValue = EditorGUI.FloatField(uiItemRect, value);
        UI_DOWN();
        if (curValue != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return curValue;
        else
            return value;
    }

    protected string TextField(string text)
    {
        string curValue = text;
        curValue = EditorGUI.TextField(uiItemRect, text);
        UI_DOWN();
        if (curValue != text)
            OnPropertyChanged();

        return curValue;
    }

    protected Object ObjectField(string label, Object value, float labSep, float objSep, System.Type type)
    {
        if (!string.IsNullOrEmpty(label))
            GUI.Label(ScaleRect(uiItemRect, TextAnchor.MiddleLeft, labSep), label);

        Object selected = value;
        Rect objRect = ScaleRect(uiItemRect, TextAnchor.MiddleRight, objSep);
        selected = EditorGUI.ObjectField(objRect, value, type, true);
        string tip = value != null ? value.name : "none";
        GUI.Box(objRect, new GUIContent("", tip));
        UI_DOWN();

        if (selected != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return value;
    }
    protected Object ObjectField(Object value, System.Type type)
    {
        Object selected = value;
        selected = EditorGUI.ObjectField(uiItemRect, value, type, true);
        string tip = value != null ? value.name : "none";
        GUI.Box(uiItemRect, new GUIContent("", tip));
        UI_DOWN();

        if (selected != value)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return value;
    }

    protected bool Button(string des, GUIStyle style)
    {
        bool click = GUI.Button(uiItemRect, des, style);
        UI_DOWN();
        return click && TriggerNodeWindow.isEditable;
    }
    protected bool Button(string des, TextAnchor anchor, float sep)
    {
        bool click = GUI.Button(ScaleRect(uiItemRect, anchor, sep), des);
        UI_DOWN();
        return click && TriggerNodeWindow.isEditable;
    }

    protected int Popup(string label, int selectedIndex, GUIContent[] options)
    {
        Rect label_rect = new Rect(uiItemRect.xMin, uiItemRect.yMin, ItemWidth * 0.4f, buttonHeight);
        Rect input_rect = new Rect(label_rect.xMin + label_rect.width, label_rect.yMin, label_rect.width, label_rect.height);

        GUI.Label(label_rect, label);
        int selected = EditorGUI.Popup(input_rect, selectedIndex, options);
        UI_DOWN();
        if (selected != selectedIndex)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return selectedIndex;
    }
    protected int Popup(string label, int selectedIndex, string[] options)
    {
        Rect label_rect = new Rect(uiItemRect.xMin, uiItemRect.yMin, ItemWidth * 0.4f, buttonHeight);
        Rect input_rect = new Rect(label_rect.xMin + label_rect.width, label_rect.yMin, label_rect.width, label_rect.height);

        GUI.Label(label_rect, label);
        int selected = EditorGUI.Popup(input_rect, selectedIndex, options);
        UI_DOWN();
        if (selected != selectedIndex)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return selectedIndex;
    }
    protected int Popup(int selectedIndex, string[] options)
    {
        int selected = EditorGUI.Popup(uiItemRect, selectedIndex, options);
        UI_DOWN();
        if (selected != selectedIndex)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return selectedIndex;
    }
    protected int Popup(int selectedIndex, string[] options, GUIStyle style)
    {
        int selected = EditorGUI.Popup(uiItemRect, selectedIndex, options, style);
        UI_DOWN();
        if (selected != selectedIndex)
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return selected;
        else
            return selectedIndex;
    }

    protected System.Enum EnumPopup(System.Enum selected)
    {
        System.Enum orgSelection = EditorGUI.EnumPopup(uiItemRect, selected);
        UI_DOWN();
        if (!orgSelection.Equals(selected))
            OnPropertyChanged();

        if (TriggerNodeWindow.isEditable)
            return orgSelection;
        else
            return selected;
    }

    protected bool Toggle(string label, bool value)
    {
        bool result = value;
        result = GUI.Toggle(uiItemRect, result, label);
        UI_DOWN();

        if (TriggerNodeWindow.isEditable)
            return result;
        else
            return value;
    }

    protected Rect GetControlRect()
    {
        return uiItemRect;
    }
    protected void UI_UP(int gridCount = 1)
    {
        uiItemRect.y -= (uiItemRect.height + tabHeight) * gridCount;
    }
    protected void UI_DOWN(int gridCount = 1)
    {
        uiItemRect.y += (uiItemRect.height + tabHeight) * gridCount;
    }
    #endregion

    protected Rect ScaleRect(Rect r, TextAnchor anchor, float xScale)
    {
        switch (anchor)
        {
            case TextAnchor.MiddleCenter:
                return new Rect(r.xMin - r.width * (1 - xScale) * 0.5f, r.yMin, r.width * xScale, r.height);
            case TextAnchor.MiddleLeft:
                return new Rect(r.xMin, r.yMin, r.width * xScale, r.height);
            case TextAnchor.MiddleRight:
                return new Rect(r.xMin + r.width * (1 - xScale), r.yMin, r.width * xScale, r.height);
        }
        return r;
    }

    public override string ToString()
    {
        return "Position: " + Position.ToString() + "  NodeID: " + nodeID + " item height = " + ItemHeight + " item width = " + ItemWidth;
    }
}
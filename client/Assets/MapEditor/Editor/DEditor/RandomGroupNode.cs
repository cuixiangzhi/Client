using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using MapEditConfigData;

public class RandomGroupNode : BaseNode {
    TriggerActionData _randomData;
    public TriggerActionData RandomData
    {
        get
        {
            if(_randomData == null)
            {
                _randomData = new TriggerActionData();
                _randomData.node = new TriggerNodeData();
                _randomData.node.childrenWeights.Clear();
                _randomData.actionInfo = new TriggerHandlerCellInfo();
            }
            _randomData.node.childrenWeights.Clear();
            _randomData.node.childrenWeights.AddRange(weights);
            _randomData.node.NodeType = (int)BaseNode.NodeType.Action;
            _randomData.node.id = nodeID;
            _randomData.node.parents.Clear();
            _randomData.actionInfo.handlerID = TRIGGER_HANDLER_CONSTANCE_ID.RANDOM_ACTION_BEGIN;
            for (int i = 0; i < parents.Count; i++)
            {
                _randomData.node.parents.Add(parents[i].nodeID);
            }
            _randomData.node.children.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                _randomData.node.children.Add(children[i].nodeID);
            }
            _randomData.node.posx = Position.x;
            _randomData.node.posy = Position.y;
            return _randomData;
        }

        set
        {
            _randomData = value;
            parents.Clear();
            children.Clear();
            weights.Clear();
            weights.AddRange(_randomData.node.childrenWeights);
            nodeID = _randomData.node.id;
            for (int i = 0; i < _randomData.node.parents.Count; i++)
            {
                parents.Add(TriggerNodeWindow.GetNodeByID(_randomData.node.parents[i]));
            }
            for (int i = 0; i < _randomData.node.children.Count; i++)
            {
                children.Add(TriggerNodeWindow.GetNodeByID(_randomData.node.children[i]));
            }

            ResetSize();
            SetPosition(new Vector2(_randomData.node.posx, _randomData.node.posy));
        }
    }

    public RandomGroupNode()
    {
        nodeType = NodeType.Action;//没错!就是Action!
        nodeName = "随机组";
    }

    public override void Draw(List<BaseNode> selected, Color color)
    {
        //链接线
        DrawConnection(selected, color);

        bool highLight = selected.Count > 0 && selected.Exists((n) => { return n.nodeID == nodeID; });
        if (highLight)
        {
            GUILines.DrawHighLightRect(body_Rect, 5, true, Color.cyan);
        }
        GUILines.DrawHandRect(leftHand_Rect, color);

        if (Event.current.type == EventType.Repaint)
        {
            OnMouseDrag(Event.current, currentHoverNode); //重复利用该函数
        }

        OnDrawGUI(body_Rect, color);
    }

    public override void OnDrawGUI(Rect rect, Color color)
    {
        if (weights.Count != children.Count)
        {
            if (weights.Count > children.Count)
            {
                List<int> tmp = new List<int>();
                tmp.AddRange(weights);
                weights.Clear();
                for (int i = 0; i < children.Count; i++)
                {
                    weights.Add(tmp[i]);
                }
            }
            else
            {
                int to_add_count = children.Count + weights.Count;
                for (int i = 0; i < to_add_count; i++)
                {
                    weights.Add(0);
                }
            }
        }

        GUI.color = Color.white;

        GUI.Box(rect, "", GUI.skin.window);

        Rect mainRect = new Rect(rect);
        mainRect.width += 60;

        BeginNodeUI(mainRect);
        Label(string.Format("随机组 ID: {0}", nodeID), labelStyle);
        UI_DOWN();

        for (int i = 0; i < children.Count; i++)
        {
            DrawGroupHand(i, color, children[i]);
        }

        UI_DOWN();
        EndNodeUI();

        GUILines.DrawHandRect(rightHand_Rect, color);
    }

    Rect GetRightRandomRect(int index)
    {
        return new Rect(body_Rect.xMax - HandIndentPixel, body_Rect.yMin + (buttonHeight + tabHeight) * (index + 2), HandWidth, HandHeight);
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
        leftHand_Rect.center = new Vector2(Position.x + HandIndentPixel - (body_Rect.width + leftHand_Rect.width) * 0.5f, Position.y);

        rightHand_Rect = GetRightRandomRect(children.Count);
    }

    void DrawGroupHand(int index, Color color, BaseNode child)
    {
        if (index < children.Count && index >= 0)
        {
            Rect handRect = GetControlRect();
            handRect.width = HandWidth;
            handRect.height = HandHeight;
            handRect.x = body_Rect.width - HandIndentPixel;

            GUILines.DrawHandRect(handRect, color);

            GUI.color = Color.white;
            weights[index] = IntField("", weights[index]);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 1 && handRect.Contains(e.mousePosition))
                {
                    GenericMenu g = new GenericMenu();
                    g.AddItem(new GUIContent("断开", index.ToString()), false, new GenericMenu.MenuFunction2(BreakConnection), index);
                    g.ShowAsContext();
                    GUI.FocusControl(null);
                }
            }
        }
    }

    void BreakConnection(object index)
    {
        int _tobreak = (int)index;
        BaseNode _child = children[_tobreak];

        ConnectionUndo cc = new ConnectionUndo(nodeID, _child.nodeID, CennectionLinkState.BREAKING);
        cc.des = string.Format("断开节点[{0}]与节点[{1}]的链接.", nodeID, _child.nodeID);
        NodeUndo.RecordUndo(cc);

        _child.OnDelParent(this);
        OnDelChild(_child);
    }

    public override void DrawConnection(List<BaseNode> selected, Color color)
    {
        for (int index = 0; index < children.Count; index++)
        {
            if (selected.Count > 0 && selected.Exists((node) => { return node.nodeID == children[index].nodeID; }))
            {
                GUILines.DrawConnectionBezierArray(children[index].leftHand_Rect, children[index].body_Rect, GetRightRandomRect(index), body_Rect, 20, 2, Color.yellow, true);
            }
            else
            {
                GUILines.DrawConnectionBezierArray(children[index].leftHand_Rect, children[index].body_Rect, GetRightRandomRect(index), body_Rect, 20, 2, color, true);
            }
        }
    }

    public override void OnDelChild(BaseNode _child)
    {
        int index = children.IndexOf(_child);
        if (!(index < 0))
        {
            weights.RemoveAt(index);
        }
        base.OnDelChild(_child);
    }
    public override void OnAddChild(BaseNode _child)
    {
        weights.Add(0);
        base.OnAddChild(_child);
    }

    public override void OnPropertyChanged()
    {
        RandomGroupPropertyChangeUndo accc = new RandomGroupPropertyChangeUndo(this);
        accc.des = string.Format("随机行为组[{0}]属性修改.", nodeID);
        NodeUndo.RecordUndo(accc);
        TriggerNodeWindow.propChanged = true;
    }
}

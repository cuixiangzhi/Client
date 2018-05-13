using MapEditConfigData;
using System.Collections.Generic;
using UnityEngine;

public class EmptyNode : BaseNode {
    private TriggerNodeData _emptyData;
    public TriggerNodeData EmptyData
    {
        get
        {
            if (_emptyData == null)
            {
                _emptyData = new TriggerNodeData();
            }

            _emptyData.children.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                _emptyData.children.Add(children[i].nodeID);
            }

            _emptyData.parents.Clear();
            for (int i = 0; i < parents.Count; i++)
            {
                _emptyData.parents.Add(parents[i].nodeID);
            }

            _emptyData.posx = Position.x;
            _emptyData.posy = Position.y;
            _emptyData.id = nodeID;
            _emptyData.NodeType = (int)nodeType;

            return _emptyData;
        }
        set
        {
            _emptyData = value;
            nodeID = _emptyData.id;
            nodeType = NodeType.Empty;
            ResetSize();
            Vector2 position = new Vector2(_emptyData.posx, _emptyData.posy);
            SetPosition(position);

            parents.Clear();
            children.Clear();

            for (int i = 0; i < _emptyData.children.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_emptyData.children[i]);

                if (node != null)
                {
                    children.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的子节点!当前节点编号[{1}]", _emptyData.children[i], nodeID));
                }
            }

            for (int i = 0; i < _emptyData.parents.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_emptyData.parents[i]);
                if (node != null)
                {
                    parents.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的父节点!当前节点编号[{1}]", _emptyData.parents[i], nodeID));
                }
            }
        }
    }
    public EmptyNode()
    {
        nodeType = NodeType.Empty;
        nodeName = "空节点";
    }

    public override void Draw(List<BaseNode> selected, Color color)
    {
        base.Draw(selected, color);

        if (selected.Count == 0 || !selected.Exists((node) => { return node.nodeID == nodeID; }))
            GUILines.DrawHexagon(body_Rect, 1, color);
    }

    public override void OnDrawGUI(Rect rect, Color color)
    {
        BeginNodeUI(rect);

        GUI.color = Color.white;
        UI_DOWN();
        Label(string.Format("空节点 ID: {0}", EmptyData.id), labelStyle);

        EndNodeUI();
    }

    ////画个小旗
    //void DrawFlag(Vector2 pos, float height, float width)
    //{                                                          //      v2
    //    Vector2 v1 = new Vector2(0, 0);                        //      *
    //    Vector2 v2 = new Vector2(0, -height);                  //      *  *
    //    Vector2 v3 = new Vector2(width, -height * 0.5f);       //      *     *
    //    Vector2 v4 = new Vector2(0, -height * 0.5f);           //      *        *
    //    v1 += pos;                                             //   v4 * * * * * * * v3
    //    v2 += pos;                                             //      *
    //    v3 += pos;                                             //      *
    //    v4 += pos;                                             //      *
    //    Handles.DrawAAPolyLine(3, v1, v2, v3, v4);             //      * v1
    //}

    public override void OnPropertyChanged()
    {
        TriggerNodeWindow.propChanged = true;
        throw new System.Exception("What Happend?!");
    }
}
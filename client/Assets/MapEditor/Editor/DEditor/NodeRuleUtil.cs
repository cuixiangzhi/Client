using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
/*
 * 关于Node之间相连的约定:
 *      条件节点不能直接和动作节点对接, 中间需要有空节点桥接
 *      一组条件只能有一个入口和出口
 */
public class NodeRuleUtil {
    static NodeRuleUtil _ins;
    public static NodeRuleUtil Instance
    {
        get
        {
            if (_ins == null)
                _ins = new NodeRuleUtil();
            return _ins;
        }
    }

    //检测是否合法
    public string CheckConnectlegal(BaseNode parent, BaseNode child)
    {
        if (parent.children.Contains(child) && child.parents.Contains(parent))
            return string.Format("父节点[{0}]已经包含子节点[{1}]!", parent.nodeID, child.nodeID);

        if (child.nodeType == BaseNode.NodeType.Binding)
        {
            return "绑定对象节点不可有父节点!";
        }

        #region 避免死循环
        if (parent.children.Contains(child))
        {
            if (LoopTest(parent, child) && LoopTest(child, parent))
            {
                return "死循环了!";
            }
        }
        else
        {
            parent.children.Add(child);
            child.parents.Add(parent);

            if(LoopTest(parent, child) && LoopTest(child, parent))
            {
                parent.children.Remove(child);
                child.parents.Remove(parent);

                return "将会产生死循环!";
            }
            
            parent.children.Remove(child);
            child.parents.Remove(parent);
        }
        #endregion

        return "";
    }

    //void GetTeam(BaseNode node, List<BaseNode> team)
    //{
    //    team.Add(node);
    //    if (isSecureDoor(node))
    //    {
    //        for (int i = 0; i < node.parents.Count; i++)
    //        {
    //            if (!team.Contains(node.parents[i]))
    //            {
    //                GetTeam(node.parents[i], team);
    //            }
    //        }
    //    }
    //    else if (isFrontDoor(node))
    //    {
    //        for (int i = 0; i < node.children.Count; i++)
    //        {
    //            if (!team.Contains(node.children[i]))
    //                GetTeam(node.children[i], team);
    //        }
    //    }
    //    else
    //    {
    //        for (int i = 0; i < node.parents.Count; i++)
    //        {
    //            if (!team.Contains(node.parents[i]))
    //            {
    //                GetTeam(node.parents[i], team);
    //            }
    //        }

    //        for (int i = 0; i < node.children.Count; i++)
    //        {
    //            if (!team.Contains(node.children[i]))
    //                GetTeam(node.children[i], team);
    //        }
    //    }
    //}

    //string DoorConflictCheck(BaseNode parent, BaseNode child)
    //{
    //    List<BaseNode> team_parent = new List<BaseNode>();
    //    GetTeam(parent, team_parent);
    //    BaseNode parent_root = null;
    //    BaseNode parent_roof = null;
    //    for (int i = 0; i < team_parent.Count; i++)
    //    {
    //        if (isFrontDoor(team_parent[i]))
    //        {
    //            if (parent_root != null)
    //            {
    //                return string.Format("有两个入口, 请首先仔细检查Node[{0}]和Node[{1}]!!", parent_root.nodeID, team_parent[i].nodeID);//有冲突
    //            }
    //            parent_root = team_parent[i];
    //        }
    //        if (isSecureDoor(team_parent[i]))
    //        {
    //            if (parent_roof != null)
    //            {
    //                return string.Format("有两个出口检测门, 请首先仔细检测Node[{0}]和Node[{1}]", parent_roof.nodeID, team_parent[i].nodeID);//有冲突
    //            }
    //            parent_roof = team_parent[i];
    //        }
    //    }

    //    return "";//未检测到冲突
    //}

    //bool isFrontDoor(BaseNode node)
    //{
    //    if (node.children.Count == 0)
    //        return false;

    //    if (node.nodeType == BaseNode.NodeType.Empty)
    //    { 
    //        for (int i = 0; i < node.children.Count; i++)
    //        {
    //            if (node.children[i].nodeType == BaseNode.NodeType.Condition)
    //                return true;
    //        }
    //    }

    //    return false;
    //}

    //bool isSecureDoor(BaseNode node)
    //{
    //    if (node.parents.Count == 0)
    //        return false;
    //    if (node.nodeType == BaseNode.NodeType.Security)
    //    { 
    //        for (int i = 0; i < node.parents.Count; i++)
    //        {
    //            if (node.parents[i].nodeType == BaseNode.NodeType.Condition)
    //                return true;
    //        }
    //    }

    //    return false;
    //}

    //BaseNode GetEmptyRoot(BaseNode node)
    //{
    //    if (node.nodeType == BaseNode.NodeType.Empty)
    //        return node;
    //    for (int i = 0; i < node.parents.Count; i++)
    //    {
    //        BaseNode _root = GetEmptyRoot(node.parents[i]);
    //        if (_root != null)
    //            return _root;
    //    }
    //    return null;
    //}

    //BaseNode GetEmptyRoof(BaseNode node)
    //{
    //    if (node.nodeType == BaseNode.NodeType.Empty || node.nodeType == BaseNode.NodeType.Security)
    //        return node;
    //    for (int i = 0; i < node.children.Count; i++)
    //    {
    //        BaseNode _child = GetEmptyRoof(node.children[i]);
    //        if (_child != null)
    //            return _child;
    //    }

    //    return null;
    //}

    public string CheckSaveData(List<BaseNode> data)
    {
        string Error = "";

        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].parents.Count == 0 && data[i].nodeType != BaseNode.NodeType.Empty)
            {
                Error += string.Format("节点[{0}]没有父节点!\n", data[i].nodeID);
            }
            if (data[i].children.Count == 0 && data[i].nodeType != BaseNode.NodeType.Action)
            {
                Error += string.Format("节点[{0}]没有子节点, 且节点类型不为Action!\n");
            }
            if (data[i].nodeType == BaseNode.NodeType.Empty)
            { 
            }
        }

        return Error;
    }

    bool LoopTest(BaseNode node1, BaseNode node2)
    {
        if (node1.parents.Contains(node2))
            return true;
        for(int i = 0; i < node1.parents.Count; i ++)
        {
            if (LoopTest(node1.parents[i], node2))
                return true;
        }
        return false;
    }
}
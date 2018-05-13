using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeOperatorType
{
    CREATE = 1,
    DESTROY = 2
}

public enum CennectionLinkState
{
    BREAKING = 1,
    LINKING = 2
}

public interface IUndo
{
    void Undo();
    void ReverseUndo();

    string des { get; set; }
}

class UndoManager
{
    const int StackCount = 100;
    private Stack undoStack = new Stack();
    private Stack reverseStack = new Stack();

    public void RecordCommand(IUndo command)
    {
        this.reverseStack.Clear();

        if (StackCount == undoStack.Count) //超出记录数量的挤掉底部
        {
            Stack tmp = new Stack();

            while(undoStack.Count != 0)
            {
                tmp.Push(undoStack.Pop());
            }

            tmp.Pop(); //舍弃栈顶

            while(tmp.Count != 0)
            {
                undoStack.Push(tmp.Pop());
            }
        }

        undoStack.Push(command);
    }

    public void Undo()
    {
        if (undoStack.Count == 0)
            return;

        IUndo command = (IUndo)this.undoStack.Pop();
        if (command == null)
        {
            Debug.LogWarning("command is null?!");
            return;
        }

        command.Undo();
        this.reverseStack.Push(command);
    }

    public void ReverseUndo()
    {
        if (reverseStack.Count == 0)
            return;
        
        IUndo command = (IUndo)this.reverseStack.Pop();
        if (command == null)
        {
            Debug.LogWarning("command is null?!");
            return;
        }

        command.ReverseUndo();
        this.undoStack.Push(command);
    }

    public void ClearRecords()
    {
        undoStack.Clear();
        reverseStack.Clear();
    }
}

public class NodeUndo {
    static UndoManager _manager;
    static UndoManager Manager
    {
        get
        {
            if (_manager == null)
                _manager = new UndoManager();

            return _manager;
        }
    }

    public static void RecordUndo(IUndo cmd)
    {
        Manager.RecordCommand(cmd);
    }

    public static void ClearRecords()
    {
        Manager.ClearRecords();
    }

    public static void Undo()
    {
        Manager.Undo();
    }

    public static void ReverseUndo()
    {
        Manager.ReverseUndo();
    }
}

//移动
public class MoveUndo : IUndo
{
    //整体移动不记录, 只记录当前选中节点的移动
    public int nodeID;
    //记录相对根节点的坐标(好处是如果有不记录的整体移动后的回撤不会影响节点的相对位置)
    public Vector2 localPosToRoot_original = Vector2.zero;
    public Vector2 localPosToRoot_changed = Vector2.zero;
    public string des { get; set; }

    public MoveUndo(Vector2 local_org_pos, Vector2 local_cur_pos, int nodeID, string desc = "")
    {
        this.nodeID = nodeID;
        this.localPosToRoot_original = local_org_pos;
        this.localPosToRoot_changed = local_cur_pos;
    }

    public void Undo()
    {
        BaseNode root = TriggerNodeWindow.GetRootNode();
        BaseNode node = TriggerNodeWindow.GetNodeByID(nodeID);

        if (node is BindingTargetNode)
        {
            //I am a root node...
            node.Move(localPosToRoot_original);
        }
        else
        {
            node.SetPosition(root.Position + localPosToRoot_original);
        }
        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        BaseNode root = TriggerNodeWindow.GetRootNode();

        BaseNode node = TriggerNodeWindow.GetNodeByID(nodeID);

        if (node is BindingTargetNode)
        {
            //I am a root node...
            node.Move(-localPosToRoot_original);
        }
        else
        {
            node.SetPosition(root.Position + localPosToRoot_changed);
        }

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }
}

//一组节点移动
public class GroupMoveUndo : IUndo
{
    Stack undo_stack = new Stack();
    Stack redo_stack = new Stack();
    public string des { get; set; }

    public void AddUndo(IUndo undo)
    {
        undo_stack.Push(undo);
    }

    public void Undo()
    {
        while (undo_stack.Count != 0)
        {
            IUndo undo = undo_stack.Pop() as IUndo;
            undo.Undo();
            redo_stack.Push(undo);
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        while (redo_stack.Count != 0)
        {
            IUndo redo = redo_stack.Pop() as IUndo;
            redo.ReverseUndo();
            undo_stack.Push(redo);
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }

}

//链接
public class ConnectionUndo : IUndo
{
    private int parentID;
    private int childID;

    private int index_in_parent = -1;
    private int weight_in_parent = 0;
    private CennectionLinkState linkedState;//最终是否链接
    public string des { get; set; }

    public ConnectionUndo(int parent, int child, CennectionLinkState finalState)
    {
        BaseNode parentNode = TriggerNodeWindow.GetNodeByID(parent);
        BaseNode childNode = TriggerNodeWindow.GetNodeByID(child);

        index_in_parent = parentNode.children.IndexOf(childNode);
        if (index_in_parent >= 0 && parentNode is RandomGroupNode)
        {
            weight_in_parent = parentNode.weights[index_in_parent];
        }

        this.parentID = parent;
        this.childID = child;
        this.linkedState = finalState;
    }

    public void Undo()
    {
        BaseNode parent = TriggerNodeWindow.GetNodeByID(parentID);
        BaseNode child = TriggerNodeWindow.GetNodeByID(childID);
        switch (linkedState)
        {
            case CennectionLinkState.LINKING:
                BreakLink(parent, child);
                break;
            case CennectionLinkState.BREAKING:
                ConnectLink(parent, child);
                break;
        }

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, childID, parentID));

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    private void BreakLink(BaseNode parent, BaseNode child)
    {
        parent.OnDelChild(child);
        child.OnDelParent(parent);
    }

    private void ConnectLink(BaseNode parent, BaseNode child)
    {
        if (parent is RandomGroupNode)
        {
            if (parent.children.Contains(child))
            {
                index_in_parent = parent.children.IndexOf(child);
                parent.weights[index_in_parent] = weight_in_parent;
            }
            else
            {
                parent.children.Add(child);
                parent.weights.Add(weight_in_parent);
            }

            child.OnAddParent(parent);
        }
        else
        {
            parent.OnAddChild(child);
            child.OnAddParent(parent);
        }
    }

    public void ReverseUndo()
    {
        BaseNode parent = TriggerNodeWindow.GetNodeByID(parentID);
        BaseNode child = TriggerNodeWindow.GetNodeByID(childID);
        switch (linkedState)
        {
            case CennectionLinkState.LINKING:
                ConnectLink(parent, child);
                break;
            case CennectionLinkState.BREAKING:
                BreakLink(parent, child);
                break;
        }

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, childID, parentID));

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }
}
public class ConnectionGroupCommand : IUndo
{
    public string des { get; set; }
    Stack undo_stack = new Stack();
    Stack redo_stack = new Stack();
    public void AddUndo(ConnectionUndo undo)
    {
        undo_stack.Push(undo);
    }

    public void Undo()
    {
        while (undo_stack.Count != 0)
        {
            ConnectionUndo undo = undo_stack.Pop() as ConnectionUndo;
            undo.Undo();
            redo_stack.Push(undo);
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        while (redo_stack.Count != 0)
        {
            ConnectionUndo redo = redo_stack.Pop() as ConnectionUndo;
            redo.ReverseUndo();
            undo_stack.Push(redo);
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }
}

public abstract class CreateOrDestroyUndo : IUndo
{
    protected NodeOperatorType optType;
    protected Vector2 localPosToRoot;
    protected List<int> weight_in_parents = new List<int>();
    protected MapEditConfigData.TriggerNodeData node_info;
    public string des { get; set; }

    public CreateOrDestroyUndo(MapEditConfigData.TriggerNodeData node_info)
    {
        this.node_info = node_info;
        BaseNode root = TriggerNodeWindow.GetRootNode();
        BaseNode self = TriggerNodeWindow.GetNodeByID(node_info.id);

        for (int i = 0; i < self.parents.Count; i++)
        {
            int index = self.parents[i].children.IndexOf(self);
            int weight = 0;
            if (self.parents[i] is RandomGroupNode)
            {
                weight = self.parents[i].weights[index];
            }
            weight_in_parents.Add(weight);
        }

        localPosToRoot = self.Position - root.Position;
    }

    public void Undo()
    {
        switch (optType)
        {
            case NodeOperatorType.CREATE:
                DelNode();
                OnPostDelNode();
                break;
            case NodeOperatorType.DESTROY:
                AddNode();
                OnPostAddNode();
                break;
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        switch (optType)
        {
            case NodeOperatorType.CREATE:
                AddNode();
                OnPostAddNode();
                break;
            case NodeOperatorType.DESTROY:
                DelNode();
                OnPostDelNode();
                break;
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }

    public abstract void AddNode();
    public abstract void DelNode();

    public virtual void OnPostAddNode()
    {
        BaseNode root = TriggerNodeWindow.GetRootNode();
        BaseNode node = TriggerNodeWindow.GetNodeByID(node_info.id);

        node.SetPosition(root.Position + localPosToRoot);

        for (int i = 0; i < node_info.parents.Count; i++)
        {
            BaseNode parent = TriggerNodeWindow.GetNodeByID(node_info.parents[i]);
            if (parent == null)
            {
                Debug.LogError(string.Format("node[{0}] can't find parent[{1}]!!", node.nodeID, node_info.parents[i]));
                continue;
            }

            bool exist_in_parent = false;
            int index_in_parent = -1;
            if (!parent.children.Exists((_node) => { return _node.nodeID == node.nodeID; }))
            {
                parent.children.Add(node);
                index_in_parent = parent.children.Count - 1;
            }
            else
            {
                exist_in_parent = true;
                index_in_parent = parent.children.IndexOf(node);
            }

            if (!exist_in_parent && parent is RandomGroupNode)
            {
                parent.weights.Insert(index_in_parent, weight_in_parents[i]);
            }
        }
        foreach (var child in node.children)
        {
            child.OnAddParent(node);
        }

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }
    public virtual void OnPostDelNode()
    {
        //BaseNode node = TriggerNodeWindow.GetNodeByID(node_info.id);

        //foreach (var parent in node.parents)
        //{
        //    parent.OnDelChild(node);
        //}
        //foreach (var child in node.children)
        //{
        //    child.OnDelParent(node);
        //}

        //TriggerNodeWindow.nodes.RemoveAll((_node) => { return _node.nodeID == node.nodeID; });

        //ScriptableObject.DestroyImmediate(node);
    }
}

//空节点删除创建操作
class EmptyCreateDestroyUndo : CreateOrDestroyUndo
{
    private MapEditConfigData.TriggerNodeData data = new MapEditConfigData.TriggerNodeData();

    public EmptyCreateDestroyUndo(MapEditConfigData.TriggerNodeData data, NodeOperatorType optType)
        : base(data)
    {
        MapDataSerializerTools.AssignDataByFieldName(data, this.data);
        this.optType = optType;
    }

    public override void AddNode()
    {
        EmptyNode node = ScriptableObject.CreateInstance<EmptyNode>();
        node.nodeID = node_info.id;
        TriggerNodeWindow.nodes.Add(node);
    }
    public override void OnPostAddNode()
    {
        EmptyNode node = TriggerNodeWindow.GetNodeByID(node_info.id) as EmptyNode;
        MapEditConfigData.TriggerNodeData _data = new MapEditConfigData.TriggerNodeData();
        MapDataSerializerTools.AssignDataByFieldName(data, _data);
        node.EmptyData = _data;
        TriggerNodeWindow.new_trigger.nodes.emptyNodes.Add(node.EmptyData);
        base.OnPostAddNode();
    }
    public override void DelNode()
    {
        EmptyNode node = TriggerNodeWindow.GetNodeByID(data.id) as EmptyNode;
        node.OnDelete();
    }
}

//动作/随机行为组节点删除创建操作
class ActionCreateDestroyUndo : CreateOrDestroyUndo
{
    MapEditConfigData.TriggerActionData data = new MapEditConfigData.TriggerActionData();

    public ActionCreateDestroyUndo(MapEditConfigData.TriggerActionData data, NodeOperatorType optType)
        : base(data.node)
    {
        MapDataSerializerTools.AssignDataByFieldName(data, this.data);
        this.optType = optType;
    }

    public override void AddNode()
    {
        BaseNode node = null;
        if (data.actionInfo.handlerID == TRIGGER_HANDLER_CONSTANCE_ID.RANDOM_ACTION_BEGIN)
        {
            node = ScriptableObject.CreateInstance<RandomGroupNode>();
        }
        else
        {
            node = ScriptableObject.CreateInstance<ActionNode>();
        }

        node.nodeID = node_info.id;
        TriggerNodeWindow.nodes.Add(node);
    }
    public override void OnPostAddNode()
    {
        BaseNode node = TriggerNodeWindow.GetNodeByID(node_info.id);

        if (node is RandomGroupNode)
        {
            MapEditConfigData.TriggerActionData _data = new MapEditConfigData.TriggerActionData();
            MapDataSerializerTools.AssignDataByFieldName(data, _data);
            (node as RandomGroupNode).RandomData = _data;
        }
        else
        {
            MapEditConfigData.TriggerActionData _data = new MapEditConfigData.TriggerActionData();
            MapDataSerializerTools.AssignDataByFieldName(data, _data);
            (node as ActionNode).ActionData = _data;
        }

        base.OnPostAddNode();
    }
    public override void DelNode()
    {
        BaseNode node = TriggerNodeWindow.GetNodeByID(node_info.id);
        node.OnDelete();
    }
}

//条件节点删除创建操作
class ConditionCreateDestroyUndo : CreateOrDestroyUndo
{
    private MapEditConfigData.TriggerConditionData data = new MapEditConfigData.TriggerConditionData();

    public ConditionCreateDestroyUndo(MapEditConfigData.TriggerConditionData data, NodeOperatorType optType)
        : base(data.node)
    {
        MapDataSerializerTools.AssignDataByFieldName(data, this.data);
        this.optType = optType;
    }

    public override void AddNode()
    {
        ConditionNode node = ScriptableObject.CreateInstance<ConditionNode>();
        node.nodeID = node_info.id;
        TriggerNodeWindow.nodes.Add(node);
    }
    public override void OnPostAddNode()
    {
        ConditionNode node = TriggerNodeWindow.GetNodeByID(node_info.id) as ConditionNode;
        MapEditConfigData.TriggerConditionData _data = new MapEditConfigData.TriggerConditionData();
        MapDataSerializerTools.AssignDataByFieldName(data, _data);
        node.ConditionData = _data;

        base.OnPostAddNode();
    }
    public override void DelNode()
    {
        BaseNode node = TriggerNodeWindow.GetNodeByID(node_info.id);
        node.OnDelete();
    }
}

//节点删除创建操作(自动分析节点类型)
public class NodeCreateDestroyUndoUtil : IUndo
{
    public CreateOrDestroyUndo undo;

    public NodeCreateDestroyUndoUtil(BaseNode node, NodeOperatorType optType)
    {
        switch (node.nodeType)
        {
            case BaseNode.NodeType.Action:
                if (node is ActionNode)
                {
                    undo = new ActionCreateDestroyUndo((node as ActionNode).ActionData, optType);
                }
                else
                {
                    undo = new ActionCreateDestroyUndo((node as RandomGroupNode).RandomData, optType);
                }
                break;
            case BaseNode.NodeType.Condition:
                undo = new ConditionCreateDestroyUndo((node as ConditionNode).ConditionData, optType);
                break;
            case BaseNode.NodeType.Empty:
                undo = new EmptyCreateDestroyUndo((node as EmptyNode).EmptyData, optType);
                break;
        }
    }

    public void Undo()
    {
        undo.Undo();

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        undo.ReverseUndo();

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }

    public string des
    {
        get;
        set;
    }
}

//节点组删除操作
public class NodeGroupDestroyUndo : IUndo
{
    private Stack undo_stack = new Stack();
    private Stack redo_stack = new Stack();
    public string des { get; set; }

    public NodeGroupDestroyUndo()
    { 
    }
    /// <summary>
    /// 根据节点ID查找所有向后分叉节点
    /// </summary>
    /// <param name="nodeID"></param>
    public NodeGroupDestroyUndo(int nodeID)
    {
        BaseNode node = TriggerNodeWindow.GetNodeByID(nodeID);

        List<BaseNode> group = GetLeaf(node);

        foreach(var mem in group)
        {
            NodeCreateDestroyUndoUtil bnc = new NodeCreateDestroyUndoUtil(mem, NodeOperatorType.DESTROY);
            undo_stack.Push(bnc);
        }
    }

    public void AddUndo(NodeCreateDestroyUndoUtil undo)
    {
        undo_stack.Push(undo);
    }

    public void Undo()
    {
        while (undo_stack.Count != 0)
        {
            NodeCreateDestroyUndoUtil bnc = undo_stack.Pop() as NodeCreateDestroyUndoUtil;
            CreateOrDestroyUndo crdnc = bnc.undo as CreateOrDestroyUndo;
            crdnc.AddNode();
            redo_stack.Push(bnc);
        }
        while (redo_stack.Count != 0)
        {
            IUndo redo = redo_stack.Pop() as IUndo;
            undo_stack.Push(redo);
        }
        while (undo_stack.Count != 0)
        {
            NodeCreateDestroyUndoUtil bnc = undo_stack.Pop() as NodeCreateDestroyUndoUtil;
            CreateOrDestroyUndo crdnc = bnc.undo as CreateOrDestroyUndo;
            crdnc.OnPostAddNode();
            redo_stack.Push(bnc);
        }
        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);
    }

    public void ReverseUndo()
    {
        while (redo_stack.Count != 0)
        {
            IUndo undo = redo_stack.Pop() as IUndo;
            undo.ReverseUndo();
            undo_stack.Push(undo);
        }

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);
    }

    List<BaseNode> GetLeaf(BaseNode node)
    {
        List<BaseNode> list = new List<BaseNode>();
        list.Add(node);

        foreach (var child in node.children)
        {
            list.AddRange(GetLeaf(child));
        }

        return list;
    }

}

//动作属性变化
public class ActionCellPropertyChangeUndo : IUndo
{
    int nodeID;
    MapEditConfigData.TriggerHandlerCellInfo info_original;
    MapEditConfigData.TriggerHandlerCellInfo info_changed;
    public string des { get; set; }

    public ActionCellPropertyChangeUndo(ActionNode node)
    {
        this.info_original = new MapEditConfigData.TriggerHandlerCellInfo();
        MapDataSerializerTools.AssignDataByFieldName(node.ActionData.actionInfo, info_original);
        this.info_changed = null;
        this.nodeID = node.nodeID;
    }

    public void Undo()
    {
        ActionNode node = TriggerNodeWindow.GetNodeByID(nodeID) as ActionNode;

        if (info_changed == null)
        {
            info_changed = new MapEditConfigData.TriggerHandlerCellInfo();
            MapDataSerializerTools.AssignDataByFieldName(node.ActionData.actionInfo, info_changed);
        }

        MapEditConfigData.TriggerActionData randomData = node.ActionData;
        randomData.actionInfo = info_original;
        node.ActionData = randomData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }

    public void ReverseUndo()
    {
        ActionNode node = TriggerNodeWindow.GetNodeByID(nodeID) as ActionNode;

        MapEditConfigData.TriggerActionData randomData = node.ActionData;
        randomData.actionInfo = info_changed;
        node.ActionData = randomData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }
}

//随机行为组属性变化
public class RandomGroupPropertyChangeUndo : IUndo
{
    int nodeID;
    MapEditConfigData.TriggerNodeData info_original;
    MapEditConfigData.TriggerNodeData info_changed;
    public string des { get; set; }

    public RandomGroupPropertyChangeUndo(RandomGroupNode node)
    {
        this.info_original = new MapEditConfigData.TriggerNodeData();
        MapDataSerializerTools.AssignDataByFieldName(node.RandomData.node, info_original);
        this.info_changed = null;
        this.nodeID = node.nodeID;
    }

    public void Undo()
    {
        RandomGroupNode node = TriggerNodeWindow.GetNodeByID(nodeID) as RandomGroupNode;
        if (info_changed == null)
        {
            info_changed = new MapEditConfigData.TriggerNodeData();
            MapDataSerializerTools.AssignDataByFieldName(node.RandomData.node, info_changed);
        }

        MapEditConfigData.TriggerActionData randomData = node.RandomData;
        //不要改变位置
        info_original.posx = randomData.node.posx;
        info_original.posy = randomData.node.posy;
        randomData.node = info_original;
        node.RandomData = randomData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }

    public void ReverseUndo()
    {
        RandomGroupNode node = TriggerNodeWindow.GetNodeByID(nodeID) as RandomGroupNode;

        MapEditConfigData.TriggerActionData randomData = node.RandomData;
        //不要改变位置
        info_changed.posx = randomData.node.posx;
        info_changed.posy = randomData.node.posy;
        randomData.node = info_changed;
        node.RandomData = randomData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }
}

//条件属性变化
public class ConditionCellPropertyChangeUndo : IUndo
{
    int nodeID;
    MapEditConfigData.ConditionCellInfo info_original;
    MapEditConfigData.ConditionCellInfo info_changed;
    public string des { get; set; }

    public ConditionCellPropertyChangeUndo(ConditionNode node)
    {
        this.info_original = new MapEditConfigData.ConditionCellInfo();
        MapDataSerializerTools.AssignDataByFieldName(node.ConditionData.conditionInfo, info_original);
        this.info_changed = null;
        this.nodeID = node.nodeID;
    }

    public void Undo()
    {
        ConditionNode node = TriggerNodeWindow.GetNodeByID(nodeID) as ConditionNode;
        if (info_changed == null)
        {
            info_changed = new MapEditConfigData.ConditionCellInfo();
            MapDataSerializerTools.AssignDataByFieldName(node.ConditionData.conditionInfo, info_changed);
        }

        MapEditConfigData.TriggerConditionData conditionData = node.ConditionData;
        conditionData.conditionInfo = info_original;
        node.ConditionData = conditionData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }

    public void ReverseUndo()
    {
        ConditionNode node = TriggerNodeWindow.GetNodeByID(nodeID) as ConditionNode;

        MapEditConfigData.TriggerConditionData conditionData = node.ConditionData;
        conditionData.conditionInfo = info_changed;
        node.ConditionData = conditionData;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }
}

//根节点属性变化
public class BindingTargetPropertyChangeUndo : IUndo
{
    int nodeID;
    MapEditConfigData.TriggerRootNodeData info_original;
    MapEditConfigData.TriggerRootNodeData info_changed;
    public string des { get; set; }

    public BindingTargetPropertyChangeUndo(BindingTargetNode node)
    {
        nodeID = node.nodeID;
        info_original = new MapEditConfigData.TriggerRootNodeData();
        MapDataSerializerTools.AssignDataByFieldName(node.RootData, info_original);
        info_changed = null;
    }

    public void Undo()
    {
        BindingTargetNode node = TriggerNodeWindow.GetNodeByID(nodeID) as BindingTargetNode;
        if (info_changed == null)
        {
            info_changed = new MapEditConfigData.TriggerRootNodeData();
            MapDataSerializerTools.AssignDataByFieldName(node.RootData, info_changed);
        }

        node.RootData = info_original;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("撤销: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }

    public void ReverseUndo()
    {
        BindingTargetNode node = TriggerNodeWindow.GetNodeByID(nodeID) as BindingTargetNode;
        node.RootData = info_changed;

        if (!string.IsNullOrEmpty(des))
            Debug.Log("重做: " + des);

        EventManager.Instance.Trigger<EventTriggerNodeExecBegin>(EventTriggerNodeExecBegin.Get().reset(TriggerNodeWindow.new_trigger.id, node.nodeID, -1));
    }
}

using MapEditConfigData;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNode : BaseNode {
    private TriggerNodeWindow.ConditionTemplateItem _conEditData;
    private TriggerConditionData _conditionData;
    public TriggerConditionData ConditionData
    {
        get
        {
            if (_conditionData == null)
            {
                _conditionData = new TriggerConditionData();
                _conditionData.node = new TriggerNodeData();
                _conditionData.conditionInfo = new ConditionCellInfo();
                _conditionData.conditionInfo.target = new TriggerTargetInfo();
                _conditionData.conditionInfo.selectParam = new TriggerTargetInfo();
            }

            _conditionData.node.children.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                _conditionData.node.children.Add(children[i].nodeID);
            }
            _conditionData.node.parents.Clear();
            for (int i = 0; i < parents.Count; i++)
            {
                _conditionData.node.parents.Add(parents[i].nodeID);
            }
            //Debug.Log("ToSave! Position: " + Pos.center + "  NodeID: " + nodeID + "   ActionID: " + typeID);
            _conditionData.node.posx = Position.x;
            _conditionData.node.posy = Position.y;
            _conditionData.node.id = nodeID;
            _conditionData.node.NodeType = (int)nodeType;

            return _conditionData;
        }

        set
        {
            parents.Clear();
            children.Clear();
            _conditionData = value;
            if (_conditionData == null)
                _conditionData = new TriggerConditionData();
            if (_conditionData.conditionInfo == null)
                _conditionData.conditionInfo = new ConditionCellInfo();
            if(_conditionData.conditionInfo.target == null)
                _conditionData.conditionInfo.target = new TriggerTargetInfo();
            if(_conditionData.conditionInfo.selectParam == null)
                _conditionData.conditionInfo.selectParam = new TriggerTargetInfo();

            ConditionAttrTypeData.ConditionAttrTypeData conEdit = TriggerNodeWindow.m_vConditions.Find((shit) => { return shit.AttrID == _conditionData.conditionInfo.targetAttrId; });
            if (conEdit != null)
            {
                _conEditData = new TriggerNodeWindow.ConditionTemplateItem();
                _conEditData.tag = conEdit.Tag;
                _conEditData.attrID = conEdit.AttrID;
                _conEditData.desc = conEdit.Des;
                _conEditData.attrType = conEdit.AttrType;
                _conEditData.paremSource = conEdit.ParamSource;
                _conEditData.readExcelParam = conEdit.ReadExcelFuncParam;
                _conEditData.name = conEdit.Name;
                _conditionData.conditionInfo.paramSourceType = conEdit.ParamSource;
            }
            else
            {
                throw new System.Exception(string.Format("未找到编号为:[{0}]的ConditionAttrTypeData", _conditionData.conditionInfo.targetAttrId));
            }

            nodeID = _conditionData.node.id;
            ResetSize();

            Vector2 position = new Vector2(_conditionData.node.posx, _conditionData.node.posy);
            SetPosition(position);
            //Debug.Log("ToLoad! Position: " + Position + "  NodeID: " + nodeID + "   ActionID: " + _conditionData.conditionInfo.targetAttrId);
            #region 通过父子关系ID查找相应节点
            for (int i = 0; i < _conditionData.node.children.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_conditionData.node.children[i]);

                if (node != null)
                {
                    children.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的子节点!当前节点编号[{1}]", _conditionData.node.children[i], nodeID));
                }
            }

            for (int i = 0; i < _conditionData.node.parents.Count; i++)
            {
                BaseNode node = TriggerNodeWindow.GetNodeByID(_conditionData.node.parents[i]);
                if (node != null)
                {
                    parents.Add(node);
                }
                else
                {
                    Debug.LogError(string.Format("未找到编号为[{0}]的父节点!当前节点编号[{1}]", _conditionData.node.parents[i], nodeID));
                }
            }
            #endregion

            #region 初始化绑定条件
            IEditEventSender[] srcs = GameObject.FindObjectsOfType(typeof(IEditEventSender)) as IEditEventSender[];
            MapEditConfigData.TriggerTargetInfo tar = _conditionData.conditionInfo.target;
            if (tar.targetObj == null)
            {
                foreach (var src in srcs)
                {
                    if (src.id == tar.id && (int)src.ToTargetType() == tar.type)
                    {
                        tar.targetObj = src;
                        break;
                    }
                }
            }

            if (tar.targetObj != null)
            {
                List<ConditionAttrTypeData.ConditionAttrTypeData> datas = TriggerCondtionMappingTable.GetConditionAttrDatas(tar.targetObj.GetType());
                if (0 == datas.Count)
                {
                    Debug.LogWarning(tar.GetType() + "没有触发条件");
                    return;
                }

                for (int i = 0; i < datas.Count; i++)
                {
                    if (datas[i].AttrID == _conditionData.conditionInfo.targetAttrId)
                    {
                        _conditionData.conditionInfo.attrPopupIndex = i;
                        break;
                    }
                }

                if (_conditionData.conditionInfo.paramSourceType == (int)ParamSourceType.Enum)
                {
                    GetEnumDatasWithEnumType(datas[_conditionData.conditionInfo.attrPopupIndex].Name);
                    for (int i = 0; i < attrInfoDatas.Count; i++)
                    {
                        if (_conditionData.conditionInfo.enumParam == attrInfoDatas[i].Key)
                        {
                            _conditionData.conditionInfo.enumPopupIndex = i;
                            break;
                        }
                    }
                }
                else if (_conditionData.conditionInfo.paramSourceType == (int)ParamSourceType.ReadExcel)
                {
                    GetExcelDatasWithEnumType(datas[_conditionData.conditionInfo.attrPopupIndex].ReadExcelFuncParam);
                    for (int i = 0; i < excelDataNodes.Count; i++)
                    {
                        if (_conditionData.conditionInfo.excelParam == excelDataNodes[i].id)
                        {
                            _conditionData.conditionInfo.excelPopupIndex = i;
                            break;
                        }
                    }
                }
                else if (_conditionData.conditionInfo.paramSourceType == (int)ParamSourceType.UserSelect)
                {
                    foreach (var src in srcs)
                    {
                        if (src.id == _conditionData.conditionInfo.selectParam.id
                            && (int)src.ToTargetType() == _conditionData.conditionInfo.selectParam.type)
                        {
                            _conditionData.conditionInfo.selectParam.targetObj = src;
                            break;
                        }
                    }
                }
            }
            #endregion
        }
    }

    public ConditionNode()
    {
        nodeType = NodeType.Condition;
    }
    public void Init(TriggerNodeWindow.ConditionTemplateItem data)
    {
        _conEditData = data;
        ConditionData.conditionInfo.targetAttrId = _conEditData.attrID;

        ConditionData.conditionInfo.paramSourceType = _conEditData.paremSource;
        nodeName = data.desc;
    }

    public override void OnDrawGUI(Rect rect, Color color)
    {
        GUI.color = Color.magenta;
        GUI.Box(rect, "", GUI.skin.window);
        BeginNodeUI(rect);
        DrawConditionCellData(ConditionData.conditionInfo, color);
        EndNodeUI();
    }

    //画打开的折叠项的每一条数据
    void DrawConditionCellData(MapEditConfigData.ConditionCellInfo condition, Color color)
    {
        GUI.color = Color.white;
        Label(string.Format("条件 ID: {0}", ConditionData.node.id), labelStyle);

        Label(_conEditData.desc, labelStyle);

        if (condition.target.targetObj == null)
            GUI.color = Color.yellow;
        condition.target.targetObj = ObjectField(condition.target.targetObj, typeof(IEditEventSender));
        GUI.color = Color.white;

        if (condition.target.targetObj != null)
        {
            IEditEventSender src = condition.target.targetObj as IEditEventSender;

            if (TriggerNodeWindow.triggerTargetTable[_conEditData.desc].Contains((int)src.ToTargetType()))
            {
                condition.target.id = src.id;
                condition.target.type = (int)src.ToTargetType();
            }
            else
            {
                condition.target.id = -1;
                condition.target.targetObj = null;
            }
        }

        switch ((StageAttrTypeType)_conEditData.attrType)
        {
            case StageAttrTypeType.AT_Bool:
                condition.operatorId = (int)(BoolOperatorType)EnumPopup((BoolOperatorType)condition.operatorId);
                break;
            case StageAttrTypeType.AT_Value:
                condition.operatorId = (int)(NumberOperatorType)EnumPopup((NumberOperatorType)condition.operatorId);
                break;
        }

        switch ((ParamSourceType)condition.paramSourceType)
        {
            case ParamSourceType.UserInputFloat:
                condition.inputParam = FloatField(condition.inputParam);
                break;
            case ParamSourceType.UserInputInt:
                condition.inputIntParam = IntField(condition.inputIntParam);
                break;
            case ParamSourceType.UserSelect:
                if (null == condition.selectParam)
                    condition.selectParam = new MapEditConfigData.TriggerTargetInfo();

                if (condition.selectParam.targetObj == null)
                    GUI.color = Color.yellow;
                condition.selectParam.targetObj = ObjectField(condition.selectParam.targetObj, typeof(IEditEventSender));
                GUI.color = Color.white;

                if (condition.selectParam.targetObj != null)
                {
                    condition.selectParam.id = (condition.selectParam.targetObj as IEditEventSender).ID;
                    condition.selectParam.type = (int)(condition.selectParam.targetObj as IEditEventSender).ToTargetType();
                }
                else
                {
                    condition.selectParam.id = -1;
                    condition.selectParam.type = 0;
                }
                break;
            case ParamSourceType.Enum:
                GetEnumDatasWithEnumType(_conEditData.name);
                if (attrInfoDatas[condition.enumPopupIndex].Key != condition.enumParam)
                {
                    for (int i = 0; i < attrInfoDatas.Count; i++)
                    {
                        if (condition.enumParam == attrInfoDatas[i].Key)
                        {
                            condition.enumPopupIndex = i;
                            break;
                        }
                    }
                }
                condition.enumPopupIndex = Popup(condition.enumPopupIndex, enumAttrDes);
                condition.enumParam = attrInfoDatas[condition.enumPopupIndex].Key;
                break;
            case ParamSourceType.ReadExcel:
                GetExcelDatasWithEnumType(_conEditData.readExcelParam);
                if (condition.excelParam != excelDataNodes[condition.excelPopupIndex].id)
                {
                    for (int i = 0; i < excelDataNodes.Count; i++)
                    {
                        if (condition.excelParam == excelDataNodes[i].id)
                        {
                            condition.excelPopupIndex = i;
                            break;
                        }
                    }
                }
                condition.excelPopupIndex = Popup(condition.excelPopupIndex, excelDatas);
                condition.excelParam = excelDataNodes[condition.excelPopupIndex].id;
                break;
        }
    }

    #region 私有辅助函数
    List<AttrInfoData.AttrInfoData> attrInfoDatas = new List<AttrInfoData.AttrInfoData>();
    string _oldEnumDataType = "";
    string[] enumAttrDes;
    void GetEnumDatasWithEnumType(string attrName)
    {
        if (attrName == _oldEnumDataType)
            return;
        _oldEnumDataType = attrName;
        attrInfoDatas = TriggerCondtionMappingTable.GetDatasWithEnumType(attrName);
        enumAttrDes = new string[attrInfoDatas.Count];
        for (int tempIndex = 0; tempIndex < attrInfoDatas.Count; tempIndex++)
        {
            enumAttrDes[tempIndex] = attrInfoDatas[tempIndex].Des;
        }
    }

    int _oldExcelDataType = -1;
    string[] excelDatas;
    List<ExcelDataNode> excelDataNodes;
    void GetExcelDatasWithEnumType(int type)
    {
        if (type == _oldExcelDataType)
        {
            return;
        }
        _oldExcelDataType = type;

        excelDataNodes = TriggerCondtionMappingTable.GetDatasWithEnumType((ExcelDataType)type);

        excelDatas = new string[excelDataNodes.Count];
        for (int i = 0; i < excelDataNodes.Count; i++)
        {
            excelDatas[i] = excelDataNodes[i].data;
        }
    }
    #endregion

    public override string ToString()
    {
        return "Position: " + Position.ToString() + "  NodeID: " + nodeID + "  PopIndex: " + ConditionData.conditionInfo.attrPopupIndex + "  AttrID:" + ConditionData.conditionInfo.targetAttrId + "\n" + GetInfo(ConditionData);
    }

    string GetInfo(MapEditConfigData.TriggerConditionData data)
    {
        string tmp = "";
        tmp += "pop:" + data.conditionInfo.attrPopupIndex;
        tmp += "  attrID:" + data.conditionInfo.targetAttrId;

        tmp += "  pcount:" + data.node.parents.Count + " [";
        for (int i = 0; i < data.node.parents.Count; i++)
        {
            tmp += data.node.parents[i] + ",";
        }
        tmp += "]  ccount:" + data.node.children.Count + " [";
        for (int i = 0; i < data.node.children.Count; i++)
        {
            tmp += data.node.children[i] + ",";
        }
        tmp += "]";

        tmp += " operatorId:" + data.conditionInfo.operatorId;
        tmp += " paramSourceType:" + data.conditionInfo.paramSourceType;
        tmp += " enumParam:" + data.conditionInfo.enumParam;

        return tmp;
    }

    public override void OnPropertyChanged()
    {
        ConditionCellPropertyChangeUndo cccc = new ConditionCellPropertyChangeUndo(this);
        cccc.des = string.Format("Condition Node[{0}] Property Changed", nodeID);
        NodeUndo.RecordUndo(cccc);
        TriggerNodeWindow.propChanged = true;
    }
}
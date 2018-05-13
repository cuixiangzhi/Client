using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Text;
using System.IO;
using ProtoBuf;

public class TRIGGER_HANDLER_CONSTANCE_ID
{
    public const int RANDOM_ACTION_BEGIN = -110;

    public const int RANDOM_ACTION_END = -111;

    public const int RANDOM_ACTION_SET_WEIGHT_BEGIN = -120;
}

[CustomEditor(typeof(TriggerEditData))]
public class TriggerEditorDataInspector : Editor
{
    string TRIGGER_TEMPLATE_FOLDER = "Assets/Res/Resource/Datum/TriggerDataTemplate/";

    float LEFT_LABLE_WIDTH = 100F;
    //float INPUT_FILED_WIDTH = 100F;
    float ADD_BTN_WIDTH = 20F;
    int FONT_SIZE = 14;
    float SELECT_OBJECT_WIDTH = 100F;
    //float RELATION_WIDTH = 40F;

	const int MAX_PARAM_NUM = 6;

    const int TRIGGER_CINEMA_ACTION_ID = -102;

    List<StageTriggerEventData.StageTriggerEventData> m_vStageEvents;
    List<StageEventHandlerData.StageEventHandlerData> m_vHandlers;

    List<int> m_vCommonEventIds = new List<int>();//绑定对象集合公共事件ID

    string[] TriggerHandlerTypes = new string[] { "刷第N波怪" };

    TriggerEditData m_obj = null;

    //3个Foldout的开关,绑定对象,触发条件,处理函数
    public static bool triggerBindObjectsFoldout = true;
    public static bool triggerConditionFoldout = true;
    public static bool triggerHandlerFoldout = true;

    //触发条件,处理函数,绑定对象的信息
    //TriggerConditionInfo conditionData = new TriggerConditionInfo();
    //HandlerInfo handlerData = new HandlerInfo();

    Vector2 scrollposition = Vector2.zero;

    void OnEnable()
    {
        DatasManager.Instance.LoadAllDatas();
        m_vStageEvents = DatasManager.Instance.GetDatas<StageTriggerEventData.StageTriggerEventData>();
		m_vHandlers = DatasManager.Instance.GetDatas<StageEventHandlerData.StageEventHandlerData>();
        TriggerHandlerTypes = new string[m_vHandlers.Count];
        for (int i = 0; i < m_vHandlers.Count; i++)
        {
            TriggerHandlerTypes[i] = m_vHandlers[i].Des;
        }

        m_obj = target as TriggerEditData;

        InitTriggerDataInfo();
    }

    void InitTriggerDataInfo()
    {
        Transform root = MapEditorUtlis.GetEditRoot();
        IEditEventSender[] srcs = root.GetComponentsInChildren<IEditEventSender>();

        #region 初始化绑定目标
        for (int i = 0; i < m_obj.Data.nodes.root.targets.Count; i++)
        {
            MapEditConfigData.TriggerTargetInfo tar = m_obj.Data.nodes.root.targets[i];
            for (int idx = 0; idx < srcs.Length; idx++)
            {
                if (srcs[idx].id == tar.id && (int)srcs[idx].ToTargetType() == tar.type)
                {
                    tar.targetObj = srcs[idx];
                    break;
                }
            }
        }
        #endregion

        #region 初始化绑定事件
        for (int i = 0; i < m_vStageEvents.Count; i++)
        {
            if (m_vStageEvents[i].ID == m_obj.Data.nodes.root.eventInfo.eventID)
            {
                m_obj.Data.nodes.root.eventInfo.eventPopupIndex = i;
                break;
            }
        }

        #endregion

        #region 初始化绑定条件
        //foreach (var grid in m_obj.Data.conditionInfo.conditionGrids)
        foreach (var cell in m_obj.Data.nodes.conditions)
        {
            //foreach(var cell in grid.co.conditionCells)
            {
                MapEditConfigData.TriggerTargetInfo tar = cell.conditionInfo.target;
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
                        if (datas[i].AttrID == cell.conditionInfo.targetAttrId)
                        {
                            cell.conditionInfo.attrPopupIndex = i;
                            break;
                        }
                    }

                    if (cell.conditionInfo.paramSourceType == (int)ParamSourceType.Enum)
                    {
                        List<AttrInfoData.AttrInfoData> infodatas = TriggerCondtionMappingTable.GetDatasWithEnumType(datas[cell.conditionInfo.attrPopupIndex].Name);
                        for (int i = 0; i < infodatas.Count; i++)
                        {
                            if (cell.conditionInfo.enumParam == infodatas[i].Key)
                            {
                                cell.conditionInfo.enumPopupIndex = i;
                                break;
                            }
                        }
                    }
                    else if (cell.conditionInfo.paramSourceType == (int)ParamSourceType.ReadExcel)
                    {
                        List<ExcelDataNode> dataNodes = TriggerCondtionMappingTable.GetDatasWithEnumType((ExcelDataType)datas[cell.conditionInfo.attrPopupIndex].ReadExcelFuncParam);
                        for (int i = 0; i < dataNodes.Count; i++)
                        {
                            if (cell.conditionInfo.excelParam == dataNodes[i].id)
                            {
                                cell.conditionInfo.excelPopupIndex = i;
                                break;
                            }
                        }
                    }
                    else if (cell.conditionInfo.paramSourceType == (int)ParamSourceType.UserSelect)
                    {
                        foreach (var src in srcs)
                        {
                            if (src.id == cell.conditionInfo.selectParam.id
                                && (int)src.ToTargetType() == cell.conditionInfo.selectParam.type)
                            {
                                cell.conditionInfo.selectParam.targetObj = src;
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 初始化Action
        //foreach (var handlerCell in m_obj.Data.handlerInfo.handlerInfo)
        foreach (var handlerCell in m_obj.Data.nodes.actions)
        {
            //if(handlerCell.actionInfo.handlerID != 0)
            //{
            //    for(int i = 0; i < m_vHandlers.Count; i++)
            //    {
            //        if(m_vHandlers[i].ID == handlerCell.actionInfo.handlerID)
            //        {
            //            handlerCell.actionInfo.handlerIdPopupIndex = i;
            //            break;
            //        }
            //    }
            //}

            for (int i = 0; i < handlerCell.actionInfo.handlerParams.Count; i++)
            {
                var handlerParam = handlerCell.actionInfo.handlerParams[i];
                StageEventHandlerData.StageEventHandlerData data = m_vHandlers.Find((hd) => { return hd.ID == handlerCell.actionInfo.handlerID; });//[handlerCell.actionInfo.handlerIdPopupIndex];

                PropertyInfo paramSourceType = data.GetType().GetProperty(string.Format("Param{0}SourceType", i + 1));
                if (null == paramSourceType)
                {
                    continue;
                }
                ParamSourceType paramSourceTypeValue = (ParamSourceType)paramSourceType.GetValue(data, null);

                if (paramSourceTypeValue == ParamSourceType.UserSelect)
                {
                    foreach (var src in srcs)
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
        }
        #endregion
    }

    public override void OnInspectorGUI()
    {
        DrawTriggerPage();

        Repaint();
    }

    //画编辑触发器界面
    void DrawTriggerPage()
    {
        scrollposition = GUILayout.BeginScrollView(scrollposition, false, false);

        EditorGUILayout.BeginVertical();

        #region 触发器名称
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GetAddBtnOptions());
        EditorGUILayout.LabelField("触发器名称:", GetContentWidth("触发器名称:"));
        m_obj.name = EditorGUILayout.TextField(m_obj.name, GetContentWidth(m_obj.name));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GetAddBtnOptions());
        GUILayout.Label(string.Format("触发器ID:{0}", m_obj.id));
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("");
        #endregion

        #region 触发器绑定对象
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GetAddBtnOptions());
        triggerBindObjectsFoldout = EditorGUILayout.Foldout(triggerBindObjectsFoldout, "触发器绑定对象");
        EditorGUILayout.EndHorizontal();

        if (triggerBindObjectsFoldout)
        {
            if (m_obj.Data.nodes.root.targets.Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("\t", GetAddBtnOptions());
                if (GUILayout.Button("+", GetAddBtnOptions()))
                {
                    m_obj.Data.nodes.root.targets.Add(new MapEditConfigData.TriggerTargetInfo());
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                for (int i = 0; i < m_obj.Data.nodes.root.targets.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GetAddBtnOptions());
                    GUILayout.Label("", GetAddBtnOptions());
                    if (GUILayout.Button("-", GetAddBtnOptions()))
                    {
                        m_obj.Data.nodes.root.targets.RemoveAt(i);
                        break;
                    }
                    m_obj.Data.nodes.root.targets[i].targetObj = (IEditEventSender)EditorGUILayout.ObjectField(m_obj.Data.nodes.root.targets[i].targetObj, typeof(IEditEventSender), true, GetSelectObjectOptions());
                    if (null != m_obj.Data.nodes.root.targets[i].targetObj)
					{
                        IEditEventSender tar = m_obj.Data.nodes.root.targets[i].targetObj as IEditEventSender;
                        m_obj.Data.nodes.root.targets[i].id = tar.id;
                        m_obj.Data.nodes.root.targets[i].type = (int)tar.ToTargetType();
					}
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GetAddBtnOptions());
                GUILayout.Label("", GetAddBtnOptions());
                GUILayout.Label("", GetAddBtnOptions());
                if (GUILayout.Button("+", GetAddBtnOptions()))
                {
                    m_obj.Data.nodes.root.targets.Add(new MapEditConfigData.TriggerTargetInfo());
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        #endregion

        #region 触发器绑定事件
        ShowCommonEventEnums();
        #endregion

        m_obj.Data.defaultEnabled = GUILayout.Toggle(m_obj.Data.defaultEnabled, "默认开启状态");

        EditorGUILayout.Separator();
        GUILayout.FlexibleSpace();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        Rect r = EditorGUILayout.GetControlRect();
        r.width = 100;
        r.height = 30;
        r.center = new Vector2(Screen.width * 0.5f, r.center.y - 15);
        if (GUI.Button(r, "打开节点编辑器"))
        {
            //TriggerNodeWindow window = EditorWindow.GetWindowWithRect<TriggerNodeWindow>(new Rect(0, 0, 1200, 800), true, "条件动作编辑器");
            //TriggerNodeWindow window = EditorWindow.GetWindowWithRect<TriggerNodeWindow>(new Rect(0, 0, 1200, 800));
            TriggerNodeWindow window = EditorWindow.GetWindow<TriggerNodeWindow>();
            window.Show();
            window.InitData(m_obj.Data);
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("导出为模板"))
        {

            string filePath = EditorUtility.SaveFilePanel("保存", TRIGGER_TEMPLATE_FOLDER, "", "bytes");
            if (!string.IsNullOrEmpty(filePath))
            {
                __DoSaveTriggerData(filePath);
            }
        }
        if (GUILayout.Button("由模板导入"))
        {
            string filePath = EditorUtility.OpenFilePanel("导入", TRIGGER_TEMPLATE_FOLDER, "bytes");
            if (!string.IsNullOrEmpty(filePath))
            {
                __DoLoadriggerData(filePath);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    void __DoSaveTriggerData(string file)
    {
        MapEditorConfigParser.SaveProtoDataToBinary(m_obj.Data, file);

        EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "成功", MapEditorCons.HINT_OK);
    }

    void __DoLoadriggerData(string filePath)
    {
        string path = filePath.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "Assets");

        TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

        MapEditConfigData.TriggerInfo data = null;
        try
        {
            data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.TriggerInfo>(txt);
        }
        catch
        {
            Debug.LogError("parse trigger template error!");
        }
        finally
        {
            if (data != null)
            {
                ResetTriggerData(data);
                EditorUtility.DisplayDialog("", "加载数据成功", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("", "加载数据失败", "OK");
            }
        }
    }

    void ResetTriggerData(MapEditConfigData.TriggerInfo data)
    {
        int orgId = m_obj.Data.id;
        data.id = orgId;
        m_obj.Data = data;

        InitTriggerDataInfo();
    }

    /// <summary>
    /// 展示公共可选事件ID
    /// </summary>
    void ShowCommonEventEnums()
    {
        GUILayout.Label("");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GetAddBtnOptions());
        EditorGUILayout.LabelField("事件ID:", GetLeftTitleOptions());
        int[] tmpCommonIds;//根据当前绑定对象计算的公共事件列表
        string[] eventNames;//相应的事件说明列表
        
		GetCommonEventIDList(m_obj.Data, out tmpCommonIds, out eventNames);
		bool change = IsCommonEventChanged(tmpCommonIds);
        
		if(!change)
		{
            m_obj.Data.nodes.root.eventInfo.eventPopupIndex = EditorGUILayout.Popup(m_obj.Data.nodes.root.eventInfo.eventPopupIndex, eventNames, GetLeftTitleOptions());
            m_obj.Data.nodes.root.eventInfo.eventID = m_vCommonEventIds[m_obj.Data.nodes.root.eventInfo.eventPopupIndex];
		}
		else
		{
			if (tmpCommonIds == null || eventNames == null)
			{
				EditorGUILayout.HelpBox("未找到共有事件ID,请确认监听对象是否有共同触发事件.", MessageType.Error);
			}
			else
			{
				m_vCommonEventIds.Clear();
				bool inCurCommonEvents = false;
				
				for (int i = 0; i < tmpCommonIds.Length; i++)
				{
					m_vCommonEventIds.Add(tmpCommonIds[i]);
                    if (m_obj.Data.nodes.root.eventInfo.eventID == tmpCommonIds[i])//判断当前触发器监听ID是否在公共事件ID列表中
					{
                        m_obj.Data.nodes.root.eventInfo.eventPopupIndex = i;
						inCurCommonEvents = true;
					}
				}

				if(inCurCommonEvents)
				{
                    m_obj.Data.nodes.root.eventInfo.eventPopupIndex = EditorGUILayout.Popup(m_obj.Data.nodes.root.eventInfo.eventPopupIndex, eventNames, GetLeftTitleOptions());
                    m_obj.Data.nodes.root.eventInfo.eventID = m_vCommonEventIds[m_obj.Data.nodes.root.eventInfo.eventPopupIndex];
				}
				else
				{
                    m_obj.Data.nodes.root.eventInfo.eventPopupIndex = 0;
                    m_obj.Data.nodes.root.eventInfo.eventID = 0;
                    m_obj.Data.nodes.root.eventInfo.eventPopupIndex = EditorGUILayout.Popup(m_obj.Data.nodes.root.eventInfo.eventPopupIndex, eventNames, GetLeftTitleOptions());
				}
			}
		}

        EditorGUILayout.EndHorizontal();
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
        for (int i = 0; i < _triggerInfo.nodes.root.targets.Count; i++)
        {
            if (_triggerInfo.nodes.root.targets[i].targetObj != null)
            {
                obj_list.Add(_triggerInfo.nodes.root.targets[i].targetObj);
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
		if(null == newEvents || m_vCommonEventIds == null)
			return true;

		if(newEvents.Length != m_vCommonEventIds.Count)
			return true;

		for(int i = 0 ; i < newEvents.Length; i++)
		{
			bool find = false;
			for(int j = 0 ; j < m_vCommonEventIds.Count; j++)
			{
				if(m_vCommonEventIds[j] == newEvents[i])
				{
					find = true;
					break;
				}
			}
			if(!find)
			{
				return true;
			}
		}

		return false;
	}

    public GUILayoutOption GetLeftTitleOptions()
    {
        return GUILayout.Width(LEFT_LABLE_WIDTH);
    }

    public GUILayoutOption GetSelectObjectOptions()
    {
        return GUILayout.Width(SELECT_OBJECT_WIDTH);
    }

    public GUILayoutOption GetContentWidth(string words)
    {
        return GUILayout.Width(FONT_SIZE * words.Length);
    }

    public GUILayoutOption GetAddBtnOptions()
    {
        return GUILayout.Width(ADD_BTN_WIDTH);
    }
}
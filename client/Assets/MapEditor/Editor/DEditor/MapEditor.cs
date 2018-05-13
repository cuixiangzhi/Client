using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using ProtoBuf;
using System.Reflection;

public class MapEditor : EditorWindow
{
    [MenuItem("MapEditor/触发器运行状态监控", false)]
    static void __MenuFuncMonitorTriggerState()
    {
        //if(StageManager.Instance.CurMapData != null)
        //{
        //    __PlaceData(StageManager.Instance.CurMapData);
        //}
    }

    [MenuItem("MapEditor/升级数据-程序专用-策划别点-不然你就挂了", false)]
    static void __MenuFuncUpdateSomeThing()
    {
        if (EditorApplication.isCompiling)
            return;

        DatasManager.Instance.LoadAllDatas(() => {
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Res/Resource/Datum/Map");
            if (dir.Exists)
            {
                FileSystemInfo[] files = dir.GetFileSystemInfos("*.bytes");

                for (int f = 0; f < files.Length; f++)
                {
                    string path = "Assets/Res/Resource/Datum/Map/" + files[f].Name;
                    TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                    EditorUtility.DisplayProgressBar("", "Updating " + files[f].Name, f * 1.0f / files.Length);

                    //MapEditConfigData.MapEditConfigData data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.MapEditConfigData>(txt);
                    MapEditConfigData.MapEditConfigData data = MapEditorConfigParser.ParseDataFromStream(txt);

                    //__VerifyHandlerParamNumber(ref data);

                    foreach (var tr in data.triggerDatas)
                    {
                        foreach (var t in tr.nodes.conditions)
                        {
                            MapEditConfigData.ConditionCellInfo ap = t.conditionInfo;
                            if (null == ap)
                            {
                                continue;
                            }

                            if(ap.targetAttrId == (int)OperatorTargetType.OTT_MonsterGroupExistState)
                            {
                                ap.targetAttrId = (int)OperatorTargetType.OTT_ExistState;
                            }
                        }
                    }

                    MapEditorConfigParser.SaveProtoDataToBinary<MapEditConfigData.MapEditConfigData>(data, Application.dataPath + "/Res/Resource/Datum/Map/" + files[f].Name);
                }
                EditorUtility.DisplayDialog("Result", "升级成功", "确定");
                EditorUtility.ClearProgressBar();
            }
        });
    }

    [MenuItem("MapEditor/编辑器初始化", false)]
    static void __MenuFuncInitEditor()
    {
        DatasManager.Instance.LoadAllDatas();
    }

    [MenuItem("MapEditor/添加 主将", false)]
    static PlayerEditData __MenuFuncPlayerAdd()
	{
        Transform playerRoot = MapEditorUtlis.GetPlayerEditRoot();
        Transform player = playerRoot.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player").transform;
            player.parent = playerRoot;
        }

        Selection.activeGameObject = player.gameObject;
        PlayerEditData src = player.gameObject.AddComponent<PlayerEditData>();
        src.id = MapEditorUtlis.GenGID(src);
        src.camp = 0;
        src.allertDistance = 6;

        return src;
    }

    [MenuItem("MapEditor/添加 随从", false)]
    static void __MenuFuncServantAdd()
	{
        Transform servantRoot = MapEditorUtlis.GetServantEditRoot();

        GameObject go = new GameObject();
        ServantEditData src = go.AddComponent<ServantEditData>();
        src.id = MapEditorUtlis.GenGID(src);

		go.transform.parent = servantRoot;
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;

		Selection.activeGameObject = go;

        int index = src.id & 0x0000ffff;
        go.name = "Servant_" + index;
    }

    [MenuItem("MapEditor/添加 触发者", false)]
    static void __MenuFuncTriggerManAdd()
    {
        Transform triggerMan = MapEditorUtlis.GetTriggerManEditRoot();

        triggerMan.gameObject.AddComponent<TriggerMan>();
    }

    [MenuItem("MapEditor/添加 预留防御塔", false)]
    static void __MenuFuncTowerAdd()
    {
        int reservedId = MapEditorUtlis.GenReservedGID(EditerObjectType.EOT_Tower);
        
        MapEditConfigData.TowerPlaceInfo monster = new MapEditConfigData.TowerPlaceInfo();
        monster.id = reservedId;
        monster.name = "预留防御塔" + reservedId;
        monster.pos = new MapEditConfigData.MP_Vector3();
        monster.rot = new MapEditConfigData.MP_Vector3();

        DoAddTower(monster);
    }

    [MenuItem("MapEditor/添加 预留兵营", false)]
    static void __MenuFuncBarracksAdd()
    {
        int reservedId = MapEditorUtlis.GenReservedGID(EditerObjectType.EOT_Barracks);
        
        MapEditConfigData.TowerPlaceInfo monster = new MapEditConfigData.TowerPlaceInfo();
        monster.id = reservedId;
        monster.name = "预留兵营" + reservedId;
        monster.pos = new MapEditConfigData.MP_Vector3();
        monster.rot = new MapEditConfigData.MP_Vector3();
        
        DoAddBarracks(monster);
    }

    [MenuItem("MapEditor/添加 摄像机 &C", false)]
    static void __MenuFuncCameraAdd()
    {
        Transform cameraRoot = MapEditorUtlis.GetCameraEditRoot();
        Transform targetRoot = MapEditorUtlis.GetCameraTargetRoot();

        CameraEditData newCam = new GameObject().AddComponent<CameraEditData>();
        int id = MapEditorUtlis.GenGID(newCam);
        newCam.gameObject.name = "Camera" + id;

        Camera cam = newCam.gameObject.AddComponent<Camera>();
        cam.enabled = false;
        cam.transform.parent = cameraRoot;
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localEulerAngles = Vector3.zero;
        cam.transform.localScale = Vector3.one;
        cam.fieldOfView = 42;

        Transform target = new GameObject("Camera" + id + "_Target").transform;
        target.parent = targetRoot;

        //CameraFollow followSrc = cam.gameObject.AddComponent<CameraFollow>();
        //followSrc.target = target;

		Selection.activeGameObject = cam.gameObject;
    }

    [MenuItem("MapEditor/添加 触发器 #%T", false)]
    static void __MenuFuncTriggerAdd()
    {
        MapEditConfigData.TriggerInfo info = new MapEditConfigData.TriggerInfo();

        info.nodes = new MapEditConfigData.TriggerHandlerInfo();
        info.nodes.root = new MapEditConfigData.TriggerRootNodeData();
        info.nodes.root.node = new MapEditConfigData.TriggerNodeData();
        info.nodes.root.eventInfo = new MapEditConfigData.TriggerEventInfo();

		info.defaultEnabled = true;

        Transform root = MapEditorUtlis.GetTriggerEditRoot();
        GameObject obj = new GameObject();
		TriggerEditData src = obj.AddComponent<TriggerEditData>();
		info.id = MapEditorUtlis.GenGID(src);
		info.name = "Trigger_" + info.id;

        obj.transform.parent = root;
        obj.transform.position = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        src.Data = info;
        Selection.activeGameObject = obj;
    }

    [MenuItem("MapEditor/添加 计时器", false)]
    static void __MenuFuncTimmerAdd()
    {
        MapEditConfigData.TimmerInfo info = new MapEditConfigData.TimmerInfo();

        Transform root = MapEditorUtlis.GetTimmerEditRoot();
        GameObject obj = new GameObject(info.name);

        TimmerEditData src = obj.AddComponent<TimmerEditData>();
        info.id = MapEditorUtlis.GenGID(src);
        info.name = "Timmer_" + info.id;

		obj.transform.parent = root;
		obj.transform.position = Vector3.zero;
		obj.transform.localEulerAngles = Vector3.zero;
		obj.transform.localScale = Vector3.one;

        src.Data = info;
        Selection.activeGameObject = obj;
    }

    [MenuItem("MapEditor/添加 区域检测器", false)]
    static void __MenuFuncSurveyedAreaAdd()
    {
        MapEditConfigData.SurveyedAreaInfo info = new MapEditConfigData.SurveyedAreaInfo();
        info.radius = 1.0f;
		info.position = new MapEditConfigData.MP_Vector3();

        Transform root = MapEditorUtlis.GetSurveyAreaRootEditRoot();
        GameObject obj = new GameObject(info.name);

        SurveyedAreaEditData src = obj.AddComponent<SurveyedAreaEditData>();
        info.id = MapEditorUtlis.GenGID(src);
        info.name = "区域检测器_" + info.id;

		obj.transform.parent = root;
		obj.transform.position = Vector3.zero;
		obj.transform.localEulerAngles = Vector3.zero;
		obj.transform.localScale = Vector3.one;
        
        src.Data = info;
        Selection.activeGameObject = obj;
    }

    [MenuItem("MapEditor/添加 关卡变量", false)]
    static void __MenuFuncValueAdd()
    {
        MapEditConfigData.ValueInfo info = new MapEditConfigData.ValueInfo();

        Transform root = MapEditorUtlis.GetValueEditRoot();
        GameObject obj = new GameObject(info.name);

        ValueEditData src = obj.AddComponent<ValueEditData>();
        info.id = MapEditorUtlis.GenGID(src);
        info.name = "变量_" + info.id;

		obj.transform.parent = root;
		obj.transform.position = Vector3.zero;
		obj.transform.localEulerAngles = Vector3.zero;
		obj.transform.localScale = Vector3.one;

        src.Data = info;
        Selection.activeGameObject = obj;
    }

    [MenuItem("MapEditor/添加 机关门", false)]
    static void __MenuFuncDoorAdd()
    {
        MapEditConfigData.DoorInfo info = new MapEditConfigData.DoorInfo();
        DoAddDoor(info);
    }

	[MenuItem("MapEditor/添加 怪物 #%M", false)]
    static void __MenuFuncMonsterAdd()
    {
        MapEditConfigData.MonsterPlaceInfo info = new MapEditConfigData.MonsterPlaceInfo();
        info.id = 0;
        info.name = "";
        info.type = 0;
        info.group = 0;
        info.protection = 0;
        info.camp = 0;
        info.resumeId = 0;

        int maxGp = 0xFF;
        Transform monsterRoot = MapEditorUtlis.GetMonsterEditRoot();
        MonsterGroupEditData[] groups = monsterRoot.gameObject.GetComponentsInChildren<MonsterGroupEditData>();
        foreach (var gp in groups)
        {
            if(maxGp < gp.ID)
            {
                maxGp = gp.ID;
            }
        }
        info.group = ++maxGp;

        info.pos = new MapEditConfigData.MP_Vector3();
        info.rot = new MapEditConfigData.MP_Vector3();

        DoAddMonster(info);
	}

    [MenuItem("MapEditor/添加 客户端角色 #%M", false)]
    static void __MenuFuncClientCharacterAdd()
    {
        MapEditConfigData.ClientCharacterPlaceInfo info = new MapEditConfigData.ClientCharacterPlaceInfo();
        info.id = 0;
        info.pos = new MapEditConfigData.MP_Vector3();
        info.rot = new MapEditConfigData.MP_Vector3();

        int max = 0;
        IEditEventSender[] groups = MapEditorUtlis.GetEditRoot().GetComponentsInChildren<MonsterGroupEditData>();
        foreach(var gp in groups)
        {
            if(max < gp.id)
            {
                max = gp.id;
            }
        }
        max++;

        info.group = max;

        DoAddClientCharacter(info);
    }

	[MenuItem("MapEditor/添加 点 &P", false)]
	static void __MenuFuncPointAdd()
	{
		MapEditConfigData.PointInfo info = new MapEditConfigData.PointInfo();
		info.id = 0;
		info.position = new MapEditConfigData.MP_Vector3();
		info.rotation = new MapEditConfigData.MP_Vector3();
		
        DoAddPoint(info);
	}

    [MenuItem("MapEditor/添加 路径 &#T", false)]
    static void __MenuFuncPathAdd()
    {
        MapEditConfigData.PathInfo info = new MapEditConfigData.PathInfo();
        info.name = "";
        info.id = 0;

        DoAddPath(info);
    }

	[MenuItem("MapEditor/添加 自动寻路点", false)]
    static void __MenuFuncAutoPointAdd()
    {
        MapEditConfigData.PointInfo info = new MapEditConfigData.PointInfo();
        info.id = 0;
        info.name = "";
        info.position = new MapEditConfigData.MP_Vector3();
        info.rotation = new MapEditConfigData.MP_Vector3();
        
        DoAddAutoPoint(info);
    }

	[MenuItem("MapEditor/添加 自动寻路路径", false)]
    static void __MenuFuncAutoPathAdd()
    {
		MapEditConfigData.PathInfo info = new MapEditConfigData.PathInfo();
		info.name = "";
		info.id = 0;
		
		DoAddAutoPath(info);
    }

    [MenuItem("MapEditor/添加 关卡星级描述")]
    static StageStarDesEditData __MenuFuncStageStarDesAdd()
    {
        Transform obj = MapEditorUtlis.GetStageStarDesEditRoot();

        return obj.gameObject.AddComponent<StageStarDesEditData>();
    }

    [MenuItem("MapEditor/添加 关卡路标")]
    static MoveDirectionGuidEditData __MenuFuncMoveDirectionAdd()
    {
        Transform obj = MapEditorUtlis.GetStageMoveDirectionEditRoot();

        return obj.gameObject.AddComponent<MoveDirectionGuidEditData>();
    }

    [MenuItem("MapEditor/添加 世界地图路点", false)]
    static void __MenuFuncWorldPointAdd()
    {
        //int reservedId = MapEditorUtlis.GenReservedGID(EditerObjectType.EOT_WorldPoint);

        //MapEditConfigData.WorldPointInfo monster = new MapEditConfigData.WorldPointInfo();
        //monster.id = reservedId;
        //monster.name = "世界地图路点" + reservedId;
        //monster.pos = new MapEditConfigData.MP_Vector3();
        //monster.rot = new MapEditConfigData.MP_Vector3();

        //DoAddWorldPoint(monster);
    }

    [MenuItem("MapEditor/File/Save Map As &S", false)]
    static void __MenuFuncSaveMapData()
    {
        if (EditorApplication.isCompiling)
            return;

        string folder = Application.dataPath + "/Res/Resource/Datum/Map/";
        string filePath = EditorUtility.SaveFilePanel("保存地图文件", folder, loadedDataName, "bytes");
        if (!string.IsNullOrEmpty(filePath))
        {
            __DoSaveCfg(filePath);
        }
    }

    [MenuItem("MapEditor/File/Open Map File &L", false)]
    static void __MenuFuncLoadMapData()
    {
        if (EditorApplication.isCompiling)
            return;

        string folder = Application.dataPath + "/Res/Resource/Datum/Map/";
        string filePath = EditorUtility.OpenFilePanel("打开地图文件", folder, "bytes");
        if (!string.IsNullOrEmpty(filePath))
        {
            DatasManager.Instance.LoadAllDatas();
            __DoLoadMapData(filePath);
        }
    }

	[MenuItem("MapEditor/修改摄像机数据", false)]
	static void __MenuFuncChangeCameraData()
	{
		//EditorWindow.GetWindow<CameraEditor>(false, "修改摄像机数据", true).Show();
	}

    [MenuItem("MapEditor/检测地图数据错误", false)]
    static void __MenuFuncCheckMapConfigDatas()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Res/Resource/Datum/Map");
        if (dir.Exists)
        {
            FileSystemInfo[] files = dir.GetFileSystemInfos("*.bytes");
            for (int i = 0; i < files.Length; i++)
            {
                string configName = files[i].Name;

                EditorUtility.DisplayProgressBar("", "Checking " + files[i].Name, i * 1.0f / files.Length);

                string path = "Assets/Res/Resource/Datum/Map/" + configName;
                TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                MapEditConfigData.MapEditConfigData data = null;

                data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.MapEditConfigData>(txt);

                foreach (var group in data.monsterDatas)
                {
                    if (group.leaderInfo == null && group.memberInfos.Count == 0)
                    {
                        Debug.LogError(string.Format("{0} group: {1} is empty!", configName, group.id));
                    }

                    if (group.leaderInfo != null)
                    {
                        if (group.leaderInfo.id == 0)
                        {
                            Debug.LogError(string.Format("{0} group: {1} leader id is invalid!!", configName, group.id));
                        }
                        if (group.leaderInfo.type == 0)
                        {
                            Debug.LogError(string.Format("{0} group: {1} leader type is invalid!!", configName, group.id));
                        }
                    }

                    foreach (var mon in group.memberInfos)
                    {
                        if (mon == null)
                        {
                            Debug.LogError(string.Format("{0} group: {1} has null member monster data!", configName, group.id));
                        }
                        if (mon.id == 0)
                        {
                            Debug.LogError(string.Format("{0} group: {1} member {2} id is invalid!", configName, group.id, mon.id));
                        }
                        if (mon.type == 0)
                        {
                            Debug.LogError(string.Format("{0} group: {1} member {2} type is invalid!", configName, group.id, mon.type));
                        }
                    }
                }

                foreach (var cinema in data.cinemaDatas)
                {
                    if (string.IsNullOrEmpty(cinema.name))
                    {
                        Debug.LogError(string.Format("{0} cinema: name is null!", configName));
                    }
                }

                ////////check id repeadted
                foreach (var v1 in data.pointDatas)
                {
                    foreach (var v2 in data.pointDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} point {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.pathDatas)
                {
                    foreach (var v2 in data.pathDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} path {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.pathDatas)
                {
                    if (v1.pathType == (int)LoopType.PingPang)
                    {
                        if (v1.points.Count < 2)
                        {
                            Debug.LogError(string.Format("{0} path {1} id is pingpang type, but it has less than 2 PathNode!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.monsterDatas)
                {
                    foreach (var v2 in data.monsterDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} monsterGroup {1} id is repeated!!", configName, v1.id));
                        }
                    }

                    foreach (var mon1 in v1.memberInfos)
                    {
                        if (v1.leaderInfo != null && mon1.id == v1.leaderInfo.id)
                        {
                            Debug.LogError(string.Format("{0} monsterGroup {1} member {2} id is repeated!!", configName, v1.id, mon1.id));
                        }
                        foreach (var mon2 in v1.memberInfos)
                        {
                            if (mon1 != mon2 && mon1.id == mon2.id)
                            {
                                Debug.LogError(string.Format("{0} monsterGroup {1} member {2} id is repeated!!", configName, v1.id, mon1.id));
                            }
                        }
                    }
                }

                foreach (var v1 in data.surveyedAreaDatas)
                {
                    foreach (var v2 in data.surveyedAreaDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} surveyedArea {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.TimmerDatas)
                {
                    foreach (var v2 in data.TimmerDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} timmer {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.towerDatas)
                {
                    foreach (var v2 in data.towerDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} tower {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.triggerDatas)
                {
                    foreach (var v2 in data.triggerDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} trigger {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }

                foreach (var v1 in data.valueDatas)
                {
                    foreach (var v2 in data.valueDatas)
                    {
                        if (v1 != v2 && v1.id == v2.id)
                        {
                            Debug.LogError(string.Format("{0} value {1} id is repeated!!", configName, v1.id));
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, MapEditorCons.HINT_CHECK_COMPLETE, MapEditorCons.HINT_OK);
        }
    }

    [MenuItem("MapEditor/生成关卡掉落json文件", false)]
    static void __MenuFuncMakeDropJsonFile()
    {
        List<StageDropInfo> m_dropInfos = new List<StageDropInfo>();
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Res/Resource/Datum/Map");
        if (dir.Exists)
        {
            FileSystemInfo[] files = dir.GetFileSystemInfos("*.bytes");
            for (int i = 0; i < files.Length; i++)
            {
                string configName = files[i].Name;

                EditorUtility.DisplayProgressBar("生成进度", "Making " + configName, (float)i / files.Length);

                string path = "Assets/Res/Resource/Datum/Map/" + configName;
                TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                MapEditConfigData.MapEditConfigData data = null;

                data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.MapEditConfigData>(txt);
                StageDropInfo info = new StageDropInfo();
                info.ID = configName;
                info.Drops = new List<DropInfo>();

                foreach (var group in data.monsterDatas)
                {
                    if (group.leaderInfo != null)
                    {
                        if (group.leaderInfo.dropId != 0)
                        {
                            DropInfo drop = new DropInfo();
                            drop.EventID = group.leaderInfo.id;
                            drop.DropID = group.leaderInfo.dropId;

                            info.Drops.Add(drop);
                        }
                    }

                    foreach (var mon in group.memberInfos)
                    {
                        if (mon.dropId != 0)
                        {
                            DropInfo drop = new DropInfo();
                            drop.EventID = mon.id;
                            drop.DropID = mon.dropId;

                            info.Drops.Add(drop);
                        }
                    }
                }

                foreach (var tower in data.towerDatas)
                {
                    if (tower.dropId != 0)
                    {
                        DropInfo drop = new DropInfo();
                        drop.EventID = tower.id;
                        drop.DropID = tower.dropId;

                        info.Drops.Add(drop);
                    }
                }

                m_dropInfos.Add(info);
            }

            EditorUtility.ClearProgressBar();

            string js = "";//SimpleJson.SimpleJson.SerializeObject(m_dropInfos);

            string jsonPath = Path.GetFullPath(Application.dataPath + "../../../server/app/datum");

            FileStream fs = new FileStream(jsonPath + "/StageDropData.json", FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);
            sw.Write(js);
            sw.Flush();
            sw.Close();
            fs.Close();

            EditorUtility.DisplayDialog(
                MapEditorCons.HINT_TITLE,
                MapEditorCons.HINT_OPT_SUCESS,
                MapEditorCons.HINT_OK);
        }
    }

	//[MenuItem("MapEditor/将星级描述从章节点表搬到关卡配置中", false)]
	static void __MenuFuncMoveStageDesToCfg()
	{
        DatasManager.Instance.LoadAllDatas(
            () =>
            {
                List<SectionData.SectionData> secs = DatasManager.Instance.GetDatas<SectionData.SectionData>();
                for (int i = 0; i < secs.Count; i++)
                {
                    SectionData.SectionData secdata = secs[i];

                    EditorUtility.DisplayProgressBar("填充进度", secdata.SectionName, (float)i / secs.Count);
                    
                    StageData.StageData sd = DatasManager.Instance.GetData<StageData.StageData>(secs[i].StageID);
                    if (null == sd)
                        continue;

                    string path = "Assets/Res/Resource/Datum/Map/" + sd.MapConfigDataPath + ".bytes";
                    TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (null == txt)
                        continue;

                    MapEditConfigData.MapEditConfigData data = null;

                    data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.MapEditConfigData>(txt);

                    MapEditConfigData.StageStarDesInfo des1 = new MapEditConfigData.StageStarDesInfo();
                    des1.des = secdata.FirstStarIntro;

                    MapEditConfigData.StageStarDesInfo des2 = new MapEditConfigData.StageStarDesInfo();
                    des2.des = secdata.SecondStarIntro;

                    MapEditConfigData.StageStarDesInfo des3 = new MapEditConfigData.StageStarDesInfo();
                    des3.des = secdata.ThirdStarIntro;

                    data.starDess.Add(des1);
                    data.starDess.Add(des2);
                    data.starDess.Add(des3);

                    MapEditorConfigParser.SaveProtoDataToBinaryReplace<MapEditConfigData.MapEditConfigData>(data, Application.dataPath + "/Res/Resource/Datum/Map/" + sd.MapConfigDataPath + ".bytes");
                }

                EditorUtility.ClearProgressBar();
            }
            );
	}

	public static void DoAddTrigger(MapEditConfigData.TriggerInfo info)
    {
        Transform triggerRoot = MapEditorUtlis.GetTriggerEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = info.id.ToString();
		}

		Transform gt = new GameObject(info.name).transform;
		gt.parent = triggerRoot;
		gt.localScale = Vector3.one;
		gt.localEulerAngles = Vector3.zero;
		gt.localPosition = Vector3.zero;

		TriggerEditData src = gt.gameObject.AddComponent<TriggerEditData>();
		src.Data = info;
		Selection.activeGameObject = gt.gameObject;
    }

	public static void DoAddSurveyedArea(MapEditConfigData.SurveyedAreaInfo info)
	{
        Transform SurveyAreaRoot = MapEditorUtlis.GetSurveyAreaRootEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = info.id.ToString();
		}

		GameObject obj = new GameObject(info.name);
		obj.transform.parent = SurveyAreaRoot;
		obj.transform.position = new Vector3( info.position.x , info.position.y , info.position.z );
		obj.transform.rotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;

		Selection.activeGameObject = obj;

		SurveyedAreaEditData src = obj.AddComponent<SurveyedAreaEditData>();
		src.Data = info;
	}

	public static void DoAddTimmer(MapEditConfigData.TimmerInfo info)
	{
        Transform timerRoot = MapEditorUtlis.GetTimmerEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = info.id.ToString();
		}

		GameObject obj = new GameObject(info.name);
		
		obj.transform.parent = timerRoot;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		obj.transform.localEulerAngles = Vector3.zero;

		Selection.activeGameObject = obj;

		TimmerEditData src = obj.AddComponent<TimmerEditData>();
		src.Data = info;
	}

    //public static void DoAddStarValue(MapEditConfigData.StarValueInfo info)
    //{
    //    Transform starRoot = MapEditorUtlis.GetValueEditRoot();

    //    GameObject starValueTrigger = new GameObject(info.name);
    //    starValueTrigger.transform.parent = starRoot;
    //    starValueTrigger.transform.localPosition = Vector3.zero;
    //    starValueTrigger.transform.localScale = Vector3.one;
    //    starValueTrigger.transform.rotation = Quaternion.identity;

    //    starValueTrigger.AddComponent<StarValueEditData>();

    //    starValueTrigger.GetComponent<StarValueEditData>().Data = info;
    //    Selection.activeGameObject = starValueTrigger;
    //}

    public static void DoAddValue(MapEditConfigData.ValueInfo info)
    {
        Transform valueRoot = MapEditorUtlis.GetValueEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = info.id.ToString();
		}

        GameObject obj = new GameObject(info.name);

        obj.transform.parent = valueRoot;
		obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;

		Selection.activeGameObject = obj;

        ValueEditData src = obj.AddComponent<ValueEditData>();
		src.Data = info;
    }

	public static void DoAddDoor(MapEditConfigData.DoorInfo info)
	{
        Transform doorRoot = MapEditorUtlis.GetDoorEditRoot();

		GameObject obj = new GameObject();
		
		Selection.activeGameObject = obj;
		
		DoorEditData src = obj.AddComponent<DoorEditData>();
		if(info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if(string.IsNullOrEmpty(info.name))
		{
			info.name = "Door_" + info.id;
		}

		obj.transform.parent = doorRoot;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		obj.transform.rotation = Quaternion.identity;

		src.Data = info;
	}

    public static void DoAddPoint(MapEditConfigData.PointInfo info)
    {
        Transform pointRoot = MapEditorUtlis.GetPointEditRoot();
		


        GameObject obj = new GameObject(info.name);
		PointEditData src = obj.AddComponent<PointEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if (string.IsNullOrEmpty(info.name))
		{
			info.name = "Point_" + info.id.ToString();
		}

		obj.name = info.name;
        obj.transform.parent = pointRoot;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;

		Selection.activeGameObject = obj;

        

        src.Data = info;
    }

    public static void DoAddAutoPoint(MapEditConfigData.PointInfo info)
    {
        Transform autoPointRoot = MapEditorUtlis.GetAutoPointEditRoot();
        GameObject obj = new GameObject(info.name);

		AutoPointEditData src = obj.AddComponent<AutoPointEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if (string.IsNullOrEmpty(info.name))
		{
			info.name = "AutoPoint_" + info.id.ToString();
		}
		obj.name = info.name;

        obj.transform.parent = autoPointRoot;
		obj.transform.localPosition = info.position.ToVector3();
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.name = info.name;

        Selection.activeGameObject = obj;

        src.Data = info;
    }

    public static void DoAddAutoPath(MapEditConfigData.PathInfo info)
    {
        Transform autoPathRoot = MapEditorUtlis.GetAutoPathEditRoot();

        if (autoPathRoot.GetComponentsInChildren<AutoPathEditData>().Length > 0)
        {
            EditorUtility.DisplayDialog("Error!", "场景中已有一条自动路径,请检查确认!", "OK");
            return;
        }
        GameObject obj = new GameObject(info.name);
		AutoPathEditData src = obj.AddComponent<AutoPathEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if (string.IsNullOrEmpty(info.name))
		{
			info.name = "AutoPath_" + info.id.ToString();
		}
		obj.name = info.name;

        obj.transform.parent = autoPathRoot;
		obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;
        Selection.activeGameObject = obj;

        src.Data = info;
    }

	public static void DoAddPath(MapEditConfigData.PathInfo info)
	{
        Transform pathRoot = MapEditorUtlis.GetPathEditRoot();

		GameObject obj = new GameObject(info.name);
		PathEditData src = obj.AddComponent<PathEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if (string.IsNullOrEmpty(info.name))
		{
			info.name = "Path_" + info.id.ToString();
		}
		obj.name = info.name;

		obj.transform.parent = pathRoot;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		obj.transform.localEulerAngles = Vector3.zero;
		Selection.activeGameObject = obj;
        
        src.Data = info;
	}

	public static void DoAddTower(MapEditConfigData.TowerPlaceInfo info)
	{
        Transform towerRoot = MapEditorUtlis.GetTowerEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = info.id.ToString();
		}
		
		GameObject go = new GameObject( info.name );
		go.transform.parent = towerRoot;
		go.transform.localScale = Vector3.one;
		go.transform.eulerAngles = new Vector3( info.rot.x , info.rot.y , info.rot.z );
		go.transform.position = new Vector3( info.pos.x , info.pos.y , info.pos.z );
		Selection.activeGameObject = go;
		TowerEditData src = go.AddComponent<TowerEditData>();
		
		src.name = info.name;
		src.id = info.id;
		src.type = info.type;
		src.camp = info.camp;
		src.resumeId = info.resumeId;
		src.dropId = info.dropId;
        src.bornBehaviourId = info.bornBehaviourId;
        src.allertDistance = info.allertDistance;
	}

    public static void DoAddBarracks(MapEditConfigData.TowerPlaceInfo info)
    {
        Transform barracksRoot = MapEditorUtlis.GetBarracksEditRoot();
        
        if(string.IsNullOrEmpty(info.name))
        {
            info.name = "预留兵营_" + info.id.ToString();
        }
        
        GameObject go = new GameObject( info.name );
        go.transform.parent = barracksRoot;
        go.transform.localScale = Vector3.one;
        go.transform.eulerAngles = new Vector3( info.rot.x , info.rot.y , info.rot.z );
        go.transform.position = new Vector3( info.pos.x , info.pos.y , info.pos.z );
        Selection.activeGameObject = go;
        BarracksEditData src = go.AddComponent<BarracksEditData>();
        
        src.name = info.name;
        src.id = info.id;
        src.type = info.type;
        src.camp = info.camp;
        src.resumeId = info.resumeId;
        src.dropId = info.dropId;
        src.bornBehaviourId = info.bornBehaviourId;
        src.allertDistance = info.allertDistance;
    }

	public static void DoAddMonsterGroup(MapEditConfigData.MonsterGroupInfo info)
	{
		if(info.leaderInfo == null && info.memberInfos.Count == 0)
			return;

        Transform monsterRoot = MapEditorUtlis.GetMonsterEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
			info.name = "Group" + info.id;
		}

		MonsterGroupEditData gpSrc = null;
		MonsterGroupEditData[] groups = monsterRoot.gameObject.GetComponentsInChildren<MonsterGroupEditData>();

		Transform groupRoot = null;
		foreach(var gp in groups)
		{
			if(gp.id == info.id)
			{
				groupRoot = gp.transform;
			}
		}

		if( null == groupRoot )
		{
			groupRoot = new GameObject( info.name ).transform;
			groupRoot.parent = monsterRoot;
			groupRoot.localScale = Vector3.one;
			groupRoot.localEulerAngles = Vector3.zero;
			groupRoot.localPosition = Vector3.zero;
			groupRoot.gameObject.AddComponent<MonsterGroupEditData>();
		}
		gpSrc = groupRoot.gameObject.GetComponent<MonsterGroupEditData>();
		gpSrc.id = info.id;
		gpSrc.name = info.name;

        bool setPos = false;
		if(null != info.leaderInfo && 0 != info.leaderInfo.id)
		{
            setPos = true;
            groupRoot.position = info.leaderInfo.pos.ToVector3();
			DoAddMonster(info.leaderInfo);
		}
		foreach(var monster in info.memberInfos)
		{
            if(!setPos)
            {
                setPos = true;
                groupRoot.position = monster.pos.ToVector3();
            }
			DoAddMonster(monster);
		}
	}

	public static void DoAddMonster(MapEditConfigData.MonsterPlaceInfo info)
    {
        Transform monsterRoot = MapEditorUtlis.GetMonsterEditRoot();

		Transform monsterGroup = null;

		MonsterGroupEditData[] groups = monsterRoot.gameObject.GetComponentsInChildren<MonsterGroupEditData>();

		foreach(var gp in groups)
		{
			if(gp.id == info.group)
			{
				monsterGroup = gp.transform;
			}
		}

		if( null == monsterGroup )
		{
			string groupName = "Group" + info.group;
            if(info.group <= 0xFF)
            {
                groupName = "预留怪物组_" + info.group;
            }

            monsterGroup = new GameObject(groupName).transform;
			monsterGroup.parent = monsterRoot;
			monsterGroup.localScale = Vector3.one;
			monsterGroup.localEulerAngles = Vector3.zero;
            monsterGroup.position = info.pos.ToVector3();

			MonsterGroupEditData gpSrc = monsterGroup.gameObject.AddComponent<MonsterGroupEditData>();
			gpSrc.id = info.group;
			gpSrc.name = groupName;
		}
		
		GameObject go = new GameObject( info.name );
		MonsterEditData src = go.AddComponent<MonsterEditData>();
		
		if(info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if(string.IsNullOrEmpty(info.name))
		{
			info.name = "Monster_" + info.id.ToString();
		}

        go.transform.parent = monsterGroup;
        go.transform.localScale = Vector3.one;
		go.transform.eulerAngles = new Vector3( info.rot.x , info.rot.y , info.rot.z );
        go.transform.position = new Vector3( info.pos.x , info.pos.y , info.pos.z );
		Selection.activeGameObject = go;

		src.name = info.name;
		src.id = info.id;
        src.type = info.type;
        src.group = info.group;
        src.protection = info.protection;
		src.camp = info.camp;
		src.resumeId = info.resumeId;
		src.isMaster = info.isMaster;
		src.keepOnGround = info.keepOnGround;

		if(null != info.relativePos)
			src.relativePos = new Vector3(info.relativePos.x, info.relativePos.y, info.relativePos.z);
		else
			src.relativePos = Vector3.zero;

		src.dropId = info.dropId;
        src.bornBehaviorId = info.bornBehaviorId;
        src.allertDistance = info.allertDistance;
    }

    public static void DoAddClientCharacter(MapEditConfigData.ClientCharacterPlaceInfo info)
    {
        IEditEventSender[] groups = MapEditorUtlis.GetMonsterEditRoot().GetComponentsInChildren<MonsterGroupEditData>();
        foreach (var gp in groups)
        {
            if (info.group == gp.id)
            {
                UnityEditor.EditorUtility.DisplayDialog("提示", "客户端角色怪物组冲突，请检查！", "确认");
            }
        }

        Transform clientCharacterRoot = MapEditorUtlis.GetClientCharacterEditRoot();
        IEditEventSender[] clientGroups = clientCharacterRoot.GetComponentsInChildren<MonsterGroupEditData>();
        Transform groupRoot = null;
        foreach (var gp in clientGroups)
        {
            if (info.group == gp.id)
            {
                groupRoot = gp.transform;
            }
        }
        if(groupRoot == null)
        {
            groupRoot = new GameObject("Group_" + info.group).transform;
            MonsterGroupEditData gpSrc = groupRoot.gameObject.AddComponent<MonsterGroupEditData>();
            gpSrc.id = info.group;

            groupRoot.parent = clientCharacterRoot;
            groupRoot.localScale = Vector3.one;
            groupRoot.eulerAngles = info.rot.ToVector3();
            groupRoot.position = info.pos.ToVector3();
        }

        GameObject go = new GameObject();
		ClientCharacterEditData src = go.AddComponent<ClientCharacterEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		if (string.IsNullOrEmpty(info.name))
		{
			info.name = "ClientCharacter_" + info.id.ToString();
		}
        src.name = info.name;
		src.id = info.id;

        go.transform.parent = groupRoot;
        go.transform.localScale = Vector3.one;
        go.transform.eulerAngles = info.rot.ToVector3();
        go.transform.position = info.pos.ToVector3();
        Selection.activeGameObject = go;

        src.group = info.group;
        src.protection = info.protection;
        src.camp = info.camp;
        src.star = info.star;
        src.level = info.level;
        src.battleInfoId = info.battleInfoId;
        src.battleAttrId = info.battleAttrId;
        src.battleAttrId = info.battleAttrId;

        src.country = info.country;
        src.isMaster = info.isMaster;

        src.skillLevel = new int[info.skillLevel.Count];
        for(int i = 0 ; i < info.skillLevel.Count; i++)
        {
            src.skillLevel[i] = info.skillLevel[i];
        }
        
        if(null != info.relativePos)
            src.relativePos = new Vector3(info.relativePos.x, info.relativePos.y, info.relativePos.z);
        else
            src.relativePos = Vector3.zero;

        src.bornBehaviorId = info.bornBehaviorId;
        src.allertDistance = info.allertDistance;
    }

    public static void DoAddCameraPath(CameraPathDataPkg.CameraPathData info, GameObject go, bool saveOrLoad = false)
	{
        Transform cameraPathRoot = MapEditorUtlis.GetCameraPathEditRoot();

		if(string.IsNullOrEmpty(info.name))
		{
            info.name = "cameraPath";
		}

        if (go == null)
        {
			go = new GameObject(info.name);
            //go.AddComponent<CameraPath>();
            //go.AddComponent<CameraPathAnimator>();
        }

		CinemaEditData src = go.AddComponent<CinemaEditData>();
		if (info.id == 0)
		{
			info.id = MapEditorUtlis.GenGID(src);
		}
		
		go.transform.transform.parent = cameraPathRoot;
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		Selection.activeGameObject = go;

        if (saveOrLoad)
        {
            src.SaveData(info);
        }
        else
        {
            src.LoadData(info);
        }
	}

    public static void DoAddWorldPoint(object info)
    {
        //Transform wpRoot = MapEditorUtlis.GetWorldPointRoot();

        //if (string.IsNullOrEmpty(info.name))
        //{
        //    info.name = info.id.ToString();
        //}

        //GameObject go = new GameObject(info.name);
        //go.transform.parent = wpRoot;
        //go.transform.localScale = Vector3.one;
        //go.transform.eulerAngles = new Vector3(info.rot.x, info.rot.y, info.rot.z);
        //go.transform.position = new Vector3(info.pos.x, info.pos.y, info.pos.z);
        //Selection.activeGameObject = go;
        //WorldPointEditData src = go.AddComponent<WorldPointEditData>();

        //src.name = info.name;
        //src.id = info.id;
        //src.type = info.type;
        //src.camp = info.camp;
    }

    static string loadedDataName = "";
    static void __DoLoadMapData(string filePath)
    {
        Debug.Log("log root file:" + filePath);
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("Error", "未找到该文件:\n" + filePath, "OK");
            return;
        }

        string path = filePath.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "Assets");
        loadedDataName = Path.GetFileNameWithoutExtension(filePath);

        TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

        MapEditConfigData.MapEditConfigData data = null;
        try
        {
            data = MapEditorConfigParser.ParseDataFromStream<MapEditConfigData.MapEditConfigData>(txt);
        }
        catch
        {
            data = MapEditorConfigParser.ParseDataFromStream(txt);
        }
        finally
        {
            if (data != null)
            {
                __PlaceData(data);
                EditorUtility.DisplayDialog("", "加载数据成功", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("", "加载数据失败", "OK");
            }
        }
    }

    static void __PlaceData(MapEditConfigData.MapEditConfigData data)
    {
        Transform orgRoot = MapEditorUtlis.GetEditRoot();
        if(null != orgRoot)
        {
            GameObject.DestroyImmediate(orgRoot.gameObject);
        }

        Transform cameraRoot = MapEditorUtlis.GetCameraEditRoot();
        Transform targetRoot = MapEditorUtlis.GetCameraTargetRoot();

		for(int i = 0 ; i < data.CamInfo.Count; i++)
		{
			MapEditConfigData.CameraFollowInfo info = data.CamInfo[i];
			if(null == info)
				continue;

			Camera cam = new GameObject("Camera" + info.id).AddComponent<Camera>();
            cam.enabled = false;
	        cam.transform.parent = cameraRoot;
	        cam.transform.localPosition = Vector3.zero;
	        cam.transform.localEulerAngles = Vector3.zero;
	        cam.transform.localScale = Vector3.one;

            cam.fieldOfView = info.fov;


   //         CameraEditData camEdit = cam.gameObject.AddMissingComponent<CameraEditData>();
   //         camEdit.id = info.id;

	  //      CameraFollow followSrc = cam.gameObject.AddMissingComponent<CameraFollow>();
   //         followSrc.height = info.height;
			//followSrc.distance = info.distance;
			//followSrc.rotationY = info.rotationY;
   //         followSrc.autoHeight = info.autoHeight;
   //         followSrc.dragFov = info.dragFov;
   //         followSrc.fadeTime = info.fadeTime;
   //         followSrc.target = new GameObject("Camera" + info.id + "_Target").transform;
   //         followSrc.target.parent = targetRoot;
   //         followSrc.target.position = info.targetPos.ToVector3();
   //         followSrc.applyTargetPos = info.applyTargetPos;
   //         followSrc.shiftSpeedFactor = info.shiftSpeedFactor;
		}

        StageStarDesEditData starDes = __MenuFuncStageStarDesAdd();
        starDes.SetData(data.starDess);

        MoveDirectionGuidEditData moveGuid = __MenuFuncMoveDirectionAdd();
        moveGuid.Data = data.directionGuidDatas;

        MapEditor.__MenuFuncTriggerManAdd();

        PlayerEditData player = MapEditor.__MenuFuncPlayerAdd();
        if (data.PlayerData != null)
        {
            player.transform.position = new Vector3(data.PlayerData.pos.x, data.PlayerData.pos.y, data.PlayerData.pos.z);
            player.transform.eulerAngles = new Vector3(data.PlayerData.rot.x, data.PlayerData.rot.y, data.PlayerData.rot.z);
            player.camp = data.PlayerData.camp;
            player.allertDistance = data.PlayerData.allertDistance;
        }

        float taskTotal =
            data.monsterDatas.Count +
            data.TimmerDatas.Count +
            data.starValues.Count +
            data.surveyedAreaDatas.Count +
            data.valueDatas.Count +
            data.pathDatas.Count +
            data.pointDatas.Count +
            data.doors.Count +
            data.towerDatas.Count + 3 +
            data.cinemaDatas.Count +
            data.triggerDatas.Count +
            data.clientCharacters.Count;
            //data.worldPointDatas.Count;


        float stepLeft = taskTotal;

        for (int i = 0; i < data.monsterDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载怪物群...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddMonsterGroup(data.monsterDatas[i]);
        }

        for (int i = 0; i < data.TimmerDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载计时器...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddTimmer(data.TimmerDatas[i]);
        }

        for (int i = 0; i < data.surveyedAreaDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载区域检测器...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddSurveyedArea(data.surveyedAreaDatas[i]);
        }

        for (int i = 0; i < data.valueDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载关卡变量...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddValue(data.valueDatas[i]);
        }

        #region
        List<MapEditConfigData.PointInfo> allPoints = new List<MapEditConfigData.PointInfo>();
        List<MapEditConfigData.PointInfo> autoPoints = new List<MapEditConfigData.PointInfo>();
        List<MapEditConfigData.PointInfo> points = new List<MapEditConfigData.PointInfo>();
        allPoints.AddRange(data.pointDatas);

        foreach (var path in data.pathDatas)
        {
            if (path.isAutoPathFindingPath)
            {
                autoPoints.AddRange(path.points);
            }
            else
            {
                points.AddRange(path.points);
            }
        }

        List<MapEditConfigData.PointInfo> _autoPoints = new List<MapEditConfigData.PointInfo>();
        List<MapEditConfigData.PointInfo> _points = new List<MapEditConfigData.PointInfo>();

        foreach (var shit in autoPoints)
        {
            if (_autoPoints.Find((fuck) => { return fuck.id == shit.id; }) == null)//剔除重复
            {
                _autoPoints.Add(shit);
            }
        }
        foreach (var shit in points)
        {
            if (null != _points.Find((fuck) => { return shit.id == fuck.id; })) //剔除重复
            {
                _points.Add(shit);
            }
        }



        foreach (var ap in _autoPoints)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载自动路径点...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddAutoPoint(ap);
            allPoints.RemoveAll(
                (point) =>
                {
                    return point.id == ap.id;
                }
                );
        }
        foreach (var p in _points)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载路径点...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddPoint(p);
            allPoints.RemoveAll(
                (point) =>
                {
                    return point.id == p.id;
                }
                );
        }

        foreach (var point in allPoints)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载路径点...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddPoint(point);    //可能这里会把上次编辑过之后但是并未被引用的自动寻路的路径点改变为普通路径点.
        }

        foreach (var path in data.pathDatas)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载路径...", 1 - stepLeft / taskTotal);
            if (path.isAutoPathFindingPath)
            {
                MapEditor.DoAddAutoPath(path);
            }
            else
            {
                MapEditor.DoAddPath(path);
            }
        }
        #endregion

        for (int i = 0; i < data.doors.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载机关门...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddDoor(data.doors[i]);
        }

        for (int i = 0; i < data.towerDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载防御塔...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddTower(data.towerDatas[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载Servant...", 1 - stepLeft / taskTotal);
            MapEditor.__MenuFuncServantAdd();
        }
        //for (int i = 0; i < data.starValues.Count; i++)
        //{
        //    stepLeft--;
        //    EditorUtility.DisplayProgressBar("加载进度", "加载星星变量...", 1 - stepLeft / taskTotal);
        //    MapEditor.DoAddStarValue(data.starValues[i]);
        //}
        for (int i = 0; i < data.triggerDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载触发器...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddTrigger(data.triggerDatas[i]);
        }
        
        for (int i = 0; i < data.clientCharacters.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载触发器...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddClientCharacter(data.clientCharacters[i]);
        }

        //for (int i = 0; i < data.worldPointDatas.Count; i++)
        //{
        //    stepLeft--;
        //    EditorUtility.DisplayProgressBar("加载进度", "加载世界路点...", 1 - stepLeft / taskTotal);
        //    MapEditor.DoAddWorldPoint(data.worldPointDatas[i]);
        //}

        //必须放在最后加载
        for (int i = 0; i < data.cinemaDatas.Count; i++)
        {
            stepLeft--;
            EditorUtility.DisplayProgressBar("加载进度", "加载剧本...", 1 - stepLeft / taskTotal);
            MapEditor.DoAddCameraPath(data.cinemaDatas[i], null);
            Debug.Log("加载剧本 " + data.cinemaDatas[i].id);
        }
        EditorUtility.ClearProgressBar();
    }
    static void __DoSaveCfg(string filepath)
    {
        GameObject root = MapEditorUtlis.FindEditRoot();
        if(null == root)
        {
            EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "未找到地图根节点", MapEditorCons.HINT_OK);
            return;
        }

  //      Transform cameraRoot = MapEditorUtlis.GetCameraEditRoot();
  //      CameraFollow[] followSrcs = cameraRoot.GetComponentsInChildren<CameraFollow>();
		//if (null == followSrcs || followSrcs.Length == 0)
  //      {
  //          EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "未找到摄像机数据", MapEditorCons.HINT_OK);
  //          return;
  //      }

		MapEditConfigData.MapEditConfigData data = new MapEditConfigData.MapEditConfigData();

		//for(int i = 0 ; i < followSrcs.Length; i++)
		//{
  //          MapEditConfigData.CameraFollowInfo info = MapEditorUtlis.GetCameraFollowInfo(followSrcs[i]);

		//	data.CamInfo.Add(info);
		//}

        Transform playerRoot = MapEditorUtlis.GetPlayerEditRoot();
        PlayerEditData player = playerRoot.GetComponentInChildren<PlayerEditData>();
        if (null == player)
        {
            EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "未找到Player", MapEditorCons.HINT_OK);
            return;
        }

        #region ID Conflict Checker
        bool conflict = false;
        IEditEventSender[] iees = root.GetComponentsInChildren<IEditEventSender>();
        for (int i = 0; i < iees.Length; i++)
        {
            EditorUtility.DisplayProgressBar("冲突检测", "正在检测 " + iees[i].gameObject.name, i * 1.0f / iees.Length);

            if (iees[i].ToTargetType() == TriggerTargetType.StageScript || iees[i] is TriggerMan)
                continue;

            for (int j = i + 1; j < iees.Length; j++)
            {
                if (iees[j].ToTargetType() == TriggerTargetType.StageScript
				    || iees[j].ToTargetType() == TriggerTargetType.MonsterGroup
				    || iees[j].ToTargetType() == TriggerTargetType.CameraEdit
				    || iees[j] is TriggerMan)
                    continue;

                if (iees[i].id == iees[j].id)
                {
                    //TOTO 肯定出问题了
                    Debug.LogError(string.Format("场景中有相同ID:[ {0} ]冲突!!!请检测: " + iees[i].gameObject.name + "  和  " + iees[j].gameObject.name, iees[i].id));
                    Debug.LogWarning("查看" + iees[i].id + "冲突请点我!", iees[i].gameObject);
                    Debug.LogWarning("查看" + iees[i].id + "冲突请点我!", iees[j].gameObject);
                    conflict = true;
                }
            }
        }
        EditorUtility.ClearProgressBar();
        if (conflict)
        {
            Debug.Log("ID错误打印完毕!地图数据保存失败!!");
            EditorApplication.Beep();
            EditorUtility.DisplayDialog("Warning!", "检测到ID冲突!保存失败!详情请看Console控制台输出!!", "确定", "取消");
            return;
        }
        #endregion

        data.PlayerData = new MapEditConfigData.PlayerPlaceInfo();
        data.PlayerData.pos = player.transform.position.ToMapEditConfigDataVector3();
        data.PlayerData.rot = player.transform.eulerAngles.ToMapEditConfigDataVector3();
        data.PlayerData.camp = player.camp;
        data.PlayerData.allertDistance = player.allertDistance;

        Transform monsterRoot = MapEditorUtlis.GetMonsterEditRoot();
        MonsterGroupEditData[] editdatas = monsterRoot.GetComponentsInChildren<MonsterGroupEditData>();
        for (int i = 0; i < editdatas.Length; i++)
        {
            data.monsterDatas.Add(editdatas[i].Data);
        }

        Transform surveyAreaRoot = MapEditorUtlis.GetSurveyAreaRootEditRoot();
        SurveyedAreaEditData[] surveyedAreaDatas = surveyAreaRoot.GetComponentsInChildren<SurveyedAreaEditData>();
        for (int i = 0; i < surveyedAreaDatas.Length; i++)
        {
            data.surveyedAreaDatas.Add(surveyedAreaDatas[i].Data);
        }

        Transform timmerRoot = MapEditorUtlis.GetTimmerEditRoot();
        TimmerEditData[] timmerDatas = timmerRoot.GetComponentsInChildren<TimmerEditData>();
        for (int i = 0; i < timmerDatas.Length; i++)
        {
            data.TimmerDatas.Add(timmerDatas[i].Data);
        }

        #region 触发器检测
        TriggerEditData[] triggers = root.GetComponentsInChildren<TriggerEditData>();

        bool triggerError = false;

        string errorMsg = "";
        for (int i = 0; i < triggers.Length; i++)
        {
            EditorUtility.DisplayProgressBar("检测触发器", "检测[" + triggers[i].gameObject.name + "]中...", i * 1.0f / triggers.Length);

            if (__VerifyTriggerData(triggers[i], ref errorMsg))
            {
                triggerError = true;
            }

            data.triggerDatas.Add(triggers[i].Data);
        }

        EditorUtility.ClearProgressBar();

        if (triggerError)
        {
            EditorUtility.DisplayDialog("Error", errorMsg, "确定");
            return;
        }
        #endregion
        Transform valueRoot = MapEditorUtlis.GetValueEditRoot();
        ValueEditData[] values = valueRoot.GetComponentsInChildren<ValueEditData>();
        for (int i = 0; i < values.Length; i++)
        {
            data.valueDatas.Add(values[i].Data);
        }

        Transform pointRoot = MapEditorUtlis.GetPointEditRoot();
        PointEditData[] points = pointRoot.GetComponentsInChildren<PointEditData>();
        for (int i = 0; i < points.Length; i++)
        {
            data.pointDatas.Add(points[i].Data);
        }

        Transform pathRoot = MapEditorUtlis.GetPathEditRoot();
        PathEditData[] path = pathRoot.GetComponentsInChildren<PathEditData>();
        for (int i = 0; i < path.Length; i++)
        {
            data.pathDatas.Add(path[i].Data);
            if (path[i].Data.isAutoPathFindingPath)
            {
                data.autoPathFindingPathId = path[i].Data.id;
            }
        }

        Transform autoPointRoot = MapEditorUtlis.GetAutoPointEditRoot();
        AutoPointEditData[] autoPoints = autoPointRoot.GetComponentsInChildren<AutoPointEditData>();
        for (int i = 0; i < autoPoints.Length; i++)
        {
            data.pointDatas.Add(autoPoints[i].Data);
        }

        Transform autoPathRoot = MapEditorUtlis.GetAutoPathEditRoot();
        AutoPathEditData[] autoPath = autoPathRoot.GetComponentsInChildren<AutoPathEditData>();
        if (autoPath != null && autoPath.Length > 1)
        {
            EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "每个场景中,自动寻路路径最多只能有一条!请删除多余的自动路径.", MapEditorCons.HINT_OK);
            return;
        }
        for (int i = 0; i < autoPath.Length; i++)
        {
            data.pathDatas.Add(autoPath[i].Data);
            data.autoPathFindingPathId = autoPath[i].Data.id;
        }

        Transform towerRoot = MapEditorUtlis.GetTowerEditRoot();
        TowerEditData[] towers = towerRoot.GetComponentsInChildren<TowerEditData>();
        for (int i = 0; i < towers.Length; i++)
        {
            data.towerDatas.Add(towers[i].Data);
        }

        Transform cinemaRoot = MapEditorUtlis.GetCameraPathEditRoot();
        CinemaEditData[] cinemas = cinemaRoot.GetComponentsInChildren<CinemaEditData>();
        for (int i = 0; i < cinemas.Length; i++)
        {
            Debug.Log("保存剧本B__ " + cinemas[i].Data.id);
            data.cinemaDatas.Add(cinemas[i].Data);
            Debug.Log("保存剧本E__ " + data.cinemaDatas[i].id);
        }

        Transform doorRoot = MapEditorUtlis.GetDoorEditRoot();
        DoorEditData[] doors = doorRoot.GetComponentsInChildren<DoorEditData>();
        for (int i = 0; i < doors.Length; i++)
        {
            data.doors.Add(doors[i].Data);
        }

        Transform clientCharacterRoot = MapEditorUtlis.GetClientCharacterEditRoot();
        ClientCharacterEditData[] ccs = clientCharacterRoot.GetComponentsInChildren<ClientCharacterEditData>();
        for (int i = 0; i < ccs.Length; i++)
        {
            data.clientCharacters.Add(ccs[i].Data);
        }

        StageStarDesEditData ssde = root.GetComponentInChildren<StageStarDesEditData>();
		if(null != ssde)
		{
        	data.starDess.AddRange(ssde.GetData());
		}

        MoveDirectionGuidEditData mdge = root.GetComponentInChildren<MoveDirectionGuidEditData>();
        if (null != mdge)
        {
            if (__VerifyMoveDirectionData(mdge.Data))
                return;

            data.directionGuidDatas.AddRange(mdge.Data);
        }

        //Transform worldPointRoot = MapEditorUtlis.GetWorldPointRoot();
        //WorldPointEditData[] wps = worldPointRoot.GetComponentsInChildren<WorldPointEditData>();
        //for (int i = 0; i < wps.Length; i++)
        //{
        //    data.worldPointDatas.Add(wps[i].Data);
        //}

        __VerifyHandlerParamNumber(ref data);

        if (!string.IsNullOrEmpty(filepath))
        {
            MapEditorConfigParser.SaveProtoDataToBinary(data, filepath);

            EditorUtility.DisplayDialog(MapEditorCons.HINT_TITLE, "将地图信息写入文件：" + filepath + "成功", MapEditorCons.HINT_OK);
        }
    }
    
    static bool __VerifyTriggerData(TriggerEditData data, ref string errorMsg)
    {
        bool isErrorOccoured = false;
        bool isEventStageInit = data.Data.nodes.root.eventInfo.eventID == 101;
        for (int i = 0; i < data.Data.nodes.actions.Count; i++)
        {
            MapEditConfigData.TriggerHandlerCellInfo cell = data.Data.nodes.actions[i].actionInfo;
            if (cell == null)
                continue;

            StageEventHandlerData.StageEventHandlerData hd = DatasManager.Instance.GetData<StageEventHandlerData.StageEventHandlerData>(cell.handlerID);
            if(hd == null)
            {
                errorMsg += string.Format("触发器:{0} ActionID:{1} 未知的关卡ACTION ID！\n", data.ID, cell.handlerID);
                isErrorOccoured = true;
                continue;
            }

            if (hd.ForbidCalledAtInit && isEventStageInit)
            {
                errorMsg += string.Format("触发器:{0} Action:{1} 在关卡初始化时被调用，这是被禁止的！\n", data.ID, hd.Des);
                isErrorOccoured = true;
                continue;
            }

            for (int j = 1; j <= 6; j++)
            {
                System.Reflection.PropertyInfo srcTypePro = hd.GetType().GetProperty(string.Format("Param{0}SourceType", j));
                if (null == srcTypePro)
                {
                    break;
                }
                System.Reflection.PropertyInfo selTypePro = hd.GetType().GetProperty(string.Format("Param{0}SelectObjectType", j));
                if (null == selTypePro)
                {
                    break;
                }

                ParamSourceType srcType = (ParamSourceType)(int)srcTypePro.GetValue(hd, null);
                List<int> selTypes = (List<int>)selTypePro.GetValue(hd, null);
                if (srcType != ParamSourceType.None)
                {
                    if (cell.handlerParams.Count >= j)
                    {
                        isErrorOccoured = __VerifyHandlerParamValue(data, cell.handlerParams[j - 1], srcType, selTypes) || isErrorOccoured;
                    }
                }
            }
        }

        return isErrorOccoured;
    }
    static bool __VerifyMoveDirectionData(List<MapEditConfigData.MoveDirectionGuidData> data)
    {
        List<int> groups = new List<int>();
        for(int i = 0 ; i < data.Count; i++)
        {
            if(data[i].monsterGroups.Count == 0)
            {
                EditorUtility.DisplayDialog("关卡路径引导",
                    string.Format("第{0}条引导，怪物组个数为0，请检查!", i+1),
                    "确认");

                return true;
            }

            foreach(var gp in data[i].monsterGroups)
            {
                if(gp.id == 0 || gp.type == 0)
                {
                    EditorUtility.DisplayDialog("关卡路径引导",
                        string.Format("第{0}条引导，怪物组为空，请检查!", i + 1),
                        "确认");

                    return true;
                }

                if(groups.Contains(gp.id))
                {
                    EditorUtility.DisplayDialog("关卡路径引导",
                        string.Format("第{0}条引导，怪物组重复，请检查!", i + 1),
                        "确认");

                    return true;
                }
                else
                {
                    groups.Add(gp.id);
                }
            }
        }

        return false;
    }
    static void __VerifyHandlerParamNumber(ref MapEditConfigData.MapEditConfigData data)
    {
        foreach (var tr in data.triggerDatas)
        {
            foreach (var t in tr.nodes.actions)
            {
                MapEditConfigData.TriggerHandlerCellInfo ap = t.actionInfo;
                if (null == ap)
                {
                    continue;
                }

                StageEventHandlerData.StageEventHandlerData handlerData = DatasManager.Instance.GetData<StageEventHandlerData.StageEventHandlerData>(ap.handlerID);

                int paramCount = 0;
                for (int i = 0; i < 6; i++)
                {
                    PropertyInfo paramSourceType = handlerData.GetType().GetProperty(string.Format("Param{0}SourceType", i + 1));
                    if (null == paramSourceType)
                    {
                        break;
                    }
                    ParamSourceType paramSourceTypeValue = (ParamSourceType)paramSourceType.GetValue(handlerData, null);

                    if (paramSourceTypeValue == ParamSourceType.None)
                    {
                        break;
                    }

                    paramCount = i + 1;
                }

                if (t.actionInfo.handlerParams.Count > paramCount)
                {
                    while (t.actionInfo.handlerParams.Count > paramCount)
                    {
                        t.actionInfo.handlerParams.RemoveAt(t.actionInfo.handlerParams.Count - 1);
                    }
                }
                else if (t.actionInfo.handlerParams.Count < paramCount)
                {
                    Debug.LogError(tr.id + " Node: " + t.node.id + " 参数个数不足，请检查");
                }
            }
        }
    }
    static bool __VerifyHandlerParamValue(TriggerEditData data, MapEditConfigData.TriggerHandlerParamInfo param, ParamSourceType srcType, List<int> selectSourceType)
    {
        if (srcType == ParamSourceType.ReadExcel)
        {
            Debug.LogError("句柄参数不支持从excel读取!", data);
            return true;
        }
        else if (srcType == ParamSourceType.UserSelect)
        {
            if (param.handlerTarget.targetObj == null)
            {
                Transform root = MapEditorUtlis.GetEditRoot();
                List<IEditEventSender> srcs = new List<IEditEventSender>(root.GetComponentsInChildren<IEditEventSender>());

                for (int i = 0; i < srcs.Count; i++)
                {
                    if (srcs[i].id == param.handlerTarget.id
                        && (int)srcs[i].ToTargetType() == param.handlerTarget.type
                        && selectSourceType != null
                        && selectSourceType.Contains((int)srcs[i].ToTargetType()))
                    {
                        param.handlerTarget.targetObj = srcs[i];
                        return false;
                    }
                }
                Debug.LogError("拖选物体不能为空!", data);
                return true;
            }
        }
        else if (srcType == ParamSourceType.UserInputInt)
        {
            if (param.handlerIntParam == 0)
            {
                Debug.LogWarning("数值为0,请检测!", data);
                return false;
            }
        }
        else if (srcType == ParamSourceType.UserInputFloat)
        {
            if (System.Math.Abs(param.handlerFloatParam - 0) < Mathf.Epsilon)
            {
                Debug.LogWarning("数值为0,请检测!", data);
                return false;
            }
        }
        else if (srcType == ParamSourceType.UserInputString)
        {
            if (null == param.stringParam)
            {
                Debug.LogError("字符串参数不可为 null!", data);
                return true;
            }
        }

        return false;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayerDefine
{
    //场景地面
    public static string BackGroundLayerName = "background"; 
    //ngui
    public static string NGUILayerName = "NGUI Layer"; 
    //世界地图块
    public static string MapLayerName = "influence";
    //场景阻挡物
    public static string BlockLayerName = "block";
    //实体
    public static string EntityLayerName = "Entity";
    //
    public static string RendTextureLayerName = "RenderTexture";
    //建筑
    public static string BuildingLayerName = "Building";

    public static string MapColliderName = "mapCollider";

    public static string DefaultLayerName = "Default";

    public static string LockLayerName = "Lock Layer";

	public static string GPULoaderLayerName = "GPULoaderLayer";

    public static string DrawPrefabLayerName = "DrawPrefabLayer";

    public static string GuideLayerName = "GuideLayer";

    public static string T4MLayerName = "T4M";

    public static int BackGroundLayer = LayerMask.NameToLayer( BackGroundLayerName );
    
    public static int MapLayer = LayerMask.NameToLayer( MapLayerName );
    public static int BlockLayer = LayerMask.NameToLayer( BlockLayerName );
    public static int EntityLayer = LayerMask.NameToLayer( EntityLayerName );
    public static int BuildingLayer = LayerMask.NameToLayer(BuildingLayerName);
    public static int MapCollider = LayerMask.NameToLayer(MapColliderName);
    public static int DefaultLayer = LayerMask.NameToLayer(DefaultLayerName);

    public static int NGUILayer = LayerMask.NameToLayer(NGUILayerName);
    public static int RenderTextureLayer = LayerMask.NameToLayer(RendTextureLayerName);
    public static int LockLayer = LayerMask.NameToLayer(LockLayerName);
	public static int GPULoaderLayer = LayerMask.NameToLayer(GPULoaderLayerName);
    public static int DrawPrefabLayer = LayerMask.NameToLayer(DrawPrefabLayerName);
    public static int GuideLayer = LayerMask.NameToLayer(GuideLayerName);
    public static int T4MLayer = LayerMask.NameToLayer(T4MLayerName);

    public static int MainCameraLayer
    {
        get
        {
            LayerMask mask = 1 << MapLayer;
            mask |= 1 << BlockLayer;
            mask |= 1 << EntityLayer;
            mask |= 1 << BuildingLayer;
            mask |= 1 << MapCollider;
            mask |= 1 << DefaultLayer;
            mask |= 1 << T4MLayer;
            return mask;
        }
    }
}

public class SortingLayerDefine
{
	public static string SORTING_LAYER_BACKGROUND = "BACKGROUND";
	public static string SORTING_LAYER_MIDROUND = "MIDDLE";
	public static string SORTING_LAYER_FOREGROUND = "FOREGROUND";
	public static string SORTING_LAYER_SHADE = "SHADE";
}

public class ExyernalFolderPath
{
#if UNITY_EDITOR
	public static string Path 
    {
        get { return string.Format("{0}{1}", Application.dataPath, "/../Replay"); }
    }
#else
    public static string Path 
    {
        get {return string.Format("{0}{1}", Application.persistentDataPath, "/Replay");}
    }
#endif
    public const string ReplayFileName = "LastReplay.arpg";
    public const string TestReplay = "TestReplay.arpg";

    public static string GetReplayFilePath()
    {
        return string.Format("{0}/{1}", Path, ReplayFileName);
    }

    public static string GetTestReplayFilePath()
    {
        return string.Format("{0}/{1}", Path, TestReplay);
    }
}

public enum SamplePolyAreas
{
	SAMPLE_POLYAREA_GROUND,
	SAMPLE_POLYAREA_WATER,
	SAMPLE_POLYAREA_ROAD,
	SAMPLE_POLYAREA_DOOR,
	SAMPLE_POLYAREA_GRASS,
	SAMPLE_POLYAREA_JUMP,
	
	SAMPLE_POLYAREA_CUSTOM_BEGIN,
	SAMPLE_POLYAREA_CUSTOM0,
	SAMPLE_POLYAREA_CUSTOM1,
	SAMPLE_POLYAREA_CUSTOM2,
	SAMPLE_POLYAREA_CUSTOM3,
	SAMPLE_POLYAREA_CUSTOM4,
	SAMPLE_POLYAREA_CUSTOM5,
	SAMPLE_POLYAREA_CUSTOM6,
	SAMPLE_POLYAREA_CUSTOM7,
	SAMPLE_POLYAREA_CUSTOM8,
	SAMPLE_POLYAREA_CUSTOM9,
	SAMPLE_POLYAREA_CUSTOM_END,
};
public enum SamplePolyFlags
{
	SAMPLE_POLYFLAGS_WALK = 0x01,        // Ability to walk (ground, grass, road)
	SAMPLE_POLYFLAGS_SWIM = 0x02,        // Ability to swim (water).
	SAMPLE_POLYFLAGS_DOOR = 0x04,        // Ability to move through doors.
	SAMPLE_POLYFLAGS_JUMP = 0x08,        // Ability to jump.
	SAMPLE_POLYFLAGS_DISABLED = 0x10,        // Disabled polygon
	
	SAMPLE_POLYFLAGS_CUSTOM0 = 0x20,
	SAMPLE_POLYFLAGS_CUSTOM1 = 0x40,
	SAMPLE_POLYFLAGS_CUSTOM2 = 0x80,
	SAMPLE_POLYFLAGS_CUSTOM3 = 0x100,
	SAMPLE_POLYFLAGS_CUSTOM4 = 0x200,
	SAMPLE_POLYFLAGS_CUSTOM5 = 0x400,
	SAMPLE_POLYFLAGS_CUSTOM6 = 0x800,
	SAMPLE_POLYFLAGS_CUSTOM7 = 0x1000,
	SAMPLE_POLYFLAGS_CUSTOM8 = 0x2000,
	SAMPLE_POLYFLAGS_CUSTOM9 = 0x4000,
	
	SAMPLE_POLYFLAGS_ALL = 0xffff    // All abilities.
};

public enum PVP_STAGE_USER_CAMP
{
	PVP_CAMP_ATTACKER = 0,
	PVP_CAMP_DIFFENDER = 1,
};

///阵营
public enum GroupType
{
    GT_Green = 0,//lv
    GT_Red = 1,//hong
    GT_Yellow = 2,//hong
};

//敌方友方
public enum E_ALLY_TYPE
{
    None = -1,
    Friend = 0,
    Enemy = 1,
    Neutral = 4,
};

public enum AI_TYPE
{
    AI_TYPE_NONE = 0,
    AI_TYPE_PLAYER = 1,
    AI_TYPE_SERVANT = 2,
    AI_TYPE_SOLDIER = 3,
    AI_TYPE_TOWER = 4,
    AI_TYPE_MONSTER = 5,
    AI_TYPE_CLIENT_CHARACTER = 6,
    AI_TYPE_PATROL_CHARACTER = 7,
};

//关卡恢复类型
public enum STAGE_RESUME_TYPE
{
	S_RESUME_NONE = 0,
	S_RESUME_HP = 1,
	S_RESUME_MP = 2,
};

public enum HitResultType
{
    HT_NONE = 0,
    HT_MISS = 1,//闪避
    HT_CRITICAL = 2,//暴击
    HT_HIT = 3,//命中
    HT_IMMUNE = 4,//mianyi
    HT_DODGE = 5,//未命中
};

public enum HitImmuneType
{
	HIT_IMT_None = 0,
	HIT_IMT_Physics = 1,
	HIT_IMT_Magic = 2,
	HIT_IMT_All = 3,
};

public enum SPECIAL_HIT_EVENT
{
	SHE_MISS = 1,
	SHE_IMMUNE = 2,
};

public enum PhotoType
{
    PT_WHITE = 0,
    PT_BLUE = 1,
    PT_RED = 2,
    PT_GREEN = 3,
    PT_YELLOW = 4,
}

public enum BATTLE_OPERATION_TYPE
{
	BOT_NONE = 0,
	BOT_MANUAL = 1,
	BOT_AUTO = 2,
	BOT_AUTO_CAST_SKILL = 3,
};

public enum AiBehaviouState
{
    Stay = 0,
    Run = 1,
    Attack = 2,
}

public static class Vector3Ext
{
	public static float[] ToFloatArray(this Vector3 vec)
	{
		float[] ret = new float[3];
		ret[0] = vec.x;
		ret[1] = vec.y;
		ret[2] = vec.z;
		
		return ret;
	}
}

public class BornOrDeadBehaviour
{
    public int BornhSound = 0;
    public string BornAnimator = "";
    public List<int> BornParticles = new List<int>();
    public float BornShadeTime = 0;
    public int DeadSound = 0;
    public string DeadAnimator = "";
    public List<int> DeadParticles = new List<int>();
}

public class EntitySoundType
{
    public int BrithSoundType = 0;
    public int DieSoundType = 0;
    public int SelectType = 0;
    public int VictoryType = 0;
    public int EnterStageType = 0;
}


public enum IndentityType
{
	Player = 1,
	Servant = 2,
}

public enum E_IMMUNE_TYPE
{
	IMT_Hit = 1,
	IMT_Motion = 2,
	IMT_Buff = 3,
};

public enum E_HIT_IMMUNE_TYPE
{
	HIT_IMT_None = 0,
	HIT_IMT_Physics = 1,
	HIT_IMT_Magic = 2,
	HIT_IMT_All = 3,
};

/// <summary>
/// 关卡类型
/// </summary>
public enum STAGE_TYPE
{
    NONE                = -1,
    REPLAY              = 0, //录像

    SECTION             = 1, //章节				ST_NORMAL
    BOUNTY              = 7, //悬赏任务			ST_NORMAL
    BOSS                = 8, //个人boss			ST_NORMAL
    GUIDE               = 9, //新手战斗			ST_NORMAL

    STOREY              = 3, //天下无双     	ST_PVP_MANUAL
    GRADE               = 4, //群雄割据     	ST_PVP_AUTO
    SECTION_BOSS        = 13,//关卡boss
    WP_POINT            = 14,//攻打世界点
}



/// <summary>
/// 录像类型
/// </summary>
public enum E_REPLAY_TYPE
{
	RECORD = 1,
	REPLAY = 2,
}

public class HudDefine
{
    public const string PlayerHpUrl = "PlayHp";
    public const string ServantHpUrl = "RedHp";
    public const string EnemyHpUrl = "RedHp";
    public const string SmallEnemyUrl = "EnemyBossHp";
    public const string BigEnemyUrl = "EnemyBossHp";
    public const string GreenHpUrl = "GreenHp";
    public const string SmallGreenHpUrl = "SmallGreenHp";
    public const string RedHpUrl = "RedHp";
    public const string SmallRedHpUrl = "SmallRedHp";
    public const string FriendHpUrl = "FriendHp";
    
    public const string HUDUrl = "HUDTextBlood";
    public const string RHudUrl = "RHudTextBlood";
    public const string LHudUrl = "LHudTextBlood";

    public const string CityPlayerHudUrl = "CityPlayerHud";
    public const string CityLordHudUrl = "CityLordHud";
    public const string CityNpcHudUrl = "CityNpcHud";


    public const string TowerGreenHpUrl = "TowerGreenHp";
    public const string TowerRedHpUrl = "TowerRedHp";

    public const string VIPResName = "huiyuan";
}

public class DropInfo
{
	public int EventID = 0;
	public int DropID = 0;
}

public class StageDropInfo
{
	public string ID = "";
	public List<DropInfo> Drops = new List<DropInfo>();
}

public enum AwardType
{
    Normal,
    Storey,
    Grade,
}

public enum TargetType
{
    None = 0,
    Entity = 1,
    Point = 2,
    Entity_Point = 3,
}

public enum IDENTITY_TYPE
{
    IDT_None            = 0,
    IDT_Player          = 1, //主将
    IDT_Servant         = 2, //随从
};

public enum SIM_STAGE_TYPE
{
    ST_NONE             = 0,
    ST_NORMAL           = 1,
    ST_PVP_AUTO         = 2,
    ST_PVP_MANUAL       = 3,
    ST_SIEGE            = 4,
    ST_CITY_SKIRMISHES  = 5,
    ST_FIELD_SKIRMISHES = 6,
    ST_WAR              = 7,
};

//pvp布阵坐标id定义
public enum PVP_FORMATION_INDEX
{
    /*

    F1_1 F2_1 F3_1 F4_1      B1_1
    F1_2 F2_2 F3_2 F4_2      B1_2
    F1_3 F2_3 F3_3 F4_3      B1_3

    */
    
    F1_1 = 0,
    F1_2 = 1,
    F1_3 = 2,
    
    F2_1 = 3,
    F2_2 = 4,
    F2_3 = 5,
    
    F3_1 = 6,
    F3_2 = 7,
    F3_3 = 8,
    
    F4_1 = 9,
    F4_2 = 10,
    F4_3 = 11,
    
    B1_1 = 12,
    B1_2 = 13,
    B1_3 = 14,
};

public enum SIEGE_CAMP
{
    SC_ATTACKER     = 0,
    SC_DIFFENDER    = 1,
};

/// <summary>
/// 常用挂点定义
/// </summary>
public static class ModelHangPointDefine
{
    public static string H_ROOT = "H_ROOT";
    public static string H_BUFF = "H_BUFF";
    public static string H_BLOOD = "H_BLOOD";
    public static string H_CHEST = "H_CHEST";
    public static string H_WEAPON = "H_WEAPON";
    public static string H_WEAPON_1 = "H_WEAPON_1";
    public static string H_WEAPON_2 = "H_WEAPON_2";
    public static string Bip01_L_Hand = "Bip01 L Hand";
    public static string Bip01_Prop1 = "Bip01 Prop1";
    public static string Bip01_R_Hand = "Bip01 R Hand";
    public static string Bip01_Spine1 = "Bip01 Spine1";
    public static string bone01 = "bone01";

    public static List<string> All_Hand_Point = new List<string>()
    {
        H_ROOT,
        H_BUFF,
        H_BLOOD,
        H_CHEST,
        H_WEAPON,
        H_WEAPON_1,
        H_WEAPON_2,
        Bip01_L_Hand,
        Bip01_Prop1,
        Bip01_R_Hand,
        Bip01_Spine1,
        bone01
    };
} 

public class CacheTriggerResInfo
{
    public const int ACTION_CALL_TRIGGER = -102;
    public const int ACTION_PLACE_MONSTER = 1;
    public const int ACTION_CAST_SKILL = 18;
    public const int ACTION_CAST_EFFECT = 55;
    public const int ACTION_PLAY_CAMERA_PATH = 63;
    public const int ACTION_CREATE_CLIENT_CHARACTER = 71;
    public const int ACTION_SHOW_BOSS_EFFECT = 84;
    public const int ACTION_SHOW_BOSS_CINEMA_EFFECT = 85;
    public const int ACTION_SHOW_UE = 90;
    public const int ACTION_SHOW_MONSTER_WAVE_UI = 91;

	public List<int> MonsterSerials = new List<int>();
    public List<int> BattleInfos = new List<int>();
    public List<string> CameraPrafabPaths = new List<string>();
    public List<int> SkillSerials = new List<int>();
    public List<int> EffectSerials = new List<int>();
}

/// <summary>
/// 移动操作模式
/// </summary>
public enum TouchMode
{
    TapMode,        //点击模式
    JoystickMode    //摇杆模式
}
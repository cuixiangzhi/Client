using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TimmerCountType
{
    Increase = 0,
    Decrease = 1,
}

public enum CharacterBindAttrType
{
    HPPercent = 0,
    AlliveState = 1,
}

public enum TriggerType
{
	StageTrigger,
}

public enum StageSurveyedAreaType
{
	Circle = 0,
	Square = 1,
}

public enum StageValueType
{
	VT_Integer	= 0,
	VT_Bool		= 1,
}

public enum StageAttrTypeType
{
	AT_Bool		= 1,
	AT_Value	= 2,
}

public enum BoolType
{
	BL_False = 0,
	BL_True = 1,
}

/// <summary>
/// 关卡计时器重复类型
/// </summary>
public enum TimmerRepeatType
{
	TRT_Forever = 0,
	TRT_CertainTimes = 1,
}

public enum TriggerTargetType
{
	None				= 0,
	RuntimeEntity 		= 1, //触发者
	
	Monster 			= 2, //怪物
	Character 			= 3, //角色
	
	Timmer 				= 4, //计时器
	Trigger 			= 5, //触发器
	SurveyedArea		= 6, //区域检测
	
	Point      			= 7, //点
	
	BoolValue  			= 8, //Bool关卡变量
	IntValue   			= 9, //Int关卡变量

    Tower               = 10,//塔

	MonsterGroup		= 11,//怪物组

	Path				= 12,//路径

	StarValue			= 13,//关卡变量
	Door				= 14,
	StageScript			= 15,//剧本
    ClientCharacter     = 16,
	CameraEdit			= 17,
}

public enum EditerObjectType
{
	EOT_None                = 0,
	
	EOT_Player	    		= 1,
	EOT_Servant	    		= 2,
	EOT_Enemy	    		= 3,
	EOT_SurveyedArea	    = 4,
	EOT_Timmer	    		= 5,
	EOT_Trigger	    		= 6,
	EOT_Point	    		= 7,
	EOT_Value	    		= 8,
	EOT_Tower	    		= 9,
	EOT_Path	    		= 10,
	//EOT_Star	    		= 11,
	EOT_Door			    = 12,
    EOT_Barracks            = 13,
    EOT_City                = 14,
	EOT_CLIENT_CHARACTER    = 15,
    EOT_Cinema              = 16,
    EOT_Camera              = 17,
    EOT_Troop               = 18,
}

public enum TriggerEditorConstance : int
{
	FriendUnionMask		    = 0x01000000,
	EnemyUnionMask		    = 0x02000000,
	PlayerEditorMask	    = EditerObjectType.EOT_Player       <<  16, //0x00010000
	ServantEditorMask	    = EditerObjectType.EOT_Servant      <<  16,	//0x00020000

	EnemyEditorMask	        = EditerObjectType.EOT_Enemy        <<  16,
	SurveyedAreaEditorMask	= EditerObjectType.EOT_SurveyedArea <<  16,
	TimmerEditorMask	    = EditerObjectType.EOT_Timmer       <<  16,
	TriggerEditorMask	    = EditerObjectType.EOT_Trigger      <<  16,
	PointEditorMask	        = EditerObjectType.EOT_Point        <<  16,
	ValueEditorMask	        = EditerObjectType.EOT_Value        <<  16,
	TowerEditorMask	        = EditerObjectType.EOT_Tower        <<  16,
	PathEditorMask	        = EditerObjectType.EOT_Path         <<  16,
	DoorMask				= EditerObjectType.EOT_Door         <<  16,
    BarracksMask            = EditerObjectType.EOT_Barracks     <<  16,
    CityMask                = EditerObjectType.EOT_City         <<  16,
    ClientCharacterMask     = EditerObjectType.EOT_CLIENT_CHARACTER         <<  16,

	PlayerEditorID		    = FriendUnionMask   | PlayerEditorMask | 0x00000001,
	ServantEditorID		    = FriendUnionMask   | ServantEditorMask,
	EnemyEditorID		    = EnemyUnionMask    | EnemyEditorMask,

	Servant1EditorID	    = FriendUnionMask   | ServantEditorMask | 0x00000001,
	Servant2EditorID	    = FriendUnionMask   | ServantEditorMask | 0x00000002,
	Servant3EditorID	    = FriendUnionMask   | ServantEditorMask | 0x00000003,
	Servant4EditorID	    = FriendUnionMask   | ServantEditorMask | 0x00000004,

    FriendCity              = FriendUnionMask | CityMask | 0x00000001,
    EnemyCity               = EnemyUnionMask | CityMask | 0x00000001,
}

public enum ParamSourceType
{
	None			=	0,//没有参数
	ReadExcel		=	1,//读表
	Enum			=	2,//枚举
	UserSelect 		=	3,//拖选
    UserInputInt    =   4,//用户输入 int
	UserInputFloat	=	5,//用户输入 float
	UserInputString =	6,//用户输入 string
	Array_UserSelect = 13,
}

public enum MonsterConditionType
{
	MonsterSerial		=	101,
	HP					=	102,
	LiveState			=	103,
}

public enum CharacterConditionType
{
	CharacterSerial		=	201,
	HP					=	202,
	LiveState			=	203,
}

public enum TimmerConditionType
{
	TimmerRunningState	=	301,
}

public enum TriggerConditionType
{
	TriggerRunningState	=	401,
}

public enum LiveStateInfoType
{
	Alive	= 1001,		//存活
	Dead	= 1002,		//死亡
}

public enum TimmerRunningStateInfoType
{
	NotStart	=	3001,//未开启
	Paused		=	3002,//暂停
	Running		=	3003,//运行中
	Stoped		=	3004,//已停止
}

public enum TriggerRunningStateInfoType
{
	Enabled		=	4001,//开启的
	Disabled	=	4002,//关闭的
}

public enum ExistInfoType
{
    EXT_Exist       = 5001,//存在
    EXT_NotExist    = 5002,//不存在
}

public enum ObjectVisibleState
{
    Show = 1,
    Hide = 0,
}

public enum ConditionRelationType
{
	And = 0,
	Or = 1,
}

public enum OperatorType
{
	None				=	0,
	Is					=	1,
	Not					=	2,

	EqualToValue		=	10,
	LessThanValue		=	11,
	BiggerThanValue		=	13,
	
	EqualToPercent		=	14,
	LessThanPercent		=	15,
	BiggerThanPercent	=	16,
}

public enum BoolOperatorType
{
	Is					=	1,
	Not					=	2,
}
public enum NumberOperatorType
{
	None				=	0,
    EqualToValue        =   10,
    LessThanValue       =   11,
    BiggerThanValue     =   13,

    EqualToPercent      =   14,
    LessThanPercent     =   15,
    BiggerThanPercent   =   16,
}

public enum ExcelDataType
{
	MonsterData = 1,
	CharacterData = 2,
}

public class ExcelDataNode
{
	public int id;
	public string data;
}

public enum OperatorTargetType
{
	OTT_MonsterType = 1,
	OTT_CharacterType = 2,
	OTT_AliveState = 3,
	OTT_ExistState = 4,
	OTT_TimmerState = 5,
	OTT_TriggerState = 6,
	OTT_TriggerMan = 7,
	OTT_HP = 8,
	OTT_StageBoolValue = 9,
	OTT_StageIntValue = 10,
	OTT_MonsterGroupExistState = 11,
	OTT_SurveyedAreaTrapedState = 12,
};
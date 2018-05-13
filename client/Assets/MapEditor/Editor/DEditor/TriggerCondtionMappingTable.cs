using System.Collections.Generic;
using System;

public class TriggerCondtionMappingTable
{
	static Dictionary<Type,TriggerTargetType> m_first_mapping = new Dictionary<Type, TriggerTargetType>();

	public static void RegisterMappingTable()
	{
		m_first_mapping[ typeof(MonsterEditData) ] = TriggerTargetType.Monster; 
		m_first_mapping[ typeof(PlayerEditData) ]  = TriggerTargetType.Character; 
		m_first_mapping[ typeof(TimmerEditData) ]  = TriggerTargetType.Timmer; 
		m_first_mapping[ typeof(TriggerEditData) ] = TriggerTargetType.Trigger;
		m_first_mapping[ typeof(TriggerMan) ]      = TriggerTargetType.RuntimeEntity;

        m_first_mapping[typeof(ServantEditData)] = TriggerTargetType.Character;
		//m_first_mapping[ typeof(BoolValueEditData)]   = TriggerTargetType.BoolValue;
		m_first_mapping[ typeof(ValueEditData)]    = TriggerTargetType.IntValue;

        m_first_mapping[ typeof(PointEditData)]    = TriggerTargetType.Point;

        m_first_mapping[typeof(TowerEditData)] = TriggerTargetType.Tower;

		m_first_mapping[typeof(MonsterGroupEditData)] = TriggerTargetType.MonsterGroup;
        //m_first_mapping[typeof(StarValueEditData)] = TriggerTargetType.IntValue;
        m_first_mapping[typeof(SurveyedAreaEditData)] = TriggerTargetType.SurveyedArea;
	}

	public static List<ConditionAttrTypeData.ConditionAttrTypeData> GetConditionAttrDatas(Type tp)
	{
		RegisterMappingTable();

		List<ConditionAttrTypeData.ConditionAttrTypeData> ret = new List<ConditionAttrTypeData.ConditionAttrTypeData>();

		TriggerTargetType tag = TriggerTargetType.None;
		if(!m_first_mapping.TryGetValue( tp ,out tag ))
		{
			return ret;
		}

		//DatasManager.Instance.LoadAllDatas();

		List<ConditionAttrTypeData.ConditionAttrTypeData> datas = DatasManager.Instance.GetDatas<ConditionAttrTypeData.ConditionAttrTypeData>();
		
        List<ConditionAttrTypeData.ConditionAttrTypeData> temp = new List<ConditionAttrTypeData.ConditionAttrTypeData>();
        foreach (ConditionAttrTypeData.ConditionAttrTypeData data in datas)
        {
            if(data.Tag.CompareTo((int)tag) == 0)
            {
                temp.Add(data);
            }
        }
		
		foreach(var data in temp)
		{
			ret.Add(data);
		}

		return ret;
	}

    public static List<ConditionAttrTypeData.ConditionAttrTypeData> GetConditionAttrDatas(TriggerTargetType type)
    {
        RegisterMappingTable();

        List<ConditionAttrTypeData.ConditionAttrTypeData> ret = new List<ConditionAttrTypeData.ConditionAttrTypeData>();

        if (!m_first_mapping.ContainsValue(type))
        {
            return ret;
        }

        //DatasManager.Instance.LoadAllDatas();

        List<ConditionAttrTypeData.ConditionAttrTypeData> datas = DatasManager.Instance.GetDatas<ConditionAttrTypeData.ConditionAttrTypeData>();

        List<ConditionAttrTypeData.ConditionAttrTypeData> temp = new List<ConditionAttrTypeData.ConditionAttrTypeData>();
        foreach (ConditionAttrTypeData.ConditionAttrTypeData data in datas)
        {
            if (data.Tag.CompareTo((int)type) == 0)
            {
                temp.Add(data);
            }
        }

        foreach (var data in temp)
        {
            ret.Add(data);
        }

        return ret;
    }

	public static List<AttrInfoData.AttrInfoData> GetDatasWithEnumType( string attrName )
	{
		RegisterMappingTable();

		//DatasManager.Instance.LoadAllDatas();

		List<AttrInfoData.AttrInfoData> datas = DatasManager.Instance.GetDatas<AttrInfoData.AttrInfoData>();

        List<AttrInfoData.AttrInfoData> temp = new List<AttrInfoData.AttrInfoData>();
        foreach (AttrInfoData.AttrInfoData data in datas)
        {
            if (data.Tag.CompareTo(attrName) == 0)
            {
                temp.Add(data);
            }
            //UnityEngine.Debug.Log(string.Format("ID:{0}\t\tTag:{1}\t\tName:{2}\t\tKey:{3}\t\tDes:{4}", data.ID, data.Tag, data.Name, data.Key, data.Des));
        }
		return temp;
	}

	public static List<ExcelDataNode> GetDatasWithEnumType( ExcelDataType type )
	{
		RegisterMappingTable();

		//DatasManager.Instance.LoadAllDatas();
		List<ExcelDataNode> ret = new List<ExcelDataNode>();
		if(  type == ExcelDataType.MonsterData )
		{
			List<MonsterData.MonsterData> datas = DatasManager.Instance.GetDatas<MonsterData.MonsterData>();

			foreach(var data in datas)
			{
				ExcelDataNode node = new ExcelDataNode();
				node.id = data.ID;
				node.data = data.characterName;

				ret.Add(node);
			}
		}
		else if(  type == ExcelDataType.CharacterData )
		{
			List<CharacterData.CharacterData> datas = DatasManager.Instance.GetDatas<CharacterData.CharacterData>();
			
			foreach(var data in datas)
			{
				ExcelDataNode node = new ExcelDataNode();
				node.id = data.ID;
				node.data = data.characterName;
				
				ret.Add(node);
			}
		}
		
		return ret;
	}
}


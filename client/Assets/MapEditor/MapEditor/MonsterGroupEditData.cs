using UnityEngine;

[ExecuteInEditMode]
public class MonsterGroupEditData : IEditEventSender
{
	public override TriggerTargetType ToTargetType() { return TriggerTargetType.MonsterGroup; }

    public override int MinIdValue()
    {
        return MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(MonsterGroupEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	public MonsterEditData leader = null;
	public MonsterEditData[] members = new MonsterEditData[0];

	private MapEditConfigData.MonsterGroupInfo _data = new MapEditConfigData.MonsterGroupInfo();
	public MapEditConfigData.MonsterGroupInfo Data
	{
		get
		{
			_data = new MapEditConfigData.MonsterGroupInfo();

			_data.id = this.id;
			_data.name = name;

			if(null == this.leader)
			{
				_data.leaderInfo = null;
			}
			else
			{
				_data.leaderInfo = this.leader.Data;
			}

			for(int i = 0 ; i < members.Length; i++)
			{
				_data.memberInfos.Add(members[i].Data);
			}

			return _data;
		}
	}

	int m_nLastId = 0;
	void Update()
	{
		if(string.IsNullOrEmpty(name))
		{
			name = "Group" + id;
		}
		this.transform.name = name;
		int masterIndex = -1;
		MonsterEditData[] childs = this.gameObject.GetComponentsInChildren<MonsterEditData>();
		for(int i = 0 ; i < childs.Length; i++)
		{
			MonsterEditData index = childs[i];
			if(index.isMaster)
			{
				if(masterIndex != -1)
				{
					Debug.LogError("一个怪物组里只能有一个队长");
					return;
				}
				masterIndex = i;

				this.leader = index;
			}
		}

		if(masterIndex != -1)
		{
			members = new MonsterEditData[childs.Length - 1];
		}
		else
		{
			this.leader = null;
			members = new MonsterEditData[childs.Length];
		}

		if(m_nLastId != id)
		{
			m_nLastId = id;
			for(int i = 0; i < childs.Length; i++)
			{
				MonsterEditData cld = childs[i];
				cld.group = id;
			}
		}

		for(int i = 0, j = 0 ; i < childs.Length; i++)
		{
			if(i != masterIndex)
			{
				MonsterEditData index = childs[i];
				members[j] = index;
				j++;

				if(-1 != masterIndex)
				{
					GameObject tmp = new GameObject();
					tmp.transform.parent = childs[masterIndex].transform;
					tmp.transform.position = index.transform.position;
					tmp.transform.localEulerAngles = Vector3.zero;

					index.relativePos = tmp.transform.localPosition;

					Object.DestroyImmediate(tmp);
				}
				else
				{
					index.relativePos = Vector3.zero;
				}
			}
		}
	}
}

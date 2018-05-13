using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MonsterEditData : IEditEventSender 
{
    public int type = 0;
    public int group = 0;
    public float protection = 0f;
	public int camp = 1;
	public int resumeId = 0;
	public bool isMaster = false;
	public Vector3 relativePos = Vector3.zero;
	public int dropId = 0;
    public int bornBehaviorId = 0;
    public float allertDistance = 0f;
	public bool keepOnGround = true;

	public override TriggerTargetType ToTargetType() { return TriggerTargetType.Monster; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.EnemyEditorID + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(MonsterEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	MapEditConfigData.MonsterPlaceInfo _data = null;
	public MapEditConfigData.MonsterPlaceInfo Data
    {
        get
        {
			if(_data == null)
			{
				_data = new MapEditConfigData.MonsterPlaceInfo();
			}
            _data.id = id;
			_data.name = name;
            _data.type = type;
            _data.group = group;
            _data.protection = protection;
			_data.camp = camp;
			_data.resumeId = resumeId;
			_data.isMaster = isMaster;
			_data.relativePos = relativePos.ToMapEditConfigDataVector3();
			_data.dropId = dropId;
            _data.bornBehaviorId = bornBehaviorId;
            _data.allertDistance = allertDistance;
			_data.keepOnGround = keepOnGround;

			_data.pos = transform.position.ToMapEditConfigDataVector3();
			_data.rot = transform.eulerAngles.ToMapEditConfigDataVector3();

            return _data;
        }
    }

    protected Transform model;
	protected int lastType = 0;
	protected int lastGroup = 0;
	protected int lastId = 0;

	protected MonsterData.MonsterData cdata;

    void Awake()
    {
		_data = new MapEditConfigData.MonsterPlaceInfo();
    }


	private bool m_bLoadData = false;
    void Update()
    {
        base.OnUpdate();

        if(Application.isPlaying)
        {
            return;
        }

		this.transform.name = name;

        if(type != 0 && cdata == null)
        {
            //DatasManager.Instance.LoadAllDatas();
            cdata = DatasManager.Instance.GetData<MonsterData.MonsterData>(type);

			if(!m_bLoadData)
				return;
        }

		if( group != lastGroup )
        {
			if(0 == lastGroup)
			{
				lastGroup = group;
			}
			else
			{
	            lastGroup = group;
                Transform monsterRoot = MapEditorUtlis.GetMonsterEditRoot();
                if (!monsterRoot)
                {
                    Debug.LogError("扯淡!!!!!!!!");
                    return;
                }

                IEditEventSender[] ies = monsterRoot.GetComponentsInChildren<IEditEventSender>();

                for (int i = 0; i < ies.Length; i++)
                {
                    if (ies[i].id == group && ies[i].ToTargetType() == TriggerTargetType.MonsterGroup)
                    {
                        transform.parent = ies[i].transform;

                        return;
                    }
                }

                string groupNmae = "Group" + group;

                if (group <= 0xFF)
                {
                    groupNmae = "预留怪物组_" + group;
                }

                GameObject groupNode = new GameObject(groupNmae);
                MonsterGroupEditData src = groupNode.AddComponent<MonsterGroupEditData>();
                src.id = lastGroup;

                groupNode.transform.parent = MapEditorUtlis.GetMonsterEditRoot();

                groupNode.transform.localPosition = Vector3.zero;
                groupNode.transform.localEulerAngles = Vector3.zero;
                groupNode.transform.localScale = Vector3.one;

                this.transform.parent = groupNode.transform;
			}
        }

        if( id != lastId )
        {
            lastId = id;
			if( string.IsNullOrEmpty(name) )
			{
            	this.transform.name = "Monster" + "_" + group + "_" + id;
			}
			else
			{
				this.transform.name = name;
			}
        }

        if(type != lastType)
        {
            lastType = type;

            cdata = DatasManager.Instance.GetData<MonsterData.MonsterData>(type);

            if(cdata == null)
            {
                Debug.LogError("不存在的角色类型");
                type = 0;
                lastType = 0;

                if(model != null)
                {
                    GameObject.DestroyImmediate( model.gameObject );
                    model = null;
                }
                else
                {
                    int clds = this.transform.childCount;
                    
                    for(int i = 0 ; i < clds ; i++)
                    {
                        GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
                    }
                }

                return;
            }

            if(model != null)
            {
                GameObject.DestroyImmediate( model.gameObject );
                model = null;
            }
            else
            {
                int clds = this.transform.childCount;

                for(int i = 0 ; i < clds ; i++)
                {
                    GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
                }
            }
			BattleInfomationData.BattleInfomationData info = DatasManager.Instance.GetData<BattleInfomationData.BattleInfomationData> (cdata.battleInfo);
			if(null != info)
			{
                GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Resource/" + info.modelPath + ".prefab");
                OnModelResReady(info.modelPath, obj.transform);
			}
        }

        if(type == 0)
        {
            if(model == null)
            {
                if (transform.childCount != 0)
                {
                    for (int i = 0; i < transform.childCount; )
                    {
                        if (model == null && transform.GetChild(i).name.Equals(MapEditorCons.DFT_MONSTER_MODEL))
                        {
                            model = transform.GetChild(i);
                            i++;
                        }
                        else
                        {
                            GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
                        }
                    }
                }
                if (model == null)
                {
                    GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Resource/" + MapEditorCons.DFT_MONSTER_MODEL + ".prefab");
                    OnModelResReady(MapEditorCons.DFT_MONSTER_MODEL, obj.transform);
                }
            }
        }

		if(keepOnGround)
		{
			LayerMask _mask = LayerMask.NameToLayer(LayerDefine.BackGroundLayerName);
			Ray ray = new Ray(new Vector3(transform.position.x,transform.position.y+100,transform.position.z),Vector3.up*-1);
			LayerMask nl = 1 << _mask;

			RaycastHit hit;
			if (UnityEngine.Physics.Raycast(ray, out hit, 1500.0f, nl))
			{
                transform.position = new Vector3(hit.point.x, hit.point.y + MapEditorCons.OBJECT_HEIGHT_MOD, hit.point.z);
			}
			else
			{
				Debug.LogError( transform.name + " : position is invalid!" ,gameObject);
			}
		}
    }

    protected void OnModelResReady ( string modelName, Transform tf )
    {
        model = UnityEngine.GameObject.Instantiate( tf ) as Transform;
        model.name = modelName;
        model.parent = this.transform;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
		model.localScale = Vector3.one;
    }

    protected override void OnHiracheyChanged()
    {
        if(transform.parent == null)
        {
            this.id = 0;
            return;
        }

        MonsterGroupEditData gp = transform.parent.GetComponent<MonsterGroupEditData>();
        if(gp == null)
        {
            this.id = 0;
            return;
        }

        MonsterEditData[] monsterEdits = transform.parent.GetComponentsInChildren<MonsterEditData>();
        if(monsterEdits.Length > 0xFF)
        {
            Debug.LogError("当前怪物组下的怪物个数已经超过255个！");
            return;
        }

        List<MonsterEditData> ls = new List<MonsterEditData>(monsterEdits);

        for(int i = 0 ; i < 0xFF; i++)
        {
            int gpId = gp.id << 8;
            int tmpid = (int)TriggerEditorConstance.EnemyEditorMask | gpId | i;
            MonsterEditData src = ls.Find((x)=>{return x.id == tmpid;});
            if(null == src)
            {
                this.id = tmpid;
                return;
            }
        }
    }
}

public static class MapEditConfigDataVector3Ext
{
    public static MapEditConfigData.MP_Vector3 ToMapEditConfigDataVector3(this Vector3 vec)
    {
		MapEditConfigData.MP_Vector3 ret = new MapEditConfigData.MP_Vector3();
        ret.x = vec.x;
        ret.y = vec.y;
        ret.z = vec.z;
        
        return ret;
    }
}

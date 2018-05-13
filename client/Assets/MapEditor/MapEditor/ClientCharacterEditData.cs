using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientCharacterEditData : MonsterEditData
{
    public int star = 1;
    public int level = 1;
    public int battleInfoId = 0;
    public int battleAttrId = 0;
    public int country = 0;

    public int[] skillLevel;
    
    public override TriggerTargetType ToTargetType() { return TriggerTargetType.ClientCharacter; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.ClientCharacterMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(ClientCharacterEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }
    
    MapEditConfigData.ClientCharacterPlaceInfo _clientCharacterdata = null;
    public new MapEditConfigData.ClientCharacterPlaceInfo Data
    {
        get
        {
			if(_clientCharacterdata == null)
            {
				_clientCharacterdata = new MapEditConfigData.ClientCharacterPlaceInfo();
            }
			_clientCharacterdata.id = id;
			_clientCharacterdata.name = name;
			_clientCharacterdata.group = group;
			_clientCharacterdata.protection = protection;
			_clientCharacterdata.camp = camp;
            _clientCharacterdata.star = star;
            _clientCharacterdata.level = level;
            _clientCharacterdata.battleInfoId = battleInfoId;
            _clientCharacterdata.battleAttrId = battleAttrId;
            _clientCharacterdata.country = country;
            _clientCharacterdata.isMaster = isMaster;
            _clientCharacterdata.relativePos = relativePos.ToMapEditConfigDataVector3();
            _clientCharacterdata.bornBehaviorId = bornBehaviorId;
            _clientCharacterdata.allertDistance = allertDistance;
            _clientCharacterdata.skillLevel.Clear();
            _clientCharacterdata.skillLevel.AddRange(skillLevel);
			_clientCharacterdata.pos = transform.position.ToMapEditConfigDataVector3();
			_clientCharacterdata.rot = transform.eulerAngles.ToMapEditConfigDataVector3();
            
            return _clientCharacterdata;
        }
    }
    
    void Awake()
    {
        _clientCharacterdata = new MapEditConfigData.ClientCharacterPlaceInfo();
    }
    
	BattleInfomationData.BattleInfomationData battleInfoData = null; 
    void Update()
    {
        base.OnUpdate();
        this.transform.name = name;
        
        if(battleInfoId != 0 && battleInfoData == null)
        {
            battleInfoData = DatasManager.Instance.GetData<BattleInfomationData.BattleInfomationData>(battleInfoId);
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
                string groupNmae = "Group_" + group;
                if(group <= 0xFF)
                {
                    groupNmae = "预留客户端角色组_" + group;
                }
                
                if ( this.transform.parent.name != groupNmae )
                {
                    GameObject groupNode = GameObject.Find(groupNmae);
                    if( null == groupNode )
                    {
                        groupNode = new GameObject(groupNmae);
                        MonsterGroupEditData src = groupNode.AddComponent<MonsterGroupEditData>();
                        src.id = lastGroup;

                        groupNode.transform.parent = MapEditorUtlis.GetClientCharacterEditRoot();
                        
                        groupNode.transform.localPosition = Vector3.zero;
                        groupNode.transform.localEulerAngles = Vector3.zero;
                        groupNode.transform.localScale = Vector3.one;
                    }
                    
                    this.transform.parent = groupNode.transform;
                }
            }
        }
        
        if( id != lastId )
        {
            lastId = id;
            if( string.IsNullOrEmpty(name) )
            {
                this.transform.name = "ClientCharacter" + "_" + group + "_" + id;
            }
            else
            {
                this.transform.name = name;
            }
        }
        
        if(battleInfoId != lastType)
        {
            lastType = battleInfoId;
            
            battleInfoData = DatasManager.Instance.GetData<BattleInfomationData.BattleInfomationData>(battleInfoId);
            
            if(battleInfoData == null)
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
            GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Resource/" + battleInfoData.modelPath + ".prefab");
            OnModelResReady(battleInfoData.modelPath,obj.transform);
        }
    }
    
    protected new void OnModelResReady ( string modelName, Transform tf )
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
    
    void Test(){}
}



using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TowerEditData : IEditEventSender 
{
    public int type = 0;
    public int camp = 1;
	public int resumeId = 0;
	public int dropId = 0;
    public int bornBehaviourId = 0;
    public float allertDistance = 0f;
	
    public override TriggerTargetType ToTargetType() { return TriggerTargetType.Tower; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.TowerEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(TowerEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

    MapEditConfigData.TowerPlaceInfo _data = null;
    public MapEditConfigData.TowerPlaceInfo Data
    {
        get
        {
			if(_data == null)
			{
				_data = new MapEditConfigData.TowerPlaceInfo();
			}
            _data.id = id;
            _data.name = name;
            _data.type = type;
            _data.camp = camp;
			_data.resumeId = resumeId;
			_data.dropId = dropId;
            _data.bornBehaviourId = bornBehaviourId;
            _data.allertDistance = allertDistance;
			
			_data.pos = (new Vector3(transform.position.x,transform.position.y,transform.position.z)).ToMapEditConfigDataVector3();
            _data.rot = transform.eulerAngles.ToMapEditConfigDataVector3();
			
            return _data;
        }
    }
	
    protected Transform model;
    int lastType = 0;
    int lastId = 0;
	
    string lastName = "";
	
    TowerData.TowerData cdata ;
	
    void Awake()
    {
        _data = new MapEditConfigData.TowerPlaceInfo();
    }
	
    void Update()
    {
        base.OnUpdate();

        if (Application.isPlaying)
        {
            return;
        }

		this.transform.name = name;

        if(type != 0 && cdata == null)
        {
            cdata = DatasManager.Instance.GetData<TowerData.TowerData>(type);
        }
		
        if( id != lastId )
        {
            lastId = id;
            if( string.IsNullOrEmpty(name) )
            {
                this.transform.name = "Tower" + "_" + id;
            }
            else
            {
                this.transform.name = name;
            }
        }
		
        if(type != lastType)
        {
            lastType = type;
			
            cdata = DatasManager.Instance.GetData<TowerData.TowerData>(type);
			
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
                GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Resource/" + MapEditorCons.DFT_TOWER_MODEL + ".prefab");
                OnModelResReady(MapEditorCons.DFT_TOWER_MODEL, obj.transform);
            }
        }
		
        if( lastName != name )
        {
            if(!string.IsNullOrEmpty(name))
            {
                this.transform.name = name;
            }
            else
            {
                this.transform.name = "Enemy_" + id;
            }
			
            lastName = name;
        }
		
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
            Debug.LogError( transform.name + " : position is invalid!" );
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
}

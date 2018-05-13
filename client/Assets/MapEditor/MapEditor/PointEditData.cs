using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PointEditData : IEditEventSender
{
	public float speed = 0.0f;
	public float stayTime = 0.0f;
    public int weight = 10;
	[HideInInspector]
	public Transform orgParent;
    public override TriggerTargetType ToTargetType()
    {
        return TriggerTargetType.Point;
    }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.PointEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(PointEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

    private MapEditConfigData.PointInfo _data = null;
    public MapEditConfigData.PointInfo Data
    {
        get
        {
            if (_data == null)
            {
                _data = new MapEditConfigData.PointInfo();
            }
            _data.id = id;
			_data.name = name;
            _data.position = this.transform.position.ToMapEditConfigDataVector3();
			_data.rotation = this.transform.eulerAngles.ToMapEditConfigDataVector3();
			_data.speed = speed;
			_data.stayTime = stayTime;
            _data.weight = weight;

            return _data;
        }

        set
        {
            id = value.id;
			name = value.name;
			speed = value.speed;
			stayTime = value.stayTime;
			this.transform.position = value.position.ToVector3();
			this.transform.eulerAngles = value.rotation.ToVector3();
            weight = value.weight;
        }
    }

//	private GameObject thisObject = null;
    void Awake()
    {
        _data = new MapEditConfigData.PointInfo();
    }

	void Update()
	{
		LayerMask _mask = LayerMask.NameToLayer(LayerDefine.BackGroundLayerName);
		//transform.rotation = Quaternion.identity;
		Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 100, transform.position.z), Vector3.down);
		LayerMask nl = 1 << _mask;
		
		RaycastHit hit;
		if (UnityEngine.Physics.Raycast(ray, out hit, 1500.0f, nl))
		{
			transform.position = new Vector3(hit.point.x, hit.point.y + 0.05f, hit.point.z);
		}
		else
		{
			Debug.LogError( transform.name + " : position is invalid!" );
		}

		this.transform.name = name;
	}
	
	void OnDrawGizmos()
	{
		Color tmp = Gizmos.color;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position,1);
		Gizmos.color = tmp;
	}
}

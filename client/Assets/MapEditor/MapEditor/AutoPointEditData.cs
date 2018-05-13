using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AutoPointEditData : PointEditData
{
    Vector3 orgPos;

    public override TriggerTargetType ToTargetType()
    {
        return TriggerTargetType.Point;
    }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.PointEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

    private MapEditConfigData.PointInfo _data = null;
    public new MapEditConfigData.PointInfo Data
    {
        get
        {
            if (_data == null)
            {
                _data = new MapEditConfigData.PointInfo();
            }
            _data.id = id;
            _data.name = name;
            _data.position = transform.position.ToMapEditConfigDataVector3();
            _data.rotation = transform.eulerAngles.ToMapEditConfigDataVector3();
            _data.speed = speed;
            _data.stayTime = stayTime;

            return _data;
        }

        set
        {
            _data = value;

            id = value.id;
            name = value.name;
            speed = value.speed;
            stayTime = value.stayTime;

			this.transform.position = value.position.ToVector3();
			this.transform.eulerAngles = value.rotation.ToVector3();
			this.transform.localScale = Vector3.one;
        }
    }

    void OnDrawGizmos()
    {
        if (orgPos != transform.position)
        {
            LayerMask _mask = LayerMask.NameToLayer(LayerDefine.BackGroundLayerName);
            Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 100, transform.position.z), Vector3.down);
            LayerMask nl = 1 << _mask;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1500.0f, nl))
            {
                transform.position = new Vector3(hit.point.x, hit.point.y + 0.05f, hit.point.z);
            }

            orgPos = transform.position;
        }


        this.transform.name = name;

        Color tmp = Gizmos.color; ;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1);
        Gizmos.color = tmp;
    }
}

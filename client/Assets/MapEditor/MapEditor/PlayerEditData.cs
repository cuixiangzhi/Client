using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PlayerEditData : IEditEventSender 
{
	public override TriggerTargetType ToTargetType() { return TriggerTargetType.Character; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.PlayerEditorID;
    }

    public override System.Type GetBaseType()
    {
        return typeof(PlayerEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        GameObject.DestroyImmediate(gameObject);
    }

	public int camp = 0;
    public float allertDistance = 0f;

	void Update()
	{
		if(Application.isPlaying)
		{
			return;
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
}

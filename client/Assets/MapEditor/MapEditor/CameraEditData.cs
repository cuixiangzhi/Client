using UnityEngine;
using System.Collections;

public class CameraEditData : IEditEventSender 
{
	public override TriggerTargetType ToTargetType()
	{
		return TriggerTargetType.CameraEdit;
	}

    public override int MinIdValue()
    {
        return MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(CameraEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }
}

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ServantEditData : IEditEventSender
{
	public override TriggerTargetType ToTargetType() { return TriggerTargetType.Character; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.Servant1EditorID;
    }

    public override System.Type GetBaseType()
    {
        return typeof(ServantEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
        int index = this.id & 0x0000ffff;
        gameObject.name = "Servant_" + index;
        name = "Servant_" + index;
    }
}


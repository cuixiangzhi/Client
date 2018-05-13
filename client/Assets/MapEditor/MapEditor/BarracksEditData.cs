using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BarracksEditData : TowerEditData
{
    public override TriggerTargetType ToTargetType() { return TriggerTargetType.Tower; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.BarracksMask + MapEditorCons.PRESERVED_NUMBER;
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }
}

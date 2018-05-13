using UnityEngine;

[ExecuteInEditMode]
public class TriggerMan : IEditEventSender //触发者
{
	public override TriggerTargetType ToTargetType() { return TriggerTargetType.RuntimeEntity; }

    public override System.Type GetBaseType()
    {
        return typeof(TriggerMan);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        GameObject.DestroyImmediate(gameObject);
    }
}

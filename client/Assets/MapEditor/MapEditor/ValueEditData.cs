using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ValueEditData : IEditEventSender
{
    public int defaultValue = 0;
    public int maxValue = 0;

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.ValueEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(ValueEditData);
    }

    public override TriggerTargetType ToTargetType()
    {
        return TriggerTargetType.IntValue;
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

    MapEditConfigData.ValueInfo _data = null;
    public MapEditConfigData.ValueInfo Data
    {
        get
        {
            if (_data == null)
            {
                _data = new MapEditConfigData.ValueInfo();
            }
            _data.id = id;
            _data.name = name;
            _data.defaultValue = defaultValue;
            _data.maxValue = maxValue;
            _data.type = 0;

            return _data;
        }
        set
        {
            id = value.id;
            defaultValue = value.defaultValue;
            name = value.name;
            maxValue = value.maxValue;
        }
    }

    void Awake()
    {
        _data = new MapEditConfigData.ValueInfo();
    }

    void Update()
    {
        this.transform.name = name;
    }
}


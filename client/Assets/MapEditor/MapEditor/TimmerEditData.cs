using UnityEngine;
using System.Collections;

public enum TimerRepeatType
{
	Forever,
	CertainTimes,
}

[ExecuteInEditMode]
public class TimmerEditData : IEditEventSender
{
	public float time;
	public TimerRepeatType repeatType;
	public int repeatTimes;

	public override TriggerTargetType ToTargetType() { return TriggerTargetType.Timmer; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.TimmerEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(TimmerEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	MapEditConfigData.TimmerInfo  _data = null;
	public MapEditConfigData.TimmerInfo Data
	{
		get
		{
			if(_data == null)
			{
				_data = new MapEditConfigData.TimmerInfo();
			}
			_data.id = id;
            _data.name = name;
			_data.repeatTimes = repeatTimes;
			_data.repeatType = (int)repeatType;
			_data.time = time;
			
			return _data;
		}
		
		set
		{
			id = value.id;
            name = value.name;
			repeatTimes = value.repeatTimes;
			repeatType = (TimerRepeatType)value.repeatType;
			time = value.time;
		}
	}
	
	void Awake()
	{
		_data = new MapEditConfigData.TimmerInfo();
	}

	void Update()
	{
		this.transform.name = name;
	}
}

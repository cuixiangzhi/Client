using UnityEngine;

[ExecuteInEditMode]
public class TriggerEditData : IEditEventSender
{
	public override TriggerTargetType ToTargetType()
	{
		return TriggerTargetType.Trigger;
	}

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.TriggerEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);

        MapDataSerializerTools.AssignDataByFieldName((parent as TriggerEditData).Data, Data);
    }

    public override System.Type GetBaseType()
    {
        return typeof(TriggerEditData);
    }

	[SerializeField]
	private MapEditConfigData.TriggerInfo _data = null;

	[SerializeField]
	public MapEditConfigData.TriggerInfo Data
	{
		get
		{
			if(_data == null)
                _data = new MapEditConfigData.TriggerInfo();


            if (_data.nodes == null)
            {
                _data.nodes = new MapEditConfigData.TriggerHandlerInfo();
                _data.nodes.root = new MapEditConfigData.TriggerRootNodeData();
                _data.nodes.root.node = new MapEditConfigData.TriggerNodeData();
                _data.nodes.root.eventInfo = new MapEditConfigData.TriggerEventInfo();
            }
			
			_data.id = id;
			_data.name = name;
			
			return _data;
		}
		
		set
		{
            //Transform root = 
            MapEditorUtlis.GetEditRoot();
			
			_data = value;
			
			id = _data.id;
			name = _data.name;
		}
	}
	
	void Update()
	{
		this.transform.name = name;
	}
}

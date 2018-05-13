using UnityEngine;

[ExecuteInEditMode]
public class DoorEditData : IEditEventSender 
{
	public override TriggerTargetType ToTargetType()
	{
		return TriggerTargetType.Door;
	}

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.DoorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(DoorEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	public MapEditConfigData.DoorInfo Data
	{
		get
		{
			if(_data == null)
				_data = new MapEditConfigData.DoorInfo();

			_data.id = id;
			_data.state = state;
			
			if(target == null)
			{
				if(!string.IsNullOrEmpty(_data.name))
				{
					target = GameObject.Find(_data.name);
					if(target == null)
					{
						Debug.LogError("未在场景中找到名字为[" + _data.name + "]的物体!", gameObject);
					}
					else
					{
						_data.name = target.name;
						_data.position = target.transform.position.ToMapEditConfigDataVector3();
						_data.rotation = target.transform.eulerAngles.ToMapEditConfigDataVector3();
						_data.scale = target.transform.lossyScale.ToMapEditConfigDataVector3();
                        //地图中door以1编号开始，所以－1
                        _data.flag = (int)SamplePolyFlags.SAMPLE_POLYFLAGS_CUSTOM0 << (GetDoorIndex( target.name ) - 1);
					}
				}
				else
				{
					Debug.LogError("目标需要绑定到场景中美术配出的门上", gameObject);
				}
			}
			else
			{
				_data.name = target.name;
				_data.position = target.transform.position.ToMapEditConfigDataVector3();
				_data.rotation = target.transform.eulerAngles.ToMapEditConfigDataVector3();
				_data.scale = target.transform.lossyScale.ToMapEditConfigDataVector3();
                //地图中door以1编号开始，所以－1
                _data.flag = (int)SamplePolyFlags.SAMPLE_POLYFLAGS_CUSTOM0 << (GetDoorIndex( target.name ) - 1);
			}

            _data.modelName = model != null ? model.name : "";

			return _data;
		}

		set
		{
            gameObject.name = value.name;
			_data = value;
			id = value.id;
			state = value.state;
			if(!string.IsNullOrEmpty(value.name))
			{
				target = GameObject.Find(value.name);
				if(target != null)
				{
					transform.position = target.transform.position;
				}
			}
			else
			{
				Debug.LogWarning("需要拖选一个门到target上.", gameObject);
			}

            if(!string.IsNullOrEmpty(value.modelName))
            {
                for(int i = 0; i < target.transform.childCount; i++)
                {
                    if(target.transform.GetChild(i).name == value.modelName)
                    {
                        model = target.transform.GetChild(i).gameObject;
                    }
                }
            }
		}
	}

	int GetDoorIndex(string doorname)
	{
		int tmp = doorname[doorname.Length - 1] - '0';
		if(tmp < 0 || tmp > 9)
			Debug.LogError("可能是因为你拖选的门命名不规范,正确命名应该是door后面跟0~9的数字,例如:door1", gameObject);
		return tmp;
	}

	public int state = 0;//门状态 0 关闭   1 开启

	MapEditConfigData.DoorInfo _data;
	public GameObject target;
    public GameObject model;
}

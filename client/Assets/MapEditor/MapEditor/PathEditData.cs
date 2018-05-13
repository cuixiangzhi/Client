using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LoopType
{
	Once = 1,
	PingPang = 2,
	Loop = 3,
}

[ExecuteInEditMode]
public class PathEditData : IEditEventSender
{
	public override TriggerTargetType ToTargetType()
	{
		return TriggerTargetType.Path;
	}

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.PathEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(PathEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	public LoopType type = LoopType.Once;
	public List<PointEditData> points = new List<PointEditData>();

    [HideInInspector]
	private MapEditConfigData.PathInfo _data = null;
	public virtual MapEditConfigData.PathInfo Data
	{
		get
		{
			if(_data == null)
			{
				_data = new MapEditConfigData.PathInfo();
			}

			_data.id = id;
			_data.name = name;
			_data.pathType = (int)type;
			_data.points.Clear();
			foreach(var p in points)
			{
				_data.points.Add(p.Data);
			}

			_data.isAutoPathFindingPath = false;

			return _data;
		}

		set
		{
			_data = value;

			id = value.id;
			name = value.name;
			type = value.pathType == 0 ? LoopType.Once : (LoopType)value.pathType;

			points.Clear();
            Transform root = MapEditorUtlis.GetEditRoot();
			PointEditData[] srcs = root.GetComponentsInChildren<PointEditData>();

			for(int i = 0 ; i < value.points.Count; i++)
			{
				MapEditConfigData.PointInfo info = value.points[i];
				foreach(var src in srcs)
				{
					if(src.id == info.id)
					{
						points.Add(src);
						break;
					}
				}
			}
		}
	}


	[HideInInspector]
	public Color lineColor = Color.green;
	void OnDrawGizmos()
	{
		lineColor = new Color(id & 1, (id & 2) != 0 ? 1 : 0, (id & 3) != 0 ? 1 : 0);
        transform.name = name;
		Color col = Gizmos.color;
		
		Gizmos.color = lineColor;

		if(points.Count == 0)
			return;

		foreach(var p in points)
		{
			if(p == null)
			{
				return;
			}
		}
		
		Vector3 beginPoint = points[0].Data.position.ToVector3();
		
		Vector3 firstPoint = beginPoint;

		for (int i = 1; i < points.Count; i++)
		{
			Vector3 endPoint = points[i].Data.position.ToVector3();

			Gizmos.DrawLine(beginPoint,endPoint);
			
			beginPoint = endPoint;
		}

		if(type == LoopType.Loop)
		{
			Gizmos.DrawLine(firstPoint,beginPoint);
		}
		
		Gizmos.color = col;
	}
}

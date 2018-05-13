using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SurveyedAreaEditData : IEditEventSender 
{    
	public StageSurveyedAreaType ArrayType = StageSurveyedAreaType.Circle;
//	public Vector3 position = Vector3.zero;
	[HideInInspector]
	public float Radius = 0f;
	[HideInInspector]
	public Vector3 PointLeftTop = Vector3.zero;
	[HideInInspector]
	public Vector3 PointRightTop = Vector3.zero;
	[HideInInspector]
	public Vector3 PointLeftBottom = Vector3.zero;
	[HideInInspector]
	public Vector3 PointRightBottom = Vector3.zero;
	[HideInInspector]
	public bool freezeHeight = true;
	[HideInInspector]
	public Space anchor_Space = Space.Self;

	Vector3 orgPos;

	public override TriggerTargetType ToTargetType() { return TriggerTargetType.SurveyedArea; }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.SurveyedAreaEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    public override System.Type GetBaseType()
    {
        return typeof(SurveyedAreaEditData);
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

	MapEditConfigData.SurveyedAreaInfo  _data = null;
	public MapEditConfigData.SurveyedAreaInfo Data
    {
        get
        {
			if(_data == null)
			{
				_data = new MapEditConfigData.SurveyedAreaInfo();
			}
			_data.id = id;
			_data.name = name;
			_data.type = (int)ArrayType;
			_data.position = this.transform.position.ToMapEditConfigDataVector3();
			_data.radius = Radius;
			_data.pointLeftTop = PointLeftTop.ToMapEditConfigDataVector3();
			_data.pointRightTop = PointRightTop.ToMapEditConfigDataVector3();
			_data.pointLeftBottom = PointLeftBottom.ToMapEditConfigDataVector3();
			_data.pointRightBottom = PointRightBottom.ToMapEditConfigDataVector3();


            return _data;
        }

		set
		{
			id = value.id;
			name = value.name;
			ArrayType = (StageSurveyedAreaType)value.type;
			this.transform.position = value.position.ToVector3();
			Radius = value.radius;
			if((StageSurveyedAreaType)value.type == StageSurveyedAreaType.Square)
			{
                PointLeftTop = new Vector3(value.pointLeftTop.x, value.pointLeftTop.y, value.pointLeftTop.z);
                PointRightTop = new Vector3(value.pointRightTop.x, value.pointRightTop.y, value.pointRightTop.z);
                PointLeftBottom = new Vector3(value.pointLeftBottom.x, value.pointLeftBottom.y, value.pointLeftBottom.z);
                PointRightBottom = new Vector3(value.pointRightBottom.x, value.pointRightBottom.y, value.pointRightBottom.z);
			}
		}
    }

    void Awake()
    {
		_data = new MapEditConfigData.SurveyedAreaInfo();
		orgPos = transform.position;
	}

	void Update()
	{
		this.transform.name = name;
	}

	void OnDrawGizmos()
	{
		Color tmp = Gizmos.color;
		Gizmos.color = Color.green;
		Vector3 position = transform.position;
		if(ArrayType == StageSurveyedAreaType.Circle)
		{
			DrawCircle(position, Radius);
		}
		else if(ArrayType == StageSurveyedAreaType.Square)
		{
			if(freezeHeight)
			{
				PointLeftTop.y = PointRightTop.y = PointRightBottom.y = PointLeftBottom.y = transform.position.y;
			}
			if(anchor_Space == Space.Self)
			{
				if(transform.position != orgPos)
				{
					MoveAnchor(transform.position - orgPos);
				}
			}
			DrawSquare(PointLeftTop,PointRightTop,PointRightBottom,PointLeftBottom);
		}
		Gizmos.color = tmp;
		orgPos = transform.position;
	}

	void DrawCircle(Vector3 center, float radius)
	{
		Vector3 start = center + new Vector3(radius, 0, 0);
		Vector3 end;
		float theta = 0;
		for(int i = 1; i < 360; i+=2)
		{
			theta = i * Mathf.Deg2Rad;
			end = center + new Vector3(radius * Mathf.Cos(theta), 0, radius * Mathf.Sin(theta));
			Gizmos.DrawLine(start, end);
			Gizmos.DrawLine(center,end);
			start = end;
		}
	}

	void DrawSquare(Vector3 lefttop, Vector3 righttop, Vector3 rightbottom, Vector3 leftbottom)
	{
		Gizmos.DrawLine(lefttop, righttop);
		Gizmos.DrawLine(righttop, rightbottom);
		Gizmos.DrawLine(rightbottom, leftbottom);
		Gizmos.DrawLine(leftbottom, lefttop);
	}

	void MoveAnchor(Vector3 delta)
	{
		PointLeftTop += delta;
		PointRightTop += delta;
		PointRightBottom += delta;
		PointLeftBottom += delta;
	}
}

using UnityEngine;

[ExecuteInEditMode]
public class AutoPathEditData : PathEditData
{
    public override TriggerTargetType ToTargetType()
    {
        return TriggerTargetType.Path;
    }

    public override int MinIdValue()
    {
        return (int)TriggerEditorConstance.PathEditorMask + MapEditorCons.PRESERVED_NUMBER;
    }

    protected override void OnClone(IEditEventSender parent)
    {
        this.id = MapEditorUtlis.GenGID(this);
    }

    public AutoPointEditData[] autoPoints;

    [SerializeField][HideInInspector]
    MapEditConfigData.PathInfo _data = null;
    public override MapEditConfigData.PathInfo Data
    {
        get
        {
            if (_data == null)
            {
                _data = new MapEditConfigData.PathInfo();
            }

            _data.id = id;
            _data.name = name;
            _data.pathType = (int)type;
            _data.points.Clear();
            foreach (var p in autoPoints)
            {
                _data.points.Add(p.Data);
            }

            _data.isAutoPathFindingPath = true;

            return _data;
        }

        set
        {
            _data = value;

            id = value.id;
            name = value.name;
            type = (LoopType)value.pathType;

			autoPoints = new AutoPointEditData[value.points.Count];
            Transform root = MapEditorUtlis.GetAutoPointEditRoot();
            AutoPointEditData[] srcs = root.GetComponentsInChildren<AutoPointEditData>();
			for (int i = 0; i < value.points.Count; i++)
            {
				MapEditConfigData.PointInfo info = value.points[i];
                foreach (var src in srcs)
                {
                    if (src.id == info.id)
                    {
                        autoPoints[i] = src;
                        break;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        transform.name = name;
        Color col = Gizmos.color;
        Gizmos.color = Color.cyan;

        if (autoPoints.Length == 0)
            return;

       Vector3 beginPoint = autoPoints[0].Data.position.ToVector3();

        Vector3 firstPoint = beginPoint;

        for (int i = 1; i < autoPoints.Length; i++)
        {
            Vector3 endPoint = autoPoints[i].Data.position.ToVector3();

            Gizmos.DrawLine(beginPoint, endPoint);

            beginPoint = endPoint;
        }

        if (type == LoopType.Loop)
        {
            Gizmos.DrawLine(firstPoint, beginPoint);
        }

        Gizmos.color = col;
    }
}

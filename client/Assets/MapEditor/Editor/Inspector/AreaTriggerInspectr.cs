using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SurveyedAreaEditData))]
public class AreaTriggerInspectr : Editor {
	SurveyedAreaEditData test;
//	Vector3[] points;
	void OnEnable()
	{
		test = target as SurveyedAreaEditData;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		if(test.ArrayType == StageSurveyedAreaType.Square)
		{
			test.PointLeftTop = EditorGUILayout.Vector3Field("Left Top",test.PointLeftTop);
			test.PointRightTop = EditorGUILayout.Vector3Field("Right Top",test.PointRightTop);
			test.PointRightBottom = EditorGUILayout.Vector3Field("Right Bottom",test.PointRightBottom);
			test.PointLeftBottom = EditorGUILayout.Vector3Field("Left Bottom",test.PointLeftBottom);
			test.anchor_Space = (Space)EditorGUILayout.EnumPopup(new GUIContent("顶点锁定","exp:The vertexes will move when the trigger moves if self selected."), test.anchor_Space) ;
			test.freezeHeight = EditorGUILayout.Toggle(new GUIContent("FreezeHeight","All the points will be lock to the new height aligned to this gameobject"),test.freezeHeight);
		}
		else if(test.ArrayType == StageSurveyedAreaType.Circle)
		{
			test.Radius = EditorGUILayout.FloatField(new GUIContent("Radius"), test.Radius);
		}
		SceneView.RepaintAll();
	}

	void OnSceneGUI()
	{
		if(test.ArrayType == StageSurveyedAreaType.Square)
		{
			if(test.PointLeftTop.x == 0 && test.PointLeftTop.z == 0 
			   && test.PointRightTop.x == 0 && test.PointRightTop.z == 0
			   && test.PointRightBottom.x == 0 && test.PointRightBottom.z == 0
			   && test.PointLeftBottom.x == 0 && test.PointLeftBottom.z == 0)
			{
				test.PointLeftTop = test.transform.position + new Vector3(-1,0,1);
				test.PointRightTop = test.transform.position + new Vector3(1,0,1);
				test.PointRightBottom = test.transform.position + new Vector3(1,0,-1);
				test.PointLeftBottom = test.transform.position + new Vector3(-1,0,-1);
			}
			test.PointLeftTop = Handles.PositionHandle(test.PointLeftTop,Quaternion.identity);
			test.PointRightTop = Handles.PositionHandle(test.PointRightTop, Quaternion.identity);
			test.PointRightBottom = Handles.PositionHandle(test.PointRightBottom, Quaternion.identity);
			test.PointLeftBottom = Handles.PositionHandle(test.PointLeftBottom, Quaternion.identity);
			Undo.RecordObject(test,"Modify");
		}else if(test.ArrayType == StageSurveyedAreaType.Circle)
		{
			test.Radius = Handles.ScaleValueHandle(test.Radius, test.Radius * Camera.current.transform.right+ test.transform.position, Quaternion.LookRotation(Camera.current.transform.right), 1, Handles.DotCap, 1);
			Undo.RecordObject(test,"Modify");
		}
	}
}

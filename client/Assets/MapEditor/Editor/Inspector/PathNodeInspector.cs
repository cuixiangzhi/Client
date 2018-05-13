using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(PointEditData))]
public class PathNodeInspector : Editor {

	PointEditData shit;
	void OnEnable()
	{
		shit = target as PointEditData;
//		int repeatID = -1;
//		PointEditData[] brothers = shit.transform.parent.GetComponentsInChildren(typeof(PointEditData)) as PointEditData[];
//		for(int i = 0; brothers != null && i < brothers.Length; i++)
//		{
//			if(shit.transform != brothers[i])
//			{
//				if(shit.id == brothers[i].id)
//				{
//					EditorUtility.DisplayDialog("悲剧了","有跟这个重名重ID的路径点,解决一下?","中");
//				}
//			}
//		}
	}

//	public override void OnInspectorGUI ()
//	{
//		base.OnInspectorGUI ();
//	}

	public void OnSceneGUI()
	{
		Color tmp = GUI.color;
		GUI.color = Color.black;
		Handles.Label(shit.transform.position + Vector3.one, new GUIContent("ID: "+shit.id,"BLABLABLABLABLA"));
		GUI.color = tmp;
	}
}

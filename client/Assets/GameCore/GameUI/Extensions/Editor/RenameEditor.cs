using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class RenameHelper : EditorWindow
{
    [MenuItem("NGUI/RenameHelper")]
	static void Init(){
        RenameHelper window = (RenameHelper)EditorWindow.GetWindow(typeof(RenameHelper));
        window.Show();
	}

    private string prefix;
    private string suffix;
    private string BaseCountStr;
    private int BaseCountint;
	
	void OnGUI() {
        if (Selection.activeGameObject)
            Selection.activeGameObject.name = EditorGUILayout.TextField("当前选择节点", Selection.activeObject.name, GUILayout.Width(400));

        prefix = EditorGUILayout.TextField("前缀",prefix, GUILayout.Width(400));
        suffix = EditorGUILayout.TextField("后缀", suffix, GUILayout.Width(400));
        BaseCountStr = EditorGUILayout.TextField("基数", BaseCountStr, GUILayout.Width(400));
        if (GUILayout.Button("执行重命名"))
        {
            if (!int.TryParse(BaseCountStr, out BaseCountint))
            {
                this.ShowNotification(new GUIContent("please Input BaseCount !! "));
            }
            else if (Selection.activeGameObject == null)
            {
                this.ShowNotification(new GUIContent(" please select a gaemObject "));
            }
            else
            {
                setChildNameSort(Selection.activeGameObject, prefix, suffix, BaseCountint);
            }
            EditorGUILayout.BeginVertical();
        }
	}

    private void setChildNameSort(GameObject parent, string prefix, string suffix, int BaseCountint)
    {
        int childCount = parent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            parent.transform.GetChild(i).name = prefix + (i + BaseCountint) + suffix;
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DoorEditData))]
public class DoorEditInspector : Editor {

	public enum DoorState
	{
		关 = 0,
		开 = 1
	}

	DoorEditData _target;
	MapEditConfigData.DoorInfo info;

	GameObject tmpTarget;
    GameObject tmpModel;

	void OnEnable()
	{
		_target = target as DoorEditData;
		info = _target.Data;
		tmpTarget = _target.target;
        tmpModel = _target.model;
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Label(string.Format("门ID:{0}", info.id));
		GUILayout.Label(string.Format("门flag:{0}", info.flag));

		tmpTarget = (GameObject)EditorGUILayout.ObjectField(new GUIContent("绑定到门", "找到美术给出一个带BoxCollider的Door,拖选至此."),tmpTarget, typeof(GameObject), true);
		if(_target.target != tmpTarget)
		{
			_target.target = tmpTarget;
			info = _target.Data;
		}
        tmpModel = (GameObject)EditorGUILayout.ObjectField(new GUIContent("门的模型", "Door的子物体, 拖选至此."), tmpModel, typeof(GameObject), true);
        if (tmpModel != null)
        {
            if (tmpModel.transform.parent == tmpTarget.transform)
            {
                if (_target.model != tmpModel)
                {
                    _target.model = tmpModel;
                    info = _target.Data;
                }
            }
            else
            {
                if (_target.model != null)
                {
                    _target.model = null;
                    info = _target.Data;
                }
                EditorGUILayout.HelpBox(string.Format("必须是{0}的子节点!", tmpTarget.name), MessageType.Error);
            }
        }
        else if(_target.model != null)
        {
            _target.model = null;
            info = _target.Data;
        }
		if(tmpTarget == null)
		{
			EditorGUILayout.HelpBox("不可为空!",MessageType.Error);
		}

		GUILayout.Label("\n门空间信息(相对世界坐标)");
		if(info.position == null)
			info.position = new MapEditConfigData.MP_Vector3();
		GUILayout.Label("\t门位置: " + info.position.ToVector3());
		if(info.rotation == null)
			info.rotation = new MapEditConfigData.MP_Vector3();
		GUILayout.Label("\t门旋转: " + info.rotation.ToVector3());
		if(info.scale == null)
			info.scale = new MapEditConfigData.MP_Vector3();
		GUILayout.Label("\t门缩放: " + info.scale.ToVector3());
		GUILayout.Label("");

		DoorState _doorState = (DoorState)info.state;
		_doorState = (DoorState)EditorGUILayout.EnumPopup(new GUIContent("默认开关状态", "状态:\n  关闭:不可通过\n   开启:可通过"), _doorState);
		info.state = (int)_doorState;
        //info.modelName = _target.model == null ? "" : _target.model.name;

		_target.Data = info;
	}
}

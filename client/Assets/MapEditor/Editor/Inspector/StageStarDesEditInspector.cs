using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Text;

[CustomEditor(typeof(StageStarDesEditData))]
public class StageStarDesEditInspector : Editor
{
	StageStarDesEditData editObj = null;

	void OnEnable()
	{
		editObj = target as StageStarDesEditData;
        for(int i = 0 ; i < editObj.GetWrapperData().Count; i++)
        {
            StageStarDesInfoWrapper wr = editObj.GetWrapperData()[i];
			if(wr._data.bindTarget == null)
			{
				wr._data.bindTarget = new MapEditConfigData.TriggerTargetInfo();
			}

            if(wr._data.bindTarget.id != 0)
            {
                Transform root = MapEditorUtlis.GetEditRoot();
                IEditEventSender[] srcs = root.GetComponentsInChildren<IEditEventSender>();
                for(int j = 0; j < srcs.Length; j++)
                {
                    IEditEventSender src = srcs[j];
                    if(src.ID == wr._data.bindTarget.id 
                        && src.ToTargetType() == (TriggerTargetType)wr._data.bindTarget.type)
                    {
                        wr._bindObject = src;
                        break;
                    }
                }
            }
        }
	}
	
	public override void OnInspectorGUI()
	{
		__DrawStarData();
		
		Repaint();
	}
	
	//画编辑触发器界面
	void __DrawStarData()
	{
		EditorGUILayout.BeginVertical();

		GUILayout.Label("\t");

        for (int i = 0; i < editObj.GetWrapperData().Count; i++)
        {
            __DrawStarDesCell(editObj.GetWrapperData()[i], i);
        }

        if (editObj.GetWrapperData().Count < MapEditorCons.MAX_STAGE_STAR_DES 
            && GUILayout.Button("+", __GetOption(25f)))
        {
            StageStarDesInfoWrapper data = new StageStarDesInfoWrapper();
            editObj.GetWrapperData().Add(data);
        }

        GUILayout.Label("\t");
        EditorGUILayout.EndVertical();
	}

    void __DrawStarDesCell(StageStarDesInfoWrapper cell, int index)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", __GetOption(25f)))
        {
            editObj.GetWrapperData().Remove(cell);
        }

        EditorGUILayout.LabelField(string.Format("星星{0}描述:", index + 1), __GetOption(60));
        cell._data.des = EditorGUILayout.TextField(cell._data.des, __GetOption(160));
        
        EditorGUILayout.LabelField("", __GetOption(10));

        EditorGUILayout.LabelField(string.Format("绑定目标:"), __GetOption(60));
        cell._bindObject = EditorGUILayout.ObjectField(cell._bindObject, typeof(IEditEventSender),true, __GetOption(130));
        if (cell._bindObject != null)
        {
            if(
                cell._bindObject.GetType() != typeof(MonsterEditData)
                && cell._bindObject.GetType() != typeof(PlayerEditData)
                && cell._bindObject.GetType() != typeof(ServantEditData)
                && cell._bindObject.GetType() != typeof(TowerEditData)
                && cell._bindObject.GetType() != typeof(MonsterGroupEditData)
                && cell._bindObject.GetType() != typeof(TimmerEditData)
                && cell._bindObject.GetType() != typeof(ValueEditData)
                //&& cell._bindObject.GetType() != typeof(IntValueEditData)
                )
            {
                cell._bindObject = null;
                cell._data.bindTarget.type = 0;
                cell._data.bindTarget.id = 0;
            }
            else
            {
                cell._data.bindTarget.id = (int)(cell._bindObject as IEditEventSender).ID;
                cell._data.bindTarget.type = (int)(cell._bindObject as IEditEventSender).ToTargetType();
            }
        }

        if (null != cell._bindObject)
        {
            switch((TriggerTargetType)cell._data.bindTarget.type)
            {
                case TriggerTargetType.Monster:
                case TriggerTargetType.Character:
                case TriggerTargetType.Tower:
                    EditorGUILayout.LabelField("绑定属性：", __GetOption(60));
                    cell._data.bindTargetAttr = (int)(CharacterBindAttrType)EditorGUILayout.EnumPopup((CharacterBindAttrType)cell._data.bindTargetAttr, __GetOption(100));
                    break;
                case TriggerTargetType.MonsterGroup:
                    EditorGUILayout.LabelField("绑定属性：", __GetOption(60));
                    EditorGUILayout.LabelField("存在状态", __GetOption(100));
                    break;
                case TriggerTargetType.Timmer:
                    EditorGUILayout.LabelField("计时类型:", __GetOption(60));
                    cell._data.timmerShowType = (int)(TimmerCountType)EditorGUILayout.EnumPopup((TimmerCountType)cell._data.timmerShowType, __GetOption(100));
                    break;
                default:
                    break;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    GUILayoutOption[] tmpOpt = new GUILayoutOption[1];
    GUILayoutOption[] __GetOption(float width)
    {
        tmpOpt[0] = GUILayout.Width(width);

        return tmpOpt;
    }
}

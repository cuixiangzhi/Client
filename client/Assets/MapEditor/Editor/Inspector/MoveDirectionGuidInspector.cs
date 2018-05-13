using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MoveDirectionGuidEditData))]
public class MoveDirectionGuidInspector : Editor
{
    MoveDirectionGuidEditData editObj = null;

    void OnEnable()
    {
        editObj = target as MoveDirectionGuidEditData;
        Transform root = MapEditorUtlis.GetEditRoot();
        IEditEventSender[] srcs = root.GetComponentsInChildren<IEditEventSender>();

        for (int i = 0; i < editObj.Data.Count; i++)
        {
            MapEditConfigData.MoveDirectionGuidData data = editObj.Data[i];
            data.groupNumber = data.monsterGroups.Count;
            foreach (var gp in data.monsterGroups)
            {
                if(gp.id != 0 && gp.type != 0)
                {
                    for (int j = 0; j < srcs.Length; j++)
                    {
                        IEditEventSender src = srcs[j];
                        if (src.ID == gp.id && src.ToTargetType() == (TriggerTargetType)gp.type)
                        {
                            gp.targetObj = src;
                            break;
                        }
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        __DrawData();

        Repaint();
    }

    //画编辑触发器界面
    void __DrawData()
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Label("\t");

        for (int i = 0; i < editObj.Data.Count; i++)
        {
            __DrawStarDesCell(editObj.Data[i], i);
            EditorGUILayout.LabelField("");
        }

        if (GUILayout.Button("+", __GetOption(25f)))
        {
            MapEditConfigData.MoveDirectionGuidData data = new MapEditConfigData.MoveDirectionGuidData();
            editObj.Data.Add(data);
        }

        GUILayout.Label("\t");
        EditorGUILayout.EndVertical();
    }

    void __DrawStarDesCell(MapEditConfigData.MoveDirectionGuidData cell, int index)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", __GetOption(25f)))
        {
            editObj.Data.Remove(cell);
        }

        EditorGUILayout.LabelField(string.Format("箭头{0}绑定怪物组个数:", index + 1), __GetOption(120));
        cell.groupNumber = EditorGUILayout.IntField(cell.groupNumber, __GetOption(30));
        
        if(cell.groupNumber != cell.monsterGroups.Count)
        {
            if(cell.groupNumber < cell.monsterGroups.Count)
            {
                for(int i = cell.monsterGroups.Count-1; i >= cell.groupNumber; i--)
                {
                    cell.monsterGroups.Remove(cell.monsterGroups[i]);
                }
            }
            else
            {
                for (int i = cell.monsterGroups.Count; i < cell.groupNumber; i++)
                {
                    MapEditConfigData.TriggerTargetInfo data = new MapEditConfigData.TriggerTargetInfo();
                    data.targetObj = new Object();
                    cell.monsterGroups.Add(data);
                }
            }
        }
        EditorGUILayout.BeginVertical();
        foreach (var gp in cell.monsterGroups)
        {
            gp.targetObj = EditorGUILayout.ObjectField(gp.targetObj, typeof(IEditEventSender),true, __GetOption(130));
            //过滤,只能拖选怪物组或者防御塔
            if(gp.targetObj != null)
            {
                IEditEventSender obj = gp.targetObj as IEditEventSender;
                switch (obj.ToTargetType())
                {
                    case TriggerTargetType.Tower:
                        TowerEditData tedit = gp.targetObj as TowerEditData;
                        gp.id = tedit.id;
                        gp.type = (int)tedit.ToTargetType();
                        break;
                    case TriggerTargetType.MonsterGroup:
                        MonsterGroupEditData medit = gp.targetObj as MonsterGroupEditData;
                        gp.id = medit.id;
                        gp.type = (int)medit.ToTargetType();
                        break;
                    default:
                        gp.id = 0;
                        gp.type = 0;
                        break;
                }
            }
            else
            {
                gp.id = 0;
                gp.type = 0;
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    GUILayoutOption[] tmpOpt = new GUILayoutOption[1];
    GUILayoutOption[] __GetOption(float width)
    {
        tmpOpt[0] = GUILayout.Width(width);

        return tmpOpt;
    }
}

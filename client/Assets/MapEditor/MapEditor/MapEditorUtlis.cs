using UnityEngine;
using System.Collections;

public class MapEditorUtlis
{
    public static GameObject FindEditRoot()
    {
        return GameObject.Find("MapEditorRoot");
    }

    public static Transform GetEditRoot()
    {
        GameObject root = GameObject.Find("MapEditorRoot");
        if(null == root)
        {
            root = new GameObject("MapEditorRoot");
            root.transform.localPosition = Vector3.zero;
            root.transform.localEulerAngles = Vector3.zero;
            root.transform.localScale = Vector3.one;
        }

        return root.transform;
    }

    public static Transform GetPlayerEditRoot()
    {
        return __GetTempEditRoot("PlayerRoot");
    }

    public static Transform GetServantEditRoot()
    {
        return __GetTempEditRoot("ServantRoot");
    }

    public static Transform GetTriggerManEditRoot()
    {
        return __GetTempEditRoot("TriggerMan");
    }

    public static Transform GetTriggerEditRoot()
    {
        return __GetTempEditRoot("TriggerRoot");
    }

    public static Transform GetSurveyAreaRootEditRoot()
    {
        return __GetTempEditRoot("SurveyAreaRoot");
    }

    public static Transform GetTimmerEditRoot()
    {
        return __GetTempEditRoot("TimmerRoot");
    }

    public static Transform GetValueEditRoot()
    {
        return __GetTempEditRoot("ValueRoot");
    }

    public static Transform GetDoorEditRoot()
    {
        return __GetTempEditRoot("DoorRoot");
    }

    public static Transform GetPointEditRoot()
    {
        return __GetTempEditRoot("PointRoot");
    }

    public static Transform GetAutoPointEditRoot()
    {
        return __GetTempEditRoot("AutoPointRoot");
    }

    public static Transform GetAutoPathEditRoot()
    {
        return __GetTempEditRoot("AutoPathRoot");
    }

    public static Transform GetPathEditRoot()
    {
        return __GetTempEditRoot("PathRoot");
    }

    public static Transform GetTowerEditRoot()
    {
        return __GetTempEditRoot("TowerRoot");
    }

    public static Transform GetBarracksEditRoot()
    {
        return __GetTempEditRoot("BarracksRoot");
    }

    public static Transform GetMonsterEditRoot()
    {
        return __GetTempEditRoot("MonsterRoot");
    }

    public static Transform GetClientCharacterEditRoot()
    {
        return __GetTempEditRoot("ClientCharacterRoot");
    }

    public static Transform GetCameraPathEditRoot()
    {
        return __GetTempEditRoot("CameraPathRoot");
    }

    public static Transform GetStageStarDesEditRoot()
    {
        return __GetTempEditRoot("StageStarDes");
    }

    public static Transform GetStageMoveDirectionEditRoot()
    {
        return __GetTempEditRoot("StageMoveDirection");
    }

    public static Transform GetCameraEditRoot()
    {
        return __GetTempEditRoot("CameraRoot");
    }
    public static Transform GetCameraTargetRoot()
    {
        return __GetTempEditRoot("CameraTargetRoot");
    }

    private static Transform __GetTempEditRoot(string name)
    {
        Transform root = GetEditRoot();
        Transform tmpRoot = root.Find(name);
        if (null == tmpRoot)
        {
            tmpRoot = new GameObject(name).transform;
        }

        tmpRoot.transform.parent = root;
        tmpRoot.transform.localPosition = Vector3.zero;
        tmpRoot.transform.localEulerAngles = Vector3.zero;
        tmpRoot.transform.localScale = Vector3.one;

        return tmpRoot.transform;
    }

    //为每个触发器生成该场景内唯一ID
    public static int GenGID(IEditEventSender t)
    {
        Transform root = MapEditorUtlis.GetEditRoot();

        if(t is PlayerEditData)
            return (int)TriggerEditorConstance.PlayerEditorID;

        Component[] srcs = root.GetComponentsInChildren(t.GetBaseType());

        int gid = 0;
        if (null == srcs || srcs.Length == 0)
        {
            gid = t.MinIdValue();
        }
        else
        {
            int max = 0xFF;
            foreach (var src in srcs)
            {
                IEditEventSender edit = src as IEditEventSender;
                if (null == edit)
                    continue;

                if (max < edit.id)
                {
                    max = edit.id;
                }
            }

            gid = max + 1;
        }

        return gid;
    }

    //为每个触发器生成该场景内唯一ID
    public static int GenReservedGID(EditerObjectType type)
    {
        Transform root = MapEditorUtlis.GetEditRoot();

        IEditEventSender[] srcs = null;
        int maxReservedId = 0;

        switch (type)
        {
            case EditerObjectType.EOT_Player:
                srcs = root.GetComponentsInChildren<PlayerEditData>();
                maxReservedId = (int)TriggerEditorConstance.PlayerEditorID;
                break;
            case EditerObjectType.EOT_Enemy:
                srcs = root.GetComponentsInChildren<MonsterEditData>();
                maxReservedId = (int)TriggerEditorConstance.EnemyEditorID | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Servant:
                srcs = root.GetComponentsInChildren<ServantEditData>();
                maxReservedId = (int)TriggerEditorConstance.ServantEditorID | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Trigger:
                srcs = root.GetComponentsInChildren<TriggerEditData>();
                maxReservedId = (int)TriggerEditorConstance.TriggerEditorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Timmer:
                srcs = root.GetComponentsInChildren<TimmerEditData>();
                maxReservedId = (int)TriggerEditorConstance.TimmerEditorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_SurveyedArea:
                srcs = root.GetComponentsInChildren<SurveyedAreaEditData>();
                maxReservedId = (int)TriggerEditorConstance.SurveyedAreaEditorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Point:
                {
                    IEditEventSender[] srcs_points = root.GetComponentsInChildren<PointEditData>();
                    IEditEventSender[] srcs_autopoints = root.GetComponentsInChildren<AutoPointEditData>();
                    srcs = new IEditEventSender[srcs_points.Length + srcs_autopoints.Length];
                    srcs_points.CopyTo(srcs, 0);
                    srcs_autopoints.CopyTo(srcs, srcs_points.Length);

                    maxReservedId = (int)TriggerEditorConstance.PointEditorMask | MapEditorCons.PRESERVED_NUMBER;
                }
                break;
            case EditerObjectType.EOT_Value:
                srcs = root.GetComponentsInChildren<ValueEditData>();
                maxReservedId = (int)TriggerEditorConstance.ValueEditorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Tower:
                srcs = root.GetComponentsInChildren<TowerEditData>();
                maxReservedId = (int)TriggerEditorConstance.TowerEditorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Path:
                {
                    IEditEventSender[] srcs_path = root.GetComponentsInChildren<PathEditData>();
                    IEditEventSender[] srcs_autoPath = root.GetComponentsInChildren<AutoPathEditData>();
                    srcs = new IEditEventSender[srcs_path.Length + srcs_autoPath.Length];
                    srcs_path.CopyTo(srcs, 0);
                    srcs_autoPath.CopyTo(srcs, srcs_path.Length);

                    maxReservedId = (int)TriggerEditorConstance.PathEditorMask | MapEditorCons.PRESERVED_NUMBER;
                }
                break;
            case EditerObjectType.EOT_Door:
                srcs = root.GetComponentsInChildren<DoorEditData>();
                maxReservedId = (int)TriggerEditorConstance.DoorMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Barracks:
                srcs = root.GetComponentsInChildren<BarracksEditData>();
                maxReservedId = (int)TriggerEditorConstance.BarracksMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_CLIENT_CHARACTER:
                srcs = root.GetComponentsInChildren<ClientCharacterEditData>();
                maxReservedId = (int)TriggerEditorConstance.ClientCharacterMask | MapEditorCons.PRESERVED_NUMBER;
                break;
            case EditerObjectType.EOT_Cinema:
                srcs = root.GetComponentsInChildren<CinemaEditData>();
                break;
            default:
                break;
        }


        int gid = 0;
        if (null == srcs || srcs.Length == 0)
        {
            gid = 0;

            switch (type)
            {
                case EditerObjectType.EOT_Player:
                    gid = (int)TriggerEditorConstance.PlayerEditorID;
                    break;
                case EditerObjectType.EOT_Enemy:
                    gid = (int)TriggerEditorConstance.EnemyEditorID | gid;
                    break;
                case EditerObjectType.EOT_Servant:
                    gid = (int)TriggerEditorConstance.ServantEditorID | gid;
                    break;
                case EditerObjectType.EOT_Trigger:
                    gid = (int)TriggerEditorConstance.TriggerEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Timmer:
                    gid = (int)TriggerEditorConstance.TimmerEditorMask | gid;
                    break;
                case EditerObjectType.EOT_SurveyedArea:
                    gid = (int)TriggerEditorConstance.SurveyedAreaEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Point:
                    gid = (int)TriggerEditorConstance.PointEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Value:
                    gid = (int)TriggerEditorConstance.ValueEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Tower:
                    gid = (int)TriggerEditorConstance.TowerEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Path:
                    gid = (int)TriggerEditorConstance.PathEditorMask | gid;
                    break;
                case EditerObjectType.EOT_Door:
                    gid = (int)TriggerEditorConstance.DoorMask | gid;
                    break;
                case EditerObjectType.EOT_Barracks:
                    gid = (int)TriggerEditorConstance.BarracksMask | gid;
                    break;
                case EditerObjectType.EOT_CLIENT_CHARACTER:
                    gid = (int)TriggerEditorConstance.ClientCharacterMask | gid;
                    break;
            }
        }
        else
        {
            int max = 0xFF;
            foreach (var src in srcs)
            {
                if (max < src.id && src.id < maxReservedId)
                {
                    max = src.id;
                }
            }

            gid = max + 1;
        }

        return gid;
    }

    public static EditerObjectType GetObjectTypeById(int editorId)
    {
        return (EditerObjectType)((editorId & 0x000F0000) >> 16);
    }
}
    

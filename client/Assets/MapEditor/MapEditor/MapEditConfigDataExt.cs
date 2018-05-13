using UnityEngine;
using System.Collections;

namespace MapEditConfigData
{
    public partial class TriggerTargetInfo : global::ProtoBuf.IExtensible
    {
        public Object targetObj = null;
    }

    public partial class TriggerEventInfo : global::ProtoBuf.IExtensible
    {
        public int eventPopupIndex = 0;
    }

    public partial class ConditionCellInfo : global::ProtoBuf.IExtensible
    {
        public int attrPopupIndex = 0;
        public int enumPopupIndex = 0;
        public int excelPopupIndex = 0;
    }

    public partial class ConditionGridInfo : global::ProtoBuf.IExtensible
    {
        public bool gridFoldout = true;
    }

    public partial class TriggerHandlerCellInfo : global::ProtoBuf.IExtensible
    {
        public int handlerIdPopupIndex = 0;
    }

    public partial class TriggerHandlerParamInfo : global::ProtoBuf.IExtensible
    {
        public int tmpEnum = 0;
    }

    public partial class MoveDirectionGuidData : global::ProtoBuf.IExtensible
    {
        public int groupNumber = 0;
    }

    public partial class MP_Vector3 : global::ProtoBuf.IExtensible
    {
        public Vector3 ToVector3()
        {
            return new Vector3(x,y,z);
        }
    }
}

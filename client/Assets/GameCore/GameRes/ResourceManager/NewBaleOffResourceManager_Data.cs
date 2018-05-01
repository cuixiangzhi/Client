/********************************************************************************
 *	创建人：	 李彬
 *	创建时间：   2015-06-11
 *
 *	功能说明：  U3D资源载入入口，禁止程序自己使用底层接口调用。不在支持同步载入。
 *	                增加多管道资源载入。
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading;
using System.IO;

namespace Games.TLBB.Manager
{
	partial class NewBaleOffResourceManager : GameCore.BaseManager<NewBaleOffResourceManager>
    {
#if LOADLOG
        public static float s_singleFileTime = 0.03f;
        public static float s_TickFileTime    = 0.05f;
#endif
        //PriorityIndex池
        private Dictionary<int, UInt64> m_MultiPipeGenerateIndexDic = new Dictionary<int, UInt64>();
        //UnityEngine.Object资源缓存
        private Dictionary<string, UnityEngine.Object> m_ResourceLoadedObject = new Dictionary<string, UnityEngine.Object>();
        //需要加载的资源队列。。。
        private Dictionary<int, SortedDictionary<UInt64, ResourceData>> m_MultiPipeLoadResourceDic = new Dictionary<int, SortedDictionary<UInt64, ResourceData>>();
        //已经在加载资源队列。。。
        private Dictionary<int, List<ResourceData>> m_MultiPipeLoadingResourceDic = new Dictionary<int, List<ResourceData>>();
        //所有队列的位置。。。
        private Dictionary<int, ResourceData> m_ResouceDic = new Dictionary<int, ResourceData>();

        //跨场景加载。。。
        private SortedDictionary<int, ResourceData> m_DontDestroyResouceDic = new SortedDictionary<int, ResourceData>();
        private ResourceData m_LoadingDontDestroyResouce = null;
        private float m_BegLoadingDontDestroyResouceTime = 0;

        //已经完成加载的资源数据。。。
        private Dictionary<int, List<ResourceData>> m_MultiPipeInstantiateResourceDic = new Dictionary<int, List<ResourceData>>();

        private HashSet<Int32> m_StopResourceSet = new HashSet<int>();
        private List<ResourceData> m_LoadedResourceDataList = new List<ResourceData>();
        
        //上层自己取得数据池。。。
        private Dictionary<int, ResourceData> m_LoadedResourceDataDic = new Dictionary<int, ResourceData>();

        private Queue<ResourceData> m_RecycleResourceDataQueue = new Queue<ResourceData>();

        private int m_curFramePrioritySelfNum = 0;
        enum ResourcePriority:int
        {
            ResourcePriority_Min             = -2,
            ResourcePriority_SceneAsyn   = -1,
            ResourcePriority_Def              = 0,
            ResourcePriority_Pvw_            = 1, //保留无用字段
            ResourcePriority_ScenePvw    = 2,
            ResourcePriority_Self             = 3,
            ResourcePriority_SynText       = 4,
            ResourcePriority_Max            = 5,
        }
        public class ResourceData
        {
            public int loadedFrame = -1;
            public ResourceManager.DelegateResourceCallBack Call = null;
            //
            public ResourceRequest request = null;
            public UnityEngine.Object resObject = null;
            public byte[]                      resByte = null;
            //
            public bool                     remove  = false;
            //
            public String                   resPath                        = null;
            public bool                     isInstantiate                 = false;
            public bool                     isActive                        = false;
            public int                        index                           = -1;
            public ResourceType       resType                        = ResourceType.ResourceType_Invalid;

            private bool Valid = true;
            public bool isValid()
            {
                return Valid;
            }
            public ResourceData Valided()
            {
                this.Valid = true;
                return this;
            }
            public void Release()
            {
                remove = false;
                Valid = false;
                loadedFrame = -1;
                Call = null;
                request = null;
                resObject = null;
                resByte = null;
                resPath                        = null;
                isInstantiate                 = false;
                isActive                        = false;
                index                           = -1;
                resType                        = ResourceType.ResourceType_Invalid;
            }
        }
    }
}

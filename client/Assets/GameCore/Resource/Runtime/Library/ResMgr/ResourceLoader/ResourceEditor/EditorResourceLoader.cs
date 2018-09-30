/********************************************************************************
 *	创建人：	 李彬
 *	创建时间：   2016-05-12
 *
 *	功能说明：  
 *	
 *	修改记录：
*********************************************************************************/

using com.cyou.plugin.log;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.cyou.plugin.resource.loader.editor
{
    class EditorResourceLoader : ResourceLoader
    {
        private bool m_bInited = false;

        private bool isEditor = false;

        private String dataPath = null;
        public EditorResourceLoader()
        {
            isEditor = Application.isEditor;
            dataPath = Application.dataPath;
        }
       

        public override void Init()
        {
            if (m_bInited)
                return;
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_Async; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeLoadResourceDic[i] = new SortedDictionary<UInt64, ResourceData>();
                m_MultiPipeLoadingResourceDic[i] = new List<ResourceData>();
                m_MultiPipeInstantiateResourceDic[i] = new List<ResourceData>();
                m_MultiPipeGenerateIndexDic[i] = 0;
            }
            m_bInited = true;
        }
        public override void Release()
        {
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_Async; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeGenerateIndexDic[i] = 0;
            }

            foreach (KeyValuePair<int, SortedDictionary<UInt64, ResourceData>> data in m_MultiPipeLoadResourceDic)
            {
             
                data.Value.Clear();
            }

            foreach (KeyValuePair<int, List<ResourceData>> data in m_MultiPipeLoadingResourceDic)
            {
                data.Value.Clear();
            }
            
            m_ResouceDic.Clear();

            foreach (KeyValuePair<int, List<ResourceData>> data in m_MultiPipeInstantiateResourceDic)
            {              
                data.Value.Clear();
            }

            m_StopResourceSet.Clear();
            m_LoadedResourceDataList.Clear();
            
            m_RecycleResourceDataQueue.Clear();
        }

        private ResourcePriority GetResourcePriority(int priority)
        {
            //if( priority == (int)(ResourcePriority.ResourcePriority_SceneAsync)
            //    || priority == (int)(ResourcePriority.ResourcePriority_NULL) 
            //    || priority == (int)(ResourcePriority.ResourcePriority_ScenePvw)
            //  )
            //{
            //    throw new System.Exception(String.Format("{0}资源加载优先级错误！！！", priority));
            //}
            //if (priority <= (int)(ResourcePriority.ResourcePriority_Min) || priority >= (int)(ResourcePriority.ResourcePriority_Max))
            //{
            //    throw new System.Exception(String.Format("{0}资源加载优先级错误！！！", priority));
            //}
            //else
            //{
                if (priority == (int)(ResourcePriority.ResourcePriority_Async))
                {
                    return ResourcePriority.ResourcePriority_Def;
                }
                else
                {
                    return (ResourcePriority)(priority);
                }
            //}
        }
        
        private UInt64 GenerateIndex(ResourcePriority priority)
        {
            int pty = (int)priority;
            if (m_MultiPipeGenerateIndexDic.ContainsKey(pty))
            {
                UInt64 index = m_MultiPipeGenerateIndexDic[pty];
                m_MultiPipeGenerateIndexDic[pty] = ++index;
                return index;
            }
            else
            {
                throw new System.Exception(String.Format("{0}资源加载优先级错误！！！", pty));
            }
        }
  
        public override void DestroyResource(System.Object obj)
        {
            UnityEngine.Object uObj = obj as UnityEngine.Object;
            if (uObj != null)
            {
                if (uObj is UnityEngine.GameObject)
                {

                    if (m_vResourceLoadedGameObjectset.Contains(obj))
                    {
                        return;
                    }
                    else
                    {
                        GameObject.Destroy(uObj);
                    }
                }
                else
                {
                    if (uObj is UnityEngine.Component)
                    {
                        GameObject.Destroy(uObj);
                    }
                }
            }

        }
        //public override void DestroyResource(ref System.Object obj)
        //{
        //    DestroyResource(obj);
        //    obj = null;
        //}      
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        private ResourceData InstantiateResourceData()
        {
            if (m_RecycleResourceDataQueue.Count > 0)
            {
                return m_RecycleResourceDataQueue.Dequeue().Valided();
            }
            else
            {
                return (new ResourceData()).Valided();
            }
        }
        private void RecycleResourceData(ResourceData rData, bool bDestroy = true)
        {
            int index = rData.index;
            if (m_ResouceDic.ContainsKey(index))
                m_ResouceDic.Remove(index);
//             if (m_LoadedResourceDataDic.ContainsKey(index))
//                 m_LoadedResourceDataDic.Remove(index);

            if(bDestroy)
            {
                if (rData.resObject is GameObject)
                {
                    GameObject.Destroy(rData.resObject);
                }
                else
                {
                    Resources.UnloadAsset(rData.resObject);
                }
                if (rData.resObjectArray != null)
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        if (rData.resObjectArray[i] is GameObject)
                        {
                            GameObject.Destroy(rData.resObjectArray[i]);
                        }
                        else
                        {
                            Resources.UnloadAsset(rData.resObjectArray[i]);
                        }
                    }
                }
            }
            rData.config = null;
            rData.configArray = null;
            rData.resObject = null;
            rData.resObjectArray = null;

            if (rData.isValid())
            {
                rData.Release();
                m_RecycleResourceDataQueue.Enqueue(rData);
            }
            else
            {
                LogSystem.Error("ResourceData Error!!");
            }
        }

        //PriorityIndex池
        private Dictionary<int, UInt64> m_MultiPipeGenerateIndexDic = new Dictionary<int, UInt64>();
        //UnityEngine.Object资源缓存
        //private Dictionary<EditorResConfig, UnityEngine.Object> m_ResourceLoadedObject = new Dictionary<EditorResConfig, UnityEngine.Object>();
        //需要加载的资源队列。。。
        private Dictionary<int, SortedDictionary<UInt64, ResourceData>> m_MultiPipeLoadResourceDic = new Dictionary<int, SortedDictionary<UInt64, ResourceData>>();
        //已经在加载资源队列。。。
        private Dictionary<int, List<ResourceData>> m_MultiPipeLoadingResourceDic = new Dictionary<int, List<ResourceData>>();
        //所有队列的位置。。。
        private Dictionary<int, ResourceData> m_ResouceDic = new Dictionary<int, ResourceData>();

        //跨场景加载。。。
        private SortedDictionary<int, ResourceData> m_DontDestroyResouceDic = new SortedDictionary<int, ResourceData>();
        private ResourceData m_LoadingDontDestroyResouce = null;

        //已经完成加载的资源数据。。。
        private Dictionary<int, List<ResourceData>> m_MultiPipeInstantiateResourceDic = new Dictionary<int, List<ResourceData>>();

        private HashSet<Int32> m_StopResourceSet = new HashSet<int>();
        private List<ResourceData> m_LoadedResourceDataList = new List<ResourceData>();

        //上层自己取得数据池。。。
        //private Dictionary<int, ResourceData> m_LoadedResourceDataDic = new Dictionary<int, ResourceData>();

        private Queue<ResourceData> m_RecycleResourceDataQueue = new Queue<ResourceData>();

        private HashSet<System.Object> m_vResourceLoadedGameObjectset = new HashSet<System.Object>();

        public override UnityEngine.Object LoadResource(IResConfig rc)
        {
#if UNITY_EDITOR
            EditorResConfig config = (EditorResConfig)rc;

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.MainPath);
            if (obj != null && obj is GameObject)
            {
                if (m_vResourceLoadedGameObjectset.Contains(obj) == false)
                    m_vResourceLoadedGameObjectset.Add(obj);
            }
            return obj;
#else
            return null;
#endif



            //switch (config.MainPathType)
            //{
            //    case (byte)(PATH_TYPE.PATH_TYPE_RESOURCES):
            //        {
            //            UnityEngine.Object obj = Resources.Load(config.MainPath);
            //            if( obj != null && obj is GameObject)
            //            {
            //                if (m_vResourceLoadedGameObjectset.Contains(obj) == false)
            //                    m_vResourceLoadedGameObjectset.Add(obj);
            //            }
            //            return obj;
            //        }
            //    case (byte)(PATH_TYPE.PATH_TYPE_STREAMINGASSETS):
            //        {
            //            LogSystem.Error("LoadResource Cannot load streamingAssetsPath!");
            //            return null;
            //        }
            //    case (byte)(PATH_TYPE.PATH_TYPE_PERSISTENT):
            //        {
            //            LogSystem.Error("LoadResource Cannot load persistentDataPath!");
            //            return null;
            //        }
            //    default:
            //        LogSystem.Error("IResConfig.PathType is Invalid！{0} ", config.MainPathType);
            //        break;
            //}
            //return null;
        }
        public override UnityEngine.GameObject InstantiateGameObject(IResConfig rc, bool isActive)
        {
            EditorResConfig config = (EditorResConfig)rc;
            UnityEngine.GameObject obj = LoadResource(config) as GameObject;
            if (obj != null)
            {
                GameObject go =  GameObject.Instantiate(obj) as GameObject;
                if(go == null)
                {
                    return null;
                }
                go.SetActive(isActive);
                return go;

            }
            else
            {
                return null;
            }
            //switch (config.MainPathType)
            //{
            //    case (byte)(PATH_TYPE.PATH_TYPE_RESOURCES):
            //        {
            //            UnityEngine.GameObject obj = LoadResource(config) as GameObject;
            //            if( obj != null)
            //            {
            //                return GameObject.Instantiate(obj) as GameObject;
            //            }
            //            else
            //            {
            //                return null;
            //            }
            //        }
            //    case (byte)(PATH_TYPE.PATH_TYPE_STREAMINGASSETS):
            //        {
            //            LogSystem.Error("LoadResource Cannot load streamingAssetsPath!");
            //            return null;
            //        }
            //    case (byte)(PATH_TYPE.PATH_TYPE_PERSISTENT):
            //        {
            //            LogSystem.Error("LoadResource Cannot load persistentDataPath!");
            //            return null;
            //        }
            //    default:
            //        LogSystem.Error("IResConfig.PathType is Invalid！{0} ", config.MainPathType);
            //        break;
            //}
            //return null;
        }
        //public override String LoadTextResource(IResConfig rc)
        //{
        //    //todo
        //    //EditorResConfig config = (EditorResConfig)rc;
        //    //switch (config.MainPathType)
        //    //{
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_RESOURCES):
        //    //        {
        //    //            LogSystem.Error("LoadResource Cannot load Resources!");
        //    //            return null;
        //    //        }
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_STREAMINGASSETS):
        //    //        {
        //    //            return new String(System.Text.Encoding.UTF8.GetChars(CusEncoding.EncodingUtil.FileByteToLocal(LoadByteBuffer(config.MainPath, false))));
        //    //        }
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_PERSISTENT):
        //    //        {
        //    //            return new String(System.Text.Encoding.UTF8.GetChars(CusEncoding.EncodingUtil.FileByteToLocal(m_PersistentAssetsLoader.Load(config.MainPath))));
        //    //        }
        //    //    default:
        //    //        LogSystem.Error("IResConfig.PathType is Invalid！{0} ", config.MainPathType);
        //    //        break;
        //    //}
        //    return null;
        //}
        //public override Byte[] LoadByteResource(IResConfig rc)
        //{
        //    //EditorResConfig config = (EditorResConfig)rc;
        //    //switch (config.MainPathType)
        //    //{
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_RESOURCES):
        //    //        {
        //    //            LogSystem.Error("LoadResource Cannot load Resources!");
        //    //            return null;
        //    //        }
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_STREAMINGASSETS):
        //    //        {
        //    //            return LoadByteBuffer(config.MainPath,false);
        //    //        }
        //    //    case (byte)(PATH_TYPE.PATH_TYPE_PERSISTENT):
        //    //        {
        //    //            return m_PersistentAssetsLoader.Load(config.MainPath);
        //    //        }
        //    //    default:
        //    //        LogSystem.Error("IResConfig.PathType is Invalid！{0} ", config.MainPathType);
        //    //        break;
        //    //}
        //    //return null;
        //    return null;
        //}

        public override void LoadScene(IResConfig res)
        {
            EditorResConfig config = (EditorResConfig)res;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;            
            SceneManager.LoadScene(config.MainPath);
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {          
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            if (this.SceneLoadedCallBack != null)
                this.SceneLoadedCallBack(scene, loadSceneMode);
        }
        public override void StopLoading(Int32 index)
        {
            if (m_ResouceDic.ContainsKey(index) && (m_StopResourceSet.Contains(index) == false))
            {
                m_StopResourceSet.Add(index);
            }
        }
        public override bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            SortedDictionary<UInt64, ResourceData> sortedDic = m_MultiPipeLoadResourceDic[(int)rPriority];
            foreach (KeyValuePair<UInt64, ResourceData> KValue in sortedDic)
            {
                if (KValue.Key == srcIndex)
                {
                    ResourceData rData = KValue.Value;
                    sortedDic.Remove(srcIndex);
                    sortedDic.Add(destIndex, rData);
                    return true;
                }
            }
            return false;
        }
        public override Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority)
        {
            EditorResConfig config = (EditorResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            return index;
        }
        public override Int32 DontDestroyOnLoadResourceAsync(IResConfig rc, Int32 priority)
        {
            EditorResConfig config = (EditorResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            return index;
        }
        public override Int32 InstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority)
        {
            EditorResConfig config = (EditorResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            ResourcePriority rPriority = GetResourcePriority(priority);
            rData.index = index;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            ulong generateIndex = GenerateIndex(rPriority);
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        public override Int32 LoadResourceAsync(IResConfig rc, Int32 priority)
        {
            EditorResConfig config = (EditorResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        public override Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority)
        {
            EditorResConfig[] configArray = new EditorResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (EditorResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            return index;
        }
        public override Int32 DontDestroyOnLoadResourceAsync(IResConfig[] rcArray, Int32 priority)
        {
            EditorResConfig[] configArray = new EditorResConfig[rcArray.Length];
            for(int i=0;i<rcArray.Length;++i)
            {
                configArray[i] = (EditorResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            return index;
        }
        public override Int32 InstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority)
        {
            EditorResConfig[] configArray = new EditorResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (EditorResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            {
                ResourcePriority rPriority = GetResourcePriority(priority);
                ulong generateIndex = GenerateIndex(rPriority);
                do
                {
                    if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                    {
                        generateIndex = GenerateIndex(rPriority);
                    }
                    else
                    {
                        break;
                    }
                } while (true);
                m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);

            }
            return index;
        }
        public override Int32 LoadResourceAsync(IResConfig[] rcArray, Int32 priority)
        {
            EditorResConfig[] configArray = new EditorResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (EditorResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            {
                ResourcePriority rPriority = GetResourcePriority(priority);
                ulong generateIndex = GenerateIndex(rPriority);
                do
                {
                    if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                    {
                        generateIndex = GenerateIndex(rPriority);
                    }
                    else
                    {
                        break;
                    }
                } while (true);
                m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);

            }
            return index;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected bool RemoveLoadedResource(Int32 index)
        {
            ResourceData rData = null;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedResourceDataList[i];
                if (rData.index == index)
                {
                    m_LoadedResourceDataList.RemoveAt(i);
                    RecycleResourceData(rData);
                    return true;
                }
            }
            return false;
        }
        protected bool RemoveInstantiateResource(Int32 index)
        {
            int i; ResourceData rData;
            List<ResourceData> Value = null;
            Dictionary<int, List<ResourceData>>.Enumerator iter = m_MultiPipeInstantiateResourceDic.GetEnumerator();
            while (iter.MoveNext())
            {
                Value = iter.Current.Value;
                for (i = 0; i < Value.Count; ++i)
                {
                    rData = Value[i];
                    if (rData.index == index)
                    {
                        Value.RemoveAt(i);
                        RecycleResourceData(rData);
                        return true;
                    }
                }
            }
            return false;
        }
        protected bool RemoveLoadingResource(Int32 index)
        {
            int i;
            List<ResourceData> Value = null;
            Dictionary<int, List<ResourceData>>.Enumerator iter = m_MultiPipeLoadingResourceDic.GetEnumerator();
            while (iter.MoveNext())
            {
                Value = iter.Current.Value;
                for (i = 0; i < Value.Count; ++i)
                {
                    if (Value[i].index == index)
                    {
                        Value[i].remove = true;
                        return true;
                    }
                }
            }
            return false;
        }
        protected bool RemoveLoadResource(Int32 index)
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData> sortedDic = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator enu;
            Dictionary<int, SortedDictionary<UInt64, ResourceData>>.Enumerator iter = m_MultiPipeLoadResourceDic.GetEnumerator();
            while (iter.MoveNext())
            {
                sortedDic = iter.Current.Value;
                enu = sortedDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    while (true)
                    {
                        rData = enu.Current.Value;
                        if (rData.index == index)
                        {
                            sortedDic.Remove(enu.Current.Key);
                            RecycleResourceData(rData);
                            return true;
                        }
                        else
                        {
                            bool bRet = enu.MoveNext();
                            if (bRet == false)
                                break;
                        }
                    }
                }
            }
            return false;
        }
        protected void TickRemoveResource()
        {
            bool bRet;
            HashSet<Int32>.Enumerator iter = m_StopResourceSet.GetEnumerator();
            int index;
            while (iter.MoveNext())
            {
                index = iter.Current;
                bRet = ((((RemoveLoadedResource(index)) ?
                             true : RemoveInstantiateResource(index)) ?
                             true : RemoveLoadingResource(index)) ?
                             true : RemoveLoadResource(index));
                if (bRet == false)
                {
                    LogSystem.Error("RemoveResource {0} Not Find!!", index);
                }
            }
            m_StopResourceSet.Clear();
        }
        protected void TickDontDestroyOnLoad()
        {
            if (m_LoadingDontDestroyResouce != null)
            {
                {
                    if (m_LoadingDontDestroyResouce.resObject)
                    {

                        if (m_LoadingDontDestroyResouce.isInstantiate)
                        {
                            if((m_LoadingDontDestroyResouce.resObject is GameObject) == false)
                            {
                                LogSystem.Error("资源类型不能实例化!!!" + m_LoadingDontDestroyResouce.resObject.ToString());
                            }
                            m_LoadingDontDestroyResouce.resObject = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObject);
                            ((GameObject)m_LoadingDontDestroyResouce.resObject).SetActive(m_LoadingDontDestroyResouce.isActive);
                            GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObject);
                        }
                        ResourceCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObject);
                    }
                    else
                    {
                        if (m_LoadingDontDestroyResouce.isInstantiate)
                        {
                            for (int i = 0; i < m_LoadingDontDestroyResouce.resObjectArray.Length; ++i)
                            {
                                m_LoadingDontDestroyResouce.resObjectArray[i] = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObjectArray[i]);
                                ((GameObject)m_LoadingDontDestroyResouce.resObjectArray[i]).SetActive(m_LoadingDontDestroyResouce.isActive);
                                GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObjectArray[i]);
                            }
                        }
                        ResourceArrayCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObjectArray);
                    }
                    RecycleResourceData(m_LoadingDontDestroyResouce,false);
                    m_LoadingDontDestroyResouce = null;
                }
            }
            else
            {
                SortedDictionary<int, ResourceData>.Enumerator enu = m_DontDestroyResouceDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    m_LoadingDontDestroyResouce = enu.Current.Value;
                    m_DontDestroyResouceDic.Remove(enu.Current.Key);
                    if (m_LoadingDontDestroyResouce.configArray != null)
                    {
                        for(Int32 i=0;i<m_LoadingDontDestroyResouce.configArray.Length;++i)
                        {
                            m_LoadingDontDestroyResouce.resObjectArray[i] = LoadResource(m_LoadingDontDestroyResouce.configArray[i]);
                            if (m_LoadingDontDestroyResouce.resObjectArray[i] == null)
                            {
                                LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  resObjectArray == null", m_LoadingDontDestroyResouce.configArray[i].MainPath);
                                RecycleResourceData(m_LoadingDontDestroyResouce);
                                m_LoadingDontDestroyResouce = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        m_LoadingDontDestroyResouce.resObject = LoadResource(m_LoadingDontDestroyResouce.config);
                        if (m_LoadingDontDestroyResouce.resObject == null)
                        {
                            LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  request == null", m_LoadingDontDestroyResouce.config.MainPath);
                            RecycleResourceData(m_LoadingDontDestroyResouce);
                            m_LoadingDontDestroyResouce = null;
                        }
                    }
                }
            }
        }

        protected void TickLoadResource()
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator enu;
            bool bAsynPause;
            bool bRet = false;
            //bool bAsynPause = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Count > ResourceManager.FrameSceneLoadCount;
            //SortedDictionary<UInt64, ResourceData>.Enumerator enu;

            //if (bAsynPause == false)
            //{
            //    SortedDictionary<UInt64, ResourceData> sortedDic = m_MultiPipeLoadResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync];
            //    enu = sortedDic.GetEnumerator();
            //    if (enu.MoveNext())
            //    {
            //        while (true)
            //        {
            //            rData = enu.Current.Value;
            //            if( rData.remove)
            //            {
            //                RecycleResourceData(rData);
            //            }
            //            else
            //            {
            //                if (rData.configArray != null)
            //                {
            //                    bool bError = false;
            //                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
            //                    for (int i = 0; i < rData.configArray.Length; ++i)
            //                    {
            //                        rData.resObjectArray[i] = LoadResource(rData.configArray[i]);
            //                        if (rData.resObjectArray[i] == null)
            //                        {
            //                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainPath);
            //                            RecycleResourceData(rData);
            //                            bError = true;
            //                            break;
            //                        }
            //                    }
            //                    if (bError == false)
            //                        m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Add(rData);
            //                }
            //                else
            //                {
            //                    rData.resObject = LoadResource(rData.config);
            //                    if (rData.resObject == null)
            //                    {
            //                        LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainPath);
            //                        RecycleResourceData(rData);
            //                    }
            //                    else
            //                    {
            //                        m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Add(rData);
            //                    }
            //                }
            //            }

            //            sortedDic.Remove(enu.Current.Key);
            //            enu = sortedDic.GetEnumerator();
            //            bRet = enu.MoveNext();
            //            if (m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Count > ResourceManager.FrameSceneLoadCount)
            //                break;
            //            if (bRet == false)
            //                break;
            //        }
            //    }
            //}
            Dictionary<int, SortedDictionary<UInt64, ResourceData>>.Enumerator iter = m_MultiPipeLoadResourceDic.GetEnumerator();
            KeyValuePair<int, SortedDictionary<UInt64, ResourceData>> KValue;
            while (iter.MoveNext())
            {
                KValue = iter.Current;
                //if (KValue.Key == (int)ResourcePriority.ResourcePriority_SceneAsync)
                //{
                //    continue;
                //}
                //队列资源过多加速载入
                if (KValue.Key == (int)ResourcePriority.ResourcePriority_Def && (KValue.Value.Count > ResourceManager.ForceDefaultLoadCount))
                {
                    enu = KValue.Value.GetEnumerator();
                    while (enu.MoveNext())
                    {
                        rData = enu.Current.Value;

                        if (rData.remove)
                        {
                            RecycleResourceData(rData);
                        }
                        else
                        {
                            if (rData.configArray != null)
                            {
                                bool bError = false;
                                rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                for (int i = 0; i < rData.configArray.Length; ++i)
                                {
                                    rData.resObjectArray[i] = LoadResource(rData.configArray[i]);
                                    if (rData.resObjectArray[i] == null)
                                    {
                                        LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainPath);
                                        RecycleResourceData(rData);
                                        bError = true;
                                        break;
                                    }
                                }
                                if (bError == false)
                                    m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                            }
                            else
                            {
                                rData.resObject = LoadResource(rData.config);
                                if (rData.resObject == null)
                                {
                                    LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainPath);
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                                }
                            }
                        }
                       
//                         rData.request = Resources.LoadAsync(rData.config.MainPath);
//                         if (rData.request == null)
//                         {
//                             BaseLogSystem.Debug("Load Resource Failed!! resPath = {0}", rData.config.MainPath);
//                             RecycleResourceData(rData);
//                         }
//                         else
//                         {
//                             m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
//                         }
                        KValue.Value.Remove(enu.Current.Key);
                        enu = KValue.Value.GetEnumerator();
                    }
                }
                else
                ///////////////
                {
                    bAsynPause = m_MultiPipeLoadingResourceDic[KValue.Key].Count > ResourceManager.FrameSceneLoadCount;
                    if (bAsynPause == false)
                    {
                        enu = KValue.Value.GetEnumerator();
                        if (enu.MoveNext())
                        {
                            while (true)
                            {
                                rData = enu.Current.Value;

                                if (rData.remove)
                                {
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    if (rData.configArray != null)
                                    {
                                        bool bError = false;
                                        rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                        for (int i = 0; i < rData.configArray.Length; ++i)
                                        {
                                            rData.resObjectArray[i] = LoadResource(rData.configArray[i]);
                                            if (rData.resObjectArray[i] == null)
                                            {
                                                LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainPath);
                                                RecycleResourceData(rData);
                                                bError = true;
                                                break;
                                            }
                                        }
                                        if (bError == false)
                                            m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                                    }
                                    else
                                    {
                                        rData.resObject = LoadResource(rData.config);
                                        if (rData.resObject == null)
                                        {
                                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainPath);
                                            RecycleResourceData(rData);
                                        }
                                        else
                                        {
                                            m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                                        }
                                    }
                                }
                                

//                                 rData.request = Resources.LoadAsync(rData.config.MainPath);
//                                 if (rData.request == null)
//                                 {
//                                     BaseLogSystem.Debug("Load Resource Failed!! resPath = {0}", rData.config.MainPath);
//                                     RecycleResourceData(rData);
//                                 }
//                                 else
//                                 {
//                                     m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
//                                 }
                                KValue.Value.Remove(enu.Current.Key);
                                enu = KValue.Value.GetEnumerator();
                                bRet = enu.MoveNext();
                                if (m_MultiPipeLoadingResourceDic[KValue.Key].Count > ResourceManager.FrameSceneLoadCount)
                                    break;
                                if (bRet == false)
                                    break;
                            }
                        }
                    }
                }
            }
        }
        protected bool TickLoadingResource()
        {
            bool bRet = false;
            int i; ResourceData rData; List<ResourceData> dataList;
            //#region ResourcePriority_Async
            //dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync];
            //for (i = dataList.Count - 1; i >= 0; )
            //{
            //    rData = dataList[i];
            //    {
            //        if (rData.isInstantiate)
            //            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Add(rData);
            //        else
            //            m_LoadedResourceDataList.Add(rData);

            //        //rData.resObject = rData.request.asset;
            //        ////BaseLogSystem.Warn(rData.resObject.ToString());
            //        //rData.request = null;
            //        //if (rData.remove)
            //        //{
            //        //    rData.resObject = null;
            //        //    RecycleResourceData(rData);
            //        //}
            //        //else
            //        //{
            //        //    if (rData.resObject == null)
            //        //    {
            //        //        BaseLogSystem.Error("Resource Load Failed!!{0}", rData.config.MainPath);
            //        //    }
            //        //    else
            //        //    {
            //        //        if (rData.isInstantiate)
            //        //            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync].Add(rData);
            //        //        else
            //        //        {
            //        //            m_LoadedResourceDataList.Add(rData);
            //        //        }
            //        //    }
            //        //}
            //        dataList.RemoveAt(i);
            //    }
            //    break;
            //}
            //#endregion

#region ResourcePriority_SynText
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SynText];
            for (i = dataList.Count - 1; i >= 0; --i)
            {
                rData = dataList[i];
                if( rData.resObject)
                {
                    if ((rData.resObject is TextAsset) == false)
                    {
                        LogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.config.MainPath);
                        RecycleResourceData(rData);
                    }
                    else
                    {
                        if (rData.isInstantiate)
                        {
                            LogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
                            RecycleResourceData(rData);
                        }
                        else
                        {
                            m_LoadedResourceDataList.Add(rData);
                            bRet = true;
                        }
                    }
                }
                else
                {
                    if (rData.isInstantiate)
                    {
                        LogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
                        RecycleResourceData(rData);
                    }
                    else
                    {
                        bool bError = false;
                        for (int j = 0; j < rData.resObjectArray.Length;++j)
                        {
                            if ((rData.resObjectArray[j] is TextAsset) == false)
                            {
                                LogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.resObjectArray);
                                RecycleResourceData(rData);
                                bError = true;
                                break;
                            }
                        }
                        if( bError == false)
                        {
                            m_LoadedResourceDataList.Add(rData);
                            bRet = true;
                        }
                    }
                }
                
//                 rData.resObject = rData.request.asset;
//                 rData.request = null;
//                 if (rData.remove)
//                 {
//                     rData.resObject = null;
//                     RecycleResourceData(rData);
//                 }
//                 else
//                 {
//                     if (rData.resObject == null)
//                     {
//                         BaseLogSystem.Error("Resource Load Failed!!{0}", rData.config.MainPath);
//                     }
//                     else
//                     {
//                         if ((rData.resObject is TextAsset) == false)
//                         {
//                             BaseLogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.config.MainPath);
//                             rData.resObject = null;
//                             RecycleResourceData(rData);
//                         }
//                         else
//                         {
//                             if (rData.isInstantiate)
//                             {
//                                 BaseLogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
//                                 rData.resObject = null;
//                                 RecycleResourceData(rData);
//                             }
//                             else
//                             {
//                                 m_ResourceLoadedObject[rData.config] = rData.resObject;
//                                 m_LoadedResourceDataList.Add(rData);
//                                 bRet = true;
//                             }
//                         }
//                     }
//                 }
                dataList.RemoveAt(i);
            }
#endregion

#region ResourcePriority_Self
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_Self];
            while (dataList.Count > 0)
            {
                i = 0;
                rData = dataList[i];
                if (rData.isInstantiate)
                    m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Self].Add(rData);
                else
                    m_LoadedResourceDataList.Add(rData);

                //rData.resObject = rData.request.asset;
                //rData.request = null;
                //if (rData.remove)
                //{
                //    rData.resObject = null;
                //    RecycleResourceData(rData);
                //}
                //else
                //{
                //    if (rData.resObject == null)
                //    {
                //        BaseLogSystem.Error("Resource Load Failed!!{0}", rData.config);
                //    }
                //    else
                //    {
                //        m_ResourceLoadedObject[rData.config] = rData.resObject;
                //        if (rData.isInstantiate)
                //            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Self].Add(rData);
                //        else
                //        {
                //            m_LoadedResourceDataList.Add(rData);
                //        }
                //    }
                //}
                dataList.RemoveAt(i);
            }
            //取消载入限制
            //if (m_curFramePrioritySelfNum >= ResourceManager.FramePrioritySelfMax)
            //    return false;
            /////
#endregion

//            #region ResourcePriority_Pvw
//            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw];
//            while (dataList.Count > 0)
//            {
//                i = 0;
//                rData = dataList[i];
//                if (rData.isInstantiate)
//                    m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw].Add(rData);
//                else
//                    m_LoadedResourceDataList.Add(rData);

////                 rData.resObject = rData.request.asset;
////                 rData.request = null;
////                 if (rData.remove)
////                 {
////                     rData.resObject = null;
////                     RecycleResourceData(rData);
////                 }
////                 else
////                 {
////                     if (rData.resObject == null)
////                     {
////                         BaseLogSystem.Error("Resource Load Failed!!{0}", rData.config.MainPath);
////                     }
////                     else
////                     {
////                         if (rData.isInstantiate)
////                             m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw].Add(rData);
////                         else
////                         {
////                             m_LoadedResourceDataList.Add(rData);
////                         }
////                     }
////                 }
//                dataList.RemoveAt(i);
//            }
//            #endregion

#region ResourcePriority_Def
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_Def];
            //原来为IF，取消载入闲置改为while
            while (dataList.Count > 0)
            ///////
            {
                i = 0;
                rData = dataList[i];
                if (rData.isInstantiate)
                    m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Def].Add(rData);
                else
                {
                    m_LoadedResourceDataList.Add(rData);
                }
//                 rData.resObject = rData.request.asset;
//                 rData.request = null;
//                 if (rData.remove)
//                 {
//                     rData.resObject = null;
//                     RecycleResourceData(rData);
//                 }
//                 else
//                 {
//                     if (rData.resObject == null)
//                     {
//                         BaseLogSystem.Error("Resource Load Failed!!{0}", rData.config.MainPath);
//                     }
//                     else
//                     {
//                         if (rData.isInstantiate)
//                             m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Def].Add(rData);
//                         else
//                         {
//                             m_LoadedResourceDataList.Add(rData);
//                         }
//                     }
//                 }
                dataList.RemoveAt(i);
            }
#endregion
            return bRet;
        }
        protected void TickInstantiateResource()
        {
            List<ResourceData> dataList; ResourceData rData;
#region ResourcePriority_Self
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Self];
            while (dataList.Count > 0)
            {
                rData = dataList[0];
                if( rData.resObject)
                {
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                }
                else
                {
                    for(int i=0;i<rData.resObjectArray.Length;++i)
                    {
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                    }
                }
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
#endregion

            //#region ResourcePriority_Pvw
            //dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw];
            //while (dataList.Count > 0 && CurFrameScenePvwInstantiateNum < ResourceManager.FrameScenePvwLoadCount)
            //{
            //    rData = dataList[0];
            //    if (rData.resObject)
            //    {
            //        rData.resObject = GameObject.Instantiate(rData.resObject);
            //    }
            //    else
            //    {
            //        for (int i = 0; i < rData.resObjectArray.Length; ++i)
            //        {
            //            rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
            //        }
            //    }
            //    ++CurFrameScenePvwInstantiateNum;
            //    m_LoadedResourceDataList.Add(rData);
            //    dataList.RemoveAt(0);
            //}
            //if (CurFrameScenePvwInstantiateNum >= ResourceManager.FrameScenePvwLoadCount)
            //    return;
            //#endregion

#region ResourcePriority_Def
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Def];
            //取消载入闲置
            //while (dataList.Count > 0 && w < ResourceManager.FrameInstantiateGameObjectNum)
            while (dataList.Count > 0)
            {
                rData = dataList[0];
                if (rData.resObject)
                {
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                }
                else
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                    }
                }
                ++CurFrameScenePvwInstantiateNum;
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
            //取消载入闲置
            //if (w >= ResourceManager.FrameInstantiateGameObjectNum)
            //    return;
            ////////////
#endregion

            //#region ResourcePriority_Async
            //dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsync];
            ////while (dataList.Count > 0 && w < ResourceManager.FrameInstantiateGameObjectNum)
            //while (dataList.Count > 0)
            //{
            //    rData = dataList[0];
            //    if (rData.resObject)
            //    {
            //        rData.resObject = GameObject.Instantiate(rData.resObject);
            //    }
            //    else
            //    {
            //        for (int i = 0; i < rData.resObjectArray.Length; ++i)
            //        {
            //            rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
            //        }
            //    }
            //    //BaseLogSystem.Warn(rData.resObject.ToString());
            //    //++w;
            //    m_LoadedResourceDataList.Add(rData);
            //    dataList.RemoveAt(0);
            //}
            //#endregion
            return;
        }
        protected void TickLoadedResource()
        {
            ResourceData rData = null;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedResourceDataList[i];
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            if( rData.resObject)
                            {
                                (rData.resObject as GameObject).SetActive(true);
                            }
                            else
                            {
                                for (int j = 0; j < rData.resObjectArray.Length;++j )
                                {
                                    (rData.resObjectArray[j] as GameObject).SetActive(true);
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        LogSystem.Error(e.ToString());
                    }
                    if (rData.resObject)
                        ResourceCallBack(rData.index, rData.resObject);
                    else
                        ResourceArrayCallBack(rData.index, rData.resObjectArray);

                    RecycleResourceData(rData,false);
                }
                m_LoadedResourceDataList.RemoveAt(i);
            }
        }
        protected bool TickResource()
        {
            bool bRet = false;
            TickDontDestroyOnLoad();
            TickLoadResource();
            bRet |= TickLoadingResource();
            TickInstantiateResource();
            TickLoadedResource();
            return bRet;
        }

        private int CurFrameScenePvwInstantiateNum;
        public override void Tick(uint uDeltaTimeMS)
        {
            TickRemoveResource();
            CurFrameScenePvwInstantiateNum = 0;
            TickResource();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

}

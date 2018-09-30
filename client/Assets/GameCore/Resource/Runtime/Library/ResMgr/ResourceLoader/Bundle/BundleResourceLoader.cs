using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using com.cyou.plugin.log;

namespace com.cyou.plugin.resource.loader.bundle
{
    class BundleResourceLoader : ResourceLoader
    {
        private bool m_bInited = false;
        public override void Init()
        {
            if (m_bInited)
                return;

            m_SceneLoader = new SceneLoader(this);
            ResourceManager.m_AssetBundleLoader.Init();
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_Async; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeLoadResourceDic[i] = new SortedDictionary<UInt64, ResourceData>();
                m_MultiPipeGenerateIndexDic[i] = 0;
            }
            m_bInited = true;
            Instance = this;
        }
        public static BundleResourceLoader Instance
        {
            get;
            set;
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
            m_ResouceDic.Clear();
            m_InstantiateResourceList.Clear();
            m_InstantiatePriorityResourceList.Clear();
            m_InstantiateSceneAsyncResourceList.Clear();
            m_StopResourceSet.Clear();
            m_LoadedResourceDataList.Clear();
            m_LoadedPriorityResourceDataList.Clear();
          
            ForceAsyncEnd();
              
            ResourceData.ClearPool();
            m_CleanupTask.Release();
            
        }
        
        private Int32 SampTime = -1;
        protected int InvTime = 10;

        protected static int asyncLevel = -1024;

        private SceneLoader m_SceneLoader;

        public override void Tick(uint uDeltaTimeMS)
        {
            Int32 time = (Int32)Time.realtimeSinceStartup;           
            asyncLevel = ResourceManager.AsyncLevel;
            m_CleanupTask.TickCleanup(uDeltaTimeMS);

            if (time - SampTime < InvTime)
            {
                TickRemoveResource();
                TickResource();
                return;
            }
            SampTime = time;
        }
           
        protected void TickResource()
        {
            TickDontDestroyOnLoad();
            bool asyncDone = internal_TickLoadResourceAsyncDone();
         
            TickPriorityLoadResource(asyncDone);
            TickPriorityInstantiateResource();
            TickPriorityLoadedResource();
            TickLoadResource(asyncDone);
            TickInstantiateResource();
            TickLoadedResource();
        }
        protected void TickLoadedResource()
        {
            ResourceData rData = null;
            if (m_LoadedResourceDataList.Count >= 30)
            {
                
                LogSystem.Info("回调队列长度:{0}", m_LoadedResourceDataList.Count);
            }
            ResourceAsyncTime = Time.realtimeSinceStartup;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                {
                    //LogSystem.Info("资源回调耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                    break;
                }
                rData = m_LoadedResourceDataList[i];
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            if (rData.resObject)
                            {
                                (rData.resObject as GameObject).SetActive(true);
                            }
                            else
                            {
                                for (int j = 0; j < rData.resObjectArray.Length; ++j)
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

                    ResourceData.RecycleResourceData(rData);
                }
                m_LoadedResourceDataList.RemoveAt(i);
            }
        }
        protected void TickInstantiateResource()
        {
            ResourceData rData;
            if (SceneAsyncLoadedResource == false)
            {
                while (m_InstantiateSceneAsyncResourceList.Count > 0)
                {
                    rData = m_InstantiateSceneAsyncResourceList[0];
                    if (rData.resObject)
                    {
                        ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObject);
                        rData.resObject = GameObject.Instantiate(rData.resObject);
                        if (rData.isActive)
                        {
                            (rData.resObject as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObject as GameObject).SetActive(false);
                        }
                        BundlePool.AddObjectBundleData(rData.resObject, data);
                    }
                    else
                    {
                        for (int i = 0; i < rData.resObjectArray.Length; ++i)
                        {
                            ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObjectArray[i]);
                            rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                            if (rData.isActive)
                            {
                                (rData.resObjectArray[i] as GameObject).SetActive(true);
                            }
                            else
                            {
                                (rData.resObjectArray[i] as GameObject).SetActive(false);
                            }
                            BundlePool.AddObjectBundleData(rData.resObjectArray[i], data);
                        }
                    }
                    m_LoadedResourceDataList.Add(rData);
                    m_InstantiateSceneAsyncResourceList.RemoveAt(0);
                }
            }

            if (m_InstantiateResourceList.Count >= 30)
            {
                LogSystem.Info("实例化队列长度:{0}", m_InstantiateResourceList.Count);
            }

            ResourceAsyncTime = Time.realtimeSinceStartup;
            while (m_InstantiateResourceList.Count > 0)
            {
                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                {
                    LogSystem.Info("资源实例化耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                    break;
                }
                rData = m_InstantiateResourceList[0];
                if (rData.resObject)
                {
                    ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObject);
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                    if (rData.isActive)
                    {
                        (rData.resObject as GameObject).SetActive(true);
                    }
                    else
                    {
                        (rData.resObject as GameObject).SetActive(false);
                    }
                    BundlePool.AddObjectBundleData(rData.resObject, data);
                }
                else
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObjectArray[i]);
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                        if (rData.isActive)
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(false);
                        }
                        BundlePool.AddObjectBundleData(rData.resObjectArray[i], data);
                    }
                }
                m_LoadedResourceDataList.Add(rData);
                m_InstantiateResourceList.RemoveAt(0);
            }
        }

        protected void TickPriorityLoadedResource()
        {
            ResourceData rData = null;
            for (int i = m_LoadedPriorityResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedPriorityResourceDataList[i];
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            if (rData.resObject)
                            {
                                (rData.resObject as GameObject).SetActive(true);
                            }
                            else
                            {
                                for (int j = 0; j < rData.resObjectArray.Length; ++j)
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

                    ResourceData.RecycleResourceData(rData);
                }
                m_LoadedPriorityResourceDataList.RemoveAt(i);
            }
        }
        protected void TickPriorityInstantiateResource()
        {
            ResourceData rData;
            while (m_InstantiatePriorityResourceList.Count > 0)
            {
                rData = m_InstantiatePriorityResourceList[0];
                if (rData.resObject)
                {
                    ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObject);
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                    if (rData.isActive)
                    {
                        (rData.resObject as GameObject).SetActive(true);
                    }
                    else
                    {
                        (rData.resObject as GameObject).SetActive(false);
                    }
                    BundlePool.AddObjectBundleData(rData.resObject, data);
                }
                else
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        ObjectBundleData data = BundlePool.GetObjBundleData(rData.resObjectArray[i]);
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                        if (rData.isActive)
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(false);
                        }
                        BundlePool.AddObjectBundleData(rData.resObjectArray[i], data);
                    }
                }
                m_LoadedPriorityResourceDataList.Add(rData);
                m_InstantiatePriorityResourceList.RemoveAt(0);
            }
        }
        protected void TickPriorityLoadResource(bool asyncDone)
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator iter;
            SortedDictionary<UInt64, ResourceData> sortedDic;
            bool bRet = false;
            Int32 index = (int)ResourcePriority.ResourcePriority_SynText;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                iter = sortedDic.GetEnumerator();
                while (iter.MoveNext())
                {
                    rData = iter.Current.Value;
                    if (rData.remove)
                    {
                        ResourceData.RecycleResourceData(rData);
                    }
                    else
                    {
                        if (rData.configArray != null)
                        {
                            bool bError = false;
                            rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                            for (int i = 0; i < rData.configArray.Length; ++i)
                            {
                                rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                if (rData.resObjectArray[i] == null)
                                {
                                    LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                    ResourceData.RecycleResourceData(rData);
                                    bError = true;
                                    break;
                                }
                                else
                                {
                                    if ((rData.resObjectArray[i] is TextAsset) == false)
                                    {
                                        LogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.configArray[i].MainName);
                                        ResourceData.RecycleResourceData(rData);
                                        bError = true;
                                        break;
                                    }
                                }
                            }
                            if (bError == false)
                            {
                                if (rData.isInstantiate)
                                {
                                    LogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
                                    ResourceData.RecycleResourceData(rData);
                                }
                                else
                                {
                                    m_LoadedPriorityResourceDataList.Add(rData);
                                }
                            }
                        }
                        else
                        {
                            rData.resObject = Internal_LoadResource(rData.config);
                            if (rData.resObject == null)
                            {
                                LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                ResourceData.RecycleResourceData(rData);
                            }
                            else
                            {
                                if ((rData.resObject is TextAsset) == false)
                                {
                                    LogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.config.MainName);
                                    ResourceData.RecycleResourceData(rData);
                                }
                                else
                                {
                                    if (rData.isInstantiate)
                                    {
                                        LogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
                                        ResourceData.RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                            }
                        }
                    }
                    sortedDic.Remove(iter.Current.Key);
                    iter = sortedDic.GetEnumerator();
                }
            }
            index = (int)ResourcePriority.ResourcePriority_Self;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index,5))
                    {
                        return;
                    }
                }
                else
                {
                    iter = sortedDic.GetEnumerator();
                    if (iter.MoveNext())
                    {
                        while (true)
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                ResourceData.RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    bool bError = false;
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            ResourceData.RecycleResourceData(rData);
                                            bError = true;
                                            break;
                                        }
                                    }
                                    if (bError == false)
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiatePriorityResourceList.Add(rData);
                                        else
                                            m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        ResourceData.RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiatePriorityResourceList.Add(rData);
                                        else
                                            m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                            }

                            sortedDic.Remove(iter.Current.Key);
                            iter = sortedDic.GetEnumerator();
                            bRet = iter.MoveNext();
                            if (bRet == false)
                                return;
                        }
                    }
                }
            }
        }

        private bool SceneAsyncLoadedResource = false;

        private float ResourceAsyncTime = Time.realtimeSinceStartup;

        public override void ForceMemClean()
        {
            ForceAsyncEnd();
            //CachePool.CleanAllCacheData();
            BundlePool.CleanAllUnusedCacheData();
        }
        public override void ForceAsyncEnd()
        {
                if (m_vBundleAsyncDataDic.Count > 0)
                {
                    BundleData rBData;
                    Dictionary<String, BundleData>.Enumerator enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    while (enIter.MoveNext())
                    {
                        String key = enIter.Current.Key;
                        rBData = enIter.Current.Value;
                        if (rBData != null)
                        {
                            if (rBData.BundleRequest != null)
                            {
                                if (rBData.Bundle == null)
                                {
                                    rBData.Bundle = rBData.BundleRequest.assetBundle;
                                }
                            }
                            BundlePool.AddBundleData(key, rBData);                          
                            rBData.BundleRequest = null;
                            rBData.AssetRequest = null;
                        }
                        m_vBundleAsyncDataDic.Remove(key);
                        enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    }
                    m_vBundleAsyncDataDic.Clear();
                    internalLoadResourceAsyncData();
                }
            }

        protected void internalLoadResourceAsyncData()
        {
            ResourceData m_AsyncResourceData = null; 
            int count = m_AsyncResourceDataList.Count;
            for(int w=0; w < count; ++w)
            {
                m_AsyncResourceData = m_AsyncResourceDataList[w];
                if (m_AsyncResourceData != null)
                {
                    if (m_AsyncResourceData.obDataArray != null)
                    {
                        int len = m_AsyncResourceData.obDataArray.Length;
                        m_AsyncResourceData.resObjectArray = new UnityEngine.Object[len];
                        for (int i = 0; i < len; ++i)
                        {
                            BundleResConfig config = m_AsyncResourceData.configArray[i];
                            ObjectBundleData obData = m_AsyncResourceData.obDataArray[i];
                            UnityEngine.Object obj = obData.Load(config.MainName);
                            if (obj != null)
                            {
                                if (BundlePool.ContainObjBundleDataByGO(obj))
                                {
                                    BundlePool.GetObjBundleData(obj).IncrRef(config.MainName);
                                }
                                else
                                {
                                    obData.IncrRef(config.MainName);
                                    BundlePool.AddObjectBundleData(obj, obData);
                                }
                                m_AsyncResourceData.resObjectArray[i] = obj;
                            }
                            else
                            {
                                ResourceUtils.DeleteObjectBundleData(ref obData);
                                m_vKeyBundleDataDic.Remove(config.ID);
                                ResourceData.RecycleResourceData(m_AsyncResourceData);
                                m_AsyncResourceData = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        BundleResConfig config = m_AsyncResourceData.config;
                        ObjectBundleData obData = m_AsyncResourceData.obData;
                        UnityEngine.Object obj = obData.Load(config.MainName);
                        if (obj != null)
                        {
                            if (BundlePool.ContainObjBundleDataByGO(obj))
                            {
                                BundlePool.GetObjBundleData(obj).IncrRef(config.MainName);
                            }
                            else
                            {
                                m_AsyncResourceData.obData.IncrRef(config.MainName);
                                BundlePool.AddObjectBundleData(obj, obData);
                            }
                            m_AsyncResourceData.resObject = obj;
                        }
                        else
                        {
                            ResourceUtils.DeleteObjectBundleData(ref obData);
                            m_vKeyBundleDataDic.Remove(config.ID);
                            ResourceData.RecycleResourceData(m_AsyncResourceData);
                            m_AsyncResourceData = null;
                        }
                    }
                    if (m_AsyncResourceData != null)
                    {
                        if (m_AsyncResourceData.isInstantiate)
                            m_InstantiateSceneAsyncResourceList.Add(m_AsyncResourceData);
                        else
                            m_LoadedResourceDataList.Add(m_AsyncResourceData);
                    }
                    m_AsyncResourceData = null;
                }
            }
            m_AsyncResourceDataList.Clear();
        }
        protected bool internal_TickLoadResourceAsyncDone()
        {
            Int32 nCount = m_vBundleAsyncDataDic.Count; 
            if (nCount > 0)
            {
                BundleData rBData;
                Dictionary<String, BundleData>.Enumerator enIter = m_vBundleAsyncDataDic.GetEnumerator();
                if(/*ResourceManager.AsyncAsset &&*/ nCount == 1)
                {
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        if(rBData.AssetRequest == null)
                        {
                            if( rBData.Bundle == null)
                            {
                                if (rBData.BundleRequest == null)
                                {
                                    LogSystem.Error("Error!!3!!");
                                    rBData.ReloadAsync();
                                    return false;
                                }
                                if (rBData.BundleRequest.isDone == false)
                                {
                                    return false;
                                }
                                if (rBData.Bundle == null)
                                {
                                    rBData.Bundle = rBData.BundleRequest.assetBundle;
                                }
                                if (rBData.Bundle == null)
                                {
                                    rBData.ReloadAsync();
                                    return false;
                                }
                            }
                            rBData.BundleRequest = null;
                            rBData.AssetRequest = rBData.Bundle.LoadAllAssetsAsync();
                            if(rBData.AssetRequest == null)
                            {
                                rBData.ReloadAsync();
                                return false;
                            }
                            return false;
                        }
                        else
                        {
                            if (rBData.AssetRequest == null && rBData.Bundle == null)
                            {
                                LogSystem.Error("Error!!4!!");
                                rBData.ReloadAsync();
                                return false;
                            }
                            if(rBData.AssetRequest == null)
                            {
                                if(rBData.Bundle == null)
                                {
                                    LogSystem.Error("Error!!7!!");
                                    rBData.ReloadAsync();
                                    return false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (rBData.AssetRequest.isDone)
                                {
                                    String key = enIter.Current.Key;
                                                                      BundlePool.AddBundleData(key, rBData);
                                    rBData.AssetRequest = null;
                                    m_vBundleAsyncDataDic.Clear();
                                    break;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        if (rBData.BundleRequest == null)
                        {
                            LogSystem.Error("Error!!5!!");
                            rBData.ReloadAsync();
                            return false;
                        }
                        if (rBData.BundleRequest.isDone == false)
                        {
                            return false;
                        }
                    }
                    enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        String key = enIter.Current.Key;
                        if(rBData.Bundle == null)
                        {
                            rBData.Bundle = rBData.BundleRequest.assetBundle;
                        }
                        if(rBData.Bundle == null)
                        {
                            LogSystem.Error("Error!!6!!");
                            rBData.ReloadAsync();
                            return false;
                        }
                      
                        BundlePool.AddBundleData(key, rBData);
                        rBData.BundleRequest = null;
                        rBData.AssetRequest = null;
                        m_vBundleAsyncDataDic.Remove(key);
                        enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    }
                }
                if (m_vBundleAsyncDataDic.Count != 0)
                    return false;
            }
            internalLoadResourceAsyncData();
            if (m_AsyncResourceDataList.Count == 0)
            {
                //closeAsync = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool internal_TickLoadResourceAsync(int index,int count = 0)
        {
            if (m_AsyncResourceDataList.Count != 0)
                return true;

            SortedDictionary<UInt64, ResourceData>  sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count == 0)
                return  false;

            ResourceData rData = null;
            do
            {
                SortedDictionary<UInt64, ResourceData>.Enumerator iter = sortedDic.GetEnumerator();
                if (iter.MoveNext())
                {
                    rData = iter.Current.Value;
                    if (rData.remove)
                    {
                        ResourceData.RecycleResourceData(rData);
                        return true;
                    }
                    else
                    {
                        ObjectBundleData rOBData;
                        if (rData.configArray != null)
                        {
                            rData.obDataArray = new ObjectBundleData[rData.configArray.Length];
                            for (int i = 0; i < rData.configArray.Length; ++i)
                            {
                                if (Internal_LoadResourceAsync(rData.configArray[i], out rOBData))
                                {
                                    if (rOBData == null)
                                    {
                                        LogSystem.Error("6Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                        ResourceData.RecycleResourceData(rData);
                                        rData = null;
                                        return true;
                                    }
                                    else
                                    {
                                        rData.obDataArray[i] = rOBData;
                                    }
                                }
                                else
                                {
                                    LogSystem.Error("3Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                    ResourceData.RecycleResourceData(rData);
                                    rData = null;
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (Internal_LoadResourceAsync(rData.config, out rOBData))
                            {
                                if (rOBData == null)
                                {
                                    LogSystem.Error("7Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                    ResourceData.RecycleResourceData(rData);
                                    rData = null;
                                    return true;
                                }
                                else
                                {
                                    rData.obData = rOBData;
                                }
                            }
                            else
                            {
                                LogSystem.Error("4Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                ResourceData.RecycleResourceData(rData);
                                rData = null;
                                return true;
                            }
                        }
                    }
                    sortedDic.Remove(iter.Current.Key);
                }
                if (rData != null)
                {
                    m_AsyncResourceDataList.Add(rData);
                }
                rData = null;
            } while (count-- > 0);
            return true;
        }
        protected void TickLoadResource(bool asyncDone)
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator iter;
            SortedDictionary<UInt64, ResourceData> sortedDic;
            bool bRet = false;
            Int32 index = (int)ResourcePriority.ResourcePriority_Def;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index))
                    {
                        return;
                    }
                }
                else
                {
                    ResourceAsyncTime = Time.realtimeSinceStartup;
                    iter = sortedDic.GetEnumerator();
                    if (sortedDic.Count > ResourceManager.ForceDefaultLoadCount)
                    {
                        while (iter.MoveNext())
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                ResourceData.RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    bool bError = false;
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            ResourceData.RecycleResourceData(rData);
                                            bError = true;
                                            break;
                                        }
                                    }
                                    if (bError == false)
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateResourceList.Add(rData);
                                        else
                                        {
                                            m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        ResourceData.RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateResourceList.Add(rData);
                                        else
                                        {
                                            m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }
                            }
                            sortedDic.Remove(iter.Current.Key);
                            iter = sortedDic.GetEnumerator();
                        }
                    }
                    else
                    {
                        if (iter.MoveNext())
                        {
                            while (true)
                            {
                                rData = iter.Current.Value;
                                if (rData.remove)
                                {
                                    ResourceData.RecycleResourceData(rData);
                                }
                                else
                                {
                                    if (rData.configArray != null)
                                    {
                                        bool bError = false;
                                        rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                        for (int i = 0; i < rData.configArray.Length; ++i)
                                        {
                                            rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                            if (rData.resObjectArray[i] == null)
                                            {
                                                LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                                ResourceData.RecycleResourceData(rData);
                                                bError = true;
                                                break;
                                            }
                                        }
                                        if (bError == false)
                                        {
                                            if (rData.isInstantiate)
                                                m_InstantiateResourceList.Add(rData);
                                            else
                                                m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                    else
                                    {
                                        rData.resObject = Internal_LoadResource(rData.config);
                                        if (rData.resObject == null)
                                        {
                                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                            ResourceData.RecycleResourceData(rData);
                                        }
                                        else
                                        {
                                            if (rData.isInstantiate)
                                                m_InstantiateResourceList.Add(rData);
                                            else
                                                m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }

                                sortedDic.Remove(iter.Current.Key);
                                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                                {
                                    //LogSystem.Info("资源加载耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                                    break;
                                }
                                iter = sortedDic.GetEnumerator();
                                bRet = iter.MoveNext();
                                if (bRet == false)
                                    break;
                            }
                        }
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////
            index = (int)ResourcePriority.ResourcePriority_Async;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index))
                    {
                        return;
                    }
                }
                else
                {
                    sortedDic = m_MultiPipeLoadResourceDic[(int)ResourcePriority.ResourcePriority_Async];
                    ResourceAsyncTime = Time.realtimeSinceStartup;
                    if (sortedDic.Count >= 30)
                    {
                        LogSystem.Info("加载队列数量:{0}", sortedDic.Count);
                    }
                    do
                    {
                        if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                        {
                            LogSystem.Info("资源加载耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                            break;
                        }

                        if (sortedDic.Count == 0)
                            break;

                        iter = sortedDic.GetEnumerator();
                        if (iter.MoveNext())
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                ResourceData.RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            ResourceData.RecycleResourceData(rData);
                                            break;
                                        }
                                    }
                                    if (rData.isInstantiate)
                                        m_InstantiateSceneAsyncResourceList.Add(rData);
                                    else
                                        m_LoadedResourceDataList.Add(rData);
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        LogSystem.Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        ResourceData.RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateSceneAsyncResourceList.Add(rData);
                                        else
                                            m_LoadedResourceDataList.Add(rData);
                                    }
                                }
                            }
                            sortedDic.Remove(iter.Current.Key);
                        }
                    }
                    while (true);
                }
            }
        }
        protected void TickDontDestroyOnLoad()
        {
            if (m_LoadingDontDestroyResouce != null)
            {
                if (m_LoadingDontDestroyResouce.resObject)
                {

                    if (m_LoadingDontDestroyResouce.isInstantiate)
                    {
                        ObjectBundleData rData = BundlePool.GetObjBundleData(m_LoadingDontDestroyResouce.resObject);
                        if (rData != null)
                        {
                            m_LoadingDontDestroyResouce.resObject = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObject);
                            ((GameObject)m_LoadingDontDestroyResouce.resObject).SetActive(m_LoadingDontDestroyResouce.isActive);
                            GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObject);
                            BundlePool.AddObjectBundleData(m_LoadingDontDestroyResouce.resObject, rData);
                        }
                        else
                        {
                            LogSystem.Error("TickDontDestroyOnLoad 1 Error!!!!");
                        }


                    }
                    ResourceCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObject);
                }
                else
                {
                    if (m_LoadingDontDestroyResouce.isInstantiate)
                    {
                        for (int i = 0; i < m_LoadingDontDestroyResouce.resObjectArray.Length; ++i)
                        {
                            ObjectBundleData rData = BundlePool.GetObjBundleData(m_LoadingDontDestroyResouce.resObjectArray[i]);
                            if (m_LoadingDontDestroyResouce.resObjectArray[i] != null)
                            {
                                m_LoadingDontDestroyResouce.resObjectArray[i] = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObjectArray[i]);
                                ((GameObject)m_LoadingDontDestroyResouce.resObjectArray[i]).SetActive(m_LoadingDontDestroyResouce.isActive);
                                GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObjectArray[i]);
                            }
                            else
                            {
                                LogSystem.Error("TickDontDestroyOnLoad 2 Error!!!!");
                            }
                            BundlePool.AddObjectBundleData(m_LoadingDontDestroyResouce.resObjectArray[i], rData);
                        }
                    }
                    ResourceArrayCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObjectArray);
                }
                ResourceData.RecycleResourceData(m_LoadingDontDestroyResouce);
                m_LoadingDontDestroyResouce = null;
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
                        for (Int32 i = 0; i < m_LoadingDontDestroyResouce.configArray.Length; ++i)
                        {
                            m_LoadingDontDestroyResouce.resObjectArray[i] = Internal_LoadResource(m_LoadingDontDestroyResouce.configArray[i]);
                            if (m_LoadingDontDestroyResouce.resObjectArray[i] == null)
                            {
                                
                                LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  resObjectArray == null", m_LoadingDontDestroyResouce.configArray[i].MainName);
                                ResourceData.RecycleResourceData(m_LoadingDontDestroyResouce);
                                m_LoadingDontDestroyResouce = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        m_LoadingDontDestroyResouce.resObject = Internal_LoadResource(m_LoadingDontDestroyResouce.config);
                        if (m_LoadingDontDestroyResouce.resObject == null)
                        {
                            LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  request == null", m_LoadingDontDestroyResouce.config.MainName);
                            ResourceData.RecycleResourceData(m_LoadingDontDestroyResouce);
                            m_LoadingDontDestroyResouce = null;
                        }
                    }
                }
            }
        }

        protected void TickRemoveResource()
        {
            if (m_StopResourceSet.Count == 0)
                return;
            bool bRet;
            HashSet<Int32>.Enumerator iter = m_StopResourceSet.GetEnumerator();
            int index;
            while (iter.MoveNext())
            {
                index = iter.Current;
                bRet = (((RemoveLoadedResource(index)) ?
                             true : RemoveInstantiateResource(index)) ?
                             //true : RemoveLoadingResource(index)) ?
                             true : RemoveLoadResource(index));
                if (bRet == false)
                {
                    LogSystem.Warn("RemoveResource {0} Not Find!!", index);
                }
            }
            m_StopResourceSet.Clear();
        }
        protected bool RemoveLoadedResource(Int32 index)
        {
            if (m_LoadedResourceDataList.Count == 0)
                return false;

            ResourceData rData = null;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedResourceDataList[i];
                if (rData.index == index)
                {
                    m_LoadedResourceDataList.RemoveAt(i);
                    ResourceData.RecycleResourceData(rData);
                    return true;
                }
            }
            return false;
        }
        protected bool RemoveInstantiateResource(Int32 index)
        {

            ResourceData rData;
            for (int i = 0; i < m_InstantiateResourceList.Count; ++i)
            {
                rData = m_InstantiateResourceList[i];
                if (rData.index == index)
                {
                    m_InstantiateResourceList.RemoveAt(i);
                    ResourceData.RecycleResourceData(rData);
                    return true;
                }
            }
            return false;
        }

        protected bool RemoveLoadResource(Int32 index)
        {
            if (m_MultiPipeLoadResourceDic.Count == 0)
                return false;

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
                            ResourceData.RecycleResourceData(rData);
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

   

        //基于KEY索引关系
        Dictionary<Int32, ObjectBundleData> m_vKeyBundleDataDic = new Dictionary<int, ObjectBundleData>();

        //对象记录
        //Dictionary<UnityEngine.Object, ObjectBundleData> m_vObjBundleDataDic = new Dictionary<UnityEngine.Object, ObjectBundleData>();
                
        //bundle异步加载存储
        Dictionary<String, BundleData> m_vBundleAsyncDataDic = new Dictionary<string, BundleData>();

        //ResourceData m_AsyncResourceData = null;

        List<ResourceData> m_AsyncResourceDataList = new List<ResourceData>(5);

      
        
        protected BundleData LoadBundle(BundleResConfig config, Int32 index)
        {
            String bundleName = config.BundleNameArray[index];    
            BundleData mainBundle = null;
            if(m_vBundleAsyncDataDic.ContainsKey(bundleName))
            {
                ForceAsyncEnd();
            }
            if(BundlePool.ContainBundleData(bundleName))
            {
                mainBundle = BundlePool.GetBundleData(bundleName);
                if (mainBundle.Bundle != null)
                {
                    return mainBundle;
                }
            }
         
            AssetBundle bundle = ResourceManager.m_AssetBundleLoader.LoadBundle(bundleName);
            if(bundle == null)
            {
                LogSystem.Error("Failed to AssetBundleLoader.LoadBundle: {}", bundleName);
            }
            if (mainBundle == null)
            {
                mainBundle = ResourceUtils.InstantiateBundleData();
                BundlePool.AddBundleData(bundleName, mainBundle);            
            }
            mainBundle.Bundle = bundle;
            mainBundle.BundleName = bundleName;
            mainBundle.Config = config;
            //mainBundle.ConfigIndex = index;
            return mainBundle;
        }
        protected BundleData LoadBundleAsync(BundleResConfig config, Int32 index)
        {
            String bundleName = config.BundleNameArray[index];         
            BundleData mainBundle = null;
            if(m_vBundleAsyncDataDic.ContainsKey(bundleName))
            {
                return m_vBundleAsyncDataDic[bundleName];
            }

            if (BundlePool.ContainBundleData(bundleName))
            {
                mainBundle = BundlePool.GetBundleData(bundleName);
                if (mainBundle.Bundle != null)
                {
                    mainBundle.BundleRequest = null;
                    mainBundle.AssetRequest = null;
                    return mainBundle;
                }
                else if (mainBundle.BundleRequest != null)
                {
                    return mainBundle;
                }
            }

            AssetBundleCreateRequest bundleRequest = LoadBundleAsync(bundleName);
            if (bundleRequest == null)
                return null;

            if (mainBundle == null)
            {
                mainBundle = ResourceUtils.InstantiateBundleData();
            }
            mainBundle.Bundle = null;
            mainBundle.BundleName = bundleName;
            mainBundle.Config = config;
            //mainBundle.ConfigIndex = index;
            mainBundle.BundleRequest = bundleRequest;
            m_vBundleAsyncDataDic.Add(bundleName, mainBundle);
            return mainBundle;
        }
        protected BundleData LoadBundlesForObjectAsync(BundleResConfig config, ObjectBundleData rData)
        {
            rData.ClearDepBundleData();
            if (config.BundleNameArray == null/* || config.MainKeyArray == null*/)
            {
                return null;
            }
            for (int i = 1; i < config.BundleNameArray.Length; ++i)
            {             
                BundleData bundle = LoadBundleAsync(config, i);
                if (bundle == null || ((bundle.Bundle == null) && (bundle.BundleRequest == null)))
                {
                    LogSystem.Error("资源加载失败1 ！！{0} ", config.BundleNameArray[i]/*, config.MainKeyArray[i], config.MainNameArray[i]*/);
                    return null;
                }
                else
                {
                    rData.AddDepBundleData(config.BundleNameArray[i], bundle);
                }
            }            
            BundleData mainBundle = LoadBundleAsync(config,0);
            rData.MainBundle = mainBundle;
            rData.ResName = config.BundleNameArray[0];
            if (mainBundle == null || ((mainBundle.Bundle == null) && (mainBundle.BundleRequest == null)))
            {
                LogSystem.Error("资源加载失败2 ！！{0} ", config.BundleNameArray[0]/*, config.MainKeyArray[0], config.MainNameArray[0]*/);
                return null;
            }
            else
            {
                return mainBundle;
            }
        }
        protected BundleData LoadBundlesForObject(BundleResConfig config, ObjectBundleData rData)
        {
            rData.ClearDepBundleData();
            if (config.BundleNameArray == null /*|| config.MainKeyArray == null*/)
            {
                return null;
            }
            for (int i = 1; i < config.BundleNameArray.Length; ++i)
            {   
                BundleData bundle = LoadBundle(config,i);
                if (bundle == null || bundle.Bundle == null)
                {
                    LogSystem.Error("资源加载失败1 ！！{0} }", config.BundleNameArray[i]/*, config.MainKeyArray[i], config.MainNameArray[i]*/);
                    return null;
                }
                else
                {
                    rData.AddDepBundleData(config.BundleNameArray[i], bundle);
                }
            }
            BundleData mainBundle = LoadBundle(config, 0);
            rData.MainBundle = mainBundle;
            rData.ResName = config.BundleNameArray[0];
            if (mainBundle == null || mainBundle.Bundle == null)
            {
                LogSystem.Error("资源加载失败2 ！！{0} ", config.BundleNameArray[0]/*, config.MainKeyArray[0], config.MainNameArray[0]*/);
                return null;
            }
            else
            {
                return mainBundle;
            }
        }

        public override UnityEngine.Object LoadResource(IResConfig rc)
        {   
            return  Internal_LoadResource(rc);
        }

        protected bool Internal_LoadResourceAsync(IResConfig rc,out ObjectBundleData rData)
        {
            BundleResConfig config = (BundleResConfig)rc;
            if (m_vKeyBundleDataDic.ContainsKey(config.ID))
            {
                rData = m_vKeyBundleDataDic[config.ID];
                if (LoadBundlesForObjectAsync(config, rData) == null)
                {
                    ResourceUtils.DeleteObjectBundleData(ref rData);
                    m_vKeyBundleDataDic.Remove(config.ID);
                    rData = null;
                    return false;
                }
            }
            else
            {
                rData = ResourceUtils.NewObjectBundleData();
                rData.Key = config.ID;
                if (LoadBundlesForObjectAsync(config, rData) == null)
                {
                    ResourceUtils.DeleteObjectBundleData(ref rData);
                    rData = null;
                    return false;
                }
                m_vKeyBundleDataDic.Add(config.ID, rData);
            }
            return true;
        }
        protected UnityEngine.Object Internal_LoadResource(IResConfig rc)
        {
            BundleResConfig config = (BundleResConfig)rc;
            ObjectBundleData rData;
            if (m_vKeyBundleDataDic.ContainsKey(config.ID))
            {
                rData = m_vKeyBundleDataDic[config.ID];
                if (LoadBundlesForObject(config, rData) == null)
                {
                    ResourceUtils.DeleteObjectBundleData(ref rData);
                    m_vKeyBundleDataDic.Remove(config.ID);
                    return null;
                }
            }
            else
            {
                rData = ResourceUtils.NewObjectBundleData();
                rData.Key = config.ID;
                if (LoadBundlesForObject(config, rData) == null)
                {
                    ResourceUtils.DeleteObjectBundleData(ref rData);
                    return null;
                }
                m_vKeyBundleDataDic.Add(config.ID, rData);
            }
            UnityEngine.Object obj = rData.Load(config.MainName);
            if (obj != null)
            {
                if (BundlePool.ContainObjBundleDataByGO(obj))
                {
                    BundlePool.GetObjBundleData(obj).IncrRef(config.MainName);
                }
                else
                {
                    rData.IncrRef(config.MainName);
                    BundlePool.AddObjectBundleData(obj, rData);
                }
                return obj;
            }
            else
            {
                ResourceUtils.DeleteObjectBundleData(ref rData);
                m_vKeyBundleDataDic.Remove(config.ID);
                return null;
            }
        }
        public override UnityEngine.GameObject InstantiateGameObject(IResConfig rc, bool isActive)
        {
            BundleResConfig config = (BundleResConfig)rc;
            UnityEngine.Object obj = Internal_LoadResource(rc);
            if (obj == null)
                return null;

            UnityEngine.GameObject gameObj = GameObject.Instantiate(obj) as GameObject;
            if (gameObj == null)
            {
                DestroyResource(obj);
                return null;
            }
            BundlePool.AddObjectBundleData(gameObj, BundlePool.GetObjBundleData(obj));
            m_vKeyBundleDataDic[config.ID] = BundlePool.GetObjBundleData(obj);
            return gameObj;
        }
        public override void LoadScene(IResConfig res)
        {
            m_SceneLoader.LoadScene(res);
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
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
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
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
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
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if (generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.index = index;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = true;
            rData.isActive = isActive;
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
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
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
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
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
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceUtils .GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
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
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
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
        public override Int32 LoadResourceAsync(IResConfig[] rcArray, Int32 priority)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceUtils.GetCounter();
            ResourceData rData = ResourceData.InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
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
       
        public override void DestroyResource(System.Object obj)
        {
            if (obj == null)
                return;
            UnityEngine.Object uObj = obj as UnityEngine.Object;
            if (uObj != null)
            {
                if (BundlePool.ContainObjBundleDataByGO(uObj))
                {
                    ObjectBundleData rData = BundlePool.GetObjBundleData(uObj);
                    if (rData.RefCount == 1)
                    {
                        BundlePool.RemoveObjectBundleData(uObj);
                    }
                    rData.DestroyGameObject(uObj, rData.ResName);
                    rData.DecrRef(rData.ResName);
                    //return;
                }

                if (uObj is GameObject || uObj is UnityEngine.Component)
                {
                    GameObject.Destroy(uObj);
                }
            }
        }
        //public override void DestroyResource(ref System.Object obj)
        //{
        //    if (obj == null)
        //        return;
        //    DestroyResource(obj);
        //    obj = null;
        //}
      
        public AssetBundleCreateRequest LoadBundleAsync(string path)
        {

            return ResourceManager.m_AssetBundleLoader.LoadBundleAsync(path);
        }

        private ResourcePriority GetResourcePriority(int priority)
        {

            if (priority <= (int)(ResourcePriority.ResourcePriority_Min) || priority >= (int)(ResourcePriority.ResourcePriority_Max))
            {
                return ResourcePriority.ResourcePriority_Async;
            }
            else
            {
                if(m_MultiPipeGenerateIndexDic.ContainsKey(priority))
                {
                    return ResourcePriority.ResourcePriority_Async;
                }
                else
                {
                    return (ResourcePriority)(priority);
                }
            }
        }

        private UInt64 GenerateIndex(ResourcePriority priority)
        {
            int pty = (int)priority;
            if(m_MultiPipeGenerateIndexDic.ContainsKey(pty))
            {
                UInt64 index = m_MultiPipeGenerateIndexDic[pty];
                m_MultiPipeGenerateIndexDic[pty] = ++index;
                return index;
            }
            else
            {
                return 0;
            }
        }

        //所有队列的位置
        private Dictionary<int, ResourceData> m_ResouceDic = new Dictionary<int, ResourceData>();
        //PriorityIndex池
        private Dictionary<int, UInt64> m_MultiPipeGenerateIndexDic = new Dictionary<int, UInt64>();

        //需要加载的资源队列。。。
        private Dictionary<int, SortedDictionary<UInt64, ResourceData>> m_MultiPipeLoadResourceDic = new Dictionary<int, SortedDictionary<UInt64, ResourceData>>();

        //不销毁加载队列。。。
        private SortedDictionary<int, ResourceData> m_DontDestroyResouceDic = new SortedDictionary<int, ResourceData>();

        //正在加载的不销毁资源
        private ResourceData m_LoadingDontDestroyResouce = null;
        //实例化资源队列。。。
        //private Dictionary<int, List<ResourceData>> m_MultiPipeInstantiateResourceDic = new Dictionary<int, List<ResourceData>>();
        List<ResourceData> m_InstantiateResourceList = new List<ResourceData>();
        List<ResourceData> m_InstantiatePriorityResourceList = new List<ResourceData>();
        List<ResourceData> m_InstantiateSceneAsyncResourceList = new List<ResourceData>();
        //已经在加载资源队列。。。
        //private Dictionary<int, List<ResourceData>> m_MultiPipeLoadingResourceDic = new Dictionary<int, List<ResourceData>>();

        //停止加载的资源列表
        private HashSet<Int32> m_StopResourceSet = new HashSet<int>();

        //已经加载完的资源列表
        private List<ResourceData> m_LoadedResourceDataList = new List<ResourceData>();
        private List<ResourceData> m_LoadedPriorityResourceDataList = new List<ResourceData>();
        private ICleanupTask m_CleanupTask = new CleanupTask();

      
       
    }
}

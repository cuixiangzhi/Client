/********************************************************************************
 *	创建人：	 李彬
 *	创建时间： 2015-06-11   测试使用
 *
 *	功能说明：  U3D资源载入入口，禁止程序自己使用底层接口调用。不在支持同步载入。
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


namespace Games.TLBB.Manager
{
    class BundleResouceManager : GameCore.BaseManager<BundleResouceManager>
    {

        #region ICommon 接口实现
        public virtual void Init()
        {
            m_LoadingResourceData.Clear(true);
            Release();
        }

        public bool GetResource(int index, out UnityEngine.Object obj)
        {
            if (m_LoadedResourceObject.ContainsKey(index))
            {
                obj = m_LoadedResourceObject[index];
                m_LoadedResourceObject.Remove(index);
                if (m_LoadedResourcceCall.ContainsKey(index))
                    m_LoadedResourcceCall.Remove(index);
                return true;
            }
            else
            {
                obj = null;
                return false;
            }
        }
        public bool GetStreamResource(int index, out byte[] byteData)
        {
            if (m_StreamLoadedResourceByte.ContainsKey(index))
            {
                byteData = m_StreamLoadedResourceByte[index];
                m_StreamLoadedResourceByte.Remove(index);
                if (m_LoadedResourcceCall.ContainsKey(index))
                    m_LoadedResourcceCall.Remove(index);
                return true;
            }
            else
            {
                byteData = null;
                return false;
            }
        }

        protected void TickResource(uint uDeltaTimeMS)
        {
            if (m_LoadingResourceData.data != null)
            {
                ResourceData resData = (ResourceData)(m_LoadingResourceData.data);
                if (m_LoadingResourceData.request == null)
                {
                    if (m_LoadingResourceData.www == null)
                        return;
                    else
                    {
                        if( m_LoadingResourceData.www.assetBundle == null)
                        {
                            m_LoadingResourceData.Clear(resData.UnLoadBundle);
                            if (m_LoadedResourcceCall.ContainsKey(resData.index))
                            {
                                try
                                {
                                    (m_LoadedResourcceCall[resData.index])(resData.index, null, null);
                                    m_LoadedResourcceCall.Remove(resData.index);
                                    m_LoadedResourceObject.Remove(resData.index);

                                }
                                catch (System.Exception e)
                                {
                                    LogSystem.Error(e.ToString());
                                }
                            }
                            return;
                        }
                        m_LoadingResourceData.bundleStack.Push(m_LoadingResourceData.www.assetBundle);
                        m_LoadingResourceData.request = m_LoadingResourceData.www.assetBundle.LoadAsync(resData.resPath, typeof(UnityEngine.Object));
                        if (m_LoadingResourceData.request == null)
                        {
                            LogSystem.Debug("Load Resource Failed!! resPath = {0}", resData.resPath);
                            m_LoadingResourceData.Clear(resData.UnLoadBundle);
                            if (m_LoadedResourcceCall.ContainsKey(resData.index))
                            {
                                try
                                {
                                    (m_LoadedResourcceCall[resData.index])(resData.index, null, null);
                                    m_LoadedResourcceCall.Remove(resData.index);
                                    m_LoadedResourceObject.Remove(resData.index);

                                }
                                catch (System.Exception e)
                                {
                                    LogSystem.Error(e.ToString());
                                }
                            }
                            return;
                        }
                    }
                }

                if (m_LoadingResourceData.request.isDone)
                {
                    m_ResourceObject[resData.resPath] = m_LoadingResourceData.request.asset;
                    if (resData.isInstantiate)
                    {
                        try
                        {
                            m_LoadedResourceObject[resData.index] = GameObject.Instantiate(m_LoadingResourceData.request.asset);
                            if (m_LoadedResourceObject[resData.index] != null)
                            {
                                ((UnityEngine.GameObject)(m_LoadedResourceObject[resData.index])).SetActive(resData.isActive);
                            }
                        }catch(System.Exception e)
                        {
                            m_LoadedResourceObject[resData.index] = null;
                            LogSystem.Error("{0} Instantiate Failed!!! {1}", resData.resPath, e.ToString());
                        }
                    }
                    else
                    {
                        m_LoadedResourceObject[resData.index] = m_LoadingResourceData.request.asset;
                    }
                    m_LoadingResourceData.Clear(resData.UnLoadBundle);
                    if (m_LoadedResourcceCall.ContainsKey(resData.index))
                    {
                        try
                        {
                            (m_LoadedResourcceCall[resData.index])(resData.index, m_LoadedResourceObject[resData.index], null);
                            m_LoadedResourcceCall.Remove(resData.index);
                            m_LoadedResourceObject.Remove(resData.index);

                        }
                        catch (System.Exception e)
                        {
                            LogSystem.Error(e.ToString());
                        }
                    }
                }
                return;
            }
        }
        public void TickStream(uint uDeltaTimeMS)
        {
            if (m_LoadingResourceData.data != null)
            {
                ResourceData resData = (ResourceData)(m_LoadingResourceData.data);
                if (m_LoadingResourceData.www == null)
                    return;
                {
                        //压缩包查找数据 还没有做。。。。。。
                        //呆写。。。
                }
                m_StreamLoadedResourceByte[resData.index] = m_LoadingResourceData.www.bytes;
                m_LoadingResourceData.Clear(resData.UnLoadBundle);
                if (m_LoadedResourcceCall.ContainsKey(resData.index))
                {
                    try
                    {
                        (m_LoadedResourcceCall[resData.index])(resData.index, null, m_StreamLoadedResourceByte[resData.index]);
                        m_LoadedResourcceCall.Remove(resData.index);
                        m_StreamLoadedResourceByte.Remove(resData.index);

                    }
                    catch (System.Exception e)
                    {
                        LogSystem.Error(e.ToString());
                    }
                }
                return;
            }
        }
        public virtual void Tick(uint uDeltaTimeMS)
        {
            TickException();
            TickEvent();
            if (m_LoadingResourceData.data != null)
            {
                ResourceData resData = (ResourceData)(m_LoadingResourceData.data);
                switch (resData.resType)
                {
                    case ResourceType.ResourceType_Resource:
                        TickResource(uDeltaTimeMS);
                        break;
                    case ResourceType.ResourceType_Stream:
                        TickStream(uDeltaTimeMS);
                        break;
                    default:
                        {
                            LogSystem.Error("m_LoadingResourceData.data ResType is invalid!!! {0}", resData.resType);
                            m_LoadingResourceData.data = null;
                            m_LoadingResourceData.request = null;
                            break;
                        }
                }
            }
            else
            {
                if (m_LoadResourceQueue.Count == 0)
                    return; //没有资源加载

                m_LoadingResourceData.data = m_LoadResourceQueue.Dequeue();
                ResourceData newResData = (ResourceData)(m_LoadingResourceData.data);
                switch (newResData.resType)
                {
                    case ResourceType.ResourceType_Resource:
                        {
                            if (m_ResourceObject.ContainsKey(newResData.resPath))
                            {
                                UnityEngine.Object obj = m_ResourceObject[newResData.resPath];
                                if (newResData.isInstantiate)
                                {
                                    m_LoadedResourceObject[newResData.index] = GameObject.Instantiate(obj);
                                    if (m_LoadedResourceObject[newResData.index] != null)
                                    {
                                        ((UnityEngine.GameObject)(m_LoadedResourceObject[newResData.index])).SetActive(newResData.isActive);
                                    }
                                }
                                else
                                {
                                    m_LoadedResourceObject[newResData.index] = obj;
                                }
                                //回收处理
                                m_LoadingResourceData.Clear(newResData.UnLoadBundle);
                                if (m_LoadedResourcceCall.ContainsKey(newResData.index))
                                {
                                    try
                                    {
                                        (m_LoadedResourcceCall[newResData.index])(newResData.index, m_LoadedResourceObject[newResData.index], null);
                                        m_LoadedResourcceCall.Remove(newResData.index);
                                        m_LoadedResourceObject.Remove(newResData.index);

                                    }
                                    catch (System.Exception e)
                                    {
                                        LogSystem.Error(e.ToString());
                                    }
                                }
                                return;
                            }
                            else
                            {
                                BundleLoadStart(newResData.resPacket, m_LoadingResourceData);
                            }
                        }
                        break;
                    case ResourceType.ResourceType_Stream:
                        {
                            CoroutingProvide.GetInstance().StartCoroutine(LoadStreamassetResource(newResData.resPacket, m_LoadingResourceData));
                        }
                        break;
                    default:
                        {
                            LogSystem.Error("m_LoadingResourceData.data ResType is invalid!!! {0}", newResData.resType);
                            m_LoadingResourceData.Clear(newResData.UnLoadBundle);
                            if (m_LoadedResourcceCall.ContainsKey(newResData.index))
                                m_LoadedResourcceCall.Remove(newResData.index);
                            break;
                        }
                }
            }
        }
        private IEnumerator LoadStreamassetResource(System.String filePath, LoadingBundleResourceData loadingResourceData)
        {
            WWW w = new WWW(filePath);
            yield return w;
            loadingResourceData.www = w;
        }
        public virtual void Release()
        {
            m_LoadResourceQueue.Clear();
            m_ResourceObject.Clear();
            m_LoadedResourceObject.Clear();
            m_StreamLoadedResourceByte.Clear();
            m_LoadedResourcceCall.Clear();
            m_LoadingResourceData.Clear(true);
            StopBundleLoad();
        }
        public virtual void Destroy()
        {
        }
        #endregion
        private Dictionary<int, ResourceManager.DelegateResourceCallBack> m_LoadedResourcceCall = new Dictionary<int, ResourceManager.DelegateResourceCallBack>();
		private Queue<NewBaleOffResourceManager.ResourceData> m_LoadResourceQueue = new Queue<NewBaleOffResourceManager.ResourceData>();
        private Dictionary<string, UnityEngine.Object> m_ResourceObject = new Dictionary<string, UnityEngine.Object>();
        private Dictionary<int, UnityEngine.Object> m_LoadedResourceObject = new Dictionary<int, UnityEngine.Object>();
        private Dictionary<int, byte[]> m_StreamLoadedResourceByte = new Dictionary<int, byte[]>();
        private LoadingBundleResourceData m_LoadingResourceData = new LoadingBundleResourceData();
        //loading正在WWW的部分
        private Dictionary<string, WWW> m_aseetLoadingDict = new Dictionary<string, WWW>();
        //同时加载的数量
        private readonly int Max_AssetLoading_Count = 1;

        private bool FindIndex(int index)
        {
            if (index < 0)
                return false;

            foreach (ResourceData data in m_LoadResourceQueue)
            {
                if (data.index == index)
                {
                    return true;
                }
            }

            if (m_LoadingResourceData.data != null)
            {
                if (((ResourceData)m_LoadingResourceData.data).index == index)
                    return true;
            }

            if (m_LoadedResourceObject.ContainsKey(index))
                return true;

            if (m_StreamLoadedResourceByte.ContainsKey(index))
                return true;

            return false;
        }
        public bool RegisterEvent(int index, ResourceManager.DelegateResourceCallBack fun)
        {
            if (FindIndex(index) == false)
                return false;

            if (m_LoadedResourcceCall.ContainsKey(index))
            {
                LogSystem.Debug("Resource RegisterEvent index exist！");
            }
            m_LoadedResourcceCall[index] = fun;
            return true;
        }

        protected void TickException()
        {

        }

        protected void TickEvent()
        {
            foreach (KeyValuePair<int, UnityEngine.Object> data in m_LoadedResourceObject)
            {
                if (m_LoadedResourcceCall.ContainsKey(data.Key))
                {

                    ResourceManager.DelegateResourceCallBack fun = m_LoadedResourcceCall[data.Key];
                    m_LoadedResourcceCall.Remove(data.Key);
                    m_LoadedResourceObject.Remove(data.Key);
                    fun(data.Key, data.Value, null);
                }
                else
                {
                    LogSystem.Debug("resource load no reg function!!");
                    m_LoadedResourceObject.Remove(data.Key);
                }
                return;
            }
            foreach (KeyValuePair<int, byte[]> data in m_StreamLoadedResourceByte)
            {
                if (m_LoadedResourcceCall.ContainsKey(data.Key))
                {
                    ResourceManager.DelegateResourceCallBack fun = m_LoadedResourcceCall[data.Key];
                    m_LoadedResourcceCall.Remove(data.Key);
                    m_StreamLoadedResourceByte.Remove(data.Key);
                    fun(data.Key, null, data.Value);
                }
                else
                {
                    LogSystem.Debug("resource load no reg function!!");
                    m_StreamLoadedResourceByte.Remove(data.Key);
                }
                return;
            }
        }

        public UnityEngine.Object LoadResource(ResConfig resConfig)
        {
            LogSystem.Error("不支持Bundle同步资源载入！");
            return null;
        }
        public UnityEngine.GameObject InstantiateGameObject(ResConfig resConfig, bool isActive = false)
        {
            LogSystem.Error("不支持Bundle同步资源载入！");
            return null;
        }
        public byte[] LoadByteResource(ResConfig resConfig)
        {
            LogSystem.Error("不支持Bundle同步资源载入！");
            return null;
        }

        public void DontDestroyOnLoadResourceAsync(ResConfig resConfig, int ResIndex)
        {
            LogSystem.Error("Bundle DontDestroyOnLoadResourceAsync NonSupport!!");
        }
        public int LoadResourceAsync(ResConfig resConfig, int priority = 0)
        {
            ResourceType resType = ResourceType.ResourceType_Invalid;
            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, "StreamingAssets".Length)) == 0)
            {
                resType = ResourceType.ResourceType_Stream;
            }
            else if (String.Compare("Resources", resConfig.ResPath.Substring(0, "Resources".Length)) == 0)
            {
                resType = ResourceType.ResourceType_Resource;
            }
            else
            {
                return -1;
            }

            int index = ResourceManager.Singleton.GetCounter();
            if (index < 0)
                return -1;

            String resPath = resConfig.ResPath;
            if (String.IsNullOrEmpty(resPath))
                return -1;

            if (m_ResourceObject.ContainsKey(resPath))
            {
                m_LoadedResourceObject[index] = m_ResourceObject[resPath];
                return index;
            }
            else
            {
                ResourceData data;
                data.resPath = resPath;
                data.resPacket = resConfig.ResBundle;
                data.isInstantiate = false;
                data.isActive = false;
                data.index = index;
                data.resType = resType;
                data.UnLoadBundle = resConfig.UnLoadBundle;
                m_LoadResourceQueue.Enqueue(data);
                return index;
            }
        }

        public void DontDestroyOnInstantiateGameObjectAsyn(ResConfig resConfig, bool isActive, int ResIndex)
        {
            LogSystem.Error("Bundle DontDestroyOnInstantiateGameObjectAsyn NonSupport!!");
        }
       public int InstantiateGameObjectAsyn(ResConfig resConfig, bool isActive = false, int priority = 0)
       {
            ResourceType resType = ResourceType.ResourceType_Invalid;
            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, "StreamingAssets".Length)) == 0)
            {
                resType = ResourceType.ResourceType_Stream;
            }
            else if (String.Compare("Resources", resConfig.ResPath.Substring(0, "Resources".Length)) == 0)
            {
                resType = ResourceType.ResourceType_Resource;
            }
            else
            {
                return -1;
            }

            int index = ResourceManager.Singleton.GetCounter();
            if (index < 0)
                return -1;

            String resPath = resConfig.ResPath;
            if (String.IsNullOrEmpty(resPath))
                return -1;

            if (m_ResourceObject.ContainsKey(resPath))
            {
                m_LoadedResourceObject[index] = m_ResourceObject[resPath];
                return index;
            }
            else
            {
                ResourceData data;
                data.resPath = resPath;
                data.resPacket = resConfig.ResBundle;
                data.isInstantiate = true;
                data.isActive = isActive;
                data.index = index;
                data.resType = resType;
                data.UnLoadBundle = resConfig.UnLoadBundle;
                m_LoadResourceQueue.Enqueue(data);
                return index;
            }
        }

        void BundleLoadStart(string resBundle, LoadingBundleResourceData loadingResourceData, Action<string> loaderror = null)
        {
            //说明无法载入bundle或者其他什么原因 请把loadingResourceData.data赋值为null并使用LogSystem打出LOG
            AssetBundleTask task = new AssetBundleTask(resBundle, null);
            task.m_LoadError = loaderror;
            task.m_LoadFinish = null;
            task.m_Progress = null;
            if (task != null)
            {
                if (m_aseetLoadingDict.Count < Max_AssetLoading_Count)
                {
                    if (!string.IsNullOrEmpty(task.m_ResourceName))
                    {
                        SourceData sourcedata = SourceManager.Instance.GetSourcedata(task.m_ResourceName);
                        if (sourcedata != null)
                            CoroutingProvide.GetInstance().StartCoroutine(BundleFromLocal(task, loadingResourceData, sourcedata));
                    }

                }

            }
        }

        void StopBundleLoad()
        {
            //给我停止当前我载入Bundle权利。
            if (m_aseetLoadingDict != null && m_aseetLoadingDict.Count != 0)
            {
                foreach (string name in m_aseetLoadingDict.Keys)
                {
                    m_aseetLoadingDict[name].Dispose();
                    m_aseetLoadingDict[name] = null;
                    m_aseetLoadingDict.Remove(name);
                }
            }
        }

        private IEnumerator BundleFromLocal(AssetBundleTask task, LoadingBundleResourceData loadingResourceData, SourceData sourcedata)
        {
            string assetBundleName = task.m_ResourceName;

            string[] namekeyend = sourcedata.FileName.Split(new char[] { '/' });
            string namekey = namekeyend[namekeyend.Length - 1];

            string fullPath = DeviceStreamingAssetPath.PathURL + GetPlatformInfo.PlatformInfo + namekey;
            Debug.Log(namekey);
            Debug.Log(DeviceStreamingAssetPath.PathURL + GetPlatformInfo.PlatformInfo);
            Debug.Log("FullPath is" + fullPath);
            WWW www = new WWW(fullPath);
            m_aseetLoadingDict.Add(assetBundleName, www);
            while (!www.isDone)
            {
                Debug.Log(www.progress.ToString());
                Debug.Log(www.url.ToString());
                Debug.Log(www.isDone);
                yield return null;
            }
            if (www.error != null)
            {
                ;
            }
            else
            {
                loadingResourceData.www = www;
                m_aseetLoadingDict.Remove(assetBundleName);
            }

        }

        private IEnumerator BundleFromCacheOrDownload(AssetBundleTask task, LoadingBundleResourceData loadingResourceData, SourceData sourcedata)
        {
            bool errorOcurred = false;
            string assetBundleName = task.m_ResourceName;

            string[] namekeyend = sourcedata.FileName.Split(new char[] { '/' });

            string url = DeviceStreamingAssetPath.PathURL + GetPlatformInfo.PlatformInfo + namekeyend[namekeyend.Length - 1];
            Debug.Log(url);
            //           string url = ResourceSetting.ConverToFtpPath(assetBundleName);


            WWW www = null;
            try
            {
                www = WWW.LoadFromCacheOrDownload(url, 1);
            }
            finally
            {
                if( www == null)
                {
                    errorOcurred = true;
                    Debug.Log("[" + assetBundleName + "]:");
                    if (task.m_LoadError != null)
                        task.m_LoadError("[" + assetBundleName + "]:");
                    loadingResourceData.data = null;
                }
            }
            if (!errorOcurred)
            {
                m_aseetLoadingDict.Add(assetBundleName, www);
                while (!www.isDone)
                {
                    if (task.m_AssetType == AssetBundleTask.LoadType.Download_And_Loadlocal || task.m_AssetType == AssetBundleTask.LoadType.Only_Download)
                    {
                        if (task.m_Progress != null)
                        {
                            task.m_Progress(www.progress);
                        }
                    }
                    yield return null;

                }
                if (!String.IsNullOrEmpty(www.error))
                {
                    Debug.Log(assetBundleName + www.error);
                    if (task.m_LoadError != null)
                    {
                        task.m_LoadError(www.error);
                        if (m_LoadedResourcceCall.ContainsKey(((ResourceData)m_LoadingResourceData.data).index))
                            m_LoadedResourcceCall.Remove(((ResourceData)m_LoadingResourceData.data).index);
                        m_LoadingResourceData.Clear(true);
                    }
                    m_aseetLoadingDict.Remove(assetBundleName);
                    loadingResourceData.data = null;
                    errorOcurred = true;
                    yield return null;
                }

                bool mTaskEnd = false;
                if (task.m_AssetType == AssetBundleTask.LoadType.Download_And_Loadlocal || task.m_AssetType == AssetBundleTask.LoadType.Only_Loadlocal)
                {
                    if (!errorOcurred)
                    {
                        loadingResourceData.www = www;
                        Debug.Log(loadingResourceData.www.url.ToString());
                        m_aseetLoadingDict.Remove(assetBundleName);

                        mTaskEnd = true;
                    }
                }
                else
                {
                    m_aseetLoadingDict.Remove(assetBundleName);
                    mTaskEnd = true;
                }

                if (mTaskEnd)
                {
                    if (task.m_LoadFinish != null)
                    {
                        task.m_LoadFinish(task);
                    }
                }
            }
        }

    }
    class LoadingBundleResourceData
    {
        public WWW www = null;
        public AssetBundleRequest request = null;
        public IResourceData data = null;
        public Stack<AssetBundle> bundleStack = new Stack<AssetBundle>(); 
        public void Clear( bool bUnLoadBundle)
        {
            if (www != null)
            {
                www.Dispose();
            }
            if (bUnLoadBundle)
            {
                while( bundleStack.Count > 0)
                {
                    bundleStack.Pop().Unload(false);
                }
            }
            else
            {
                bundleStack.Clear();
            }
        }
    }

}

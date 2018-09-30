using UnityEngine;
using System;
using com.cyou.plugin.log;
using System.Collections.Generic;
using com.cyou.plugin.res.callback;
using UnityEngine.SceneManagement;
using com.cyou.plugin.resource.loader.bundle;
using com.cyou.plugin.resource.loader.editor;
using com.cyou.plugin.resource.loader;
using com.cyou.plugin.res.debuger;

namespace com.cyou.plugin.resource
{
    public class ResourceManager
    {

        public static IAssetBundlerLoader m_AssetBundleLoader;

        /// <summary>
        /// 异步等级，如果Priotity小于该level则异步加载，否则同步加载。
        /// </summary>
        public static Int32 AsyncLevel = 100;

        #region 外部资源配表代理，主要用于根据resID返回对应的Config
        public delegate IResConfig DelegateGetResConfig(int resID);
        public delegate IResConfig[] DelegateGetResConfigArray(int[] resIDArray);

        public DelegateGetResConfig GetResConfig
        {
            get;
            set;
        }
        public DelegateGetResConfigArray GetResConfigArray
        {
            get;
            set;
        }
        #endregion


        /// <summary>
        /// 资源加载管理者，用于负责根据不同队列加载资源
        /// </summary>
        ResourceLoader m_ResourceLoader = null;

        /// <summary>
        /// true 使用AssetBundle加载，false使用Resource加载
        /// </summary>
        private bool m_UseBundle;


        #region 资源加载回调部分
        Dictionary<Int32, ResourceCallback.DelegateResourceCallBack> m_vResourceCallBackDic = new Dictionary<int, ResourceCallback.DelegateResourceCallBack>();

        Dictionary<Int32, ResourceCallback.DelegateResourceCallBack> m_vDontDestroyResourceCallBackDic = new Dictionary<int, ResourceCallback.DelegateResourceCallBack>();

        Dictionary<Int32, ResourceCallback.DelegateResourceArrayCallBack> m_vResourceArrayCallBackDic = new Dictionary<int, ResourceCallback.DelegateResourceArrayCallBack>();

        Dictionary<Int32, ResourceCallback.DelegateResourceArrayCallBack> m_vDontDestroyResourceArrayCallBackDic = new Dictionary<int, ResourceCallback.DelegateResourceArrayCallBack>();

        ResourceCallback.DelegateSceneLoadedCallback m_vSceneLoadedCallback;


        protected void DefaultResourceCallBack(int index, System.Object obj)
        {
            if (m_vResourceCallBackDic.ContainsKey(index))
            {
                if(obj == null)
                {
                    Debug.LogError("obj == null");
                }
                m_vResourceCallBackDic[index](index, obj);
                m_vResourceCallBackDic.Remove(index);
            }
            if (m_vDontDestroyResourceCallBackDic.ContainsKey(index))
            {
                m_vDontDestroyResourceCallBackDic[index](index, obj);
                m_vResourceCallBackDic.Remove(index);
            }
        }
        protected void DefaultResourceArrayCallBack(int index, System.Object[] objArray)
        {
            if (m_vResourceArrayCallBackDic.ContainsKey(index))
            {
                m_vResourceArrayCallBackDic[index](index, objArray);
                m_vResourceArrayCallBackDic.Remove(index);
            }
            if (m_vDontDestroyResourceArrayCallBackDic.ContainsKey(index))
            {
                m_vDontDestroyResourceArrayCallBackDic[index](index, objArray);
                m_vDontDestroyResourceArrayCallBackDic.Remove(index);
            }
        }
        protected void DefaultSceneLoadedCallBack(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (m_vSceneLoadedCallback == null)
            {
                LogSystem.Warn("no scene loaded callback");
                return;
            }
            m_vSceneLoadedCallback(scene, loadSceneMode);
        }

        public void RegisterCallBack(Int32 index, ResourceCallback.DelegateResourceCallBack callBack)
        {
            if (m_vResourceCallBackDic.ContainsKey(index))
            {
#if DEBUG
                LogSystem.Error("已经注册回调的索引不能再次注册！index {0}",index);
#endif
            }
            else
            {
                m_vResourceCallBackDic.Add(index, callBack);
            }

        }
        public void RegisterDontDestroyCallBack(Int32 index, ResourceCallback.DelegateResourceCallBack callBack)
        {
            if (m_vDontDestroyResourceCallBackDic.ContainsKey(index))
            {
#if DEBUG
                LogSystem.Error("已经注册回调的索引不能再次注册！");
#endif
            }
            else
            {
                m_vDontDestroyResourceCallBackDic.Add(index, callBack);
            }

        }
        public void RegisterArrayCallBack(Int32 index, ResourceCallback.DelegateResourceArrayCallBack callBack)
        {
            if (m_vResourceArrayCallBackDic.ContainsKey(index))
            {
#if DEBUG
                LogSystem.Error("已经注册回调的索引不能再次注册！");
#endif
            }
            else
            {
                m_vResourceArrayCallBackDic.Add(index, callBack);
            }
        }
        public void RegisterArrayDontDestroyCallBack(Int32 index, ResourceCallback.DelegateResourceArrayCallBack callBack)
        {

            if (m_vDontDestroyResourceArrayCallBackDic.ContainsKey(index))
            {
                LogSystem.Error("已经注册回调的索引不能再次注册！");
            }
            else
            {
                m_vDontDestroyResourceArrayCallBackDic.Add(index, callBack);
            }
        }

        public void RegisterSceneLoadedCallBack(ResourceCallback.DelegateSceneLoadedCallback callBack)
        {
            m_vSceneLoadedCallback = callBack;
        }

        #endregion

        
        public void LoadScene(int resID)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return;
            }
            else
            {
                m_ResourceLoader.LoadScene(rc);
            }
        }
        public UnityEngine.Object LoadResource(Int32 resID)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.LoadResource(rc);
            }
        }
        //public UnityEngine.Object LoadResource(IResConfig rc)
        //{
        //    return m_ResourceLoader.LoadResource(rc);
        //}
        public UnityEngine.GameObject InstantiateGameObject(Int32 resID, bool isActive)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObject(rc, isActive);
            }
        }
        //public UnityEngine.GameObject InstantiateGameObject(IResConfig config, bool isActive)
        //{
        //    return m_ResourceLoader.InstantiateGameObject(config, isActive);
        //}

        public void StopLoading(Int32 index)
        {
            m_ResourceLoader.StopLoading(index);
        }
        public bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex)
        {
            return m_ResourceLoader.LoadingQueueReplace(priority, srcIndex, destIndex);
        }
        public Int32 DontDestroyOnInstantiateGameObjectAsync(Int32 resID, bool isActive, Int32 priority)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnInstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 DontDestroyOnLoadResourceAsync(Int32 resID, Int32 priority)
        {

            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnLoadResourceAsync(rc, priority);
            }
        }
        public Int32 InstantiateGameObjectAsync(Int32 resID, bool isActive, Int32 priority = 0)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObjectAsync(rc, isActive, priority);
            }
        }

        public Int32 InstantiateGameObjectAsync(IResConfig config, bool isActive, Int32 priority = 0)
        {
            return m_ResourceLoader.InstantiateGameObjectAsync(config, isActive, priority);
        }

        public Int32 LoadResourceAsync(Int32 resID, Int32 priority = 0)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.LoadResourceAsync(rc, priority);
            }
        }
        public Int32 DontDestroyOnInstantiateGameObjectAsync(Int32[] resIDArray, bool isActive, Int32 priority = 0)
        {

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnInstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 DontDestroyOnLoadResourceAsync(Int32[] resIDArray, Int32 priority = 0)
        {

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnLoadResourceAsync(rc, priority);
            }

        }
        public Int32 InstantiateGameObjectAsync(Int32[] resIDArray, bool isActive, Int32 priority = 0)
        {
            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 LoadResourceAsync(Int32[] resIDArray, Int32 priority = 0)
        {

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                LogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.LoadResourceAsync(rc, priority);
            }
        }

        public void DestroyResource(System.Object obj)
        {
            m_ResourceLoader.DestroyResource(obj);
        }
        //public void DestroyResource(ref System.Object obj)
        //{
        //    m_ResourceLoader.DestroyResource(obj);
        //    obj = null;
        //}
        public void Destroy(System.Object obj)
        {
            DestroyResource(obj);
        }
        //public void Destroy(ref System.Object obj)
        //{
        //    DestroyResource(obj);
        //}

        public void DestroyResourceArray(System.Object[] objArray)
        {
            if (objArray == null) return;
            for (int i = 0; i < objArray.Length; ++i)
            {
                m_ResourceLoader.DestroyResource(objArray[i]);
            }
        }
        //public void DestroyResourceArray(ref System.Object[] objArray)
        //{
        //    DestroyResourceArray(objArray);
        //    objArray = null;
        //}

        public virtual void ForceMemClean()
        {
            m_ResourceLoader.ForceMemClean();
        }

        public void ForceAsyncEnd()
        {
            m_ResourceLoader.ForceAsyncEnd();
        }


        #region 流限制配置
        public static int ForceDefaultLoadCount = 20;
        public static int FrameSceneLoadCount = 1;
        public static int FrameScenePvwLoadCount = 1;
        #endregion



        public bool IsUseBundle()
        {
            return m_UseBundle;
        }

        public static ResourceManager Singleton
        {
            get
            {
                if (__Singleton == null)
                    __Singleton = new ResourceManager();
                return __Singleton;
            }
        }
        private static ResourceManager __Singleton = null;



        #region 管理接口
        public void Init(bool useBundle)
        {
            m_UseBundle = useBundle;
            if (m_UseBundle)
            {
                m_ResourceLoader = new BundleResourceLoader();
            }
            else
            {
                m_ResourceLoader = new EditorResourceLoader();
            }
            m_ResourceLoader.ResourceCallBack = this.DefaultResourceCallBack;
            m_ResourceLoader.ResourceArrayCallBack = this.DefaultResourceArrayCallBack;
            m_ResourceLoader.SceneLoadedCallBack = this.DefaultSceneLoadedCallBack;

            m_ResourceLoader.Init();
            
        }

        private Int32 renderedFrame = 0;
        public virtual void Tick(uint uDeltaTimeMS)
        {
            if (Time.renderedFrameCount == renderedFrame)
            {
                return;
            }
            renderedFrame = Time.renderedFrameCount;
            try
            {
                m_ResourceLoader.Tick(uDeltaTimeMS);
            }catch
            {           
            }
        }
        public virtual void Release()
        {
            m_ResourceLoader.Release();
        }
        #endregion


        #region Debuger
        public void ShowDebuger(bool visible)
        {
            ResDebuger.GetInstance().Show(visible);
        }
        #endregion


    }
}

using com.cyou.plugin.res.callback;
using System;

namespace com.cyou.plugin.resource.loader
{
    /// <summary>
    /// 资源加载基类，真实处理资源的加载优先级
    /// </summary>
    abstract class ResourceLoader
    {
        public ResourceCallback.DelegateResourceCallBack ResourceCallBack
        {
            get;
            set;
        }
        public ResourceCallback.DelegateResourceArrayCallBack ResourceArrayCallBack
        {
            get;
            set;
        }
        public ResourceCallback.DelegateSceneLoadedCallback SceneLoadedCallBack
        {
            get;
            set;
        }

        
        public abstract void Init();
        public abstract void Release();
        public abstract void Tick(uint uDeltaTimeMS);
        public abstract UnityEngine.Object LoadResource(IResConfig rc);
        public abstract UnityEngine.GameObject InstantiateGameObject(IResConfig rc, bool isActive);

        public abstract void LoadScene(IResConfig res);
        public abstract void StopLoading(Int32 index);
        public abstract bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex);
        public abstract Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority);
        public abstract Int32 DontDestroyOnLoadResourceAsync(IResConfig rc, Int32 priority);
        public abstract Int32 InstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority);
        public abstract Int32 LoadResourceAsync(IResConfig rc, Int32 priority);
        public abstract Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority);
        public abstract Int32 DontDestroyOnLoadResourceAsync(IResConfig[] rcArray, Int32 priority);
        public abstract Int32 InstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority);
        public abstract Int32 LoadResourceAsync(IResConfig[] rcArray, Int32 priority);        
      
        public abstract void DestroyResource(System.Object obj);
        //public abstract void DestroyResource(ref System.Object obj);
        //public abstract void RecyclePrefab(UnityEngine.Object obj);
        //public abstract void RecyclePrefab(ref UnityEngine.Object obj);

        public virtual void ForceMemClean()
        {
        }
        public virtual void ForceAsyncEnd()
        {

        }
    }

}

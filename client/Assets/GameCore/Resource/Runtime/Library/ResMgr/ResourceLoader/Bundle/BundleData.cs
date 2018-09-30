using com.cyou.plugin.log;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cyou.plugin.resource.loader.bundle
{
    class BundleData
    {
        public BundleData()
        {
            //ConfigIndex = -1;
        }
        public bool ReloadAsync()
        {
            //todo
            //if (ConfigIndex < 0)
            //    return false;
            if (Bundle)
            {
                Bundle.Unload(true);
            }
            //BundleResourceLoader.closeAsync = true;
            //String mainPath = Config.BundleNameArray[ConfigIndex];
            //BundleRequest = BundleResourceLoader.Instance.LoadBundleAsync(mainPath);
            //Bundle = null;
            return true;
        }

        
        public BundleResConfig Config
        {
            get;
            set;
        }
        public String BundleName
        {
            get;
            set;
        }
        public AssetBundleCreateRequest BundleRequest
        {
            get;
            set;
        }
        public AssetBundleRequest AssetRequest
        {
            get;
            set;
        }
        public AssetBundle Bundle
        {
            get
            {
                return m_Bundle;
            }
            set
            {
                if (m_Bundle != value)
                {
                    m_Bundle = value;
                    if (m_Bundle != null)
                    {
                        BundleRequest = null;
                        //mainAsset = m_Bundle.mainAsset;
                    }
                }
            }
        }
    
        

        private AssetBundle m_Bundle = null;
      
        //private UnityEngine.Object mainAsset = null;

        private Int32 m_nRefCount = 0;
        public Int32 RefCount
        {
            get
            {
                return m_nRefCount;
            }
            set
            {
                m_nRefCount = value;
            }
        }
        
        public List<string> m_DirectRefPath = new List<string>();
        public List<string> m_DependensRefPath = new List<string>();

        public Dictionary<String, UnityEngine.Object> m_vLoadedResObjDic = new Dictionary<string, UnityEngine.Object>();

        Dictionary<String, UnityEngine.Object> m_vRemoveResObjDic = new Dictionary<string, UnityEngine.Object>();
        public void CleanBundleData()
        {
            if (RefCount == 0)
            {
                if (m_vRemoveResObjDic != null)
                {
                    m_vRemoveResObjDic.Clear();
                }
                if (m_vLoadedResObjDic != null)
                {
                    m_vLoadedResObjDic.Clear();
                }
             
                if (Bundle != null)
                {
#if DEBUG
                        LogSystem.Error(String.Format("1卸载资源Bundle:{0}", BundleName));
#endif
                    Bundle.Unload(true);
                    Bundle = null;
                }
            }
            else
            {

                if (m_vRemoveResObjDic != null)
                {
                    if (m_vRemoveResObjDic.Count > 0)
                    {
                        Dictionary<String, UnityEngine.Object>.Enumerator iter = m_vRemoveResObjDic.GetEnumerator();
                        while (iter.MoveNext())
                        {
#if DEBUG
                            LogSystem.Error(String.Format("卸载资源:{0} {1} ", BundleName, iter.Current.Key));
#endif
                            Resources.UnloadAsset(iter.Current.Value);
                        }
                        m_vRemoveResObjDic.Clear();
                    }
                }
            }
        }

        public void IncrRef(String resName,bool isMainAssetBundle)
        {
            m_nRefCount++;
            BundlePool.RemoveDataFromUnusedCache(this);

#if DEBUG
            if(isMainAssetBundle)
            {
                m_DirectRefPath.Add(resName);
            }
            else
            {
                m_DependensRefPath.Add(resName);
            }
#endif
            


        }

        public void DestroyGameObject(UnityEngine.Object obj, String resName)
        {

            if (String.IsNullOrEmpty(resName))
            {
                LogSystem.Error("DestroyGameObject resName == null ！！！");
            }

            if (m_vLoadedResObjDic.ContainsKey(resName))
            {
                UnityEngine.Object rObj = m_vLoadedResObjDic[resName];
                m_vLoadedResObjDic.Remove(resName);
                m_vRemoveResObjDic.Add(resName, rObj);
            }
            

        }
        public virtual void DecrRef(String resName, bool isMainAssetBundle)
        {
            if (m_nRefCount < 1)
            {
                m_nRefCount = 0;
                LogSystem.Error("引用计数出错！！！{0} {1} {2}", m_nRefCount, BundleName, Bundle);
                return;
            }
#if DEBUG
            if (isMainAssetBundle)
            {
                m_DirectRefPath.Remove(resName);
            }
            else
            {
                m_DependensRefPath.Remove(resName);
            }
#endif

            m_nRefCount--;
            if (m_nRefCount == 0)
            {
                BundlePool.AddDataToUnusedCache(this);
                BundlePool.RemoveBundleDataFromUsedPool(this.BundleName);
            }
          
        }
        public UnityEngine.Object Load(string name)
        {
            if (Bundle == null)
            {
                LogSystem.Error("Bundle不存在@！！！！{0} {1} ", BundleName, name);
                return null;
            }
            if (String.IsNullOrEmpty(name))
            {
                LogSystem.Error("name不存在@！！！！{0} {1} ", BundleName, name);
                return null;
            }
            UnityEngine.Object obj = null;
            if (m_vLoadedResObjDic.ContainsKey(name))
                return m_vLoadedResObjDic[name];
            if (m_vRemoveResObjDic.ContainsKey(name))
            {
                obj = m_vRemoveResObjDic[name];
                m_vLoadedResObjDic.Add(name,obj);
                return obj;
            }

            obj = Bundle.LoadAsset(name);
           
            if (obj == null)
            {
                LogSystem.Error("{0} 资源不存在 {1} ！！！", BundleName, name);
                return null;
            }
            if(!m_vLoadedResObjDic.ContainsKey(name))
                m_vLoadedResObjDic.Add(name, obj);
            else
            {
                LogSystem.Warn("{0} 资源被多次加载:: {1} ！！！", BundleName, name);
            }
            return obj;
        }
       
    }
}

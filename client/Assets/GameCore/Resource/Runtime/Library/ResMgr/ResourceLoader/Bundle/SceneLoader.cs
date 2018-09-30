using com.cyou.plugin.log;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.cyou.plugin.resource.loader.bundle
{

    class SceneLoader
    {
        private BundleResourceLoader m_BundleResourceLoader;


        ObjectBundleData m_NeedDestorySceneBundleData = null;
        //当前场景bundle数据
        ObjectBundleData m_CurrSceneBundleData = null;

        public SceneLoader(BundleResourceLoader bundleResourceLoader)
        {
            this.m_BundleResourceLoader = bundleResourceLoader;
        }
        public void LoadScene(IResConfig res)
        {
          

            BundleResConfig config = (BundleResConfig)res;
            LogSystem.Info("Start to load scene: {0}", config.MainName);

            if (config.BundleNameArray == null || config.MainName == null)
            {
                LogSystem.Error("Failed to load sence : {0} ", config.MainName);
                return;
            }
              
            if(m_CurrSceneBundleData != null)
                m_NeedDestorySceneBundleData = m_CurrSceneBundleData;


            ObjectBundleData objBundleData = ResourceUtils.NewObjectBundleData();
            for (int i = 1; i < config.BundleNameArray.Length; ++i)
            {
                String bundleName = config.BundleNameArray[i];

                BundleData bundleData = LoadBundle(bundleName, config);    
                if (bundleData == null || bundleData.Bundle == null  )
                {
                    LogSystem.Error("场景资源加载失败1 ！！{0} ", config.BundleNameArray[i]);
                    return;
                }
                objBundleData.AddDepBundleData(bundleName, bundleData);

            }
            String mainBundleName = config.BundleNameArray[0];
            BundleData mainBundle = LoadBundle(mainBundleName, config);
            
            if (mainBundle == null || mainBundle.Bundle == null)
            {
                LogSystem.Error("场景资源加载失败2 ！！{0} ", config.BundleNameArray[0]);
                return ;
            }
            objBundleData.MainBundle = mainBundle;
            objBundleData.ResName = config.MainName;

            
            m_CurrSceneBundleData = objBundleData;

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.LoadScene(config.MainName);
            m_CurrSceneBundleData.IncrRef(config.MainName);

        }
        private void UnloadLastSceneBundle()
        {
            if (m_NeedDestorySceneBundleData == null)
                return;

            m_NeedDestorySceneBundleData.DecrRef(m_NeedDestorySceneBundleData.ResName);
          

            if(m_NeedDestorySceneBundleData.MainBundle != null && m_NeedDestorySceneBundleData.MainBundle.RefCount == 0)
            {
                m_NeedDestorySceneBundleData.MainBundle.Bundle.Unload(true);
                BundlePool.RemoveDataFromUnusedCache(m_NeedDestorySceneBundleData.MainBundle);
            }
            for(int i = 0; i <  m_NeedDestorySceneBundleData.DependBundleList.Count; i++)
            {
                if(m_NeedDestorySceneBundleData.DependBundleList[i].RefCount == 0)
                {
                    m_NeedDestorySceneBundleData.DependBundleList[i].Bundle.Unload(true);
                    BundlePool.RemoveDataFromUnusedCache(m_NeedDestorySceneBundleData.DependBundleList[i]);
                }
            }

        }
        private BundleData LoadBundle(string bundleName, BundleResConfig config)
        {
            BundleData bundleData = null;
            if (BundlePool.ContainBundleData(bundleName))
            {
                bundleData = BundlePool.GetBundleData(bundleName);
                if (bundleData.Bundle != null)
                {
                    return bundleData;
                }
            }

            AssetBundle bundle = ResourceManager.m_AssetBundleLoader.LoadBundle(bundleName);

            if (bundleData == null)
            {
                bundleData = ResourceUtils.InstantiateBundleData();
                BundlePool.AddBundleData(bundleName, bundleData);
            }
            bundleData.Bundle = bundle;
            bundleData.BundleName = bundleName;
            bundleData.Config = config;          
            return bundleData;
        }
        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            UnloadLastSceneBundle();

            if (this.m_BundleResourceLoader.SceneLoadedCallBack != null)
                this.m_BundleResourceLoader.SceneLoadedCallBack(scene, loadSceneMode);
        }
    }
}

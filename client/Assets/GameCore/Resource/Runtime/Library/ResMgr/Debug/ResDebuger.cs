
using System;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    public class ResDebuger : MonoBehaviour
    {

        private static ResDebuger m_ResDebuger = null;
        private bool m_DebugShow = false;
        private IDebugUI m_DebugUI = null;

        private DebugUI_UsedAssetBundle m_UsedAssetBundleView = null;
        private DebugUI_AbPkgDetail m_AbPkgDetailView = null;
        private DebugUI_UnusedAssetBundle m_UnusedAssetBundleView = null;
        private DebugUI_ObjectAssetBundle m_ObjectAssetBundleView = null;
        private DebugUI_LoadingQueue m_LoadingQueueView = null;
        private DebugUI_Main m_DebugUI_Main = null;

        private float m_LastUpdateTime = 0;
        public static ResDebuger GetInstance()
        {
            if (m_ResDebuger == null)
            {
                GameObject go = new GameObject();
                go.name = "ResDebuger";
                m_ResDebuger = go.AddComponent<ResDebuger>();
                GameObject.DontDestroyOnLoad(go);

            }
            return m_ResDebuger;
        }

        public void Show(bool visible)
        {
            m_DebugShow = visible;
            ShowMain();


        }

        public void ShowMain()
        {
            if(m_DebugUI_Main == null)
            {
                m_DebugUI_Main = new DebugUI_Main();
            }
            m_DebugUI = m_DebugUI_Main;
            m_DebugUI_Main.RefreshData();
        }
        public void ShowUsedAssetBundlewView()
        {
            if (m_UsedAssetBundleView == null)
            {
                m_UsedAssetBundleView = new DebugUI_UsedAssetBundle();
            }
            m_DebugUI = m_UsedAssetBundleView;
        }
        public void ShowAbPkgDetail(string bundleName)
        {
            if (m_AbPkgDetailView == null)
            {
                m_AbPkgDetailView = new DebugUI_AbPkgDetail();
            }
            m_AbPkgDetailView.SetBundleName(bundleName);
            m_DebugUI = m_AbPkgDetailView;
        }

        public void ShowUnusedAssetBundlewView()
        {
            if (m_UnusedAssetBundleView == null)
            {
                m_UnusedAssetBundleView = new DebugUI_UnusedAssetBundle();
            }
            m_DebugUI = m_UnusedAssetBundleView;
        }

        public void ShowObjectAssetBundleView()
        {
            if (m_ObjectAssetBundleView == null)
            {
                m_ObjectAssetBundleView = new DebugUI_ObjectAssetBundle();
            }
            m_DebugUI = m_ObjectAssetBundleView;
        }

        public void ShowLoadQueueView()
        {
            if (m_LoadingQueueView == null)
            {
                m_LoadingQueueView = new DebugUI_LoadingQueue();
            }
            m_DebugUI = m_LoadingQueueView;
        }

        void OnGUI()
        {
            if (!m_DebugShow || m_DebugUI == null)
                return;

            GUI.Box(new Rect(90, 25, 600, 300), GUIContent.none);
            m_DebugUI.Draw();
        }
        void Update()
        {
            float time = Time.realtimeSinceStartup;
            if (time - m_LastUpdateTime > 1)
            {
                if (m_DebugUI != null)
                    m_DebugUI.Update();

                m_LastUpdateTime = time;
            }
           
        }
    }
}

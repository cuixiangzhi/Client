using com.cyou.plugin.resource.loader.bundle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class DebugUI_UsedAssetBundle : IDebugUI
    {
        private Vector2 sp = new Vector2(0,100);
        private Vector2 lt = new Vector2(100, 100);
        GUI_ListView m_ListView = new GUI_ListView();
        private List<BundleData> bundles = new List<BundleData>();
        public DebugUI_UsedAssetBundle()
        {
            m_ListView.SetPosition(100, 100);
            m_ListView.SetTitleHeight(20.0f);
            m_ListView.SetRowHeight(18.0f);        
            m_ListView.AddTitle("AssetBundle", 200.0f);
            m_ListView.AddTitle("Bundle RefCnt", 80.0f);
            m_ListView.AddTitle("Detail", 80.0f);
            m_ListView.AddRowClickedListener(OnRowClickedListener);
            RefreshData();
        }
        public void Draw()
        {
            //GUILayout.BeginArea(new Rect(Screen.width / 2, Screen.height / 2, 200, 200));
            ////在这之间绘制控件
            //GUILayout.EndArea();
            //sp = GUI.BeginScrollView(new Rect(200, 200, 200, 200), sp, new Rect(0, 0, Screen.width, 400));
            //GUI.Label(new Rect(100, 200, Screen.width, 50), "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            //GUI.EndScrollView();
            
            if (GUI.Button(new Rect(100, 50, 100, 20), "Back"))
            {
                ResDebuger.GetInstance().ShowMain();
            }
            if (GUI.Button(new Rect(200, 50, 100, 20), "Refresh"))
            {
                RefreshData();
            }

            

            m_ListView.Draw();
        }
        private void RefreshData()
        {
            m_ListView.ClearRowData();
            bundles.Clear();
            //HashSet<BundleData> unsedBundles = BundlePool.m_BundleUnUsedPool;
            //foreach (BundleData data in unsedBundles)
            //{
            //    m_ListView.AddRow(data.BundleName,data.RefCount.ToString());
            //}

            Dictionary<String, BundleData>.Enumerator en = BundlePool.m_vBundleDataDic.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current.Value.Bundle != null)
                {
                    m_ListView.AddRow(en.Current.Value.BundleName, en.Current.Value.RefCount.ToString(),"Detail");
                    bundles.Add(en.Current.Value);
                }
            }
        }
        private void OnRowClickedListener(int row, int column)
        {
            ResDebuger.GetInstance().ShowAbPkgDetail(bundles[row].BundleName);
            //Dictionary<String, UnityEngine.Object>.Enumerator en = bundles[row].m_vLoadedResObjDic.GetEnumerator();
            //while (en.MoveNext())
            //{
            //    Debug.LogError("loadedResObjDic " + en.Current.Key + "    " + en.Current.Value.name);
                
            //}
        }
        public void Update()
        {
            RefreshData();
        }
    }
}

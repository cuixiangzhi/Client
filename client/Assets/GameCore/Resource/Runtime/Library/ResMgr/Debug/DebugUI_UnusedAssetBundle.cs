using com.cyou.plugin.resource.loader.bundle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class DebugUI_UnusedAssetBundle : IDebugUI
    {
        private Vector2 sp = new Vector2(0,100);
        private Vector2 lt = new Vector2(100, 100);
        GUI_ListView m_ListView = new GUI_ListView();
        private List<BundleData> bundles = new List<BundleData>();
        public DebugUI_UnusedAssetBundle()
        {
            m_ListView.SetPosition(100, 100);
            m_ListView.SetTitleHeight(20.0f);
            m_ListView.SetRowHeight(18.0f);        
            m_ListView.AddTitle("AssetBundle", 200.0f);
            m_ListView.AddTitle("Bundle RefCnt", 100.0f);            
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

            HashSet < BundleData > unusedBundles  = BundlePool.m_BundleUnUsedPool;
            if(unusedBundles.Count <=0)
            {
                m_ListView.AddRow("None","None");
                return;
            }
            HashSet<BundleData>.Enumerator iter = unusedBundles.GetEnumerator();
            while (iter.MoveNext())
            {
                m_ListView.AddRow(iter.Current.BundleName, iter.Current.RefCount.ToString());

            }           
        }
        private void OnRowClickedListener(int row, int column)
        {
            Debug.LogError("row " + row + "  column " + column + "  " + bundles[row].BundleName);
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

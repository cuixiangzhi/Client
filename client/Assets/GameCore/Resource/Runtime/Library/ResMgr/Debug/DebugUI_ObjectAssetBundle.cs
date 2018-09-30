using com.cyou.plugin.resource.loader.bundle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class DebugUI_ObjectAssetBundle : IDebugUI
    {
        private Vector2 sp = new Vector2(0,100);
        private Vector2 lt = new Vector2(100, 100);
        GUI_ListView m_ListView = new GUI_ListView();
        private List<BundleData> bundles = new List<BundleData>();
        public DebugUI_ObjectAssetBundle()
        {
            m_ListView.SetPosition(100, 100);
            m_ListView.SetTitleHeight(20.0f);
            m_ListView.SetRowHeight(18.0f);        
            m_ListView.AddTitle("Object ", 200.0f);
            m_ListView.AddTitle("ObjectBundleData", 200.0f);            
            m_ListView.AddRowClickedListener(OnRowClickedListener);
            RefreshData();
        }
        public void Draw()
        {
            
            if (GUI.Button(new Rect(100, 50, 100, 20), "Back"))
            {
                ResDebuger.GetInstance().ShowMain();
            }
            if (GUI.Button(new Rect(200, 50, 100, 20),"Refresh"))
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

            Dictionary < UnityEngine.Object, ObjectBundleData > allObjectBundleDataMap  = BundlePool.m_vObjBundleDataDic;
            
            foreach (KeyValuePair<UnityEngine.Object, ObjectBundleData> pair in allObjectBundleDataMap)
            {
                if (pair.Key != null)
                {
                    m_ListView.AddRow(pair.Key.name,pair.Value.MainBundle.BundleName);
                }
            }
            
        }
        private void OnRowClickedListener(int row, int column)
        {   
            //ResDebuger.GetInstance().ShowAbPkgDetail(bundles[row].BundleName);
        }
        public void Update()
        {
            RefreshData();
        }
    }
}

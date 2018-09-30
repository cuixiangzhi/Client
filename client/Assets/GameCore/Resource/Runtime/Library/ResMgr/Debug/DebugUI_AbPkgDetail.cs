using com.cyou.plugin.resource.loader.bundle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class DebugUI_AbPkgDetail : IDebugUI
    {
        private Vector2 sp = new Vector2(0, 100);
        private Vector2 lt = new Vector2(100, 100);
        GUI_ListView m_ListView = new GUI_ListView();

        private string bundleName;
        public DebugUI_AbPkgDetail()
        {            
            m_ListView.SetPosition(100, 100);
            m_ListView.SetTitleHeight(20.0f);
            m_ListView.SetRowHeight(18.0f);
            m_ListView.AddTitle("BundleName " + this.bundleName, 500.0f);
            
        }
        public void SetBundleName(string bundleName)
        {
            this.bundleName = bundleName;
            RefreshData();
        }
        public void Draw()
        {
            GUI.Box(new Rect(90, 25, 600, 300), GUIContent.none);
            if (GUI.Button(new Rect(100, 50, 100, 20), "Back"))
            {
                ResDebuger.GetInstance().ShowUsedAssetBundlewView();
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

            BundleData bundle = BundlePool.m_vBundleDataDic[this.bundleName];
            //Dictionary<String, UnityEngine.Object>.Enumerator en = bundle.m_vLoadedResObjDic.GetEnumerator();
            //while (en.MoveNext())
            //{
            //    m_ListView.AddRow(en.Current.Key);
            //}
            List<string> directRefPaths =  bundle.m_DirectRefPath;
            foreach(string  path in directRefPaths)
            {
                m_ListView.AddRow("Direct:: " + path);
            }
            List<string> dependenRefPaths = bundle.m_DependensRefPath;
            foreach (string path in dependenRefPaths)
            {
                m_ListView.AddRow("Depends:: " + path);
            }
        }

        public void Update()
        {
            RefreshData();
        }
    }
}

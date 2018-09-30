using com.cyou.plugin.resource.loader.bundle;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class DebugUI_Main : IDebugUI
    {
      
        public void Draw()
        {

            if (GUI.Button(new Rect(100, 50, 100, 20), "Refresh"))
            {
                RefreshData();
            }

            if (GUI.Button(new Rect(100, 100, 200, 20), "AllAssetBundle"))
            {
                ResDebuger.GetInstance().ShowUsedAssetBundlewView();
            }

            if (GUI.Button(new Rect(300, 100, 200, 20), "AllUnsedAssetBundle"))
            {
                ResDebuger.GetInstance().ShowUnusedAssetBundlewView();
            }

            if (GUI.Button(new Rect(500, 100, 200, 20), "AllObject"))
            {
                ResDebuger.GetInstance().ShowObjectAssetBundleView();
            }
            //if (GUI.Button(new Rect(700, 100, 200, 20), "LoadingQueue"))
            //{
            //    ResDebuger.GetInstance().ShowObjectAssetBundleView();
            //}

            GUI.Label(new Rect(100, 150, 500, 20), "All UsedAssetBundle count : " + allUsedAssetBundleCnt);
            GUI.Label(new Rect(100, 180, 500, 20), "All UnusedAssetBundle count : " + allUnusedAssetBundleCnt);
            //GUI.Label(new Rect(100, 210, 500, 20), "AllObject count : " + 100);

        }
        int allUsedAssetBundleCnt = 0;
        int allUnusedAssetBundleCnt = 0;
        public void RefreshData()
        {
            allUsedAssetBundleCnt = BundlePool.m_vBundleDataDic.Count;
            allUnusedAssetBundleCnt  = BundlePool.m_BundleUnUsedPool.Count;
        }
        public void Update()
        {
            RefreshData();
        }
    }
}

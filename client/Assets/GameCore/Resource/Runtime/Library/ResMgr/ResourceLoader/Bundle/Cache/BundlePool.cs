using com.cyou.plugin.log;
using System;
using System.Collections.Generic;

namespace com.cyou.plugin.resource.loader.bundle
{
    class BundlePool
    {
        public static void Print()
        {
            PrintUsedBundle();
            PrintUnusedBundle();
        }

        #region 回收的池

        public static HashSet<BundleData> m_BundleUnUsedPool = new HashSet<BundleData>();

        public static void AddDataToUnusedCache(BundleData data)
        {
            if (m_BundleUnUsedPool.Contains(data))
            {
                LogSystem.Error("该BundleData已经在池中了，逻辑错误{0} ", data.BundleName);
                return;
            }
            m_BundleUnUsedPool.Add(data);
        }
        public static void RemoveDataFromUnusedCache(BundleData data)
        {
            if (!m_BundleUnUsedPool.Contains(data))
                return;
            m_BundleUnUsedPool.Remove(data);
        }


        internal static List<BundleData> m_RemoveBundleDataPool = new List<BundleData>();
        public static void CleanAllUnusedCacheData()
        {
            //Debug.LogError("清理资源内存！！！");
            if (m_BundleUnUsedPool.Count > 0)
            {
                HashSet<BundleData>.Enumerator iter = m_BundleUnUsedPool.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.CleanBundleData();
                    if (iter.Current.Bundle == null)
                    {
                        m_RemoveBundleDataPool.Add(iter.Current);
                    }
                }
                for (int i = 0; i < m_RemoveBundleDataPool.Count; ++i)
                {
                    m_BundleUnUsedPool.Remove(m_RemoveBundleDataPool[i]);
                    if(m_vBundleDataDic.ContainsKey(m_RemoveBundleDataPool[i].BundleName))
                    {
                        m_vBundleDataDic.Remove(m_RemoveBundleDataPool[i].BundleName);
                    }
                }
                
                m_RemoveBundleDataPool.Clear();
            }
        }
        public static void PrintUnusedBundle()
        {
            foreach (BundleData data in m_BundleUnUsedPool) 
            {
                LogSystem.Error("Unused AssetBundle: Name {0}, Ref {1}", data.BundleName, data.RefCount);
            }
        }
        #endregion


        #region bundle存储
        public static Dictionary<String, BundleData> m_vBundleDataDic = new Dictionary<string, BundleData>(15000);
        private static void PrintUsedBundle()
        {
            Dictionary<String, BundleData>.Enumerator en = m_vBundleDataDic.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current.Value.Bundle != null)
                {
                    LogSystem.Error("Used AssetBundle: Name {0}, Ref {1}", en.Current.Value.BundleName,en.Current.Value.RefCount);
                    LogSystem.Error(en.Current.Value.BundleName);                    
                }
            }
        }
        public static void AddBundleData(String key, BundleData rBData)
        {
            if (m_vBundleDataDic.ContainsKey(key) == false)
            {
                m_vBundleDataDic.Add(key, rBData);
            }
        }
        public static void RemoveBundleDataFromUsedPool(String key)
        {
            if (m_vBundleDataDic.ContainsKey(key))
            {
                m_vBundleDataDic.Remove(key);
            }
            else
            {
                LogSystem.Error("Cann't find bundle from used pool ,bundleName {0}", key);
            }
        }
        public static bool ContainBundleData(string key)
        {
            return m_vBundleDataDic.ContainsKey(key);
        }
        public static BundleData GetBundleData(string key)
        {
            return m_vBundleDataDic[key];
        }

        #endregion



        #region  对象记录
        public static Dictionary<UnityEngine.Object, ObjectBundleData> m_vObjBundleDataDic = new Dictionary<UnityEngine.Object, ObjectBundleData>();

        public static void AddObjectBundleData(UnityEngine.Object obj, ObjectBundleData obd)
        {
            m_vObjBundleDataDic.Add(obj, obd);
        }
        public static void RemoveObjectBundleData(UnityEngine.Object obj)
        {
            m_vObjBundleDataDic.Remove(obj);
        }
        public static bool ContainObjBundleDataByGO(UnityEngine.Object obj)
        {
            return m_vObjBundleDataDic.ContainsKey(obj);
        }
        public static ObjectBundleData GetObjBundleData(UnityEngine.Object obj)
        {
            if (m_vObjBundleDataDic.ContainsKey(obj))
            {
                return m_vObjBundleDataDic[obj];
            }

            return null;
        }

        #endregion
    }
}

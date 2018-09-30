using com.cyou.plugin.resource.loader.bundle;
using System;
using System.Collections.Generic;

namespace com.cyou.plugin.resource
{
    /// <summary>
    /// 资源加载的工具类，外部不调用。
    /// </summary>
    class ResourceUtils
    {

        #region ObjectBundleData 的数据池，减少GC使用
        private static Queue<ObjectBundleData> m_RecycleObjectBundleDataQueue = new Queue<ObjectBundleData>();
        
        /// <summary>
        /// 实例化一个ObjectBundleData结构体，用池管理起来，仿制内存频繁申请 
        /// </summary>
        /// <returns></returns>
        public static ObjectBundleData NewObjectBundleData()
        {
            if (m_RecycleObjectBundleDataQueue.Count > 0)
            {
                return m_RecycleObjectBundleDataQueue.Dequeue().Valided();
            }
            else
            {
                return (new ObjectBundleData()).Valided();
            }
        }
        /// <summary>
        /// 回收ObjectBundleData结构体，用于再次利用
        /// </summary>
        /// <param name="rData"></param>
        public static void DeleteObjectBundleData(ref ObjectBundleData rData)
        {
            m_RecycleObjectBundleDataQueue.Enqueue(rData.Release());
            rData = null;
        }
        #endregion


        #region BundleData池，不过目前没有使用

        public static BundleData InstantiateBundleData()
        {
            return new BundleData();
        }

        #endregion



        //默认IndexBuffer
        private static int m_nIndexCounter = -1;
        public static int GetCounter()
        {
            ++m_nIndexCounter;
            if (m_nIndexCounter < 0)
                throw new System.Exception(String.Format("GetCounter Resource Totals > {0}  ??????????????????", Int32.MaxValue));//理论不会超过。
            return m_nIndexCounter;
        }
    }
}

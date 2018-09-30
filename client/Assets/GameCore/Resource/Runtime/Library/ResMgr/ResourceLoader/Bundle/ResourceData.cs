using System.Collections.Generic;

namespace com.cyou.plugin.resource.loader.bundle
{

    class ResourceData
    {
        public UnityEngine.Object resObject = null;
        public UnityEngine.Object[] resObjectArray = null;
        public ObjectBundleData obData = null;
        public ObjectBundleData[] obDataArray = null;
        //
        public bool remove = false;
        //
        public BundleResConfig config = null;
        public BundleResConfig[] configArray = null;
        public bool isInstantiate = false;
        public bool isActive = false;
        public int index = -1;

        private bool Valid = true;
        public bool isValid()
        {
            return Valid;
        }
        public ResourceData Valided()
        {
            this.Valid = true;
            return this;
        }
        public ResourceData Release()
        {
            remove = false;
            Valid = false;
            resObject = null;
            resObjectArray = null;
            obData = null;
            obDataArray = null;
            config = null;
            configArray = null;
            isInstantiate = false;
            isActive = false;
            index = -1;
            return this;
        }


        //垃圾内存池 
        private static Queue<ResourceData> m_RecycleResourceDataQueue = new Queue<ResourceData>();

        public static ResourceData InstantiateResourceData()
        {
            if (m_RecycleResourceDataQueue.Count > 0)
            {
                return m_RecycleResourceDataQueue.Dequeue().Valided();
            }
            else
            {
                return (new ResourceData()).Valided();
            }
        }
        public static void RecycleResourceData(ResourceData rData)
        {
            if (rData == null)
                return;
            m_RecycleResourceDataQueue.Enqueue(rData.Release());
        }
        public static void ClearPool()
        {
            m_RecycleResourceDataQueue.Clear();
        }

    }
}

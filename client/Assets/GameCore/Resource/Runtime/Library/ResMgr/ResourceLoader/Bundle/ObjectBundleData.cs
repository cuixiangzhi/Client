using com.cyou.plugin.log;
using System;
using System.Collections.Generic;

namespace com.cyou.plugin.resource.loader.bundle
{
    class ObjectBundleData
    {
        public Int32 Key
        {
            get;
            set;
        }
        public Int32 RefCount
        {
            get;
            set;
        }

        public String ResName
        {
            get;
            set;
        }
        public BundleData MainBundle
        {
            set
            {
                mainBundle = value;
            }
            get
            {
                return mainBundle;
            }
        }

        private BundleData mainBundle = null;

        public List<BundleData> DependBundleList = new List<BundleData>();
        public void ClearDepBundleData()
        {
            DependBundleList.Clear();
        }
        public void AddDepBundleData(String resName, BundleData bundleData)
        {
            DependBundleList.Add(bundleData);
        }

        public void DestroyGameObject(UnityEngine.Object obj, String resName)
        {
            if (mainBundle != null)
            {
                mainBundle.DestroyGameObject(obj, resName);
            }
        }
        private bool Valid = true;
        public bool isValid()
        {
            return Valid;
        }
        public ObjectBundleData Valided()
        {
            this.Valid = true;
            return this;
        }
        public ObjectBundleData Release()
        {
            Key = -1;
            RefCount = 0;
            ResName = null;
            MainBundle = null;
            DependBundleList.Clear();
            Valid = false;
            return this;
        }    
        public UnityEngine.Object Load(String resName)
        {
            LogSystem.Info("Load object ::: {0}", resName);
            return mainBundle.Load(resName);
            
        }
        public void IncrRef(String resName)
        {
            RefCount++;
            mainBundle.IncrRef(resName,true);
            for (int i = 0; i < DependBundleList.Count; ++i)
            {
                if (DependBundleList[i] != null)
                {
                    DependBundleList[i].IncrRef(resName, false);
                }
                else
                {
                    LogSystem.Error("IncrRef关联资源丢失????");
                }
            }
        }
        public void DecrRef(String resName)
        {
            RefCount--;
            if (RefCount < 0)
            {
                LogSystem.Error("资源引用计数小于0 ??");
            }
            mainBundle.DecrRef(resName,true);
            for (int i = 0; i < DependBundleList.Count; ++i)
            {
                if (DependBundleList[i] != null)
                {
                    DependBundleList[i].DecrRef(resName, false);
                }

                else
                {
                    LogSystem.Error("DecrRef关联资源丢失????");
                }
            }
        }



       
    }
}

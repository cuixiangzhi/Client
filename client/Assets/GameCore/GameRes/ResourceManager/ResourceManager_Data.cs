/********************************************************************************
 *	创建人：	 李彬
 *	创建时间：   2015-06-11
 *
 *	功能说明： 
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using Games.TLBB.Manager.IO;
using AxpTools;


namespace Games.TLBB.Manager
{
    public enum ResourcePriority : int
    {
        ResourcePriority_Min = -3,
        ResourcePriority_Async = -2,
        ResourcePriority_SceneAsync = -1, //保留 已经无用
        ResourcePriority_Def = 0,  //默认载入级别  
        ResourcePriority_NULL = 1,  //保留 已经无用
        ResourcePriority_ScenePvw = 2,  //保留 已经无用
        ResourcePriority_Self = 3,  //自己的加载
        ResourcePriority_SynText = 4,  //文本加载 //没实现
        ResourcePriority_Max = 5,
    }
    public abstract partial class ResourceManager
    {
        #region CallFunStack
        static string GetStackTraceModelName()
        {
            //当前堆栈信息
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] sfs = st.GetFrames();
            //过虑的方法名称,以下方法将不会出现在返回的方法调用列表中
            string _filterdName = "ResponseWrite,ResponseWriteError,";
            string _fullName = string.Empty, _methodName = string.Empty;
            Int32 _fileLineNum;
            for (int i = 1; i < sfs.Length; ++i)
            {
                //非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
                if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) break;
                _methodName = sfs[i].GetMethod().Name;//方法名称
                _fileLineNum = sfs[i].GetFileLineNumber();//没有PDB文件的情况下将始终返回0
                if (_filterdName.Contains(_methodName)) continue;
                _fullName = _methodName + "()"+_fileLineNum+"->" + _fullName;
            }
            st = null;
            sfs = null;
            _filterdName = _methodName = null;
            return _fullName.TrimEnd('-','>');
        }
	    #endregion

        #region 解压APK文件
        private void ReadEntryFile(ZipEntry theEntry, ZipInputStream s)
        {
            byte[] data = null;
            if (theEntry.IsDirectory)
                return;

            if (theEntry.Name.Length <= 11)
                return;

            if ((string.Compare(theEntry.Name, 0, "assets/TLBB", 0, 11) != 0) && (string.Compare(theEntry.Name, 0, "assets/ExportScenes", 0, 19) != 0))
                return;

            int length = (int)(s.Length);
            data = new byte[length];
            if (s.Read(data, 0, length) > 0)
            {
                try
                {
                    //m_CatchLuaDic.Add(theEntry.Name.Substring(7), data);
                }
                catch (System.Exception e)
                {
                    BaseLogSystem.internal_Error(e.ToString());
                }
            }
#if DEBUG
            else
            {
                LogSystem.Error("缓存lua文件{0}失败！", theEntry.Name);
            }
#endif
        }
        private void DecompressAPK()
        {
            //using (ZipInputStream s = new ZipInputStream(File.OpenRead("c:/StreamingAssets.zip")))
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(Application.dataPath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    ReadEntryFile(theEntry, s);
                    s.CloseEntry();
                    continue;
                }
                s.Close();
            }
        }

        #endregion
        #region 静态变量
        public static String s_strReadWritePath = Application.persistentDataPath;
        #endregion
        
        #region 流限制配置
        public static int ForceDefaultLoadCount     = 20;
        public static int FrameSceneLoadCount       = 1;
        public static int FrameScenePvwLoadCount    = 1; 
        #endregion
        static ResourceManager()
        {
            PersistentAssetsPath = Application.persistentDataPath + "/Download/";
            //UseAndroidLibPath  = false;
        }
        public ResourceManager()
        {
            __Singleton = this;
        }
        public ResourceManager(IAxpSystem axpfilesystem)
        {
            /*
            if (axpfilesystem!=null)
            {
                UseBundle = true;
            }
            else
            {
                UseBundle = false;
            }
            if( UseBundle)
            {
                InitAxpFileSystem(axpfilesystem);
                m_ResourceLoader = new BundleResourceLoader();
            }
            else
            {
                m_ResourceLoader = new EditorResourceLoader();
            }
            m_ResourceLoader.ResourceCallBack  = this.DefaultResourceCallBack;
            m_ResourceLoader.ResourceArrayCallBack = this.DefaultResourceArrayCallBack;
            */
            //__Singleton = this;
        }
        protected void InitResourceLoader(bool useBundle)
        {
            UseBundle = useBundle;
            if (UseBundle)
            {
                m_ResourceLoader = new BundleResourceLoader();
            }
            else
            {
                m_ResourceLoader = new EditorResourceLoader();
            }
            m_ResourceLoader.ResourceCallBack = this.DefaultResourceCallBack;
            m_ResourceLoader.ResourceArrayCallBack = this.DefaultResourceArrayCallBack;
        }
        public bool InitAxp(IAxpSystem axpfilesystem)
        {
            if (UseBundle)
            {
                return InitAxpFileSystem(axpfilesystem);
            }
            return true;
        }
        public static ResourceManager Singleton
        {
            get { return __Singleton; }
        }
        private static ResourceManager __Singleton = null;
        
        #region 回调委托函数

        public delegate void DelegateResourceCallBack(int index, System.Object obj);

        public delegate void DelegateResourceArrayCallBack(int index, System.Object[] objArray);

        #endregion
        
        //默认IndexBuffer
        protected int m_nIndexCounter = -1;
        public int GetCounter()
        {
            ++m_nIndexCounter;
            if (m_nIndexCounter < 0)
                throw new System.Exception(String.Format("GetCounter Resource Totals > {0}  ??????????????????", Int32.MaxValue));//理论不会超过。
            return m_nIndexCounter;
        }

        //默认GroupIndexBuffer
        protected int m_nGroupIndexCounter = -1;
        public int GetGroupCounter()
        {
            ++m_nGroupIndexCounter;
            if (m_nGroupIndexCounter < 0)
                throw new System.Exception(String.Format("GetGroupCounter Resource Totals > {0}  ??????????????????", Int32.MaxValue));//理论不会超过。
            return m_nGroupIndexCounter;
        }

        #region 管理接口
        public void Init()
        {
            m_ResourceLoader.Init();
        }

        //private AsyncOperation asyncUnusedAssetsOp = null;

        //private float unusedAssetsTime = 0;
        private Int32 renderedFrame = 0;
        public virtual void Tick(uint uDeltaTimeMS)
        {
            if(Time.renderedFrameCount == renderedFrame)
            {
                return;
            }
            renderedFrame = Time.renderedFrameCount;
            try
            {
                m_ResourceLoader.Tick(uDeltaTimeMS);
            }
            catch { }
//             if (asyncUnusedAssetsOp == null)
//             {
//                 float realTime = Time.realtimeSinceStartup;
//                 if (realTime - unusedAssetsTime >= 30)
//                 {
//                     unusedAssetsTime = realTime;
//                     asyncUnusedAssetsOp = Resources.UnloadUnusedAssets();
//                     asyncUnusedAssetsOp.priority = 0;
//                    // GC.Collect();
//                 }
//             }
//             else
//             {
//                 if (asyncUnusedAssetsOp.isDone)
//                 {
//                     asyncUnusedAssetsOp = null;
//                     unusedAssetsTime = Time.realtimeSinceStartup;
//                 }
//             }
        }
        public virtual void Release()
        {
            m_ResourceLoader.Release();
//            asyncUnusedAssetsOp = null;
            //unusedAssetsTime = Time.realtimeSinceStartup;
        }
        #endregion
        #region IResConfig

        public delegate IResConfig DelegateGetResConfig(int resID);
        public delegate IResConfig[] DelegateGetResConfigArray(int[] resIDArray);
        public static HashSet<Int32> IndependentConfig
        {
            get;
            set;
        }
        public DelegateGetResConfig GetResConfig
        {
            get;
            set;
        }
        public DelegateGetResConfigArray GetResConfigArray
        {
            get;
            set;
        }
        #endregion

        ResourceLoader m_ResourceLoader = null;

        HashSet<Int32> m_vIndexSet = new HashSet<Int32>();

        Dictionary<Int32, DelegateResourceCallBack> m_vResourceCallBackDic = new Dictionary<int, DelegateResourceCallBack>();

        Dictionary<Int32, DelegateResourceCallBack> m_vDontDestroyResourceCallBackDic = new Dictionary<int, DelegateResourceCallBack>();

        Dictionary<Int32, DelegateResourceArrayCallBack> m_vResourceArrayCallBackDic = new Dictionary<int, DelegateResourceArrayCallBack>();

        Dictionary<Int32, DelegateResourceArrayCallBack> m_vDontDestroyResourceArrayCallBackDic = new Dictionary<int, DelegateResourceArrayCallBack>();
        public static bool UseBundle
        {
            get;
            private set;
        }

        public static bool UseLuaDefaultPath = true;
    }
}

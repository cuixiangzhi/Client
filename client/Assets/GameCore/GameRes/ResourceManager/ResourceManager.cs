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
    public abstract partial class ResourceManager
    {
        public static String SuffixName
        {
            get;
            set;
        }
        public static String PersistentAssetsPath
        {
            get;
            set;
        }
//         public static bool UseAndroidLibPath
//         {
//             get
//             {
//                 return m_bUseAndroidLibPath;
//             }
//             set
//             {
//                 if(Application.platform == RuntimePlatform.Android)
//                 {
//                     m_bUseAndroidLibPath = value;
//                 }
//                 else
//                 {
//                     m_bUseAndroidLibPath = false;
//                 }
//             }
//         }
//         private static bool m_bUseAndroidLibPath = false;
        public void PrintBundleList(String strFileFullPath)
        {
            m_ResourceLoader.PrintBundleList(strFileFullPath);
        }
        public byte[] LoadByteBuffer(string mainPath, Int32 offset, Int32 length, bool lockFile = false)
        {
            return m_ResourceLoader.LoadByteBuffer(mainPath, offset, length, lockFile);
        }
        public Byte[] LoadByteBuffer(string mainPath,bool lockFile = false)
        {
            return m_ResourceLoader.LoadByteBuffer(mainPath, lockFile);
        }
        public Byte[] LoadTextBuffer(string mainPath, bool decode = true)
        {
            return m_ResourceLoader.LoadTextBuffer(mainPath, decode);
        }
        public String LoadTextString(string mainPath, bool decode = true)
        {
            return m_ResourceLoader.LoadTextString(mainPath, decode);
        }
        public static void InitAndroidStreamingAssetsLoader(StreamingAssetsLoader loader)
        {
            ResourceLoader.m_StreamingAssetsLoader = loader;
        }
        private static bool InitAxpFileSystem(IAxpSystem axpfilesystem)
        {
            ResourceLoader.m_AxpFileSystem = axpfilesystem;
            if (!ResourceLoader.m_AxpFileSystem.Initial())
            {
#if DEBUG
                BaseLogSystem.Error("AxpFileSystem Initial Failed.");
#endif
                return false;
            }
            else
            {
#if DEBUG
                BaseLogSystem.Info("AxpFileSystem Initial Success.");
#endif
                return true;
            }

        }

        public static IAxpSystem GetAxpFileSystem()
        {
            return ResourceLoader.m_AxpFileSystem;
        }
//         public byte[] LoadLuaFile(string path)
//         {
//             if(UseBundle)
//             {
//                 if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(path))
//                 {
//                     uint t_DataSize, t_Offset;
//                     AxpFilePath axpFilePath;
//                     string t_axpFileName;
//                     return CusEncoding.EncodingUtil.FileByteToLocal(ResourceLoader.m_AxpFileSystem.openFileByBuffer(path, out t_axpFileName, out axpFilePath, out t_DataSize, out t_Offset));
//                 }
//                 byte[] data = ResourceLoader.m_PersistentAssetsLoader.Load(path);
//                 if (data == null)
//                     data = ResourceLoader.m_StreamingAssetsLoader.Load(path);
//                 return data;
//             }
//             else
//             {
//                 return CusEncoding.EncodingUtil.FileByteToLocal(File.ReadAllBytes(Application.dataPath + "/../StreamingAssets/" + path));
//                 //return ResourceLoader.m_StreamingAssetsLoader.Load(path);
//             }
//         }
        //public string LoadTableFile(string path)
        //{
        //    if (UseBundle)
        //    {
        //        Byte[] bytes = LoadByteBuffer(path);
        //        if (bytes != null)
        //        {
        //            return System.Text.Encoding.UTF8.GetString(bytes);
        //            //return new String(System.Text.Encoding.UTF8.GetChars(CusEncoding.EncodingUtil.FileByteToLocal(bytes)));
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        return (Resources.Load(path) as TextAsset).text;
        //    }
        //}
        
        public bool InitTSRData(string path,string key, int type)
        {
            return m_ResourceLoader.InitTSRData(path, key, type);
        }
        public bool InitFont(string path,string key,bool defaultFont = false)
        {
            return m_ResourceLoader.InitFont(path, key, defaultFont);
        }

        public Font GetFont(string key)
        {
            return m_ResourceLoader.GetFont(key);
        }
        public Font GetDefaultFont()
        {
            return m_ResourceLoader.GetDefaultFont();
        }
        //         public bool ReleaseTSRData(string key)
        //         {
        //             return m_ResourceLoader.ReleaseTSRData(key);
        //         }
        public AssetBundle LoadBundle(string path)
        {
            return m_ResourceLoader.LoadBundle(path);
        }

#if DEBUG
        protected bool ValidatePriority(Int32 priority)
        {
            if((priority > ((int)(ResourcePriority.ResourcePriority_Min))) && (priority < ((int)(ResourcePriority.ResourcePriority_Max))))
                return true;
            else
                return false;
        }
#endif
        protected void DefaultResourceCallBack(int index, System.Object obj)
        {
             if (m_vResourceCallBackDic.ContainsKey(index))
             {
                 m_vResourceCallBackDic[index](index, obj);
                 m_vResourceCallBackDic.Remove(index);
             }
             if (m_vDontDestroyResourceCallBackDic.ContainsKey(index))
             {
                 m_vDontDestroyResourceCallBackDic[index](index, obj);
                 m_vResourceCallBackDic.Remove(index);
             }
        }
        protected void DefaultResourceArrayCallBack(int index, System.Object[] objArray)
        {
            if (m_vResourceArrayCallBackDic.ContainsKey(index))
            {
                m_vResourceArrayCallBackDic[index](index, objArray);
                m_vResourceArrayCallBackDic.Remove(index);
            }
            if (m_vDontDestroyResourceArrayCallBackDic.ContainsKey(index))
            {
                m_vDontDestroyResourceArrayCallBackDic[index](index, objArray);
                m_vDontDestroyResourceArrayCallBackDic.Remove(index);
            }
        }

        public void RegisterCallBack(Int32 index, DelegateResourceCallBack callBack)
        {
            //if (m_vIndexSet.Contains(index))
            {
                if (m_vResourceCallBackDic.ContainsKey(index))
                {
#if DEBUG
                    BaseLogSystem.Error("已经注册回调的索引不能再次注册！");
#endif
                }
                else
                {
                    m_vResourceCallBackDic.Add(index, callBack);
                }
            }
//             else
//             {
//                 BaseLogSystem.Error("没有加载！不能注册回调！");
//             }
        }
        public void RegisterDontDestroyCallBack(Int32 index, DelegateResourceCallBack callBack)
        {
            //if (m_vIndexSet.Contains(index))
            {
                if (m_vDontDestroyResourceCallBackDic.ContainsKey(index))
                {
#if DEBUG
                    BaseLogSystem.Error("已经注册回调的索引不能再次注册！");
#endif
                }
                else
                {
                    m_vDontDestroyResourceCallBackDic.Add(index, callBack);
                }
            }
//             else
//             {
//                 BaseLogSystem.Error("没有加载！不能注册回调！");
//             }
        }
        public void RegisterArrayCallBack(Int32 index, DelegateResourceArrayCallBack callBack)
        {
           // if (m_vIndexSet.Contains(index))
            {
                if (m_vResourceArrayCallBackDic.ContainsKey(index))
                {
#if DEBUG
                    BaseLogSystem.Error("已经注册回调的索引不能再次注册！");
#endif
                }
                else
                {
                    m_vResourceArrayCallBackDic.Add(index, callBack);
                }
            }
//             else
//             {
//                 BaseLogSystem.Error("没有加载！不能注册回调！");
//             }
        }
        public void RegisterArrayDontDestroyCallBack(Int32 index, DelegateResourceArrayCallBack callBack)
        {
           // if (m_vIndexSet.Contains(index))
            {
                if (m_vDontDestroyResourceArrayCallBackDic.ContainsKey(index))
                {
                    BaseLogSystem.internal_Error("已经注册回调的索引不能再次注册！");
                }
                else
                {
                    m_vDontDestroyResourceArrayCallBackDic.Add(index, callBack);
                }
            }
//             else
//             {
//                 BaseLogSystem.Error("没有加载！不能注册回调！");
//             }
        }
        public UnityEngine.Object LoadResource(Int32 resID)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.LoadResource(rc);
            }
        }
        public UnityEngine.GameObject InstantiateGameObject(Int32 resID, bool isActive)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObject(rc, isActive);
            }
        }
        public UnityEngine.GameObject InstantiateGameObject(IResConfig config, bool isActive)
        {
            return m_ResourceLoader.InstantiateGameObject(config, isActive);
        }
        public String LoadTextResource(Int32 resID)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.LoadTextResource(rc);
            }
        }
        public String LoadTextResource(IResConfig config)
        {
            return m_ResourceLoader.LoadTextResource(config);
        }
        public byte[] LoadByteResource(Int32 resID)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return null;
            }
            else
            {
                return m_ResourceLoader.LoadByteResource(rc);
            }
        }

        public byte[] LoadByteResource(IResConfig config)
        {
            return m_ResourceLoader.LoadByteResource(config);
        }

        public byte[] LoadFileInStreamingAssets(string fileName)
        {
            return m_ResourceLoader.LoadFileInStreamingAssets(fileName);
        }
        public void StopLoading(Int32 index)
        {
            m_ResourceLoader.StopLoading(index);
        }
        public bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex)
        {
            return m_ResourceLoader.LoadingQueueReplace(priority, srcIndex, destIndex);
        }
        public Int32 DontDestroyOnInstantiateGameObjectAsync(Int32 resID, bool isActive, Int32 priority)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnInstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 DontDestroyOnLoadResourceAsync(Int32 resID, Int32 priority)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnLoadResourceAsync(rc, priority);
            }
        }
        public Int32 InstantiateGameObjectAsync(Int32 resID, bool isActive, Int32 priority=0)
        {
            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObjectAsync(rc, isActive, priority);
            }
        }

        public Int32 InstantiateGameObjectAsync(IResConfig config, bool isActive, Int32 priority = 0)
        {
            return m_ResourceLoader.InstantiateGameObjectAsync(config, isActive, priority);
        }

        public Int32 LoadResourceAsync(Int32 resID, Int32 priority=0)
        {
#if DEBUG
            if(ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig rc = GetResConfig(resID);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源{0} 配置不存在！", resID);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.LoadResourceAsync(rc, priority);
            }
        }
        public Int32 DontDestroyOnInstantiateGameObjectAsync(Int32[] resIDArray, bool isActive, Int32 priority=0)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnInstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 DontDestroyOnLoadResourceAsync(Int32[] resIDArray, Int32 priority=0)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.DontDestroyOnLoadResourceAsync(rc, priority);
            }

        }
        public Int32 InstantiateGameObjectAsync(Int32[] resIDArray, bool isActive, Int32 priority=0)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.InstantiateGameObjectAsync(rc, isActive, priority);
            }
        }
        public Int32 LoadResourceAsync(Int32[] resIDArray, Int32 priority=0)
        {
#if DEBUG
            if (ValidatePriority(priority) == false)
            {
                BaseLogSystem.Error("priority Error！{0}", priority);
            }
#endif

            IResConfig[] rc = GetResConfigArray(resIDArray);
            if (rc == null)
            {
#if DEBUG
                BaseLogSystem.Error("资源组 {0} 配置不存在！", resIDArray);
#endif
                return -1;
            }
            else
            {
                return m_ResourceLoader.LoadResourceAsync(rc, priority);
            }
        }

//         public void ConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj)
//         {
//             m_ResourceLoader.ConnectObject(parentObj, obj);
//         }

//         public void DisConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj)
//         {
//             m_ResourceLoader.DisConnectObject(parentObj, obj);
//         }
//         public UnityEngine.GameObject Instantiate(UnityEngine.Object obj)
//         {
//             return GameObject.Instantiate(obj) as GameObject;
//             //return m_ResourceLoader.Instantiate(obj);
//         }
//         public UnityEngine.GameObject Instantiate(UnityEngine.Object obj, Vector3 position, Quaternion rotation)
//         {
//             return GameObject.Instantiate(obj, position,rotation) as GameObject;
//             //return m_ResourceLoader.Instantiate(obj);
//         }
        public void DestroyResource(System.Object obj)
        {
            m_ResourceLoader.DestroyResource(obj);
        }
        public void DestroyResource(ref System.Object obj)
        {
            m_ResourceLoader.DestroyResource(obj);
            obj = null;
        }
        public void Destroy(System.Object obj)
        {
            DestroyResource(obj);
        }
        public void Destroy(ref System.Object obj)
        {
            DestroyResource(obj);
        }

        public void DestroyResourceArray(System.Object[] objArray)
        {
            if (objArray == null) return;
            for (int i = 0; i < objArray.Length;++i)
            {
                m_ResourceLoader.DestroyResource(objArray[i]);
            }
        }
        public void DestroyResourceArray(ref System.Object[] objArray)
        {
            DestroyResourceArray(objArray);
            objArray = null;
        }
        public virtual void MemClean()
        {
            m_ResourceLoader.MemClean();
        }

        public virtual void ForceMemClean()
        {
            m_ResourceLoader.ForceMemClean();
        }

        public void ForceAsyncEnd()
        {
            m_ResourceLoader.ForceAsyncEnd();
        }

        private delegate AssetBundle delegateLoadBundle(byte[] byteData);

        public static String MemCleanSizeKey = "MemCleanSize";
        public static Int32 MemCleanSize = 240;

//         public static String MemCleanIncrKey = "MemCleanIncr";
//         public static Int32 MemCleanIncr = 100;

        public static float MaxMono = 1024;

        public static String MemoryTimeIntervalKey = "MemoryTimeInterval";
        public static Int32 MemoryTimeInterval = 300;

//         public static String MinCollectKey = "MinCollect";
//         public static Int32 MinCollect = 70;//MB
// 
//         public static String TimeIntervalCollectKey = "TimeIntervalCollect";
//         public static Int32 TimeIntervalCollect = 300;//S
// 
//         public static String MaxCollectKey = "MaxCollect";
//         public static Int32 MaxCollect = 120;//MB

        public static String MonoCleanSizeKey = "MonoCleanSize";
        public static Int32  MonoCleanSize    = 100;//MB 超过这个size不在清理。
        public static String GCMonoKey        = "GCMono";//MB
        public static bool   GCMono           = true;//MB
        public static float  DropFrameTime    = 600;//second
        public static Int32  DropFrame        = 20;

        public static String AsyncLevelKey = "AsyncLevel";
        public static Int32  AsyncLevel     = -1024;

        public static String AsyncAssetKey = "AsyncAsset";
        public static bool AsyncAsset = false;

        //         public void RecyclePrefab(UnityEngine.Object obj)
        //         {
        //             m_ResourceLoader.RecyclePrefab(obj);
        //         }
        //         public void RecyclePrefab(ref UnityEngine.Object obj)
        //         {
        //             m_ResourceLoader.RecyclePrefab(ref obj);
        //         }
        //         public void RecyclePrefabArray(UnityEngine.Object[] objArray)
        //         {
        //             if (objArray == null) return;
        //             for (int i = 0; i < objArray.Length; ++i)
        //             {
        //                 m_ResourceLoader.RecyclePrefab(objArray[i]);
        //             }
        //         }
        //         public void RecyclePrefabArray(ref UnityEngine.Object[] objArray)
        //         {
        //             if (objArray == null) return;
        //             for (int i = 0; i < objArray.Length; ++i)
        //             {
        //                 m_ResourceLoader.RecyclePrefab(objArray[i]);
        //             }
        //             objArray = null;
        //         }
    }
}

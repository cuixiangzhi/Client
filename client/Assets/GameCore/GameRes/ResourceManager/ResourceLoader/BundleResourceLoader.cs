/********************************************************************************
 *	创建人：	 李彬
 *	创建时间：   2016-05-12
 *
 *	功能说明：  
 *	
 *	修改记录：
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AxpTools;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Games.TLBB.Manager.IO
{
    class BundleResourceLoader : ResourceLoader
    {
        private bool m_bInited = false;
        public override void Init()
        {
            if (m_bInited)
                return;
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_Async; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeLoadResourceDic[i] = new SortedDictionary<UInt64, ResourceData>();
                m_MultiPipeGenerateIndexDic[i] = 0;
            }
;
            m_bInited = true;
            Instance = this;
        }
        protected static BundleResourceLoader Instance
        {
            get;
            set;
        }
        //#region Platform
        //public BundleResourceLoader()
        //{
        //      if((Application.isEditor == false) && (Application.platform == RuntimePlatform.Android))
        //      {
        //          loadBundle = AndroidPlatformLoadBundle;
        //      }
        //      else
        //      {
        //          loadBundle = OtherPlatformLoadBundle;
        //      }
        //}
        //private delegate AssetBundle delegateLoadBundle(byte[] byteData);

        //private delegateLoadBundle loadBundle = null;
        //private AssetBundle AndroidPlatformLoadBundle(byte[] byteData)
        //{
        //    //return AssetBundle.CreateFromMemoryImmediate(parseBuffer(byteData));
        //    return AssetBundle.LoadFromMemory(byteData);
        //}

        //private AssetBundle OtherPlatformLoadBundle(byte[] byteData)
        //{
        //    //return AssetBundle.CreateFromMemoryImmediate(byteData);
        //    return AssetBundle.LoadFromMemory(byteData);
        //}
        //private byte[] parseBuffer(byte[] byteData)
        //{
        //    GCHandle cbuf = GCHandle.Alloc(byteData, GCHandleType.Pinned);
        //    int uncompressedSize = 0, res2 = byteData.Length;
        //    res2 -= 4;
        //    uncompressedSize = (int)BitConverter.ToInt32(byteData, res2);
        //    if( uncompressedSize > 10485760*2) //20
        //    {
        //        BaseLogSystem.Error("资源大于20MB ??????????????????????");
        //        return null;
        //    }
        //    if( uncompressedSize <10)
        //    {
        //        BaseLogSystem.Error("资源大小非法{0} ??????????????????????",uncompressedSize);
        //        return null;
        //    }
        //    //byte[] buff = new byte[uncompressedSize];
        //    byte[] buff = null;
        //    System.Array.Resize(ref buff, uncompressedSize);
        //    GCHandle obuf = GCHandle.Alloc(buff, GCHandleType.Pinned);
        //    int res = DecompressBuffer(cbuf.AddrOfPinnedObject(), obuf.AddrOfPinnedObject(), uncompressedSize);
        //    cbuf.Free();
        //    obuf.Free();
        //    if (res != res2)
        //    {
        //        BaseLogSystem.Error("parseBuffer Error !!");
        //        return null;
        //    }
        //    else
        //    {
        //        return buff;
        //    }
        //}

        //[DllImport("Game", EntryPoint = "LZ4DecompressBuffer")]
        //internal static extern int DecompressBuffer(IntPtr buffer, IntPtr outbuffer, int bufferLength);

        //#endregion
        //
        //Encrypt
        //Decrypt

        public override Byte[] LoadByteBuffer(string mainPath, Int32 offset, Int32 length, bool lockFile)
        {
            if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(mainPath))
            {
                return ResourceLoader.m_AxpFileSystem.openFileByBuffer(mainPath, (uint)offset, (uint)length, lockFile);
            }
            byte[] byteData = ResourceLoader.m_PersistentAssetsLoader.Load(mainPath);
            if (byteData != null)
            {
                return byteData;
            }
            else
            {
                return ResourceLoader.m_StreamingAssetsLoader.Load(mainPath);
            }
        }
        public override Byte[] LoadByteBuffer(string mainPath, bool lockFile)
        {
            if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(mainPath))
            {
                return ResourceLoader.m_AxpFileSystem.openFileByBuffer(mainPath, lockFile);
            }
            byte[] byteData = ResourceLoader.m_PersistentAssetsLoader.Load(mainPath);
            if (byteData != null)
            {
                return byteData;
            }
            else
            {
                return ResourceLoader.m_StreamingAssetsLoader.Load(mainPath);
            }
        }
        public override Byte[] LoadTextBuffer(string mainPath, bool decode)
        {
            byte[] byteData = LoadByteBuffer(mainPath,false);
            if (byteData != null)
            {
                if (decode)
                {
                    MemoryStream mem = new MemoryStream(byteData);
                    StreamReader sr = new StreamReader(mem);
                    String data = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    mem.Close();
                    mem.Dispose();
                    if (data != null)
                    {
                        return Convert.FromBase64String(data);
                    }
                }
                else
                {
                    return byteData;
                }

            }
            return null;
        }
        public override String LoadTextString(string mainPath, bool decode)
        {
            byte[] byteData = LoadTextBuffer(mainPath, decode);
            if (byteData != null)
            {
                if (decode)
                {
                    MemoryStream mem = new MemoryStream(byteData);
                    StreamReader sr = new StreamReader(mem);
                    String data = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    mem.Close();
                    mem.Dispose();
                    return data;
                }
                else
                {
                    return new String(System.Text.Encoding.UTF8.GetChars(byteData));
                }
            }
            else
            {
                return null;
            }
        }

        public override void PrintBundleList(String strFileFullPath)
        {
            StreamWriter sw = File.CreateText(strFileFullPath);
            if (sw != null)
            {
                List<String> strBundleList = new List<string>();
                Dictionary<String, BundleData>.Enumerator en = m_vBundleDataDic.GetEnumerator();
                while (en.MoveNext())
                {
                    if (en.Current.Value.Bundle != null)
                    {
                        strBundleList.Add(en.Current.Value.BundleName);
                    }
                }
                strBundleList.Sort();
                foreach (string strname in strBundleList)
                {
                    sw.WriteLine(strname);
                }
                sw.Close();
                sw.Dispose();
            }
        }
        public override void Release()
        {
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_Async; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeGenerateIndexDic[i] = 0;
            }

            foreach (KeyValuePair<int, SortedDictionary<UInt64, ResourceData>> data in m_MultiPipeLoadResourceDic)
            {
                data.Value.Clear();
            }
            m_ResouceDic.Clear();
            m_InstantiateResourceList.Clear();
            m_InstantiatePriorityResourceList.Clear();
            m_InstantiateSceneAsyncResourceList.Clear();
            m_StopResourceSet.Clear();
            m_LoadedResourceDataList.Clear();
            m_LoadedPriorityResourceDataList.Clear();
            //              foreach (KeyValuePair<Int32, ObjectBundleData> pair in m_vKeyBundleDataDic)
            //             {
            //                  pair.Value.Release(false, false);
            //              }
            //              m_vKeyBundleDataDic.Clear();
            //             if(m_AsyncResourceData != null)
            //             {
            //                 RecycleResourceData(m_AsyncResourceData);
            //                 m_AsyncResourceData = null;
            //             }
            ForceAsyncEnd();
            Dictionary<UnityEngine.Object, ObjectBundleData> objBundleDataDic = new Dictionary<UnityEngine.Object, ObjectBundleData>();
            foreach (KeyValuePair<UnityEngine.Object, ObjectBundleData> pair in m_vObjBundleDataDic)
            {
                if (pair.Key != null)
                {
                    objBundleDataDic.Add(pair.Key, pair.Value);
                }
            }
            m_vObjBundleDataDic.Clear();
            m_vObjBundleDataDic = objBundleDataDic;
            MemClean();
            m_RecycleResourceDataQueue.Clear();
            //RecordLastTotalAllocatedMemory = true;
            waitMonoFrame = -1;
            lastGCTime = Time.realtimeSinceStartup + 60;
        }

        private bool  memClean = false;
        private float memCleanTime = -1;
        private bool  forceMemClean = false;
        public override void MemClean()
        {
            forceMemClean = true;
            memClean = true;
            memCleanTime = Time.realtimeSinceStartup;
        }

        private Int32 TotalReservedMemory = -1;
        //private Int32 LastTotalAllocatedMemory = -1;
        //private bool  RecordLastTotalAllocatedMemory = false;
        private Int32 TotalReservedMemoryTime = -1;
        private Int32 SampTime = -1;
        protected  int InvTime = 10;
        protected int waitMonoFrame = -1;

        protected static bool closeAsync = false;
        protected static int asyncLevel = -1024;
        public override void Tick(uint uDeltaTimeMS)
        {
            Int32 time = (Int32)Time.realtimeSinceStartup;
            if( closeAsync)
            {
                asyncLevel = -1024;
            }
            else
            {
                asyncLevel = ResourceManager.AsyncLevel;
            }
            if (time - SampTime < InvTime)
            {
                TickRemoveResource();
                TickResource();
                return;
            }
            
            if (time - memCleanTime > 10)
            {
                memClean = true;
                memCleanTime = time;
            }
            TotalReservedMemory = (Int32)(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1048576.0f));
            Int32 TotalMonoMemory = (Int32)(GC.GetTotalMemory(false) / (1048576.0f));
            TotalReservedMemory += TotalMonoMemory;
            //Int32 TotalAllocatedMemory = (Int32)(UnityEngine.Profiler.GetTotalAllocatedMemory() / (1048576.0f));
//             if (RecordLastTotalAllocatedMemory)
//             {
//                 RecordLastTotalAllocatedMemory = false;
//                 if(TotalAllocatedMemory >= LastTotalAllocatedMemory)
//                 {
//                     LastTotalAllocatedMemory = TotalAllocatedMemory - (ResourceManager.MemCleanIncr / 2);
//                 }
//                 else
//                 {
//                     LastTotalAllocatedMemory = TotalAllocatedMemory;
//                 }
//                 //Debug.Log("增加清理后内存" + LastTotalAllocatedMemory);
//             }
            SampTime = time;
            if (TotalReservedMemory > ResourceManager.MemCleanSize)
            {
                if (((time - TotalReservedMemoryTime) > ResourceManager.MemoryTimeInterval))
                {
                    ResourceManager.Singleton.MemClean();
                    //RecordLastTotalAllocatedMemory = true;
                    TotalReservedMemoryTime = (Int32)Time.realtimeSinceStartup;
                    return;
                }
            }
            if ( waitMonoFrame > 0 )
            {
                waitMonoFrame--;
                if(waitMonoFrame == 0)
                {
                    waitMonoFrame = -1;
                    if (TotalMonoMemory > ResourceManager.MaxMono)
                    {
                        if (Debug.unityLogger.logEnabled == false)
                        {
                            BaseLogSystem.internal_Error("Mono Memory!");
                           // Application.Quit();
                        }
                    }
                }
                return;
            }
            else
            {
                if (TotalMonoMemory > ResourceManager.MaxMono)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    waitMonoFrame = 10;
                    return;
                }
            }
            if(ResourceManager.GCMono)
            {
                if (Time.realtimeSinceStartup - lastGCTime > 60)
                {
                    lastGCTime = Time.realtimeSinceStartup;
                    int size = ResourceManager.MonoCleanSize;
                    if (size <= 0)
                    {
                        return;
                    }
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                }
            }
            //             if(TotalAllocatedMemory - LastTotalAllocatedMemory > ResourceManager.MemCleanIncr)
            //             {
            //                 ResourceManager.Singleton.MemSlowClean();
            //                 RecordLastTotalAllocatedMemory = true;
            //                 return;
            //                 //Debug.Log("调用了增长回收" + LastTotalAllocatedMemory);
            //             }
        }
        private float lastGCTime = -1;
        protected void TickResource()
        {
            if (forceMemClean)
            {
                ForceAsyncEnd();
                forceMemClean = false;
                BundleData.CleanAllCacheData();
                return;
            }
            TickDontDestroyOnLoad();
            bool asyncDone = internal_TickLoadResourceAsyncDone();
            if (asyncDone && memClean)
            {
                TotalReservedMemory = (Int32)(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1048576.0f));
                TotalReservedMemory += (Int32)(GC.GetTotalMemory(false) / (1048576.0f));
                if (TotalReservedMemory > ResourceManager.MemCleanSize)
                {
                    BundleData.CleanAllCacheData();
                }
                memClean = false;
                return;
            }
            TickPriorityLoadResource(asyncDone);
            TickPriorityInstantiateResource();
            TickPriorityLoadedResource();
            TickLoadResource(asyncDone);
            TickInstantiateResource();
            TickLoadedResource();
        }
        protected void TickLoadedResource()
        {
            ResourceData rData = null;
            if (m_LoadedResourceDataList.Count >= 30)
            {
                BaseLogSystem.internal_Info("回调队列长度:{0}", m_LoadedResourceDataList.Count);
            }
            ResourceAsyncTime = Time.realtimeSinceStartup;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                {
                    //BaseLogSystem.internal_Info("资源回调耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                    break;
                }
                rData = m_LoadedResourceDataList[i];
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            if (rData.resObject)
                            {
                                (rData.resObject as GameObject).SetActive(true);
                            }
                            else
                            {
                                for (int j = 0; j < rData.resObjectArray.Length; ++j)
                                {
                                    (rData.resObjectArray[j] as GameObject).SetActive(true);
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        BaseLogSystem.internal_Error(e.ToString());
                    }
                    if (rData.resObject)
                        ResourceCallBack(rData.index, rData.resObject);
                    else
                        ResourceArrayCallBack(rData.index, rData.resObjectArray);

                    RecycleResourceData(rData);
                }
                m_LoadedResourceDataList.RemoveAt(i);
            }
        }
        protected void TickInstantiateResource()
        {
            ResourceData rData;
            if (SceneAsyncLoadedResource == false)
            {
                while (m_InstantiateSceneAsyncResourceList.Count > 0)
                {
                    rData = m_InstantiateSceneAsyncResourceList[0];
                    if (rData.resObject)
                    {
                        ObjectBundleData data = m_vObjBundleDataDic[rData.resObject];
                        rData.resObject = GameObject.Instantiate(rData.resObject);
                        if (rData.isActive)
                        {
                            (rData.resObject as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObject as GameObject).SetActive(false);
                        }
                        m_vObjBundleDataDic.Add(rData.resObject, data);
                    }
                    else
                    {
                        for (int i = 0; i < rData.resObjectArray.Length; ++i)
                        {
                            ObjectBundleData data = m_vObjBundleDataDic[rData.resObjectArray[i]];
                            rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                            if (rData.isActive)
                            {
                                (rData.resObjectArray[i] as GameObject).SetActive(true);
                            }
                            else
                            {
                                (rData.resObjectArray[i] as GameObject).SetActive(false);
                            }
                            m_vObjBundleDataDic.Add(rData.resObjectArray[i], data);
                        }
                    }
                    m_LoadedResourceDataList.Add(rData);
                    m_InstantiateSceneAsyncResourceList.RemoveAt(0);
                }
            }

            if (m_InstantiateResourceList.Count >= 30)
            {
                BaseLogSystem.internal_Info("实例化队列长度:{0}", m_InstantiateResourceList.Count);
            }

            ResourceAsyncTime = Time.realtimeSinceStartup;
            while (m_InstantiateResourceList.Count > 0)
            {
                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                {
                    BaseLogSystem.internal_Info("资源实例化耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                    break;
                }
                rData = m_InstantiateResourceList[0];
                if (rData.resObject)
                {
                    ObjectBundleData data = m_vObjBundleDataDic[rData.resObject];
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                    if (rData.isActive)
                    {
                        (rData.resObject as GameObject).SetActive(true);
                    }
                    else
                    {
                        (rData.resObject as GameObject).SetActive(false);
                    }
                    m_vObjBundleDataDic.Add(rData.resObject, data);
                }
                else
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        ObjectBundleData data = m_vObjBundleDataDic[rData.resObjectArray[i]];
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                        if (rData.isActive)
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(false);
                        }
                        m_vObjBundleDataDic.Add(rData.resObjectArray[i], data);
                    }
                }
                m_LoadedResourceDataList.Add(rData);
                m_InstantiateResourceList.RemoveAt(0);
            }
        }

        protected void TickPriorityLoadedResource()
        {
            ResourceData rData = null;
            for (int i = m_LoadedPriorityResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedPriorityResourceDataList[i];
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            if (rData.resObject)
                            {
                                (rData.resObject as GameObject).SetActive(true);
                            }
                            else
                            {
                                for (int j = 0; j < rData.resObjectArray.Length; ++j)
                                {
                                    (rData.resObjectArray[j] as GameObject).SetActive(true);
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        BaseLogSystem.internal_Error(e.ToString());
                    }
                    if (rData.resObject)
                        ResourceCallBack(rData.index, rData.resObject);
                    else
                        ResourceArrayCallBack(rData.index, rData.resObjectArray);

                    RecycleResourceData(rData);
                }
                m_LoadedPriorityResourceDataList.RemoveAt(i);
            }
        }
        protected void TickPriorityInstantiateResource()
        {
            ResourceData rData;
            while (m_InstantiatePriorityResourceList.Count > 0)
            {
                rData = m_InstantiatePriorityResourceList[0];
                if (rData.resObject)
                {
                    ObjectBundleData data = m_vObjBundleDataDic[rData.resObject];
                    rData.resObject = GameObject.Instantiate(rData.resObject);
                    if (rData.isActive)
                    {
                        (rData.resObject as GameObject).SetActive(true);
                    }
                    else
                    {
                        (rData.resObject as GameObject).SetActive(false);
                    }
                    m_vObjBundleDataDic.Add(rData.resObject, data);
                }
                else
                {
                    for (int i = 0; i < rData.resObjectArray.Length; ++i)
                    {
                        ObjectBundleData data = m_vObjBundleDataDic[rData.resObjectArray[i]];
                        rData.resObjectArray[i] = GameObject.Instantiate(rData.resObjectArray[i]);
                        if (rData.isActive)
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(true);
                        }
                        else
                        {
                            (rData.resObjectArray[i] as GameObject).SetActive(false);
                        }
                        m_vObjBundleDataDic.Add(rData.resObjectArray[i], data);
                    }
                }
                m_LoadedPriorityResourceDataList.Add(rData);
                m_InstantiatePriorityResourceList.RemoveAt(0);
            }
        }
        protected void TickPriorityLoadResource(bool asyncDone)
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator iter;
            SortedDictionary<UInt64, ResourceData> sortedDic;
            bool bRet = false;
            Int32 index = (int)ResourcePriority.ResourcePriority_SynText;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                iter = sortedDic.GetEnumerator();
                while (iter.MoveNext())
                {
                    rData = iter.Current.Value;
                    if (rData.remove)
                    {
                        RecycleResourceData(rData);
                    }
                    else
                    {
                        if (rData.configArray != null)
                        {
                            bool bError = false;
                            rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                            for (int i = 0; i < rData.configArray.Length; ++i)
                            {
                                rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                if (rData.resObjectArray[i] == null)
                                {
                                    BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                    RecycleResourceData(rData);
                                    bError = true;
                                    break;
                                }
                                else
                                {
                                    if ((rData.resObjectArray[i] is TextAsset) == false)
                                    {
                                        BaseLogSystem.internal_Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.configArray[i].MainName);
                                        RecycleResourceData(rData);
                                        bError = true;
                                        break;
                                    }
                                }
                            }
                            if (bError == false)
                            {
                                if (rData.isInstantiate)
                                {
                                    BaseLogSystem.internal_Error("ResourcePriority_Syn Can not Instantiate!!");
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    m_LoadedPriorityResourceDataList.Add(rData);
                                }
                            }
                        }
                        else
                        {
                            rData.resObject = Internal_LoadResource(rData.config);
                            if (rData.resObject == null)
                            {
                                BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                RecycleResourceData(rData);
                            }
                            else
                            {
                                if ((rData.resObject is TextAsset) == false)
                                {
                                    BaseLogSystem.internal_Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.config.MainName);
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    if (rData.isInstantiate)
                                    {
                                        BaseLogSystem.internal_Error("ResourcePriority_Syn Can not Instantiate!!");
                                        RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                            }
                        }
                    }
                    sortedDic.Remove(iter.Current.Key);
                    iter = sortedDic.GetEnumerator();
                }
            }
            //Int32 asyncLevel = ResourceManager.AsyncLevel;
            index = (int)ResourcePriority.ResourcePriority_Self;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index,5))
                    {
                        return;
                    }
                }
                else
                {
                    iter = sortedDic.GetEnumerator();
                    if (iter.MoveNext())
                    {
                        while (true)
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    bool bError = false;
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            RecycleResourceData(rData);
                                            bError = true;
                                            break;
                                        }
                                    }
                                    if (bError == false)
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiatePriorityResourceList.Add(rData);
                                        else
                                            m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiatePriorityResourceList.Add(rData);
                                        else
                                            m_LoadedPriorityResourceDataList.Add(rData);
                                    }
                                }
                            }

                            sortedDic.Remove(iter.Current.Key);
                            iter = sortedDic.GetEnumerator();
                            bRet = iter.MoveNext();
                            if (bRet == false)
                                return;
                        }
                    }
                }
            }
        }

        private bool SceneAsyncLoadedResource = false;

        private float ResourceAsyncTime = Time.realtimeSinceStartup;

        public override void ForceMemClean()
        {
            ForceAsyncEnd();
            BundleData.CleanAllCacheData();
        }
        public override void ForceAsyncEnd()
        {
            if(m_vBundleAsyncDataDic.Count > 0)
            {
                BundleData rBData;
                Dictionary<String, BundleData>.Enumerator enIter = m_vBundleAsyncDataDic.GetEnumerator();
                enIter = m_vBundleAsyncDataDic.GetEnumerator();
                while (enIter.MoveNext())
                {
                    String key = enIter.Current.Key;
                    rBData = enIter.Current.Value;
                    if( rBData != null)
                    {
                        if (rBData.BundleRequest != null)
                        {
                            if (rBData.Bundle == null)
                            {
                                rBData.Bundle = rBData.BundleRequest.assetBundle;
                            }
                        }
                        if (m_vBundleDataDic.ContainsKey(key) == false)
                        {
                            m_vBundleDataDic.Add(key, rBData);
                        }
                        rBData.BundleRequest = null;
                        rBData.AssetRequest = null;
                    }
                    m_vBundleAsyncDataDic.Remove(key);
                    enIter = m_vBundleAsyncDataDic.GetEnumerator();
                }
                m_vBundleAsyncDataDic.Clear();
                internalLoadResourceAsyncData();
            }
        }

        protected void internalLoadResourceAsyncData()
        {
            ResourceData m_AsyncResourceData = null; 
            int count = m_AsyncResourceDataList.Count;
            for(int w=0; w < count; ++w)
            {
                m_AsyncResourceData = m_AsyncResourceDataList[w];
                if (m_AsyncResourceData != null)
                {
                    if (m_AsyncResourceData.obDataArray != null)
                    {
                        int len = m_AsyncResourceData.obDataArray.Length;
                        m_AsyncResourceData.resObjectArray = new UnityEngine.Object[len];
                        for (int i = 0; i < len; ++i)
                        {
                            BundleResConfig config = m_AsyncResourceData.configArray[i];
                            ObjectBundleData obData = m_AsyncResourceData.obDataArray[i];
                            UnityEngine.Object obj = obData.Load(config.MainName);
                            if (obj != null)
                            {
                                if (m_vObjBundleDataDic.ContainsKey(obj))
                                {
                                    m_vObjBundleDataDic[obj].IncrRef(config.MainName);
                                }
                                else
                                {
                                    obData.IncrRef(config.MainName);
                                    m_vObjBundleDataDic.Add(obj, obData);
                                }
                                m_AsyncResourceData.resObjectArray[i] = obj;
                            }
                            else
                            {
                                RecycleObjectBundleData(ref obData);
                                m_vKeyBundleDataDic.Remove(config.Key);
                                RecycleResourceData(m_AsyncResourceData);
                                m_AsyncResourceData = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        BundleResConfig config = m_AsyncResourceData.config;
                        ObjectBundleData obData = m_AsyncResourceData.obData;
                        UnityEngine.Object obj = obData.Load(config.MainName);
                        if (obj != null)
                        {
                            if (m_vObjBundleDataDic.ContainsKey(obj))
                            {
                                m_vObjBundleDataDic[obj].IncrRef(config.MainName);
                            }
                            else
                            {
                                m_AsyncResourceData.obData.IncrRef(config.MainName);
                                m_vObjBundleDataDic.Add(obj, obData);
                            }
                            m_AsyncResourceData.resObject = obj;
                        }
                        else
                        {
                            RecycleObjectBundleData(ref obData);
                            m_vKeyBundleDataDic.Remove(config.Key);
                            RecycleResourceData(m_AsyncResourceData);
                            m_AsyncResourceData = null;
                        }
                    }
                    if (m_AsyncResourceData != null)
                    {
                        if (m_AsyncResourceData.isInstantiate)
                            m_InstantiateSceneAsyncResourceList.Add(m_AsyncResourceData);
                        else
                            m_LoadedResourceDataList.Add(m_AsyncResourceData);
                    }
                    m_AsyncResourceData = null;
                }
            }
            m_AsyncResourceDataList.Clear();
        }
        protected bool internal_TickLoadResourceAsyncDone()
        {
            Int32 nCount = m_vBundleAsyncDataDic.Count; 
            if (nCount > 0)
            {
                BundleData rBData;
                Dictionary<String, BundleData>.Enumerator enIter = m_vBundleAsyncDataDic.GetEnumerator();
                if(ResourceManager.AsyncAsset && nCount == 1)
                {
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        if(rBData.AssetRequest == null)
                        {
                            if( rBData.Bundle == null)
                            {
                                if (rBData.BundleRequest == null)
                                {
                                    BaseLogSystem.internal_Error("Error!!3!!");
                                    rBData.ReloadAsync();
                                    return false;
                                }
                                if (rBData.BundleRequest.isDone == false)
                                {
                                    return false;
                                }
                                if (rBData.Bundle == null)
                                {
                                    rBData.Bundle = rBData.BundleRequest.assetBundle;
                                }
                                if (rBData.Bundle == null)
                                {
                                    rBData.ReloadAsync();
                                    return false;
                                }
                            }
                            rBData.BundleRequest = null;
                            rBData.AssetRequest = rBData.Bundle.LoadAllAssetsAsync();
                            if(rBData.AssetRequest == null)
                            {
                                rBData.ReloadAsync();
                                return false;
                            }
                            return false;
                        }
                        else
                        {
                            if (rBData.AssetRequest == null && rBData.Bundle == null)
                            {
                                BaseLogSystem.internal_Error("Error!!4!!");
                                rBData.ReloadAsync();
                                return false;
                            }
                            if(rBData.AssetRequest == null)
                            {
                                if(rBData.Bundle == null)
                                {
                                    BaseLogSystem.internal_Error("Error!!7!!");
                                    rBData.ReloadAsync();
                                    return false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (rBData.AssetRequest.isDone)
                                {
                                    String key = enIter.Current.Key;
                                    if (m_vBundleDataDic.ContainsKey(key) == false)
                                    {
                                        m_vBundleDataDic.Add(key, rBData);
                                    }
                                    rBData.AssetRequest = null;
                                    m_vBundleAsyncDataDic.Clear();
                                    break;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        if (rBData.BundleRequest == null)
                        {
                            BaseLogSystem.internal_Error("Error!!5!!");
                            rBData.ReloadAsync();
                            return false;
                        }
                        if (rBData.BundleRequest.isDone == false)
                        {
                            return false;
                        }
                    }
                    enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    while (enIter.MoveNext())
                    {
                        rBData = enIter.Current.Value;
                        String key = enIter.Current.Key;
                        if(rBData.Bundle == null)
                        {
                            rBData.Bundle = rBData.BundleRequest.assetBundle;
                        }
                        if(rBData.Bundle == null)
                        {
                            BaseLogSystem.internal_Error("Error!!6!!");
                            rBData.ReloadAsync();
                            return false;
                        }
                        if (m_vBundleDataDic.ContainsKey(key) == false)
                        {
                            m_vBundleDataDic.Add(key, rBData);
                        }
                        rBData.BundleRequest = null;
                        rBData.AssetRequest = null;
                        m_vBundleAsyncDataDic.Remove(key);
                        enIter = m_vBundleAsyncDataDic.GetEnumerator();
                    }
                }
                if (m_vBundleAsyncDataDic.Count != 0)
                    return false;
            }
            internalLoadResourceAsyncData();
            if (m_AsyncResourceDataList.Count == 0)
            {
                closeAsync = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool internal_TickLoadResourceAsync(int index,int count = 0)
        {
            if (m_AsyncResourceDataList.Count != 0)
                return true;

            SortedDictionary<UInt64, ResourceData>  sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count == 0)
                return  false;

            ResourceData rData = null;
            do
            {
                SortedDictionary<UInt64, ResourceData>.Enumerator iter = sortedDic.GetEnumerator();
                if (iter.MoveNext())
                {
                    rData = iter.Current.Value;
                    if (rData.remove)
                    {
                        RecycleResourceData(rData);
                        return true;
                    }
                    else
                    {
                        ObjectBundleData rOBData;
                        if (rData.configArray != null)
                        {
                            rData.obDataArray = new ObjectBundleData[rData.configArray.Length];
                            for (int i = 0; i < rData.configArray.Length; ++i)
                            {
                                if (Internal_LoadResourceAsync(rData.configArray[i], out rOBData))
                                {
                                    if (rOBData == null)
                                    {
                                        BaseLogSystem.internal_Error("6Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                        RecycleResourceData(rData);
                                        rData = null;
                                        return true;
                                    }
                                    else
                                    {
                                        rData.obDataArray[i] = rOBData;
                                    }
                                }
                                else
                                {
                                    BaseLogSystem.internal_Error("3Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                    RecycleResourceData(rData);
                                    rData = null;
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (Internal_LoadResourceAsync(rData.config, out rOBData))
                            {
                                if (rOBData == null)
                                {
                                    BaseLogSystem.internal_Error("7Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                    RecycleResourceData(rData);
                                    rData = null;
                                    return true;
                                }
                                else
                                {
                                    rData.obData = rOBData;
                                }
                            }
                            else
                            {
                                BaseLogSystem.internal_Error("4Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                RecycleResourceData(rData);
                                rData = null;
                                return true;
                            }
                        }
                    }
                    sortedDic.Remove(iter.Current.Key);
                }
                if (rData != null)
                {
                    m_AsyncResourceDataList.Add(rData);
                }
                rData = null;
            } while (count-- > 0);
            return true;
        }
        protected void TickLoadResource(bool asyncDone)
        {
            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator iter;
            SortedDictionary<UInt64, ResourceData> sortedDic;
            bool bRet = false;
            //Int32 asyncLevel = ResourceManager.AsyncLevel;
            Int32 index = (int)ResourcePriority.ResourcePriority_Def;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index))
                    {
                        return;
                    }
                }
                else
                {
                    ResourceAsyncTime = Time.realtimeSinceStartup;
                    iter = sortedDic.GetEnumerator();
                    if (sortedDic.Count > ResourceManager.ForceDefaultLoadCount)
                    {
                        while (iter.MoveNext())
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    bool bError = false;
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            RecycleResourceData(rData);
                                            bError = true;
                                            break;
                                        }
                                    }
                                    if (bError == false)
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateResourceList.Add(rData);
                                        else
                                        {
                                            m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateResourceList.Add(rData);
                                        else
                                        {
                                            m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }
                            }
                            sortedDic.Remove(iter.Current.Key);
                            iter = sortedDic.GetEnumerator();
                        }
                    }
                    else
                    {
                        if (iter.MoveNext())
                        {
                            while (true)
                            {
                                rData = iter.Current.Value;
                                if (rData.remove)
                                {
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    if (rData.configArray != null)
                                    {
                                        bool bError = false;
                                        rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                        for (int i = 0; i < rData.configArray.Length; ++i)
                                        {
                                            rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                            if (rData.resObjectArray[i] == null)
                                            {
                                                BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                                RecycleResourceData(rData);
                                                bError = true;
                                                break;
                                            }
                                        }
                                        if (bError == false)
                                        {
                                            if (rData.isInstantiate)
                                                m_InstantiateResourceList.Add(rData);
                                            else
                                                m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                    else
                                    {
                                        rData.resObject = Internal_LoadResource(rData.config);
                                        if (rData.resObject == null)
                                        {
                                            BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                            RecycleResourceData(rData);
                                        }
                                        else
                                        {
                                            if (rData.isInstantiate)
                                                m_InstantiateResourceList.Add(rData);
                                            else
                                                m_LoadedResourceDataList.Add(rData);
                                        }
                                    }
                                }

                                sortedDic.Remove(iter.Current.Key);
                                if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                                {
                                    //BaseLogSystem.internal_Info("资源加载耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                                    break;
                                }
                                iter = sortedDic.GetEnumerator();
                                bRet = iter.MoveNext();
                                if (bRet == false)
                                    break;
                            }
                        }
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////
            index = (int)ResourcePriority.ResourcePriority_Async;
            sortedDic = m_MultiPipeLoadResourceDic[index];
            if (sortedDic.Count > 0)
            {
                if (asyncLevel >= index)
                {
                    if (asyncDone && internal_TickLoadResourceAsync(index))
                    {
                        return;
                    }
                }
                else
                {
                    sortedDic = m_MultiPipeLoadResourceDic[(int)ResourcePriority.ResourcePriority_Async];
                    ResourceAsyncTime = Time.realtimeSinceStartup;
                    if (sortedDic.Count >= 30)
                    {
                        BaseLogSystem.internal_Info("加载队列数量:{0}", sortedDic.Count);
                    }
                    do
                    {
                        if (Time.realtimeSinceStartup - ResourceAsyncTime >= 0.005f)
                        {
                            BaseLogSystem.internal_Info("资源加载耗时:{0}", Time.realtimeSinceStartup - ResourceAsyncTime);
                            break;
                        }

                        if (sortedDic.Count == 0)
                            break;

                        iter = sortedDic.GetEnumerator();
                        if (iter.MoveNext())
                        {
                            rData = iter.Current.Value;
                            if (rData.remove)
                            {
                                RecycleResourceData(rData);
                            }
                            else
                            {
                                if (rData.configArray != null)
                                {
                                    rData.resObjectArray = new UnityEngine.Object[rData.configArray.Length];
                                    for (int i = 0; i < rData.configArray.Length; ++i)
                                    {
                                        rData.resObjectArray[i] = Internal_LoadResource(rData.configArray[i]);
                                        if (rData.resObjectArray[i] == null)
                                        {
                                            BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.configArray[i].MainName);
                                            RecycleResourceData(rData);
                                            break;
                                        }
                                    }
                                    if (rData.isInstantiate)
                                        m_InstantiateSceneAsyncResourceList.Add(rData);
                                    else
                                        m_LoadedResourceDataList.Add(rData);
                                }
                                else
                                {
                                    rData.resObject = Internal_LoadResource(rData.config);
                                    if (rData.resObject == null)
                                    {
                                        BaseLogSystem.internal_Error("Load Resource Failed!! resPath = {0}", rData.config.MainName);
                                        RecycleResourceData(rData);
                                    }
                                    else
                                    {
                                        if (rData.isInstantiate)
                                            m_InstantiateSceneAsyncResourceList.Add(rData);
                                        else
                                            m_LoadedResourceDataList.Add(rData);
                                    }
                                }
                            }
                            sortedDic.Remove(iter.Current.Key);
                        }
                    }
                    while (true);
                }
            }
        }
        protected void TickDontDestroyOnLoad()
        {
            if (m_LoadingDontDestroyResouce != null)
            {
                if (m_LoadingDontDestroyResouce.resObject)
                {

                    if (m_LoadingDontDestroyResouce.isInstantiate)
                    {
                        ObjectBundleData rData = m_vObjBundleDataDic[m_LoadingDontDestroyResouce.resObject];
                        if (rData != null)
                        {
                            m_LoadingDontDestroyResouce.resObject = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObject);
                            ((GameObject)m_LoadingDontDestroyResouce.resObject).SetActive(m_LoadingDontDestroyResouce.isActive);
                            GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObject);
                            m_vObjBundleDataDic.Add(m_LoadingDontDestroyResouce.resObject, rData);
                        }
                        else
                        {
                            BaseLogSystem.internal_Error("TickDontDestroyOnLoad 1 Error!!!!");
                        }


                    }
                    ResourceCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObject);
                }
                else
                {
                    if (m_LoadingDontDestroyResouce.isInstantiate)
                    {
                        for (int i = 0; i < m_LoadingDontDestroyResouce.resObjectArray.Length; ++i)
                        {
                            ObjectBundleData rData = m_vObjBundleDataDic[m_LoadingDontDestroyResouce.resObjectArray[i]];
                            if (m_LoadingDontDestroyResouce.resObjectArray[i] != null)
                            {
                                m_LoadingDontDestroyResouce.resObjectArray[i] = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObjectArray[i]);
                                ((GameObject)m_LoadingDontDestroyResouce.resObjectArray[i]).SetActive(m_LoadingDontDestroyResouce.isActive);
                                GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObjectArray[i]);
                            }
                            else
                            {
                                BaseLogSystem.internal_Error("TickDontDestroyOnLoad 2 Error!!!!");
                            }
                            m_vObjBundleDataDic.Add(m_LoadingDontDestroyResouce.resObjectArray[i], rData);
                        }
                    }
                    ResourceArrayCallBack(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObjectArray);
                }
                RecycleResourceData(m_LoadingDontDestroyResouce);
                m_LoadingDontDestroyResouce = null;
            }
            else
            {
                SortedDictionary<int, ResourceData>.Enumerator enu = m_DontDestroyResouceDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    m_LoadingDontDestroyResouce = enu.Current.Value;
                    m_DontDestroyResouceDic.Remove(enu.Current.Key);
                    if (m_LoadingDontDestroyResouce.configArray != null)
                    {
                        for (Int32 i = 0; i < m_LoadingDontDestroyResouce.configArray.Length; ++i)
                        {
                            m_LoadingDontDestroyResouce.resObjectArray[i] = Internal_LoadResource(m_LoadingDontDestroyResouce.configArray[i]);
                            if (m_LoadingDontDestroyResouce.resObjectArray[i] == null)
                            {
                                BaseLogSystem.internal_Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  resObjectArray == null", m_LoadingDontDestroyResouce.configArray[i].MainName);
                                RecycleResourceData(m_LoadingDontDestroyResouce);
                                m_LoadingDontDestroyResouce = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        m_LoadingDontDestroyResouce.resObject = Internal_LoadResource(m_LoadingDontDestroyResouce.config);
                        if (m_LoadingDontDestroyResouce.resObject == null)
                        {
                            BaseLogSystem.internal_Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  request == null", m_LoadingDontDestroyResouce.config.MainName);
                            RecycleResourceData(m_LoadingDontDestroyResouce);
                            m_LoadingDontDestroyResouce = null;
                        }
                    }
                }
            }
        }

        protected void TickRemoveResource()
        {
            if (m_StopResourceSet.Count == 0)
                return;
            bool bRet;
            HashSet<Int32>.Enumerator iter = m_StopResourceSet.GetEnumerator();
            int index;
            while (iter.MoveNext())
            {
                index = iter.Current;
                bRet = (((RemoveLoadedResource(index)) ?
                             true : RemoveInstantiateResource(index)) ?
                             //true : RemoveLoadingResource(index)) ?
                             true : RemoveLoadResource(index));
                if (bRet == false)
                {
                    BaseLogSystem.internal_Warn("RemoveResource {0} Not Find!!", index);
                }
            }
            m_StopResourceSet.Clear();
        }
        protected bool RemoveLoadedResource(Int32 index)
        {
            if (m_LoadedResourceDataList.Count == 0)
                return false;

            ResourceData rData = null;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedResourceDataList[i];
                if (rData.index == index)
                {
                    m_LoadedResourceDataList.RemoveAt(i);
                    RecycleResourceData(rData);
                    return true;
                }
            }
            return false;
        }
        protected bool RemoveInstantiateResource(Int32 index)
        {

            ResourceData rData;
            for (int i = 0; i < m_InstantiateResourceList.Count; ++i)
            {
                rData = m_InstantiateResourceList[i];
                if (rData.index == index)
                {
                    m_InstantiateResourceList.RemoveAt(i);
                    RecycleResourceData(rData);
                    return true;
                }
            }
            return false;
        }

        protected bool RemoveLoadResource(Int32 index)
        {
            if (m_MultiPipeLoadResourceDic.Count == 0)
                return false;

            ResourceData rData = null;
            SortedDictionary<UInt64, ResourceData> sortedDic = null;
            SortedDictionary<UInt64, ResourceData>.Enumerator enu;
            Dictionary<int, SortedDictionary<UInt64, ResourceData>>.Enumerator iter = m_MultiPipeLoadResourceDic.GetEnumerator();
            while (iter.MoveNext())
            {
                sortedDic = iter.Current.Value;
                enu = sortedDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    while (true)
                    {
                        rData = enu.Current.Value;
                        if (rData.index == index)
                        {
                            sortedDic.Remove(enu.Current.Key);
                            RecycleResourceData(rData);
                            return true;
                        }
                        else
                        {
                            bool bRet = enu.MoveNext();
                            if (bRet == false)
                                break;
                        }
                    }
                }
            }
            return false;
        }

        public bool BundleDirtyMark
        {
            get;
            set;
        }

        //基于KEY索引关系
        Dictionary<Int32, ObjectBundleData> m_vKeyBundleDataDic = new Dictionary<int, ObjectBundleData>();

        //对象记录
        Dictionary<UnityEngine.Object, ObjectBundleData> m_vObjBundleDataDic = new Dictionary<UnityEngine.Object, ObjectBundleData>();

        //bundle存储
        Dictionary<String, BundleData> m_vBundleDataDic = new Dictionary<string, BundleData>(15000);
        
        //bundle异步加载存储
        Dictionary<String, BundleData> m_vBundleAsyncDataDic = new Dictionary<string, BundleData>();


        //ResourceData m_AsyncResourceData = null;

        List<ResourceData> m_AsyncResourceDataList = new List<ResourceData>(5);

        Queue<ObjectBundleData> m_RecycleObjectBundleDataQueue = new Queue<ObjectBundleData>();
        enum BundleDataType : byte
        {
            BundleDataType_UIPrefab = 1,
            BundleDataType_Prefab = 2,
            BundleDataType_Material = 3,
            BundleDataType_Resource = 4,
        };
        private BundleData InstantiateBundleData(bool isTSR)
        {
            if (isTSR)
            {
                return new TSRBundleData();
            }
            else
            {
                return new BundleData();
            }
        }

        private ObjectBundleData InstantiateObjectBundleData()
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
        private void RecycleObjectBundleData(ref ObjectBundleData rData)
        {
            m_RecycleObjectBundleDataQueue.Enqueue(rData.Release());
            rData = null;
        }

        protected class ObjectBundleData
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
            }

            private BundleData mainBundle = null;

            private List<String> ResNameList = new List<string>();
            private List<BundleData> BundleList = new List<BundleData>();
            public void ClearDepBundleData()
            {
                ResNameList.Clear();
                BundleList.Clear();
            }
            public void AddDepBundleData(String resName, BundleData bundleData)
            {
                ResNameList.Add(resName);
                BundleList.Add(bundleData);
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
                ResNameList.Clear();
                BundleList.Clear();
                Valid = false;
                return this;
            }
//             public void Release(bool unLoadBundle, bool unloadAllLoadedObjects)
//             {
//                 if (mainBundle.RefCount == 0)
//                 {
//                     for (int i = 0; i < BundleList.Count; ++i)
//                     {
//                         if (BundleList[i] != null)
//                         {
//                             BundleList[i].CleanBundleData(true);
//                         }
//                     }
//                 }
//             }
            public UnityEngine.Object Load(String resName)
            {
                return mainBundle.Load(resName);
            }
            public void IncrRef(String resName)
            {
                RefCount++;
                mainBundle.IncrRef(resName);
                for (int i = 0; i < BundleList.Count; ++i)
                {
                    if (BundleList[i] != null)
                    {
                        BundleList[i].IncrRef(ResNameList[i]);
                    }
                    else
                    {
                        BaseLogSystem.internal_Error("IncrRef关联资源丢失????");
                    }
                }
            }
            public void DecrRef(String resName)
            {
                RefCount--;
                if (RefCount < 0)
                {
                    BaseLogSystem.internal_Error("资源引用计数小于0 ??");
                }
                mainBundle.DecrRef(resName);
                for (int i = 0; i < BundleList.Count; ++i)
                {
                    if (BundleList[i] != null)
                    {
                        BundleList[i].DecrRef(ResNameList[i]);
                    }
                    else
                    {
                        BaseLogSystem.internal_Error("DecrRef关联资源丢失????");
                    }
                }
            }
        }

        protected class BundleData
        {
            public BundleData()
            {
                ConfigIndex = -1;
            }
            public bool ReloadAsync()
            {
                if (ConfigIndex < 0)
                    return false;
                if( Bundle)
                {
                    Bundle.Unload(true);
                }
                BundleResourceLoader.closeAsync = true;
                String mainPath = Config.MainPathArray[ConfigIndex];
                BundleRequest = BundleResourceLoader.Instance.LoadBundleAsync(mainPath);
                Bundle = null;
                return true;
            }
            protected class CacheData
            {
                public String Key;
                public UnityEngine.Object Value;
                public BundleData Data;
                public CacheData(BundleData data, String key, UnityEngine.Object value)
                {
                    Data = data;
                    Key = key;
                    Value = value;
                }
                public CacheData SetCacheData(BundleData data, String key, UnityEngine.Object value)
                {
                    Data = data;
                    Key = key;
                    Value = value;
                    return this;
                }

                public void ClearCacheData()
                {
                    Data = null;
                    Key = null;
                    Value = null;
                }
                public override bool Equals(object obj)
                {
                    if (obj == null)
                    {
                        BaseLogSystem.internal_Error("池异常1 {0} {1} {2}", this.Key, this.Value, this.Data);
                        return false;
                    }
                    if ((obj.GetType().Equals(this.GetType())) == false)
                    {
                        BaseLogSystem.internal_Error("池异常2 {0} {1}", this.GetType(), obj.GetType());
                        return false;
                    }
                    CacheData cData = (CacheData)obj;
                    if (Data == null || cData.Data == null)
                    {
                        BaseLogSystem.internal_Error("池异常3 {0} {1}", Data, cData.Data);
                        return false;
                    }
                    if (Data.Equals(cData.Data) && cData.Key == Key && cData.Value == Value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                public override int GetHashCode()
                {
                    if (this.Key == null)
                    {
                        return this.Data.GetHashCode();
                    }
                    else
                    {
                        return this.Key.GetHashCode() + this.Data.GetHashCode();
                    }
                }
                public static CacheData InstantiateCacheData(BundleData data, String key, UnityEngine.Object value)
                {
                    if (m_CacheDataQueue.Count > 0)
                    {
                        return m_CacheDataQueue.Dequeue().SetCacheData(data, key, value);
                    }
                    else
                    {
                        return new CacheData(data, key, value);
                    }
                }
                public static void RecycleCacheData(CacheData data)
                {
                    data.ClearCacheData();
                    m_CacheDataQueue.Enqueue(data);
                }
                public static void ClearCacheDataPool()
                {
                    m_CacheDataQueue.Clear();
                }
                private static Queue<CacheData> m_CacheDataQueue = new Queue<CacheData>();
            }
            static SortedDictionary<long, Dictionary<CacheData, CacheData>> m_CachePool = new SortedDictionary<long, Dictionary<CacheData, CacheData>>();
            static Dictionary<CacheData, long> m_CacheDataPool = new Dictionary<CacheData, long>();

            internal static HashSet<BundleData> m_BundleDataPool = new HashSet<BundleData>();
            internal static HashSet<BundleData> m_MatBundleDataPool = new HashSet<BundleData>();
            internal static bool PreRemoveDataFromCache(BundleData data, String key, UnityEngine.Object value)
            {
                switch (data.type)
                {
                    //case BundleDataType.Material:
                    case BundleDataType.Multi_Material:
                        {
                            if (m_MatBundleDataPool.Contains(data) == false)
                            {
                                m_MatBundleDataPool.Add(data);
                            }
                        }
                        return true;
                    case BundleDataType.Prefab:
                    case BundleDataType.Multi_Prefab:
                        //case BundleDataType.Resource:
                        return true;
                    default:
                        return false;
                }
            }
            internal static bool PreAddDataToCache(BundleData data, String key, UnityEngine.Object value)
            {
                switch (data.type)
                {
                    //case BundleDataType.Material:
                    case BundleDataType.Multi_Material:
                        {
                            if (m_MatBundleDataPool.Contains(data) == false)
                            {
                                m_MatBundleDataPool.Add(data);
                            }
                        }
                        return true;
                    case BundleDataType.Prefab:
                    case BundleDataType.Multi_Prefab:
                        //case BundleDataType.Resource:
                        {
                            if (m_BundleDataPool.Contains(data) == false)
                            {
                                m_BundleDataPool.Add(data);
                            }
                        }
                        return true;
                    default:
                        return false;
                }
            }
            internal static void RemoveDataFromCache(BundleData data, String key, UnityEngine.Object value)
            {
                if (PreRemoveDataFromCache(data, key, value))
                    return;

                CacheData cData = CacheData.InstantiateCacheData(data, key, value);
                long index = Internal_RemoveDataFromCache(cData);
                if (index >= 0)
                {
                    Dictionary<CacheData, CacheData> tempDic = m_CachePool[index];
                    CacheData tempData = tempDic[cData];
                    if (tempDic.Count == 1)
                    {
                        m_CachePool.Remove(index);
                    }
                    else
                    {
                        tempDic.Remove(cData);
                    }
                    CacheData.RecycleCacheData(tempData);
                }
                CacheData.RecycleCacheData(cData);
            }
            protected static long Internal_RemoveDataFromCache(CacheData cData)
            {
                //不维护CacheData
                if (m_CacheDataPool.ContainsKey(cData))
                {
                    long index = m_CacheDataPool[cData];
                    m_CacheDataPool.Remove(cData);
                    return index;
                }
                else
                {
                    return -1;
                }
            }

            internal static void AddDataToCache(BundleData data, String key, UnityEngine.Object value)
            {
                if (PreAddDataToCache(data, key, value))
                    return;

                CacheData cData;
                cData = CacheData.InstantiateCacheData(data, key, value);
                long time = Games.TLBB.Common.Timer.TotalMinutes;
                if (m_CacheDataPool.ContainsKey(cData))
                {
                    long index = m_CacheDataPool[cData];
                    Dictionary<CacheData, CacheData> tempDic = m_CachePool[index];
                    CacheData tempData = tempDic[cData];
                    CacheData.RecycleCacheData(cData);
                    if (index == time)
                        return;

                    m_CachePool[index].Remove(tempData);
                    m_CacheDataPool[tempData] = time;
                    if (m_CachePool.ContainsKey(time))
                    {
                        m_CachePool[time].Add(tempData, tempData);
                    }
                    else
                    {
                        m_CachePool.Add(time, new Dictionary<CacheData, CacheData>() { { tempData, tempData } });
                    }
                }
                else
                {
                    m_CacheDataPool.Add(cData, time);
                    if (m_CachePool.ContainsKey(time))
                    {
                        m_CachePool[time].Add(cData, cData);
                    }
                    else
                    {
                        m_CachePool.Add(time, new Dictionary<CacheData, CacheData>() { { cData, cData } });
                    }
                }
            }

            internal static List<BundleData> m_RemoveBundleDataPool = new List<BundleData>();
            internal static void ClearBundleDataPool(bool CleanMulti_Prefab)
            {
                if (m_MatBundleDataPool.Count > 0)
                {
                    HashSet<BundleData>.Enumerator iter = m_MatBundleDataPool.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        iter.Current.CleanBundleData(CleanMulti_Prefab);
                        if (iter.Current.Bundle == null)
                        {
                            m_RemoveBundleDataPool.Add(iter.Current);
                        }
                    }
                    for (int i = 0; i < m_RemoveBundleDataPool.Count; ++i)
                    {
                        m_MatBundleDataPool.Remove(m_RemoveBundleDataPool[i]);
                    }
                    m_RemoveBundleDataPool.Clear();
                }
                if (m_BundleDataPool.Count > 0)
                {
                    HashSet<BundleData>.Enumerator iter = m_BundleDataPool.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        iter.Current.CleanBundleData(CleanMulti_Prefab);
                        if (iter.Current.Bundle == null)
                        {
                            m_RemoveBundleDataPool.Add(iter.Current);
                        }
                    }
                    for (int i = 0; i < m_RemoveBundleDataPool.Count; ++i)
                    {
                        m_BundleDataPool.Remove(m_RemoveBundleDataPool[i]);
                    }
                    m_RemoveBundleDataPool.Clear();
                }

            }
            //             public static void CleanOverflowCacheData(bool CleanMulti_Prefab)
            //             {
            //                 ClearBundleDataPool(CleanMulti_Prefab);
            //                 if (m_nCacheDataSize > ResourceManager.ResourceCacheSize)
            //                 {
            //                     CacheData cData;
            //                     Int32 nCleanSize = ResourceManager.ResourceCacheCleanSize + m_nCacheDataSize - ResourceManager.ResourceCacheSize;
            //                     List<long> removeList = new List<long>();
            //                     long index;
            //                     foreach (KeyValuePair<long, Dictionary<CacheData, CacheData>> pair in m_CachePool)
            //                     {
            //                         if (nCleanSize <= 0)
            //                         {
            //                             break;
            //                         }
            //                         Dictionary<CacheData, CacheData>.Enumerator iter = pair.Value.GetEnumerator();
            //                         while (iter.MoveNext())
            //                         {
            //                             cData = iter.Current.Key;
            //                             index = Internal_RemoveDataFromCache(cData);
            //                             if (index < 0)
            //                             {
            //                                 BaseLogSystem.Error("删除资源错误！！！{0} {1}", cData.Key, cData.Data);
            //                             }
            //                             else
            //                             {
            //                                 m_CachePool[index].Remove(cData);
            //                                 nCleanSize -= GetDataSize(cData.Key);
            //                                 //Resources.UnloadAsset(cData.Value);
            //                                 cData.Data.CleanBundleData(false);
            //                                 CacheData.RecycleCacheData(cData);
            //                                 if (nCleanSize <= 0)
            //                                 {
            //                                     break;
            //                                 }
            //                                 iter = pair.Value.GetEnumerator();
            //                             }
            //                         }
            //                         if (pair.Value.Count == 0)
            //                             removeList.Add(pair.Key);
            //                     }
            //                     for (int i = 0; i < removeList.Count; ++i)
            //                     {
            //                         m_CachePool.Remove(removeList[i]);
            //                     }
            //                     m_nCacheDataSize -= nCleanSize;
            //                     if( m_nCacheDataSize < 0)
            //                     {
            //                         m_nCacheDataSize = 0;
            //                     }
            //                 }
            //             }
            public static void CleanAllCacheData()
            {
                //Debug.LogError("清理资源内存！！！");
                ClearBundleDataPool(true);
                CacheData cData;
                long index;
                foreach (KeyValuePair<long, Dictionary<CacheData, CacheData>> pair in m_CachePool)
                {
                    Dictionary<CacheData, CacheData>.Enumerator iter = pair.Value.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        cData = iter.Current.Key;
                        index = Internal_RemoveDataFromCache(cData);
                        if (index < 0)
                        {
                            BaseLogSystem.internal_Error("删除资源错误！！！{0} {1}", cData.Key, cData.Data);
                        }
                        else
                        {
                            m_CachePool[index].Remove(cData);
                            //Resources.UnloadAsset(cData.Value);
                            cData.Data.CleanBundleData(true);
                            CacheData.RecycleCacheData(cData);
                            iter = pair.Value.GetEnumerator();
                        }
                    }
                }
                m_CachePool.Clear();
                m_CacheDataPool.Clear();
                CacheData.ClearCacheDataPool();
            }
            public BundleResConfig Config
            {
                get;
                set;
            }
            public Int32 ConfigIndex
            {
                get;
                set;
            }
            public String BundleName
            {
                get;
                set;
            }
            public AssetBundleCreateRequest BundleRequest
            {
                get;
                set;
            }
            public AssetBundleRequest AssetRequest
            {
                get;
                set;
            }
            public AssetBundle Bundle
            {
                get
                {
                    return m_Bundle;
                }
                set
                {
                    
                    if (m_Bundle != value)
                    {
                        m_Bundle = value;
                        if (m_Bundle != null)
                        {
                            BundleRequest = null;
                            //mainAsset = m_Bundle.mainAsset;
                            if (m_Bundle.GetAllAssetNames().Length == 1)
                            {
                                mainAsset = m_Bundle.LoadAsset(m_Bundle.GetAllAssetNames()[0]);
                            }

#if DEBUG
                            if( type != BundleDataType.Unknow)
                            {
                                if (mainAsset is GameObject)
                                {
                                    if( type != BundleDataType.Prefab)
                                    {
                                        BaseLogSystem.Error("资源类型不符！！Prefab {0} {1}",type,mainAsset);
                                    }
                                    
                                }
                                else if (mainAsset is Material)
                                {
                                    if (type != BundleDataType.Multi_Material)
                                    {
                                        BaseLogSystem.Error("资源类型不符！！Material {0} {1}", type, mainAsset);
                                    }
                                }
                                else
                                {
                                    if (type != BundleDataType.Resource)
                                    {
                                        BaseLogSystem.Error("资源类型不符！！Resource {0} {1}", type, mainAsset);
                                    }
                                }
                            }
#endif
                            if (mainAsset != null)
                            {
                                if (mainAsset is GameObject)
                                {
                                    type = BundleDataType.Prefab;
                                }
                                else if (mainAsset is Material)
                                {
                                    //BaseLogSystem.internal_Error("资源类型不对！！！！{0} Material", mainAsset);
                                    type = BundleDataType.Multi_Material;
                                }
                                else
                                {
                                    type = BundleDataType.Resource;
                                }
                            }
                        }
                        else
                        {
                            mainAsset = null;
                            type = BundleDataType.Unknow;
                        }
                    }
                }
            }
            internal enum BundleDataType : byte
            {
                Unknow = 0,
                Resource = 1,
                //Multi_Resource  = 2,
                Prefab = 3,
                Multi_Prefab = 4,
                //Material        = 5,
                Multi_Material = 6,
            }

            private BundleDataType type = BundleDataType.Unknow;

            private AssetBundle m_Bundle = null;
            public UnityEngine.Object MainAsset
            {
                get
                {
                    return mainAsset;
                }
            }
            private UnityEngine.Object mainAsset = null;

            private Int32 m_nRefCount = 0;
            public Int32 RefCount
            {
                get
                {
                    return m_nRefCount;
                }
                set
                {
                    m_nRefCount = value;
                }
            }
            Dictionary<String, Int32> m_vResRefDic = new Dictionary<string, int>();

            Dictionary<String, UnityEngine.Object> m_vLoadedResObjDic = new Dictionary<string, UnityEngine.Object>();

            Dictionary<String, UnityEngine.Object> m_vRemoveResObjDic = new Dictionary<string, UnityEngine.Object>();
            public void CleanBundleData(bool CleanMulti_Prefab)
            {
                if (RefCount == 0)
                {
                    if (m_vRemoveResObjDic != null)
                    {
                        m_vRemoveResObjDic.Clear();
                    }
                    if (m_vLoadedResObjDic != null)
                    {
                        m_vLoadedResObjDic.Clear();
                    }
                    if (m_vResRefDic != null)
                    {
                        m_vResRefDic.Clear();
                    }
                    if (Bundle != null)
                    {
#if DEBUG
                        BaseLogSystem.Error(String.Format("1卸载资源Bundle:{0} {1}", BundleName, type));
#endif
                        Bundle.Unload(true);
                        Bundle = null;
                    }
                }
                else
                {
                    if (CleanMulti_Prefab && type == BundleDataType.Multi_Prefab)
                    {
                        if (m_vRemoveResObjDic != null)
                        {
                            m_vRemoveResObjDic.Clear();
                        }
                        if (m_vLoadedResObjDic != null)
                        {
                            m_vLoadedResObjDic.Clear();
                        }
                        if (m_vResRefDic != null)
                        {
                            m_vResRefDic.Clear();
                        }
                        if (Bundle != null)
                        {
#if DEBUG
                            BaseLogSystem.Error(String.Format("2卸载资源Bundle:{0} {1}", BundleName, type));
#endif
                           Bundle.Unload(true);
                           Bundle = null;
                        }
                        RefCount = 0;
                    }
                    else
                    {
                        if (m_vRemoveResObjDic != null)
                        {
                            if (m_vRemoveResObjDic.Count > 0)
                            {
                                Dictionary<String, UnityEngine.Object>.Enumerator iter = m_vRemoveResObjDic.GetEnumerator();
                                while (iter.MoveNext())
                                {
#if DEBUG
                                    BaseLogSystem.Error(String.Format("卸载资源:{0} {1} {2}", BundleName, iter.Current.Key, type));
#endif
                                    Resources.UnloadAsset(iter.Current.Value);
                                }
                                m_vRemoveResObjDic.Clear();
                            }
                        }
                    }
                }
            }
            protected virtual void IncrObject(String resName, UnityEngine.Object obj)
            {
#if DEBUG
                if (m_vLoadedResObjDic == null || m_vRemoveResObjDic == null)
                {
                    BaseLogSystem.Error("数据池不存在！！！");
                }
                if( String.IsNullOrEmpty(resName))
                {
                    BaseLogSystem.Error("数据资源名字为空！！！");
                }
                if( obj == null)
                {
                    BaseLogSystem.Error("数据资源为空！！！");
                }
                switch (type)
                {
                    //case BundleDataType.Multi_Resource:
                    case BundleDataType.Multi_Material:
                    case BundleDataType.Multi_Prefab:
                        break;
                    default:
                        BaseLogSystem.Error("资源类型不对！！！！！！！！！！");
                        break;
                }

#endif
                if (type == BundleDataType.Multi_Material)
                {
                    if (m_vRemoveResObjDic.ContainsKey(resName))
                    {
                        m_vRemoveResObjDic.Remove(resName);
                    }
                    if (m_vLoadedResObjDic.ContainsKey(resName) == false)
                    {
                        m_vLoadedResObjDic.Add(resName, obj);
                    }
                }
                else if (type == BundleDataType.Multi_Prefab)
                {
                    if (m_vLoadedResObjDic.ContainsKey(resName) == false)
                    {
                        m_vLoadedResObjDic.Add(resName, obj);
                    }
                }
                RemoveDataFromCache(this, resName, obj);

            }
            public virtual void IncrRef(String resName)
            {
                if (mainAsset != null)
                {
                    m_nRefCount++;
#if DEBUG
                    BaseLogSystem.Error(String.Format("加载资源:{0} {1} {2}", BundleName, resName, type));
#endif
                    RemoveDataFromCache(this, null, null);
                }
                else
                {
                    if (resName == null)
                    {
                        BaseLogSystem.internal_Error("引用资源名字为空！！ {0} {1}", BundleName, Bundle);
                        return;
                    }
                    UnityEngine.Object obj = Load(resName);
                    if (m_vResRefDic.ContainsKey(resName))
                    {
                        m_vResRefDic[resName] += 1;
                    }
                    else
                    {
                        m_vResRefDic.Add(resName, 1);
                    }

#if DEBUG
                    if (type == BundleDataType.Unknow)
                    {
                        BaseLogSystem.Error("资源没有类型！！！！{0} {1}", BundleName, Bundle);
                    }
#endif
                    IncrObject(resName, obj);
                    m_nRefCount++;
                }
            }
            protected virtual void DecrObject(String resName)
            {
#if DEBUG
                if (m_vLoadedResObjDic == null || m_vRemoveResObjDic == null)
                {
                    BaseLogSystem.Error("数据池不存在！！！");
                }
                if (String.IsNullOrEmpty(resName))
                {
                    BaseLogSystem.Error("数据资源名字为空！！！");
                }
                switch (type)
                {
                    //case BundleDataType.Multi_Resource:
                    case BundleDataType.Multi_Material:
                    case BundleDataType.Multi_Prefab:
                        break;
                    default:
                        BaseLogSystem.Error("资源类型不对！！！！！！！！！！");
                        break;
                }
#endif
                UnityEngine.Object obj = null;
                if (type == BundleDataType.Multi_Material)
                {
                    if (m_vLoadedResObjDic.ContainsKey(resName) == false)
                    {
                        BaseLogSystem.internal_Error("资源 {0} 引用关系不存在！！", resName);
                        return;
                    }
                    obj = m_vLoadedResObjDic[resName];
                    if (m_vRemoveResObjDic.ContainsKey(resName) == false)
                    {
                        m_vRemoveResObjDic.Add(resName, obj);
                    }
                    m_vLoadedResObjDic.Remove(resName);
                }
                else
                {
                    AddDataToCache(this, resName, obj);
                }
            }

            public void DestroyGameObject(UnityEngine.Object obj, String resName)
            {
                if (type == BundleDataType.Prefab || type == BundleDataType.Multi_Prefab)
                {
                    if (mainAsset != null)
                    {
                        if (mainAsset != obj)
                        {
                            GameObject.Destroy(obj);
                        }
                    }
                    else
                    {
                        if (m_vLoadedResObjDic != null)
                        {
#if DEBUG
                            if(String.IsNullOrEmpty(resName))
                            {
                                BaseLogSystem.Error("DestroyGameObject resName == null ！！！");
                            }
                            
#endif
                            if (m_vLoadedResObjDic.ContainsKey(resName))
                            {
                                UnityEngine.Object rObj = m_vLoadedResObjDic[resName];
                                if (rObj != obj)
                                {
                                    GameObject.Destroy(obj);
                                }
                            }
                        }
                    }
                }
            }
            public virtual void DecrRef(String resName)
            {
                if (m_nRefCount < 1)
                {
                    m_nRefCount = 0;
                    BaseLogSystem.internal_Error("引用计数出错！！！{0} {1} {2}", m_nRefCount, BundleName, Bundle);
                    return;
                }
                if (mainAsset != null)
                {
                    m_nRefCount--;
                    if (m_nRefCount == 0)
                    {
                        AddDataToCache(this, null, null);
                    }
                }
                else
                {
                    if (resName == null)
                    {
                        BaseLogSystem.internal_Error("引用资源名字为空！！ {0} {1}", BundleName, Bundle);
                        return;
                    }
                    if (m_vResRefDic.ContainsKey(resName))
                    {
                        if (m_vResRefDic[resName] == 1)
                        {
                            m_vResRefDic[resName] = 0;
                            DecrObject(resName);
                        }
                        else
                        {
                            m_vResRefDic[resName] -= 1;
                        }
                        m_nRefCount--;
                    }
                    else
                    {
                        BaseLogSystem.internal_Error("资源引用错误！！！{0} {1} {2}", resName, BundleName, Bundle);
                        return;
                    }
                }
            }
            public UnityEngine.Object Load(string name)
            {
                if (mainAsset != null)
                {
#if DEBUG
                    if( String.IsNullOrEmpty(name) == false)
                    {
                        BaseLogSystem.Error("资源名字不为空？？？？");
                    }
#endif
                    return mainAsset;
                }
                else
                {
#if DEBUG
                    if( mainAsset != null)
                    {
                        BaseLogSystem.Error("主资源存在？？？？");
                    }
#endif
                    if (Bundle == null)
                    {
                        BaseLogSystem.internal_Error("Bundle不存在@！！！！{0} {1} {2}", BundleName, name, mainAsset);
                        return null;
                    }
                    if (String.IsNullOrEmpty(name))
                    {
                        BaseLogSystem.internal_Error("name不存在@！！！！{0} {1} {2}", BundleName, name, mainAsset);
                        return null;
                    }
                    //UnityEngine.Object obj = Bundle.LoadAsset(name);// Bundle.Load(name);
                    UnityEngine.Object obj = null;
                    //临时注释掉GameObject 打一个Bundle临时判断GetAllAssetNames 会产生大量内存问题。
                    //这个判断只是为了过滤掉材质Bundle
                    if(Config!=null)
                    {
                        if (ResourceManager.IndependentConfig.Contains(Config.ID))
                        {
                            Bundle.LoadAllAssets();
                            mainAsset = Bundle.LoadAsset(name);
                            obj = mainAsset;
                            type = BundleDataType.Prefab;
                        }
                        else
                        {
                            obj = Bundle.LoadAsset(name);// Bundle.Load(name);
                        }
                    }
                    else
                    {
                        obj = Bundle.LoadAsset(name);// Bundle.Load(name);
                    }
                    
                    if (obj == null)
                    {
                        BaseLogSystem.internal_Error("{0} 资源不存在 {1} ！！！", BundleName, name);
                        return obj;
                    }
                    if (type == BundleDataType.Unknow)
                    {
#if DEBUG

                        if (m_vLoadedResObjDic != null || m_vRemoveResObjDic != null || m_vResRefDic !=null)
                        {
                            BaseLogSystem.Error("数据池存在？？？？");
                        }
#endif
                        //m_vLoadedResObjDic = new Dictionary<string, UnityEngine.Object>();

                        //m_vRemoveResObjDic = new Dictionary<string, UnityEngine.Object>();

                        //m_vResRefDic = new Dictionary<string, int>();

                        if (obj is GameObject)
                        {
                            type = BundleDataType.Multi_Prefab;
                        }
                        else if (obj is Material || obj is TextAsset)
                        {
                            type = BundleDataType.Multi_Material;
                        }
                        else
                        {
                            BaseLogSystem.internal_Error("资源类型错误！！！{0}", obj.GetType());
                            //type = BundleDataType.Multi_Resource;
                        }
                    }
#if DEBUG
                    else
                    {
                        BaseLogSystem.Error(String.Format("加载资源:{0} {1} {2}", BundleName, name, type));
                        switch (type)
                        {
                            case BundleDataType.Multi_Prefab:
                                {
                                    if (!(obj is GameObject))
                                    {
                                        BaseLogSystem.Error("资源类型不符！！！Multi_Prefab {0} {1}", obj, obj.GetType());
                                    }
                                }
                                break;
                            case BundleDataType.Multi_Material:
                                {
                                    if (!(obj is Material || obj is TextAsset))
                                    {
                                        BaseLogSystem.Error("资源类型不符！！！Multi_Material {0} {1}", obj, obj.GetType());
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
#endif
                    return obj;
                }
            }

            private bool Valid = true;
            public bool isValid()
            {
                return Valid;
            }
            public BundleData Valided()
            {
                this.Valid = true;
                return this;
            }

        }
        protected class TSRBundleData : BundleData
        {
            protected override void IncrObject(String resName, UnityEngine.Object obj)
            {

            }
            protected override void DecrObject(String resName)
            {

            }
            public override void IncrRef(String ResName)
            {
                RefCount = 1;

            }
            public override void DecrRef(String ResName)
            {
                RefCount = 1;
            }
        }
        protected bool NeedLoadBundle(String mainPath, String key, bool streamingAssets)
        {
            BundleData mainBundle = null;
            if (m_vBundleDataDic.ContainsKey(key))
            {
                mainBundle = m_vBundleDataDic[key];
                if (mainBundle.Bundle == null)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }
        //protected BundleData LoadBundle(BundleResConfig config,String mainPath, String key, String mainName)
        //LoadBundle(config, config.MainPathArray[i], config.MainKeyArray[i], config.MainNameArray[i]);
        protected BundleData LoadBundle(BundleResConfig config, Int32 index)
        {
            String mainPath = config.MainPathArray[index];
            String key = config.MainKeyArray[index];
            String mainName = config.MainNameArray[index];
            BundleData mainBundle = null;
            if(m_vBundleAsyncDataDic.ContainsKey(key))
            {
                ForceAsyncEnd();
            }
            if (m_vBundleDataDic.ContainsKey(key))
            {
                mainBundle = m_vBundleDataDic[key];
                if (mainBundle.Bundle != null)
                {
                    return mainBundle;
                }
            }

            AssetBundle bundle = LoadBundle(mainPath);
            if (mainBundle == null)
            {
                mainBundle = InstantiateBundleData(false);
                if (m_vBundleDataDic.ContainsKey(key) == false)
                {
                    m_vBundleDataDic.Add(key, mainBundle);
                }
                else
                {
                    BaseLogSystem.internal_Error("{0} Error!!!", key);
                }
            }
            mainBundle.Bundle = bundle;
            mainBundle.BundleName = key;
            mainBundle.Config = config;
            mainBundle.ConfigIndex = index;
            return mainBundle;
        }

        //        protected BundleData LoadBundleAsync(BundleResConfig config,String mainPath, String key, String mainName)
        protected BundleData LoadBundleAsync(BundleResConfig config, Int32 index)
        {
            String mainPath = config.MainPathArray[index];
            String key = config.MainKeyArray[index];
            String mainName = config.MainNameArray[index];
            BundleData mainBundle = null;
            if(m_vBundleAsyncDataDic.ContainsKey(key))
            {
                return m_vBundleAsyncDataDic[key];
            }
            if (m_vBundleDataDic.ContainsKey(key))
            {
                mainBundle = m_vBundleDataDic[key];
                if (mainBundle.Bundle != null)
                {
                    mainBundle.BundleRequest = null;
                    mainBundle.AssetRequest = null;
                    return mainBundle;
                }
                else if(mainBundle.BundleRequest != null)
                {
                    return mainBundle;
                }
            }
            AssetBundleCreateRequest bundleRequest = LoadBundleAsync(mainPath);
            if (bundleRequest == null)
                return null;

            if (mainBundle == null)
            {
                mainBundle = InstantiateBundleData(false);
            }
            mainBundle.Bundle = null;
            mainBundle.BundleName = key;
            mainBundle.Config = config;
            mainBundle.ConfigIndex = index;
            mainBundle.BundleRequest = bundleRequest;
            m_vBundleAsyncDataDic.Add(key, mainBundle);
            return mainBundle;
        }
        protected BundleData LoadBundlesForObjectAsync(BundleResConfig config, ObjectBundleData rData)
        {
            rData.ClearDepBundleData();
            if (config.MainPathArray == null || config.MainKeyArray == null)
            {
                return null;
            }
            for (int i = 1; i < config.MainPathArray.Length; ++i)
            {
                //BundleData bundle = LoadBundleAsync(config,config.MainPathArray[i], config.MainKeyArray[i], config.MainNameArray[i]);
                BundleData bundle = LoadBundleAsync(config, i);
                if (bundle == null || ((bundle.Bundle == null) && (bundle.BundleRequest == null)))
                {
                    BaseLogSystem.internal_Error("资源加载失败1 ！！{0} {1} {2}", config.MainPathArray[i], config.MainKeyArray[i], config.MainNameArray[i]);
                    return null;
                }
                else
                {
                    rData.AddDepBundleData(config.MainNameArray[i], bundle);
                }
            }
            //BundleData mainBundle = LoadBundleAsync(config,config.MainPathArray[0], config.MainKeyArray[0], config.MainNameArray[0]);
            BundleData mainBundle = LoadBundleAsync(config,0);
            rData.MainBundle = mainBundle;
            rData.ResName = config.MainNameArray[0];
            if (mainBundle == null || ((mainBundle.Bundle == null) && (mainBundle.BundleRequest == null)))
            {
                BaseLogSystem.internal_Error("资源加载失败2 ！！{0} {1} {2}", config.MainPathArray[0], config.MainKeyArray[0], config.MainNameArray[0]);
                return null;
            }
            else
            {
                return mainBundle;
            }
        }
        protected BundleData LoadBundlesForObject(BundleResConfig config, ObjectBundleData rData)
        {
            rData.ClearDepBundleData();
            if (config.MainPathArray == null || config.MainKeyArray == null)
            {
                return null;
            }
            for (int i = 1; i < config.MainPathArray.Length; ++i)
            {
                //BundleData bundle = LoadBundle(config,config.MainPathArray[i], config.MainKeyArray[i], config.MainNameArray[i]);
                BundleData bundle = LoadBundle(config,i);
                if (bundle == null || bundle.Bundle == null)
                {
                    BaseLogSystem.internal_Error("资源加载失败1 ！！{0} {1} {2}", config.MainPathArray[i], config.MainKeyArray[i], config.MainNameArray[i]);
                    return null;
                }
                else
                {
                    rData.AddDepBundleData(config.MainNameArray[i], bundle);
                }
            }
            //BundleData mainBundle = LoadBundle(config,config.MainPathArray[0], config.MainKeyArray[0], config.MainNameArray[0]);
            BundleData mainBundle = LoadBundle(config, 0);
            rData.MainBundle = mainBundle;
            rData.ResName = config.MainNameArray[0];
            if (mainBundle == null || mainBundle.Bundle == null)
            {
                BaseLogSystem.internal_Error("资源加载失败2 ！！{0} {1} {2}", config.MainPathArray[0], config.MainKeyArray[0], config.MainNameArray[0]);
                return null;
            }
            else
            {
                return mainBundle;
            }
        }

        public override UnityEngine.Object LoadResource(IResConfig rc)
        {
            UnityEngine.Profiling.Profiler.BeginSample(rc.MainName);
            UnityEngine.Object obj =  Internal_LoadResource(rc);
            UnityEngine.Profiling.Profiler.EndSample();
            return obj;
        }

        protected bool Internal_LoadResourceAsync(IResConfig rc,out ObjectBundleData rData)
        {
            BundleResConfig config = (BundleResConfig)rc;
            if (m_vKeyBundleDataDic.ContainsKey(config.Key))
            {
                rData = m_vKeyBundleDataDic[config.Key];
                if (LoadBundlesForObjectAsync(config, rData) == null)
                {
                    RecycleObjectBundleData(ref rData);
                    m_vKeyBundleDataDic.Remove(config.Key);
                    rData = null;
                    return false;
                }
            }
            else
            {
                rData = InstantiateObjectBundleData();
                rData.Key = config.Key;
                if (LoadBundlesForObjectAsync(config, rData) == null)
                {
                    RecycleObjectBundleData(ref rData);
                    rData = null;
                    return false;
                }
                m_vKeyBundleDataDic.Add(config.Key, rData);
            }
            return true;
        }
        protected UnityEngine.Object Internal_LoadResource(IResConfig rc)
        {
            BundleResConfig config = (BundleResConfig)rc;
            ObjectBundleData rData;
            if (m_vKeyBundleDataDic.ContainsKey(config.Key))
            {
                rData = m_vKeyBundleDataDic[config.Key];
                if (LoadBundlesForObject(config, rData) == null)
                {
                    RecycleObjectBundleData(ref rData);
                    m_vKeyBundleDataDic.Remove(config.Key);
                    return null;
                }
            }
            else
            {
                rData = InstantiateObjectBundleData();
                rData.Key = config.Key;
                if (LoadBundlesForObject(config, rData) == null)
                {
                    RecycleObjectBundleData(ref rData);
                    return null;
                }
                m_vKeyBundleDataDic.Add(config.Key, rData);
            }
            UnityEngine.Object obj = rData.Load(config.MainName);
            if (obj != null)
            {
                if (m_vObjBundleDataDic.ContainsKey(obj))
                {
                    m_vObjBundleDataDic[obj].IncrRef(config.MainName);
                }
                else
                {
                    rData.IncrRef(config.MainName);
                    m_vObjBundleDataDic.Add(obj, rData);
                }
                return obj;
            }
            else
            {
                RecycleObjectBundleData(ref rData);
                m_vKeyBundleDataDic.Remove(config.Key);
                return null;
            }
        }
        public override UnityEngine.GameObject InstantiateGameObject(IResConfig rc, bool isActive)
        {
            BundleResConfig config = (BundleResConfig)rc;
            UnityEngine.Object obj = Internal_LoadResource(rc);
            if (obj == null)
                return null;

            UnityEngine.GameObject gameObj = GameObject.Instantiate(obj) as GameObject;
            if (gameObj == null)
            {
                DestroyResource(obj);
                return null;
            }
            m_vObjBundleDataDic.Add(gameObj, m_vObjBundleDataDic[obj]);
            m_vKeyBundleDataDic[config.Key] = m_vObjBundleDataDic[obj];
            return gameObj;
        }

        public override String LoadTextResource(IResConfig rc)
        {
            BundleResConfig config = (BundleResConfig)rc;
            if (config.MainPathArray.Length != 1)
            {
                return null;
            }
            else
            {
                byte[] byteData = null;
                if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(config.MainPathArray[0]))
                {
                    byteData = ResourceLoader.m_AxpFileSystem.openFileByBuffer(config.MainPathArray[0],false);
                }
                else
                {
                    byteData = m_PersistentAssetsLoader.Load(config.MainPathArray[0]);
//                     if(byteData == null)
//                     {
//                         byteData = m_StreamingAssetsLoader.Load(config.MainPathArray[0]);
//                     }
                }
                if (byteData != null)
                {
                    MemoryStream mem = new MemoryStream(byteData);
                    StreamReader sr = new StreamReader(mem);
                    String data = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    mem.Close();
                    mem.Dispose();
                    if (data != null)
                    {
                        return new String(System.Text.Encoding.UTF8.GetChars(Convert.FromBase64String(data)));
                    }
                }
                return null;
            }
        }

        public override Byte[] LoadByteResource(IResConfig rc)
        {
            BundleResConfig config = (BundleResConfig)rc;
            if (config.MainPathArray.Length != 1)
            {
                return null;
            }
            else
            {
                if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(config.MainPathArray[0]))
                {
                    return ResourceLoader.m_AxpFileSystem.openFileByBuffer(config.MainPathArray[0],false);
                }
                else
                {
                    return m_PersistentAssetsLoader.Load(config.MainPathArray[0]);
                }
            }
        }
        public override Byte[] LoadFileInStreamingAssets(string fileName)
        {
            return m_StreamingAssetsLoader.Load(fileName);
        }

        public override void StopLoading(Int32 index)
        {
            if (m_ResouceDic.ContainsKey(index) && (m_StopResourceSet.Contains(index) == false))
            {
                m_StopResourceSet.Add(index);
            }
        }
        public override bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            SortedDictionary<UInt64, ResourceData> sortedDic = m_MultiPipeLoadResourceDic[(int)rPriority];
            foreach (KeyValuePair<UInt64, ResourceData> KValue in sortedDic)
            {
                if (KValue.Key == srcIndex)
                {
                    ResourceData rData = KValue.Value;
                    sortedDic.Remove(srcIndex);
                    sortedDic.Add(destIndex, rData);
                    return true;
                }
            }
            return false;
        }
        public override Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority)
        {
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            return index;
        }
        public override Int32 DontDestroyOnLoadResourceAsync(IResConfig rc, Int32 priority)
        {
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            return index;
        }
        public override Int32 InstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if (generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.index = index;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        public override Int32 LoadResourceAsync(IResConfig rc, Int32 priority)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig config = (BundleResConfig)rc;
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = config;
            rData.configArray = null;
            rData.resObjectArray = null;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        public override Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority)
        {
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            return index;
        }
        public override Int32 DontDestroyOnLoadResourceAsync(IResConfig[] rcArray, Int32 priority)
        {
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            return index;
        }
        public override Int32 InstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.index = index;
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        public override Int32 LoadResourceAsync(IResConfig[] rcArray, Int32 priority)
        {
            ResourcePriority rPriority = GetResourcePriority(priority);
            ulong generateIndex = GenerateIndex(rPriority);
            if( generateIndex == 0)
            {
                return -1;
            }
            BundleResConfig[] configArray = new BundleResConfig[rcArray.Length];
            for (int i = 0; i < rcArray.Length; ++i)
            {
                configArray[i] = (BundleResConfig)(rcArray[i]);
            }
            int index = ResourceManager.Singleton.GetCounter();
            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.config = null;
            rData.configArray = configArray;
            rData.resObjectArray = new UnityEngine.Object[rcArray.Length];
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.index = index;
            do
            {
                if (m_MultiPipeLoadResourceDic[(int)rPriority].ContainsKey(generateIndex))
                {
                    generateIndex = GenerateIndex(rPriority);
                }
                else
                {
                    break;
                }
            } while (true);
            m_MultiPipeLoadResourceDic[(int)rPriority].Add(generateIndex, rData);
            return index;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        public override void ConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj)
        {

        }
        public override void DisConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj)
        {

        }
        public override UnityEngine.GameObject Instantiate(UnityEngine.Object obj)
        {
            return null;
        }

        public override void DontDestroyOnLoad(UnityEngine.Object obj)
        {

        }
        public override void DestroyResource(System.Object obj)
        {
            if (obj == null)
                return;
            UnityEngine.Object uObj = obj as UnityEngine.Object;
            if (uObj != null)
            {
                if (m_vObjBundleDataDic.ContainsKey(uObj))
                {
                    ObjectBundleData rData = m_vObjBundleDataDic[uObj];
                    if (m_vObjBundleDataDic[uObj].RefCount == 1)
                    {
                        m_vObjBundleDataDic.Remove(uObj);
                    }
                    rData.DestroyGameObject(uObj, rData.ResName);
                    rData.DecrRef(rData.ResName);
                    return;
                }
//                 else
//                 {
//                     BaseLogSystem.internal_Info("资源引擎没找到！！{0}", uObj);
//                 }
                if (uObj is GameObject)
                {
                    GameObject.Destroy(uObj);
                }
            }
        }
        public override void DestroyResource(ref System.Object obj)
        {
            if (obj == null)
                return;
            DestroyResource(obj);
            obj = null;
        }
        public override void RecyclePrefab(UnityEngine.Object obj)
        {
            if (obj is UnityEngine.GameObject)
                GameObject.Destroy(obj);
            else
                Resources.UnloadAsset(obj);

        }
        public override void RecyclePrefab(ref UnityEngine.Object obj)
        {
            if (obj is UnityEngine.GameObject)
                GameObject.Destroy(obj);
            else
                Resources.UnloadAsset(obj);
            obj = null;

        }

        private Font m_DefaultFont = null;
        public override bool InitFont(string path, string key, bool defaultFont)
        {
            if (m_vBundleDataDic.ContainsKey(key))
            {
                BaseLogSystem.internal_Error("InitDefaultFont Error! {0}", path);
                return false;
            }
            AssetBundle ab = LoadBundle(path);
            if (ab != null)
            {
                TSRBundleData data = new TSRBundleData();
                data.Bundle = ab;
                data.BundleName = key;
                m_vBundleDataDic[key] = data;
                if(defaultFont)
                {
                    m_DefaultFont = data.MainAsset as Font;
                    if (m_DefaultFont == null)
                    {
                        BaseLogSystem.internal_Error("默认字体不存在！！！");
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public override Font GetDefaultFont()
        {
            return m_DefaultFont;
        }
        public override Font GetFont(string key)
        {
            if (m_vBundleDataDic.ContainsKey(key))
            {
                return m_vBundleDataDic[key].MainAsset as Font;
            }
            else
            {
                return null;
            }
        }
        public override bool InitTSRData(string path, string key, int type)
        {
            key = path;
            if (m_vBundleDataDic.ContainsKey(key))
            {
                BaseLogSystem.internal_Error("InitTSRData Error! {0}", path);
                return false;
            }
            
            AssetBundle ab = LoadBundle(path);
            if (ab != null)
            {
                BundleData data;
                if ((type&2) == 2)
                {
                    data = new BundleData();
                }
                else
                {
                    data = new TSRBundleData();
                }
                data.Bundle = ab;
                data.BundleName = key;
                m_vBundleDataDic[key] = data;
                if ((type&1) == 1)
                {
                    //ab.LoadAll();
                    ab.LoadAllAssets();
                    //Shader.WarmupAllShaders();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        //         internal AssetBundle LoadBundle(byte[] byteData)
        //         {
        //             return loadBundle(byteData);
        //         }
        public override AssetBundle LoadBundle(string path)
        {
            if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(path))
            {
                string t_axpFileName, t_axpFileFullName;
                AxpFilePath axpFilePath;
                int offset = ResourceLoader.m_AxpFileSystem.getFileOffset(path, out t_axpFileName, out t_axpFileFullName, out axpFilePath);
                if (offset < 0)
                {
                    BaseLogSystem.internal_Error("AxpFileSystem load assetbundle failed 1: {0}", path);
                    return null;
                }
                else
                {
                    //return AssetBundle.LoadFromMemory(ResourceLoader.m_AxpFileSystem.openFileByBuffer(path));
                    if (AxpFilePath.StreamingAssetsPath == axpFilePath)
                    {
                        if(TotalReservedMemory > ResourceManager.MemCleanSize)
                        {
                            if (ResourceLoader.m_AxpFileSystem.MemoryInfo(t_axpFileName) == 1024)
                            {
                                return null;
                            }
                            else
                            {
                                return AssetBundle.LoadFromFile(ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath + t_axpFileName, 0, (ulong)offset);
                            }
                        }
                        else
                        {
                            return AssetBundle.LoadFromFile(ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath + t_axpFileName, 0, (ulong)offset);
                        }
                    }
                    else
                    {
                        return AssetBundle.LoadFromFile(t_axpFileFullName, 0, (ulong)offset);
                    }
                }
            }
//             string tempPath = String.Format("{0}{1}", ResourceManager.PersistentAssetsPath, path);
//             if (File.Exists(tempPath))
//             {
//                 return AssetBundle.LoadFromFile(tempPath);
//             }
//             else
            {
                return AssetBundle.LoadFromFile(String.Format("{0}{1}", ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath, path));
            }
        }
        protected AssetBundleCreateRequest LoadBundleAsync(string path)
        {
            if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(path))
            {
                string t_axpFileName, t_axpFileFullName;
                AxpFilePath axpFilePath;
                int offset = ResourceLoader.m_AxpFileSystem.getFileOffset(path, out t_axpFileName, out t_axpFileFullName, out axpFilePath);
                if (offset < 0)
                {
                    BaseLogSystem.internal_Error("AxpFileSystem load assetbundle failed 1: {0}", path);
                    return null;
                }
                else
                {
                    //return AssetBundle.LoadFromMemory(ResourceLoader.m_AxpFileSystem.openFileByBuffer(path));
                    if (AxpFilePath.StreamingAssetsPath == axpFilePath)
                    {
                        if (TotalReservedMemory > ResourceManager.MemCleanSize)
                        {
                            if (ResourceLoader.m_AxpFileSystem.MemoryInfo(t_axpFileName) == 1024)
                            {
                                return null;
                            }
                            else
                            {
                                return AssetBundle.LoadFromFileAsync(ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath + t_axpFileName, 0, (ulong)offset);
                            }
                        }
                        else
                        {
                            return AssetBundle.LoadFromFileAsync(ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath + t_axpFileName, 0, (ulong)offset);
                        }
                    }
                    else
                    {
                        return AssetBundle.LoadFromFileAsync(t_axpFileFullName, 0, (ulong)offset);
                    }
                }
            }
            //             string tempPath = String.Format("{0}{1}", ResourceManager.PersistentAssetsPath, path);
            //             if (File.Exists(tempPath))
            //             {
            //                 return AssetBundle.LoadFromFile(tempPath);
            //             }
            //             else
            {
                return AssetBundle.LoadFromFileAsync(String.Format("{0}{1}", ResourceLoader.m_StreamingAssetsLoader.StreamingAssetsPath, path));
            }
        }
        //         public override AssetBundle LoadBundle(string path)
        //         {
        //             Byte[] byteData = null;
        //             if (ResourceLoader.m_AxpFileSystem != null && ResourceLoader.m_AxpFileSystem.Exists(path))
        //             {
        //                 byteData = ResourceLoader.m_AxpFileSystem.openFileByBuffer(path);
        //                 if (byteData != null)
        //                 {
        //                     return LoadBundle(byteData);
        //                 }
        //                 else
        //                 {
        //                     BaseLogSystem.Error("AxpFileSystem load assetbundle failed.: {0}", path);
        //                 }
        //             }
        //             string tempPath = ResourceManager.PersistentAssetsPath + path;
        //             byteData = ResourceLoader.m_PersistentAssetsLoader.Load(path);
        //             if (byteData != null)
        //             {
        //                 return LoadBundle(byteData);
        //             }
        //             else
        //             {
        //                 byteData = ResourceLoader.m_StreamingAssetsLoader.Load(path);
        //                 if (byteData != null)
        //                 {
        //                     return LoadBundle(byteData);
        //                 }
        //                 else
        //                 {
        //                     BaseLogSystem.Error("load assetbundle failed.: {0}", path);
        //                     return null;
        //                 }
        //             }
        //         }
        private ResourcePriority GetResourcePriority(int priority)
        {
//             int asyncLevel = ResourceManager.AsyncLevel;
//             if(asyncLevel >= priority)
//             {
//                 return (ResourcePriority)(asyncLevel);
//             }
            if (priority <= (int)(ResourcePriority.ResourcePriority_Min) || priority >= (int)(ResourcePriority.ResourcePriority_Max))
            {
                return ResourcePriority.ResourcePriority_Async;
            }
            else
            {
                if(m_MultiPipeGenerateIndexDic.ContainsKey(priority))
                {
                    return ResourcePriority.ResourcePriority_Async;
                }
                else
                {
                    return (ResourcePriority)(priority);
                }
//                 if (Enum.IsDefined(typeof(ResourcePriority), priority))
//                 {
//                     return (ResourcePriority)(priority);
//                 }
//                 else
//                 {
//                     return ResourcePriority.ResourcePriority_Async;
//                 }
            }
        }

        private UInt64 GenerateIndex(ResourcePriority priority)
        {
            int pty = (int)priority;
            if(m_MultiPipeGenerateIndexDic.ContainsKey(pty))
            {
                UInt64 index = m_MultiPipeGenerateIndexDic[pty];
                m_MultiPipeGenerateIndexDic[pty] = ++index;
                return index;
            }
            else
            {
                return 0;
            }
        }

        //所有队列的位置
        private Dictionary<int, ResourceData> m_ResouceDic = new Dictionary<int, ResourceData>();
        //PriorityIndex池
        private Dictionary<int, UInt64> m_MultiPipeGenerateIndexDic = new Dictionary<int, UInt64>();

        //需要加载的资源队列。。。
        private Dictionary<int, SortedDictionary<UInt64, ResourceData>> m_MultiPipeLoadResourceDic = new Dictionary<int, SortedDictionary<UInt64, ResourceData>>();

        //不销毁加载队列。。。
        private SortedDictionary<int, ResourceData> m_DontDestroyResouceDic = new SortedDictionary<int, ResourceData>();

        //正在加载的不销毁资源
        private ResourceData m_LoadingDontDestroyResouce = null;
        //实例化资源队列。。。
        //private Dictionary<int, List<ResourceData>> m_MultiPipeInstantiateResourceDic = new Dictionary<int, List<ResourceData>>();
        List<ResourceData> m_InstantiateResourceList = new List<ResourceData>();
        List<ResourceData> m_InstantiatePriorityResourceList = new List<ResourceData>();
        List<ResourceData> m_InstantiateSceneAsyncResourceList = new List<ResourceData>();
        //已经在加载资源队列。。。
        //private Dictionary<int, List<ResourceData>> m_MultiPipeLoadingResourceDic = new Dictionary<int, List<ResourceData>>();

        //停止加载的资源列表
        private HashSet<Int32> m_StopResourceSet = new HashSet<int>();

        //已经加载完的资源列表
        private List<ResourceData> m_LoadedResourceDataList = new List<ResourceData>();
        private List<ResourceData> m_LoadedPriorityResourceDataList = new List<ResourceData>();

        //垃圾内存池 
        private Queue<ResourceData> m_RecycleResourceDataQueue = new Queue<ResourceData>();

        private ResourceData InstantiateResourceData()
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
        private void RecycleResourceData(ResourceData rData)
        {
            if (rData == null)
                return;
            m_RecycleResourceDataQueue.Enqueue(rData.Release());
        }
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
        }
    }
}

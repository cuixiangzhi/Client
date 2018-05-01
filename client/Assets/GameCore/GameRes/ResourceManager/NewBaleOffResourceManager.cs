/********************************************************************************
 *	创建人：	    李彬
 *	创建时间：   2015-11-12
 *
 *	功能说明：  U3D资源载入入口，禁止程序自己使用底层接口调用。不在支持同步载入（给UI的同步以后会删除，部分配置文件支持同步）
 *	                增加多管道资源载入。
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading;
using System.IO;

namespace Games.TLBB.Manager
{
	partial class NewBaleOffResourceManager : GameCore.BaseManager<NewBaleOffResourceManager>
    {
        private UInt64 GenerateIndex(ResourcePriority priority)
        {
            UInt64 index = m_MultiPipeGenerateIndexDic[(int)priority];
            m_MultiPipeGenerateIndexDic[(int)priority] = ++index;
            return index;
        }
        #region ICommon 接口实现
        public virtual void Init()
        {
            int i;
            for (i = (int)ResourcePriority.ResourcePriority_SceneAsyn; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeLoadResourceDic[i] = new SortedDictionary<UInt64, ResourceData>();
                m_MultiPipeLoadingResourceDic[i] = new List<ResourceData>();
                m_MultiPipeInstantiateResourceDic[i] = new List<ResourceData>();
                m_MultiPipeGenerateIndexDic[i] = 0;
            }
            Release();
        }
        public bool GetResource(int index, out UnityEngine.Object obj)
        {
            if (m_LoadedResourceDataDic.ContainsKey(index))
            {
                obj = m_LoadedResourceDataDic[index].resObject;
                RecycleResourceData(m_ResouceDic[index]);
                m_LoadedResourceDataDic.Remove(index);
                return true;
            }
            else
            {
                obj = null;
                return false;
            }
        }
        public bool GetStreamResource(int index, out byte[] byteData)
        {
            if (m_LoadedResourceDataDic.ContainsKey(index))
            {
                byteData = m_LoadedResourceDataDic[index].resByte;
                RecycleResourceData(m_ResouceDic[index]);
                return true;
            }
            else
            {
                byteData = null;
                return false;
            }
        }
        protected bool RemoveLoadResource(Int32 index)
        {
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
        protected void TickDontDestroyOnLoad()
        {
            if (m_LoadingDontDestroyResouce != null && m_LoadingDontDestroyResouce.request != null)
            {
                if (m_LoadingDontDestroyResouce.request.isDone || (Time.realtimeSinceStartup - m_BegLoadingDontDestroyResouceTime > 0.5f))
                {
#if LOADLOG
                    float _BegLoadTime = Time.realtimeSinceStartup;
#endif
                    m_LoadingDontDestroyResouce.resObject = m_LoadingDontDestroyResouce.request.asset;
#if LOADLOG
                    float _EndLoadTime = Time.realtimeSinceStartup;
                    if (_EndLoadTime - _BegLoadTime > s_singleFileTime)
                    {
                        Debug.LogError(String.Format("LOADLOG: {0} Time :{1}", m_LoadingDontDestroyResouce.resObject, _EndLoadTime - _BegLoadTime));
                    }
#endif
                    if (m_LoadingDontDestroyResouce.resObject == null)
                    {
                        LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  resObject == null", m_LoadingDontDestroyResouce.resPath);
                    }
                    else
                    {
                        if (m_LoadingDontDestroyResouce.isInstantiate)
                        {
                            m_LoadingDontDestroyResouce.resObject = GameObject.Instantiate(m_LoadingDontDestroyResouce.resObject);
                            ((GameObject)m_LoadingDontDestroyResouce.resObject).SetActive(m_LoadingDontDestroyResouce.isActive);
                            GameObject.DontDestroyOnLoad(m_LoadingDontDestroyResouce.resObject);
                        }
                        ResourceManager.Singleton.PushDontDestroyOnLoad(m_LoadingDontDestroyResouce.index, m_LoadingDontDestroyResouce.resObject);
                    }
                    RecycleResourceData(m_LoadingDontDestroyResouce);
                    m_LoadingDontDestroyResouce = null;
                }
          
            }
            else
            {
                SortedDictionary<int, ResourceData>.Enumerator enu = m_DontDestroyResouceDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    m_LoadingDontDestroyResouce = enu.Current.Value;
                    m_DontDestroyResouceDic.Remove(enu.Current.Key);
                    m_LoadingDontDestroyResouce.request = Resources.LoadAsync(m_LoadingDontDestroyResouce.resPath);
                    if (m_LoadingDontDestroyResouce.request == null)
                    {
                        LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}  request == null", m_LoadingDontDestroyResouce.resPath);
                        RecycleResourceData(m_LoadingDontDestroyResouce);
                        m_LoadingDontDestroyResouce = null;
                    }
                    else
                    {
                        m_BegLoadingDontDestroyResouceTime = Time.realtimeSinceStartup;
                    }
                }
            }
//             SortedDictionary<int, ResourceData>.Enumerator enu = m_DontDestroyResouceDic.GetEnumerator();
//             if (enu.MoveNext())
//             {
//                 ResourceData rData = enu.Current.Value;
// #if LOADLOG
//                 float _BegLoadTime = Time.realtimeSinceStartup;
// #endif
//                 rData.resObject = Resources.Load(rData.resPath);
//                 if( rData.resObject == null)
//                 {
//                     LogSystem.Debug("TickDontDestroyOnLoad Failed!! resPath = {0}", rData.resPath);
//                 }
//                 else
//                 {
// #if LOADLOG
//                 float _EndLoadTime = Time.realtimeSinceStartup;
//                 if (_EndLoadTime - _BegLoadTime > s_singleFileTime)
//                 {
//                     Debug.LogError(String.Format("LOADLOG: {0} Time :{1}", rData.resObject, _EndLoadTime - _BegLoadTime));
//                 }
// #endif
//                     if( rData.isInstantiate)
//                     {
//                         rData.resObject = GameObject.Instantiate(rData.resObject);
//                         ((GameObject)rData.resObject).SetActive(rData.isActive);
//                         GameObject.DontDestroyOnLoad(rData.resObject);
//                     }
//                     ResourceManager.Singleton.PushDontDestroyOnLoad(rData.index, rData.resObject);
//                 }
//                 m_DontDestroyResouceDic.Remove(enu.Current.Key);
//                 RecycleResourceData(rData);
//             }
        }
        protected void TickLoadResource()
        {
            ResourceData rData = null;
            bool bAsynPause = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn].Count > ResourceManager.PipeLoadResourceCount;
            SortedDictionary<UInt64, ResourceData>.Enumerator enu;
            bool bRet = false;
            if (bAsynPause == false)
            {
                SortedDictionary<UInt64, ResourceData> sortedDic = m_MultiPipeLoadResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn];
                enu = sortedDic.GetEnumerator();
                if (enu.MoveNext())
                {
                    while (true)
                    {
                        rData = enu.Current.Value;
                        rData.request = Resources.LoadAsync(rData.resPath);
                        if (rData.request == null)
                        {
							GameCore.LogMgr.Log("Load Resource Failed!! resPath = {0}", rData.resPath);
                            RecycleResourceData(rData);
                        }
                        else
                        {
                            m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn].Add(rData);
                        }
                        sortedDic.Remove(enu.Current.Key);
                        enu = sortedDic.GetEnumerator();
                        bRet = enu.MoveNext();
                        if (m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn].Count > ResourceManager.PipeLoadResourceCount)
                            break;
                        if (bRet == false)
                            break;
                    }
                }
            }
            Dictionary<int, SortedDictionary<UInt64, ResourceData>>.Enumerator iter = m_MultiPipeLoadResourceDic.GetEnumerator();
            KeyValuePair<int, SortedDictionary<UInt64, ResourceData>> KValue;
            while(iter.MoveNext())
            {
                KValue = iter.Current;
                if (KValue.Key == (int)ResourcePriority.ResourcePriority_SceneAsyn)
                {
                    continue;
                }
                //队列资源过多加速载入
                if (KValue.Key == (int)ResourcePriority.ResourcePriority_Def && (KValue.Value.Count > 10))
                {
                    enu = KValue.Value.GetEnumerator();
                    while (enu.MoveNext())
                    {
                        rData = enu.Current.Value;
                        rData.request = Resources.LoadAsync(rData.resPath);
                        if (rData.request == null)
                        {
                            LogSystem.Debug("Load Resource Failed!! resPath = {0}", rData.resPath);
                            RecycleResourceData(rData);
                        }
                        else
                        {
                            m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                        }
                        KValue.Value.Remove(enu.Current.Key);
                        enu = KValue.Value.GetEnumerator();
                    }
                }
                else
                ///////////////
                {
                    bAsynPause = m_MultiPipeLoadingResourceDic[KValue.Key].Count > ResourceManager.PipeLoadResourceCount;
                    if (bAsynPause == false)
                    {
                        enu = KValue.Value.GetEnumerator();
                        if (enu.MoveNext())
                        {
                            while (true)
                            {
                                rData = enu.Current.Value;
                                rData.request = Resources.LoadAsync(rData.resPath);
                                if (rData.request == null)
                                {
                                    LogSystem.Debug("Load Resource Failed!! resPath = {0}", rData.resPath);
                                    RecycleResourceData(rData);
                                }
                                else
                                {
                                    m_MultiPipeLoadingResourceDic[KValue.Key].Add(rData);
                                }
                                KValue.Value.Remove(enu.Current.Key);
                                enu = KValue.Value.GetEnumerator();
                                bRet = enu.MoveNext();
                                if (m_MultiPipeLoadingResourceDic[KValue.Key].Count > ResourceManager.PipeLoadResourceCount)
                                    break;
                                if (bRet == false)
                                    break;
                            }
                        }
                    }
                }
            }
        }
        protected bool RemoveLoadingResource(Int32 index)
        {
            int i;
            List<ResourceData> Value = null;
            Dictionary<int, List<ResourceData>>.Enumerator iter = m_MultiPipeLoadingResourceDic.GetEnumerator();
            while(iter.MoveNext())
            {
                Value = iter.Current.Value;
                for (i = 0; i < Value.Count; ++i)
                {
                    if (Value[i].index == index)
                    {
                        Value[i].remove = true;
                        return true;
                    }
                }
            }
            return false;
        }
        protected bool TickLoadingResource()
        {
#if LOADLOG
                float _BegTickLoadTime = Time.realtimeSinceStartup;
#endif
            bool bRet = false;
            int i; ResourceData rData; List<ResourceData> dataList;
            #region ResourcePriority_Asyn
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn];
            for (i = dataList.Count - 1; i >= 0; )
            {
                rData = dataList[i];
                {
                    rData.resObject = rData.request.asset;
                    rData.request = null;
                    if (rData.remove)
                    {
                        rData.resObject = null;
                        RecycleResourceData(rData);
                    }
                    else
                    {
                        if (rData.resObject == null)
                        {
                            LogSystem.Error("Resource Load Failed!!{0}", rData.resPath);
                        }
                        else
                        {
                            if (rData.isInstantiate)
                                m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn].Add(rData);
                            else
                            {
                                m_LoadedResourceDataList.Add(rData);
                            }
                        }
                    }
                    dataList.RemoveAt(i);
                }
                break;
            }
            #endregion

            #region ResourcePriority_SynText
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_SynText];
            for (i = dataList.Count - 1; i >= 0; --i)
            {
                rData = dataList[i];
                rData.resObject = rData.request.asset;
                rData.request = null;
                if (rData.remove)
                {
                    rData.resObject = null;
                    RecycleResourceData(rData);
                }
                else
                {
                    if (rData.resObject == null)
                    {
                        LogSystem.Error("Resource Load Failed!!{0}", rData.resPath);
                    }
                    else
                    {
                        if ((rData.resObject is TextAsset) == false)
                        {
                            LogSystem.Error("ResourcePriority_SynText Can not Load Other Type!!! {0}", rData.resPath);
                            rData.resObject = null;
                            RecycleResourceData(rData);
                        }
                        else
                        {
                            if (rData.isInstantiate)
                            {
                                LogSystem.Error("ResourcePriority_Syn Can not Instantiate!!");
                                rData.resObject = null;
                                RecycleResourceData(rData);
                            }
                            else
                            {
                                m_ResourceLoadedObject[rData.resPath] = rData.resObject;
                                m_LoadedResourceDataList.Add(rData);
                                bRet = true;
                            }
                        }
                    }
                }
                dataList.RemoveAt(i);
            }
            #endregion

            #region ResourcePriority_Self
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_Self];
            while (dataList.Count > 0 && m_curFramePrioritySelfNum < ResourceManager.FramePrioritySelfMax)
            {
                i = 0;
                rData = dataList[i];
                rData.resObject = rData.request.asset;
                rData.request = null;
                if (rData.remove)
                {
                    rData.resObject = null;
                    RecycleResourceData(rData);
                }
                else
                {
                    if (rData.resObject == null)
                    {
                        LogSystem.Error("Resource Load Failed!!{0}", rData.resPath);
                    }
                    else
                    {
                        m_ResourceLoadedObject[rData.resPath] = rData.resObject;
                        if (rData.isInstantiate)
                            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Self].Add(rData);
                        else
                        {
                            m_LoadedResourceDataList.Add(rData);
                        }
                    }
                }
                dataList.RemoveAt(i);
                ++m_curFramePrioritySelfNum;
            }
            //取消载入限制
            //if (m_curFramePrioritySelfNum >= ResourceManager.FramePrioritySelfMax)
            //    return false;
            /////
            #endregion

            #region ResourcePriority_Pvw
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw];
            while (dataList.Count > 0)
            {
                i = 0;
                rData = dataList[i];
                rData.resObject = rData.request.asset;
                rData.request = null;
                if (rData.remove)
                {
                    rData.resObject = null;
                    RecycleResourceData(rData);
                }
                else
                {
                    if (rData.resObject == null)
                    {
                        LogSystem.Error("Resource Load Failed!!{0}", rData.resPath);
                    }
                    else
                    {
                        if (rData.isInstantiate)
                            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw].Add(rData);
                        else
                        {
                            m_LoadedResourceDataList.Add(rData);
                        }
                    }
                }
                dataList.RemoveAt(i);
            }
            #endregion

            #region ResourcePriority_Def
            dataList = m_MultiPipeLoadingResourceDic[(int)ResourcePriority.ResourcePriority_Def];
            //原来为IF，取消载入闲置改为while
            while (dataList.Count > 0)
            ///////
            {
                i = 0;
                rData = dataList[i];
                rData.resObject = rData.request.asset;
                rData.request = null;
                if (rData.remove)
                {
                    rData.resObject = null;
                    RecycleResourceData(rData);
                }
                else
                {
                    if (rData.resObject == null)
                    {
                        LogSystem.Error("Resource Load Failed!!{0}", rData.resPath);
                    }
                    else
                    {
                        if (rData.isInstantiate)
                            m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Def].Add(rData);
                        else
                        {
                            m_LoadedResourceDataList.Add(rData);
                        }
                    }
                }
                dataList.RemoveAt(i);
            }
            #endregion
#if LOADLOG
            float _EndTickLoadTime = Time.realtimeSinceStartup;
            if (_EndTickLoadTime - _BegTickLoadTime > s_TickFileTime)
            {
                Debug.LogError(String.Format("LOADLOG Time :{0}", _EndTickLoadTime - _BegTickLoadTime));
            }
#endif
            return bRet;
        }
        protected bool RemoveInstantiateResource(Int32 index)
        {
            int i; ResourceData rData;
            List<ResourceData> Value = null;
            Dictionary<int, List<ResourceData>>.Enumerator iter = m_MultiPipeInstantiateResourceDic.GetEnumerator();
            while( iter.MoveNext())
            {
                Value = iter.Current.Value;
                for (i = 0; i < Value.Count; ++i)
                {
                    rData = Value[i];
                    if (rData.index == index)
                    {
                        Value.RemoveAt(i);
                        RecycleResourceData(rData);
                        return true;
                    }
                }
            }
            return false;
        }
        int w = 0;
        protected void TickInstantiateResource()
        {
            List<ResourceData> dataList; ResourceData rData;
            #region ResourcePriority_Self
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Self];
            while (dataList.Count > 0)
            {
                rData = dataList[0];
                rData.resObject = GameObject.Instantiate(rData.resObject);
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
            #endregion

            #region ResourcePriority_Pvw
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_ScenePvw];
            while (dataList.Count > 0 && w < ResourceManager.FrameInstantiateGameObjectNum)
            {
                rData = dataList[0];
                rData.resObject = GameObject.Instantiate(rData.resObject);
                ++w;
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
            if (w >= ResourceManager.FrameInstantiateGameObjectNum)
                return;
            #endregion

            #region ResourcePriority_Def
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_Def];
           //取消载入闲置
            //while (dataList.Count > 0 && w < ResourceManager.FrameInstantiateGameObjectNum)
            while (dataList.Count > 0)
            {
                rData = dataList[0];
                rData.resObject = GameObject.Instantiate(rData.resObject);
                ++w;
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
            //取消载入闲置
            //if (w >= ResourceManager.FrameInstantiateGameObjectNum)
            //    return;
            ////////////
            #endregion

            #region ResourcePriority_Asyn
            dataList = m_MultiPipeInstantiateResourceDic[(int)ResourcePriority.ResourcePriority_SceneAsyn];
            //while (dataList.Count > 0 && w < ResourceManager.FrameInstantiateGameObjectNum)
            while (dataList.Count > 0)
            {
                rData = dataList[0];
                rData.resObject = GameObject.Instantiate(rData.resObject);
                //++w;
                m_LoadedResourceDataList.Add(rData);
                dataList.RemoveAt(0);
            }
            #endregion
            return;
        }
        protected bool RemoveLoadedResource(Int32 index)
        {
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

        protected void TickLoadedResource()
        {
            ResourceData rData = null;
            for (int i = m_LoadedResourceDataList.Count - 1; i >= 0; --i)
            {
                rData = m_LoadedResourceDataList[i];
                if (rData.Call == null)
                {
                    rData.loadedFrame = Time.frameCount;
                    m_LoadedResourceDataDic[rData.index] = rData;
                }
                else
                {
                    try
                    {
                        if (rData.isActive)
                        {
                            (rData.resObject as GameObject).SetActive(true);
                        }
                    }
                    catch (System.Exception e)
                    {
                        LogSystem.Error(e.ToString());
                    }
                    rData.Call(rData.index, rData.resObject, rData.resByte);
                    RecycleResourceData(rData);
                }
                m_LoadedResourceDataList.RemoveAt(i);
            }
        }
        protected void TickLoadedResource2()
        {
            Dictionary<int, ResourceData>.Enumerator iter = m_LoadedResourceDataDic.GetEnumerator();
            KeyValuePair<int, ResourceData> data;
            while(iter.MoveNext())
            {
                data = iter.Current;
                if (Time.frameCount - data.Value.loadedFrame > 300)
                {
                    m_LoadedResourceDataDic.Remove(data.Key);
                    LogSystem.Error("No access to resources!!{0}   {1}", data.Value.resObject, data.Value.resByte);
                    RecycleResourceData(data.Value);
                    return;
                }
            }
        }

        protected bool RemoveLoadedResource2(Int32 index)
        {
            KeyValuePair<int, ResourceData> data;
            Dictionary<int, ResourceData>.Enumerator iter = m_LoadedResourceDataDic.GetEnumerator();
            while(iter.MoveNext())
            {
                data = iter.Current;
                if (data.Key == index)
                {
                    m_LoadedResourceDataDic.Remove(data.Key);
                    RecycleResourceData(data.Value);
                    return true;
                }
            }
            return false;
        }
        protected void TickRemoveResource()
        {
            bool bRet;
            HashSet<Int32>.Enumerator iter = m_StopResourceSet.GetEnumerator();
            int index;
            while (iter.MoveNext())
            {
                index = iter.Current;
                bRet = ((((RemoveLoadedResource2(index) ? true : RemoveLoadedResource(index)) ?
                             true : RemoveInstantiateResource(index)) ?
                             true : RemoveLoadingResource(index)) ?
                             true : RemoveLoadResource(index));
                if (bRet == false)
                {
                    LogSystem.Error("RemoveResource {0} Not Find!!", index);
                }
            }
            m_StopResourceSet.Clear();
        }
        protected bool TickResource()
        {
            bool bRet = false;
            TickDontDestroyOnLoad();
            TickLoadResource();
            bRet |= TickLoadingResource();
            TickInstantiateResource();
            TickLoadedResource();
            TickLoadedResource2();
            return bRet;
        }
        public virtual void Tick(uint uDeltaTimeMS)
        {
            TickRemoveResource();
            m_curFramePrioritySelfNum = 0;
            w = 0;
            while (m_curFramePrioritySelfNum < ResourceManager.FramePrioritySelfMax)
            {
                if (TickResource() == false)
                {
                    return;
                }
            }
        }
        public virtual void Release()
        {
            //             foreach (KeyValuePair<string, UnityEngine.Object> data in m_ResourceLoadedObject)
            //                 if( data.Value!=null)
            //                     GameObject.Destroy(data.Value);
            m_ResourceLoadedObject.Clear();

            int i;
            for (i = (int)ResourcePriority.ResourcePriority_SceneAsyn; i < (int)ResourcePriority.ResourcePriority_Max; ++i)
            {
                m_MultiPipeGenerateIndexDic[i] = 0;
            }

            foreach (KeyValuePair<int, SortedDictionary<UInt64, ResourceData>> data in m_MultiPipeLoadResourceDic)
            {
                //                 foreach (ResourceData rData in data.Value)
                //                     if (rData.resObject != null)
                //                         GameObject.Destroy(rData.resObject);
                data.Value.Clear();
            }

            foreach (KeyValuePair<int, List<ResourceData>> data in m_MultiPipeLoadingResourceDic)
            {
                //                 foreach (ResourceData rData in data.Value)
                //                     if (rData.resObject != null)
                //                         GameObject.Destroy(rData.resObject);
                data.Value.Clear();
            }

            //             foreach (KeyValuePair<int, ResourceData> data in m_ResouceDic)
            //                 if (data.Value.resObject != null)
            //                     GameObject.Destroy(data.Value.resObject);
            m_ResouceDic.Clear();

            foreach (KeyValuePair<int, List<ResourceData>> data in m_MultiPipeInstantiateResourceDic)
            {
                //                 foreach (ResourceData rData in data.Value)
                //                     if (rData.resObject != null)
                //                         GameObject.Destroy(rData.resObject);
                data.Value.Clear();
            }

            m_StopResourceSet.Clear();
            //             foreach (ResourceData rData in m_LoadedResourceDataList)
            //                 if (rData.resObject != null)
            //                     GameObject.Destroy(rData.resObject);
            m_LoadedResourceDataList.Clear();

            //             foreach (KeyValuePair<int, ResourceData> data in m_LoadedResourceDataDic)
            //                 if (data.Value.resObject != null)
            //                     GameObject.Destroy(data.Value.resObject);
            m_LoadedResourceDataDic.Clear();
            m_RecycleResourceDataQueue.Clear();
        }
        public virtual void Destroy()
        {
        }
        #endregion
        private ResourcePriority GetResourcePriority(int priority)
        {
            if (priority <= (int)(ResourcePriority.ResourcePriority_Min) || priority >= (int)(ResourcePriority.ResourcePriority_Max))
            {
                return ResourcePriority.ResourcePriority_SceneAsyn;
            }
            else
            {
                return (ResourcePriority)(priority);
            }
        }
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
            int index = rData.index;
            if (m_ResouceDic.ContainsKey(index))
                m_ResouceDic.Remove(index);
            if (m_LoadedResourceDataDic.ContainsKey(index))
                m_LoadedResourceDataDic.Remove(index);

            if (rData.isValid())
            {
                rData.Release();
                m_RecycleResourceDataQueue.Enqueue(rData);
            }
            else
            {
                LogSystem.Error("ResourceData Error!!{0}  {1}  {2}", rData.resPath, rData.resObject, rData.resByte);
            }
        }
        private bool FindIndex(int index)
        {
            if (index < 0)
                return false;
            if (m_ResouceDic.ContainsKey(index))
                return true;
            else
                return false;
        }
        public bool RegisterEvent(int index, ResourceManager.DelegateResourceCallBack fun)
        {
            if (index < 0)
                return false;
            if (FindIndex(index) == false)
                return false;

            if (m_ResouceDic[index].Call != null)
            {
                LogSystem.Debug("Resource RegisterEvent index exist！");
            }
            else
            {
                m_ResouceDic[index].Call = fun;
            }
            return true;
        }

        public UnityEngine.Object LoadResource(ResConfig resConfig)
        {
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                return null;
            }
            if (String.Compare("Resources", resConfig.ResPath.Substring(0, index)) != 0)
            {
                return null;
            }
            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
                return null;
#if LOADLOG
                float _BegLoadTime = Time.realtimeSinceStartup;
#endif
                UnityEngine.Object  obj = Resources.Load(resPath);
#if LOADLOG
            if( obj != null)
            {
                float _EndLoadTime = Time.realtimeSinceStartup;
                if (_EndLoadTime - _BegLoadTime > s_singleFileTime)
                {
                    Debug.LogError(String.Format("LOADLOG: {0} Time :{1}", obj, _EndLoadTime - _BegLoadTime));
                }
            }
#endif
            return obj;
        }
        public UnityEngine.GameObject InstantiateGameObject(ResConfig resConfig, bool isActive = false)
        {
            UnityEngine.Object obj = LoadResource(resConfig);
            if (obj != null)
            {
                UnityEngine.GameObject gameObject = (UnityEngine.GameObject)GameObject.Instantiate(obj);
                if (gameObject != null)
                {
                    gameObject.SetActive(isActive);
                }
                return gameObject;
            }
            else
            {
                return null;
            }
        }
        public byte[] LoadByteResource(ResConfig resConfig)
        {
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                return null;
            }

            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, index)) != 0)
            {
                return null;
            }

            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
                return null;

            if (ResourceManager.platformType == PlatformType.PlatformIOS || ResourceManager.platformType == PlatformType.PlatformPC)
            {
                String strFullPath = Application.streamingAssetsPath + "/" + resPath;
                byte[] bydata = System.IO.File.ReadAllBytes(strFullPath);
                if (bydata == null || bydata.Length == 0)
                {
                    return null;
                }
                else
                {
                    return bydata;
                }

            }
            else if (ResourceManager.platformType == PlatformType.PlatformAndroid)
            {
                return ResourceManager.Singleton.GetLuaByte(resPath);
            }
            else
                return null;
        }
        public void StopLoadingResource(int index)
        {
            if (m_ResouceDic.ContainsKey(index) && (m_StopResourceSet.Contains(index) == false))
            {
                m_StopResourceSet.Add(index);
            }
        }
        public bool LoadResourceQueueReplace(int priority, UInt64 srcIndex, UInt64 destIndex)
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
        public void DontDestroyOnLoadResourceAsync(ResConfig resConfig, int ResIndex)
        {
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                LogSystem.Error("无效资源！{0}",resConfig.ResPath);
                return;
            }

            ResourceType resType = ResourceType.ResourceType_Invalid;
            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, index)) == 0)
            {
                resType = ResourceType.ResourceType_Stream;
            }
            else if (String.Compare("Resources", resConfig.ResPath.Substring(0, index)) == 0)
            {
                resType = ResourceType.ResourceType_Resource;
            }
            else
            {
                LogSystem.Error("无效资源！{0}", resConfig.ResPath);
                return ;
            }

            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
            {
                LogSystem.Error("无效资源！{0}", resConfig.ResPath);
                return;
            }

            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[ResIndex] = rData;
            rData.resPath = resPath;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.resType = resType;
            rData.index = ResIndex;
        }

        public int LoadResourceAsync(ResConfig resConfig, int priority = 0)
        {
            if (priority == 1)
                return -1;
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                return -1;
            }

            ResourceType resType = ResourceType.ResourceType_Invalid;
            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, index)) == 0)
            {
                LogSystem.Error("StreamingAssets NonSupport!!{0}", resConfig.ResPath);
                resType = ResourceType.ResourceType_Stream;
            }
            else if (String.Compare("Resources", resConfig.ResPath.Substring(0, index)) == 0)
            {
                resType = ResourceType.ResourceType_Resource;
            }
            else
            {
                return -1;
            }

            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
                return -1;

            index = ResourceManager.Singleton.GetCounter();
            if (index < 0)
                return -1;

            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            rData.resPath = resPath;
            rData.isInstantiate = false;
            rData.isActive = false;
            rData.resType = resType;
            rData.index = index;
            if (m_ResourceLoadedObject.ContainsKey(resPath))
            {
                rData.resObject = m_ResourceLoadedObject[resPath];
                rData.index = index;
                m_LoadedResourceDataList.Add(rData);
            }
            else
            {
                ResourcePriority rPriority = GetResourcePriority(priority);
                try
                {
                    if (resConfig.GenerateIndex > 0)
                        m_MultiPipeLoadResourceDic[(int)rPriority].Add(resConfig.GenerateIndex, rData);
                    else
                    {
                        ulong generateIndex = GenerateIndex(rPriority);
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
                    }
                }
                catch (System.Exception e)
                {
                    LogSystem.Error(e.ToString() + rData.resPath);
                    return -1;
                }
            }
            return index;
        }
        public void DontDestroyOnInstantiateGameObjectAsyn(ResConfig resConfig, bool isActive, int ResIndex)
        {
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                LogSystem.Error("无效资源！{0}",resConfig.ResPath);
                return;
            }

            ResourceType resType = ResourceType.ResourceType_Invalid;
            if (String.Compare("StreamingAssets", resConfig.ResPath.Substring(0, index)) == 0)
            {
                LogSystem.Error("StreamingAssets NonSupport!!{0}", resConfig.ResPath);
                resType = ResourceType.ResourceType_Stream;
            }
            else if (String.Compare("Resources", resConfig.ResPath.Substring(0, index)) == 0)
            {
                resType = ResourceType.ResourceType_Resource;
            }
            else
            {
                LogSystem.Error("无效资源！{0}", resConfig.ResPath);
                return ;
            }

            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
            {
                LogSystem.Error("无效资源！{0}", resConfig.ResPath);
                return;
            }

            ResourceData rData = InstantiateResourceData();
            m_DontDestroyResouceDic[ResIndex] = rData;
            rData.resPath = resPath;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.resType = resType;
            rData.index = ResIndex;
        }
        public int InstantiateGameObjectAsyn(ResConfig resConfig, bool isActive = false, int priority = 0)
        {
            if (priority == 1)
                return -1;
            int index = resConfig.ResPath.IndexOf("/", 0);
            if (index <= 0)
            {
                return -1;
            }

            if (String.Compare("Resources", resConfig.ResPath.Substring(0, index)) != 0)
            {
                return -1;
            }

            String resPath = resConfig.ResPath.Substring(index + 1);
            if (String.IsNullOrEmpty(resPath))
                return -1;

            index = ResourceManager.Singleton.GetCounter();
            if (index < 0)
                return -1;

            ResourceData rData = InstantiateResourceData();
            m_ResouceDic[index] = rData;
            ResourcePriority rPriority = GetResourcePriority(priority);
            rData.index = index;
            rData.resPath = resPath;
            rData.isInstantiate = true;
            rData.isActive = isActive;
            rData.resType = ResourceType.ResourceType_Resource;
            if (m_ResourceLoadedObject.ContainsKey(resPath))
            {
                rData.resObject = m_ResourceLoadedObject[resPath];
                m_MultiPipeInstantiateResourceDic[(int)rPriority].Add(rData);
            }
            else
            {
                try
                {
                    if (resConfig.GenerateIndex > 0)
                        m_MultiPipeLoadResourceDic[(int)rPriority].Add(resConfig.GenerateIndex, rData);
                    else
                        m_MultiPipeLoadResourceDic[(int)rPriority].Add(GenerateIndex(rPriority), rData);
                }
                catch (System.Exception e)
                {
                    LogSystem.Error(e.ToString() + rData.resPath);
                    return -1;
                }
            }
            return index;
        }
    }
}

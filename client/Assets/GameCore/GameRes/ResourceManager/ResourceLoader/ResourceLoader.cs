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

namespace Games.TLBB.Manager.IO
{
    abstract class ResourceLoader
    {
        public static StreamingAssetsLoader m_StreamingAssetsLoader = null;
        public static PersistentAssetsLoader m_PersistentAssetsLoader = null;

        public static IAxpSystem m_AxpFileSystem = null;
        static ResourceLoader()
        {
            if ((Application.isEditor == false) && (Application.platform == RuntimePlatform.Android))
            {
                //临时测试
//                 LoadSOPath = "/data/data/com.tencent.tmgp.tstl/lib/";
//                 if (Directory.Exists(LoadSOPath) == false)
//                 {
// #if DEBUG
//                     BaseLogSystem.Error(LoadSOPath + "  目录不存在！？？？？ResourceLoader");
// #endif
//                 }
                // m_StreamingAssetsLoader = new AndroidStreamingAssetsLoader();
            }
            else
            {
                m_StreamingAssetsLoader = new IOSStreamingAssetsLoader();
            }
            m_PersistentAssetsLoader = new PersistentAssetsLoader();
        }

        public static String LoadSOPath
        {
            get;
            set;
        }
        /// 只给Android的接口 DLL外不要调用
        public static byte[] AndroidLoadSO(string name)
        {
            String path = LoadSOPath + name;
            if (File.Exists(path))
                return File.ReadAllBytes(path);
            else
                return null;
        }
        public Games.TLBB.Manager.ResourceManager.DelegateResourceCallBack ResourceCallBack
        {
            get;
            set;
        }
        public Games.TLBB.Manager.ResourceManager.DelegateResourceArrayCallBack ResourceArrayCallBack
        {
            get;
            set;
        }
        public abstract Byte[] LoadByteBuffer(string mainPath,Int32 offset, Int32 length, bool lockFile);
        public abstract Byte[] LoadByteBuffer(string mainPath, bool lockFile);
        public abstract Byte[] LoadTextBuffer(string mainPath, bool decode);
        public abstract String LoadTextString(string mainPath, bool decode);
        public abstract void PrintBundleList(String strFileFullPath);
        public abstract void Init();
        public abstract void Release();
        public abstract void Tick(uint uDeltaTimeMS);
        public abstract UnityEngine.Object LoadResource(IResConfig rc);
        public abstract UnityEngine.GameObject InstantiateGameObject(IResConfig rc, bool isActive);

        public abstract Byte[] LoadByteResource(IResConfig rc);
        public abstract Byte[] LoadFileInStreamingAssets(string fileName);
        public abstract String LoadTextResource(IResConfig rc);
        public abstract void StopLoading(Int32 index);
        public abstract bool LoadingQueueReplace(Int32 priority, UInt64 srcIndex, UInt64 destIndex);
        public abstract Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority);
        public abstract Int32 DontDestroyOnLoadResourceAsync(IResConfig rc, Int32 priority);
        public abstract Int32 InstantiateGameObjectAsync(IResConfig rc, bool isActive, Int32 priority);
        public abstract Int32 LoadResourceAsync(IResConfig rc, Int32 priority);
        public abstract Int32 DontDestroyOnInstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority);
        public abstract Int32 DontDestroyOnLoadResourceAsync(IResConfig[] rcArray, Int32 priority);
        public abstract Int32 InstantiateGameObjectAsync(IResConfig[] rcArray, bool isActive, Int32 priority);
        public abstract Int32 LoadResourceAsync(IResConfig[] rcArray, Int32 priority);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public abstract void ConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj);
        public abstract void DisConnectObject(UnityEngine.Object parentObj, UnityEngine.Object obj);
        public abstract UnityEngine.GameObject Instantiate(UnityEngine.Object obj);
        public abstract void DontDestroyOnLoad(UnityEngine.Object obj);
        public abstract void DestroyResource(System.Object obj);
        public abstract void DestroyResource(ref System.Object obj);
        public abstract void RecyclePrefab(UnityEngine.Object obj);
        public abstract void RecyclePrefab(ref UnityEngine.Object obj);
        public abstract bool InitTSRData(string path, string key, int type);
        public abstract bool InitFont(string path, string key,bool defaultFont);
        public abstract Font GetFont(string key);
        public abstract Font GetDefaultFont();
        public virtual AssetBundle LoadBundle(string path)
        {
            return null;
        }
        public virtual void MemClean()
        {

        }

        public virtual void ForceMemClean()
        {

        }

        public virtual void ForceAsyncEnd()
        {

        }
    }

}

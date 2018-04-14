using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityObj = UnityEngine.Object;
using System.IO;

namespace GameCore
{
    internal class LoadInfo
    {
        internal string mPath;
        internal AssetBundle mBundle = null;
        internal AssetBundleRequest mRequest = null;
        internal Action<string, UnityObj> mCallBack = null;
        internal List<Action<string, UnityObj>> mCallBacks = new List<Action<string, UnityObj>>(4);

        internal void OnLoadFinish(AsyncOperation aop)
        {
            //建立缓存
            PoolInfo pool = ResMgr.AllocPoolInfo(mPath);
            pool.SetObj(mRequest.asset);
            //执行回调
            mCallBack(mPath, pool.GetObj());
            for(int i = 0;i < mCallBacks.Count;i++)
            {
                mCallBacks[i](mPath, pool.GetObj());
            }
            //清理数据
            mBundle.Unload(false);
            mRequest.completed -= OnLoadFinish;
            mRequest = null;
            mCallBack = null;
            mCallBacks.Clear();
            ResMgr.mLoadCacheDic.Remove(mPath);
            ResMgr.mLoadFree.Push(this);
        }
    }

    internal class PoolInfo
    {
        internal string mPath;

        internal bool IsActive()
        {
            return false;
        }

        internal UnityObj GetObj()
        {
            return null;
        }

        internal void SetObj(UnityObj obj)
        {
            
        }

        internal void ClearObj()
        {

        }
    }

    public static class ResMgr
    {
        internal static int DEFAULT_SIZE = 1024;
        internal static Dictionary<string, LoadInfo> mLoadCacheDic = new Dictionary<string, LoadInfo>(DEFAULT_SIZE);
        internal static Dictionary<string, PoolInfo> mPoolCacheDic = new Dictionary<string, PoolInfo>(DEFAULT_SIZE);
        internal static Stack<LoadInfo> mLoadFree = new Stack<LoadInfo>();
        internal static Stack<PoolInfo> mPoolFree = new Stack<PoolInfo>();
        internal static HashSet<string> mNullAssets = new HashSet<string>();
        internal static List<string> mTmpList = new List<string>(DEFAULT_SIZE);
        internal static byte[] mBuffer = new byte[1024 * 1024 * 2];
        internal static LuaByteBuffer mNullBuffer = new LuaByteBuffer(null, 0);
        internal static Transform mPoolLoad = null;
        internal static Transform mPoolInstance = null;

        public static void Init()
        {
            GameObject ROOT = new GameObject("GAME_POOL");
            mPoolLoad = new GameObject("LOAD").transform;
            mPoolLoad.transform.parent = ROOT.transform;
            mPoolInstance = new GameObject("INSTANCE").transform;
            mPoolInstance.transform.parent = ROOT.transform;
            UnityObj.DontDestroyOnLoad(ROOT);

            mLoadCacheDic.Clear();
            mPoolCacheDic.Clear();
            mLoadFree.Clear();
            mPoolFree.Clear();
            mNullAssets.Clear();
        }

        public static void Exit()
        {
            mPoolLoad = null;
            mPoolInstance = null;
            mLoadCacheDic.Clear();
            mPoolCacheDic.Clear();
            mLoadFree.Clear();
            mPoolFree.Clear();
            mNullAssets.Clear();
        }

        public static UnityObj LoadAsset(string path)
        {
            //空资源
            {
                if (mNullAssets.Contains(path))
                    return null;
            }
            //旧资源
            {
                PoolInfo pool = null;
                if (mPoolCacheDic.TryGetValue(path, out pool))
                {
                    UnityObj obj = pool.GetObj();
                    if (obj)
                    {
                        return obj;
                    }
                }
            }
            //新资源
            {
                AssetBundle bundle = null;
                LoadInfo load = null;
                if(mLoadCacheDic.TryGetValue(path, out load))
                {
                    bundle = load.mBundle;
                }
                else
                {
                    bundle = PkgMgr.LoadBundle(path);
                }
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (bundle == null || !bundle.Contains(fileName))
                {
                    PkgMgr.UnloadBundle(bundle);
                    mNullAssets.Add(path);
                    LogMgr.LogWarning("bundle asset is null {0}",path);
                    return null;
                }
                else
                {
                    UnityObj obj = bundle.LoadAsset(fileName);
                    PoolInfo pool = AllocPoolInfo(path);
                    pool.SetObj(obj);
                    return pool.GetObj();
                }
            }
        }

        public static void LoadAssetAsync(string path, Action<string, UnityObj> callBack)
        {
            //空资源
            {
                if (mNullAssets.Contains(path))
                { 
                    callBack(path, null);
                    return;
                }
            }
            //旧资源
            {
                PoolInfo pool = null;
                if (mPoolCacheDic.TryGetValue(path, out pool))
                {
                    UnityObj obj = pool.GetObj();
                    if (obj)
                    {
                        callBack(path,obj);
                        return;
                    }
                }
            }
            //新资源
            {
                LoadInfo load = null;
                if (mLoadCacheDic.TryGetValue(path, out load))
                {
                    load.mCallBacks.Add(callBack);
                }
                else
                {
                    AssetBundle bundle = PkgMgr.LoadBundle(path);
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    if (bundle == null || !bundle.Contains(fileName))
                    {
                        PkgMgr.UnloadBundle(bundle);
                        mNullAssets.Add(path);
                        LogMgr.LogWarning("bundle asset is null {0}", path);
                        callBack(path, null);
                        return;
                    }
                    else
                    {
                        load = AllocLoadInfo(path);
                        load.mCallBack = callBack;
                        load.mBundle = bundle;
                        load.mRequest = bundle.LoadAssetAsync(fileName);
                        load.mRequest.completed += load.OnLoadFinish;
                        return;
                    }
                }
            }
        }

        public static LuaByteBuffer LoadBytes(string path)
        {
            //空资源
            {
                if (mNullAssets.Contains(path))
                    return mNullBuffer;
            }
            //大资源
            int len = PkgMgr.LoadBytes(mBuffer);
            if (len >= mBuffer.Length)
            {
                LogMgr.LogWarning("2M buffer too small to read file {0}", path);
                return mNullBuffer;
            }
            //空资源
            if(len <= 0)
            { 
                LogMgr.LogWarning("bytes is null {0}",path);
                return mNullBuffer;
            }
            return new LuaByteBuffer(mBuffer, len);
        }

        public static void LoadBytesAsync(string path,Action<string, LuaByteBuffer> callBack)
        {

        }

        public static void LoadScene(string path,Action<string> callBack)
        {

        }

        public static void LoadSceneAsync(string path, Action<string> callBack)
        {

        }

        public static void UnloadAsset(UnityObj obj)
        {
            if(obj is GameObject)
            {

            }
            else
            {
                Resources.UnloadAsset(obj);
            }
        }

        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
            mTmpList.Clear();
            var emrPool = mPoolCacheDic.GetEnumerator();
            while(emrPool.MoveNext())
            {
                if(!emrPool.Current.Value.IsActive())
                {
                    emrPool.Current.Value.ClearObj();
                    mPoolFree.Push(emrPool.Current.Value);
                    mTmpList.Add(emrPool.Current.Key);
                }
            }
            for(int i = 0;i < mTmpList.Count;i++)
            {
                mPoolCacheDic.Remove(mTmpList[i]);
            }
            Resources.UnloadUnusedAssets();
        }

        internal static LoadInfo AllocLoadInfo(string path)
        {
            LoadInfo load = null;
            if (!mLoadCacheDic.TryGetValue(path, out load))
            {
                load = mLoadFree.Pop();
                if (load == null)
                {
                    load = new LoadInfo();
                }
            }
            load.mPath = path;
            mLoadCacheDic[path] = load;
            return load;
        }

        internal static PoolInfo AllocPoolInfo(string path)
        {
            PoolInfo pool = null;
            if(!mPoolCacheDic.TryGetValue(path,out pool))
            {
                pool = mPoolFree.Pop();
                if (pool == null)
                {
                    var emr = mPoolCacheDic.GetEnumerator();
                    while (emr.MoveNext())
                    {
                        if (!emr.Current.Value.IsActive())
                        {
                            pool = emr.Current.Value;
                            pool.ClearObj();
                            break;
                        }
                    }
                    if (pool != null)
                    {
                        mPoolCacheDic.Remove(pool.mPath);
                    }
                    else
                    {
                        pool = new PoolInfo();
                    }
                }
            }
            pool.mPath = path;
            mPoolCacheDic[path] = pool;
            return pool;
        }
    }
}

using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityObj = UnityEngine.Object;

namespace GameCore
{
    internal class LoadInfo
    {
        internal string mPath;
        internal AssetBundle mBundle = null;
        internal AsyncOperation mOption = null;
        internal Action<string, UnityObj> mCallBack = null;
        internal List<Action<string, UnityObj>> mCallBacks = new List<Action<string, UnityObj>>(4);

        public void OnLoadFinish(AsyncOperation aop)
        {
            aop.completed -= OnLoadFinish;
            mBundle.Unload(false);
            mOption = null;

            ResMgr.mLoadCacheDic.Remove(mPath);
            ResMgr.mLoadFree.Push(this);
            
        }
    }

    internal class PoolInfo
    {
        internal string mPath;

        internal bool HasObj()
        {
            return false;
        }

        internal UnityObj GetObj()
        {
            return null;
        }

        internal UnityObj SetObj(UnityObj obj)
        {
            return null;
        }
    }

    public static class ResMgr
    {
        internal static Dictionary<string, LoadInfo> mLoadCacheDic = new Dictionary<string, LoadInfo>();
        internal static Dictionary<string, PoolInfo> mPoolCacheDic = new Dictionary<string, PoolInfo>();
        internal static Stack<LoadInfo> mLoadFree = new Stack<LoadInfo>();
        internal static Stack<PoolInfo> mPoolFree = new Stack<PoolInfo>();
        internal static HashSet<string> mNullAssets = new HashSet<string>();
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
                if (bundle == null || !bundle.Contains(path))
                {
                    mNullAssets.Add(path);
                    LogMgr.LogWarning("bundle asset is null {0}",path);
                    return null;
                }
                else
                {
                    UnityObj obj = bundle.LoadAsset(path);
                    PoolInfo pool = AllocPoolInfo(path,obj);
                    return pool.GetObj();
                }
            }
        }

        public static void LoadAssetAsync(string path, Action<string, UnityObj> callBack)
        {

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

        public static void UnloadAsset()
        {

        }

        public static void UnloadUnusedAssets()
        {

        }

        internal static LoadInfo AllocLoadInfo(string path)
        {
            return null;
        }

        internal static PoolInfo AllocPoolInfo(string path,UnityObj obj)
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
                        if (!emr.Current.Value.HasObj())
                        {
                            pool = emr.Current.Value;
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
            pool.SetObj(obj);
            mPoolCacheDic[path] = pool;
            return pool;
        }
    }
}

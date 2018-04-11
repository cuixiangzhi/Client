using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityObj = UnityEngine.Object;

namespace GameCore
{
    class LoadInfo
    {
        public bool mLoadFlag = false;
        public AssetBundle mBundle = null;
        public AsyncOperation mOption = null;
        public Action<string, UnityObj> mCallBack = null;
        public List<Action<string, UnityObj>> mCallBacks = null;
    }

    class PoolInfo
    {
        public string mPath;

        public PoolInfo(string path)
        {
            mPath = path;
        }

        public bool HasObj()
        {
            return false;
        }

        public UnityObj GetObj()
        {
            return null;
        }

        public UnityObj SetObj(UnityObj obj)
        {
            return null;
        }
    }

    public static class ResMgr
    {
        private static Dictionary<string, LoadInfo> mLoadCacheDic = new Dictionary<string, LoadInfo>();
        private static Dictionary<string, PoolInfo> mPoolCacheDic = new Dictionary<string, PoolInfo>();
        private static Stack<LoadInfo> mLoadFree = new Stack<LoadInfo>();
        private static Stack<PoolInfo> mPoolFree = new Stack<PoolInfo>();
        private static HashSet<string> mNullAssets = new HashSet<string>();

        public static void Init()
        {
            mLoadCacheDic.Clear();
            mPoolCacheDic.Clear();
            mLoadFree.Clear();
            mPoolFree.Clear();
            mNullAssets.Clear();
        }

        public static void Exit()
        {
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
                if (bundle == null)
                {
                    mNullAssets.Add(path);
                    LogMgr.LogWarning("asset is null {0}",path);
                    return null;
                }
                else
                {
                    UnityObj obj = bundle.LoadAsset(path);
                    PoolInfo pool = AllocPoolInfo(path);
                    return pool.SetObj(obj);
                }
            }
        }

        public static void LoadAssetAsync(string path, Action<string, UnityObj> callBack)
        {
            
        }

        public static LuaByteBuffer LoadBytes(string path)
        {
            return new LuaByteBuffer();
        }

        public static void LoadBytesAsync(string path,Action<string, LuaByteBuffer> callBack)
        {

        }

        public static void LoadScene()
        {

        }

        public static void LoadSceneAdditive()
        {

        }

        public static void UnloadAsset()
        {

        }

        public static void UnloadUnusedAssets()
        {

        }

        private static LoadInfo AllocLoadInfo()
        {
            return null;
        }

        private static PoolInfo AllocPoolInfo(string path)
        {
            PoolInfo pool = null;
            if(mPoolCacheDic.TryGetValue(path,out pool))
            {
                return pool;
            }
            pool = mPoolFree.Pop();
            if(pool == null)
            {
                var emr = mPoolCacheDic.GetEnumerator();
                while(emr.MoveNext())
                {
                    if(!emr.Current.Value.HasObj())
                    {
                        pool = emr.Current.Value;
                        pool.mPath = path;
                        break;
                    }
                }
                if(pool != null)
                {
                    mPoolCacheDic.Remove(pool.mPath);
                }
                else
                {
                    pool = new PoolInfo(path);
                }
            }
            mPoolCacheDic[path] = pool;
            return pool;
        }
    }
}

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
        internal string mFileName;
        internal AssetBundle mBundle = null;
        internal AsyncOperation mRequest = null;
        internal Action<string, UnityObj> mCallBack = null;
        internal List<Action<string, UnityObj>> mCallBacks = new List<Action<string, UnityObj>>(4);

        internal void OnLoadFinish(AsyncOperation aop)
        {
            mRequest.completed -= OnLoadFinish;
            if(mBundle == null)
            {
                mCallBack(mFileName, null);
            }
            else
            {
                if (mBundle.isStreamedSceneAssetBundle)
                {
                    mCallBack(mFileName, null);
                    for (int i = 0; i < mCallBacks.Count; i++)
                    {
                        mCallBacks[i](mFileName, null);
                    }
                }
                else
                {
                    AssetBundleRequest req = mRequest as AssetBundleRequest;
                    PoolInfo pool = ResMgr.AllocPoolInfo(mFileName);
                    pool.SetObj(req.asset);
                    mCallBack(mFileName, pool.GetObj());
                    for (int i = 0; i < mCallBacks.Count; i++)
                    {
                        mCallBacks[i](mFileName, pool.GetObj());
                    }
                }
                //清理数据    
                PkgMgr.UnloadBundle(mFileName);
            }
            mBundle = null;
            mRequest = null;
            mCallBack = null;
            mCallBacks.Clear();
            ResMgr.mLoadDic.Remove(mFileName);
            ResMgr.mLoadFree.Push(this);
        }
    }

    internal class PoolInfo
    {
        internal string mFileName;
        internal WeakReference mWeakLoadObj = null;
        internal GameObject mLoadObj = null;
        internal List<UnityObj> mSpawnObjs = new List<UnityObj>(16);
        internal List<UnityObj> mDeSpawnObjs = new List<UnityObj>(16);
        internal float mLastUseTime = -1;
        internal float MAX_CACHE_TIME = 5;

        internal bool IsActive()
        {
            if(mLoadObj != null)
            {
                if(mSpawnObjs.Count <= 0)
                {
                    float passedTime = Time.time - mLastUseTime;
                    return passedTime >= MAX_CACHE_TIME;
                }
                return false;
            }
            else
            {
                return mWeakLoadObj.IsAlive;
            }
        }

        internal bool Contains(UnityObj obj)
        {
            if(mLoadObj != null)
            {
                return mSpawnObjs.Contains(obj);
            }
            else
            {
                return (mWeakLoadObj.Target as UnityObj) == obj;
            }
        }

        internal UnityObj GetObj()
        {
            if(mLoadObj != null)
            {
                GameObject obj = null;
                for(int i = 0;i < mDeSpawnObjs.Count;i++)
                {
                    if(mDeSpawnObjs[i] != null)
                    {
                        obj = mDeSpawnObjs[i] as GameObject;
                        mDeSpawnObjs.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        mDeSpawnObjs.RemoveAt(i--);
                    }
                }
                if(obj == null)
                {
                    obj = UnityObj.Instantiate<GameObject>(mLoadObj);
                }
                obj.transform.parent = ResMgr.mPoolInstance;
                mSpawnObjs.Add(obj);
                return obj;
            }
            else
            {
                return mWeakLoadObj.Target as UnityObj;
            }
        }

        internal void SetObj(UnityObj obj)
        {
            if(obj is GameObject)
            {
                mLoadObj = obj as GameObject;
                mLoadObj.transform.parent = ResMgr.mPoolLoad;
            }
            else
            {
                mWeakLoadObj = new WeakReference(obj);
            }
        }

        internal void ClearObj()
        {
            if(mLoadObj != null)
            {
                for(int i = 0;i < mDeSpawnObjs.Count;i++)
                {
                    if(mDeSpawnObjs[i] != null)
                    {
                        UnityObj.Destroy(mDeSpawnObjs[i]);
                    }
                }
                if(mSpawnObjs.Count != 0)
                {
                    LogMgr.LogError("pool has spawned object when clear!!");
                }
                mDeSpawnObjs.Clear();
                mSpawnObjs.Clear();
            }
        }

        internal void RecycleObj(UnityObj obj)
        {
            if(mLoadObj != null)
            {
                mSpawnObjs.Remove(obj);
                mDeSpawnObjs.Add(obj);
            }
            mLastUseTime = Time.time;
        }
    }

    public static class ResMgr
    {
        internal static int DEFAULT_SIZE = 1024;
        internal static int DEFAULT_FILE = 10;
        //加载缓存
        internal static Dictionary<string, LoadInfo> mLoadDic = new Dictionary<string, LoadInfo>(DEFAULT_SIZE);
        internal static Dictionary<string, PoolInfo> mPoolDic = new Dictionary<string, PoolInfo>(DEFAULT_SIZE);
        //缓存备用
        internal static Stack<LoadInfo> mLoadFree = new Stack<LoadInfo>();
        internal static Stack<PoolInfo> mPoolFree = new Stack<PoolInfo>();
        //加载文件
        internal static Queue<string> mByteFiles = new Queue<string>(DEFAULT_SIZE);
        internal static Queue<Action<string, LuaByteBuffer>> mByteCalls = new Queue<Action<string, LuaByteBuffer>>(DEFAULT_SIZE);
        //空的资源
        internal static HashSet<string> mNullAssets = new HashSet<string>();
        //临时数组
        internal static List<string> mTmpList = new List<string>(DEFAULT_SIZE);
        //数据BUFF
        internal static byte[] mBuffer = new byte[1024 * 1024 * 2];
        internal static LuaByteBuffer mNullBuffer = new LuaByteBuffer(null, 0);
        //资源结点
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
        }

        public static void Loop()
        {
            //异步文件
            {
                if (mByteFiles.Count > 0)
                {
                    int tmp = 0;
                    while (mByteFiles.Count > 0 && tmp < DEFAULT_FILE)
                    {
                        string path = mByteFiles.Dequeue();
                        Action<string, LuaByteBuffer> callBack = mByteCalls.Dequeue();
                        callBack(path, LoadBytes(path));
                        ++tmp;
                    }
                }
            }
        }

        public static void Exit()
        {
            mPoolLoad = null;
            mPoolInstance = null;

            mLoadDic.Clear();
            mPoolDic.Clear();

            mLoadFree.Clear();
            mPoolFree.Clear();

            mByteFiles.Clear();
            mByteCalls.Clear();

            mNullAssets.Clear();
        }

        public static UnityObj LoadAsset(string fileName)
        {
            //空资源
            {
                if (mNullAssets.Contains(fileName))
                    return null;
            }
            //旧资源
            {
                PoolInfo pool = null;
                if (mPoolDic.TryGetValue(fileName, out pool))
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
                if(mLoadDic.TryGetValue(fileName, out load))
                {
                    bundle = load.mBundle;
                }
                else
                {
                    bundle = PkgMgr.LoadBundle(fileName);
                }
                if (bundle == null || !bundle.Contains(fileName))
                {
                    PkgMgr.UnloadBundle(fileName);
                    mNullAssets.Add(fileName);
                    LogMgr.LogError("bundle asset is null {0} !!", fileName);
                    return null;
                }
                else
                {
                    UnityObj obj = bundle.LoadAsset(fileName);
                    PoolInfo pool = AllocPoolInfo(fileName);
                    pool.SetObj(obj);
                    PkgMgr.UnloadBundle(fileName);
                    return pool.GetObj();
                }
            }
        }

        public static void LoadAssetAsync(string fileName, Action<string, UnityObj> callBack)
        {
            //空资源
            {
                if (mNullAssets.Contains(fileName))
                { 
                    callBack(fileName, null);
                    return;
                }
            }
            //旧资源
            {
                PoolInfo pool = null;
                if (mPoolDic.TryGetValue(fileName, out pool))
                {
                    UnityObj obj = pool.GetObj();
                    if (obj)
                    {
                        callBack(fileName, obj);
                        return;
                    }
                }
            }
            //新资源
            {
                LoadInfo load = null;
                if (mLoadDic.TryGetValue(fileName, out load))
                {
                    load.mCallBacks.Add(callBack);
                }
                else
                {
                    AssetBundle bundle = PkgMgr.LoadBundle(fileName);
                    if (bundle == null || !bundle.Contains(fileName))
                    {
                        PkgMgr.UnloadBundle(fileName);
                        mNullAssets.Add(fileName);
                        LogMgr.LogError("bundle asset is null {0} !!", fileName);
                        callBack(fileName, null);
                        return;
                    }
                    else
                    {
                        load = AllocLoadInfo(fileName);
                        load.mCallBack = callBack;
                        load.mBundle = bundle;
                        load.mRequest = bundle.LoadAssetAsync(fileName);
                        load.mRequest.completed += load.OnLoadFinish;
                        return;
                    }
                }
            }
        }

        public static LuaByteBuffer LoadBytes(string fileName)
        {
            //空资源
            {
                if (mNullAssets.Contains(fileName))
                    return mNullBuffer;
            }
            //大资源
            int len = PkgMgr.LoadBytes(fileName, mBuffer);
            if (len >= mBuffer.Length)
            {
                LogMgr.LogError("2M buffer too small to read file {0} !!", fileName);
                return mNullBuffer;
            }
            //空资源
            if(len <= 0)
            { 
                LogMgr.LogError("bytes is null {0} !!", fileName);
                return mNullBuffer;
            }
            return new LuaByteBuffer(mBuffer, len);
        }

        public static void LoadBytesAsync(string fileName, Action<string, LuaByteBuffer> callBack)
        {
            //空资源
            {
                if (mNullAssets.Contains(fileName))
                { 
                    callBack(fileName, mNullBuffer);
                    return;
                }
            }
            //等待中
            {
                mByteFiles.Enqueue(fileName);
                mByteCalls.Enqueue(callBack);
            }
        }

        public static bool LoadScene(string fileName, bool additive = false)
        {
            Scene scene = SceneManager.GetSceneByName(fileName);
            if (scene.isLoaded)
            {
                LogMgr.LogError("scene %s alreay loaded,can't load !!", fileName);
                return true;
            }

            LoadInfo load = null;
            
            if (mLoadDic.TryGetValue(fileName, out load))
            {
                //卸载中&加载中
                if (load.mBundle != null)
                    LogMgr.LogError("scene is loading %s,can't load !!", fileName);
                else
                    LogMgr.LogError("scene is unloading %s,can't load !!", fileName);
                return false;
            }
            else
            {
                //新场景
                AssetBundle bundle = PkgMgr.LoadBundle(fileName);
                if(bundle != null || SceneUtility.GetBuildIndexByScenePath(fileName) >= 0)
                {
                    SceneManager.LoadScene(fileName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                    return true;
                }
                else
                {
                    LogMgr.LogError("scene is null %s !!", fileName);
                    return false;
                }              
            }
        }

        public static bool LoadSceneAsync(string fileName, Action<string, UnityObj> callBack, bool additive = false)
        {
            if(SceneManager.GetSceneByName(fileName).IsValid())
            {
                LogMgr.LogError("scene %s alreay loaded,can't load !!", fileName);
                callBack(fileName,null);
                return true;
            }
            LoadInfo load = null;
            if (mLoadDic.TryGetValue(fileName, out load))
            {
                //卸载中&加载中
                if (load.mBundle != null)
                    LogMgr.LogError("scene is loading %s,can't load !!", fileName);
                else
                    LogMgr.LogError("scene is unloading %s,can't load !!", fileName);
                return false;
            }
            else
            {
                //新场景
                AssetBundle bundle = PkgMgr.LoadBundle(fileName);
                if(bundle != null || SceneUtility.GetBuildIndexByScenePath(fileName) >= 0)
                {
                    load = AllocLoadInfo(fileName);
                    load.mBundle = bundle;
                    load.mRequest = SceneManager.LoadSceneAsync(fileName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                    load.mRequest.completed += load.OnLoadFinish;
                    load.mCallBack = callBack;
                    return true;
                }
                else
                {
                    LogMgr.LogError("scene is null %s !!", fileName);
                    return false;
                }
            }
        }

        public static void UnloadAsset(UnityObj obj, bool unloadAll)
        {
            PoolInfo pool = null;
            var emrPool = mPoolDic.GetEnumerator();
            while (emrPool.MoveNext())
            {
                if (emrPool.Current.Value.Contains(obj))
                {
                    pool = emrPool.Current.Value;
                    break;
                }
            }
            if(pool != null)
            {
                if (unloadAll)
                {
                    pool.ClearObj();
                    mPoolFree.Push(pool);
                    mPoolDic.Remove(pool.mFileName);
                }                
                else
                {
                    pool.RecycleObj(obj);
                }                 
            }
            else
            {
                if (obj != null)
                    UnityObj.Destroy(obj);
                LogMgr.LogError("can't find pool for object %s!!", obj == null ? "NULL" : obj.name);
            }
        }

        public static bool UnloadScene(string fileName, Action<string, UnityObj> callBack)
        {
            LoadInfo load = null;
            if(mLoadDic.TryGetValue(fileName, out load))
            {
                if (load.mBundle != null)
                    LogMgr.LogError("scene is loading %s,can't unload!!", fileName);
                else
                    LogMgr.LogError("scene is unloading %s,can't unload!!", fileName);
                return false;
            }
            else
            {
                if(SceneManager.GetSceneByName(fileName).IsValid())
                {
                    load = AllocLoadInfo(fileName);
                    load.mCallBack = callBack;
                    load.mBundle = null;
                    load.mRequest = SceneManager.UnloadSceneAsync(fileName);
                    load.mRequest.completed += load.OnLoadFinish;
                    return true;
                }
                else
                {
                    LogMgr.LogError("scene %s not loaded,can't unload!!", fileName);
                    return false;
                }
            }
        }

        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
            mTmpList.Clear();
            var emrPool = mPoolDic.GetEnumerator();
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
                mPoolDic.Remove(mTmpList[i]);
            }
            Resources.UnloadUnusedAssets();
        }

        internal static LoadInfo AllocLoadInfo(string fileName)
        {
            LoadInfo load = mLoadFree.Count > 0 ? mLoadFree.Pop() : null;
            if (load == null)
            {
                load = new LoadInfo();
            }
            load.mFileName = fileName;
            mLoadDic[fileName] = load;
            return load;
        }

        internal static PoolInfo AllocPoolInfo(string fileName)
        {
            PoolInfo pool = null;
            if(!mPoolDic.TryGetValue(fileName, out pool))
            {
                pool = mPoolFree.Count > 0 ? mPoolFree.Pop() : null;
                if (pool == null)
                {
                    var emr = mPoolDic.GetEnumerator();
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
                        mPoolDic.Remove(pool.mFileName);
                    }
                    else
                    {
                        pool = new PoolInfo();
                    }
                }
            }
            pool.mFileName = fileName;
            mPoolDic[fileName] = pool;
            return pool;
        }
    }
}

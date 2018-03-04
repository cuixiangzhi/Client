using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;
using UnityObj = UnityEngine.Object;
using SceneMgr = UnityEngine.SceneManagement.SceneManager;

namespace GameCore
{
    public static class AssetMgr
    {
        private class ObjectPool
        {
            public UnityObj mLoadedObj = null;
            public List<UnityObj> mUsingObj = null;
            public List<UnityObj> mUnUsingObj = null;
            public float mLastUseTime = 0;

            public static ObjectPool Create(int hash,UnityObj obj)
            {
                if (obj == null)
                    return null;
                ObjectPool pool = null;
                for (int i = 0; i < mPool.Count; i++)
                {
                    if (mPool[i].mLoadedObj == null)
                    {
                        pool = mPool[i];
                        pool.Init(obj);
                        break;
                    }
                }
                if (pool == null) pool = new ObjectPool(hash, obj);
                return pool;
            }

            public ObjectPool(int hash,UnityObj obj)
            {
                mUsingObj = new List<UnityObj>(CALL_BACK_SIZE);
                mUnUsingObj = new List<UnityObj>(CALL_BACK_SIZE);
                mPool.Add(this);
                mPoolIndex[hash] = mPool.Count - 1;
                Init(obj);
            }

            public void Init(UnityObj obj)
            {
                if (obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    go.SetActive(false);
                    go.transform.parent = mPoolParent;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                }
                mLoadedObj = obj;
            }

            public UnityObj CreateObj()
            {
                UnityObj obj = null;
                mLastUseTime = Time.time;
                if(mUnUsingObj.Count != 0)
                {
                    obj = mUnUsingObj[mUnUsingObj.Count - 1];
                    mUsingObj.Add(obj);
                    mUnUsingObj.RemoveAt(mUnUsingObj.Count - 1);
                    return obj;
                }
                if(obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    go = UnityObj.Instantiate<GameObject>(go, mPoolParent, false);
                    obj = go;
                }
                else
                {
                    obj = mLoadedObj;
                }
                mUsingObj.Add(obj);
                return obj;
            }

            public void DestroyObj(UnityObj obj)
            {
                mUsingObj.Remove(obj);
                mUnUsingObj.Add(obj);
                mLastUseTime = Time.unscaledTime;
            }
        }
        private class LoadCallBack
        {
            public bool sync = true;
            public List<Action<UnityObj>> callBacks = null;

            public static void CreateCallBack(string path,int hash, Action<UnityObj> callBack,bool sync)
            {
                int idx = -1;
                if (mCallBackIndex.ContainsKey(hash))
                {
                    idx = mCallBackIndex[hash];
                    mCallBack[idx].callBacks.Add(callBack);
                    mCallBack[idx].sync = mCallBack[idx].sync && sync;
                }
                else
                {
                    //回收旧的
                    for (int i = 0; i < mCallBack.Count; i++)
                    {
                        if (mCallBack[i].callBacks.Count == 0)
                        {
                            mCallBack[idx].sync = sync;
                            mCallBack[i].callBacks.Add(callBack);
                            mCallBackIndex[hash] = i;
                            return;
                        }
                    }
                    //新建回调
                    {
                        LoadCallBack lc = new LoadCallBack();
                        lc.sync = sync;
                        lc.callBacks = new List<Action<UnityObj>>(CALL_BACK_SIZE);
                        mCallBack.Add(lc);
                        mCallBackIndex[hash] = mCallBack.Count - 1;
                    }
                }
            }
        }

        //资源对象池
        private static List<ObjectPool> mPool = null;
        private static Dictionary<int, int> mPoolIndex = null;
        //资源加载回调
        private static List<LoadCallBack> mCallBack = null;
        private static Dictionary<int, int> mCallBackIndex = null;
        //异步加载
        private static List<string> mAsyncPath = null;
        private static List<AsyncOperation> mAsyncOp = null;
        private static List<AssetBundle> mAsyncBundle = null;
        private static HashSet<int> mNullAssets = null;
        //池子大小,缓存时间
        private static int CALL_BACK_SIZE = 16;
        private static int POOL_SIZE = 1024;
        private static int CACHE_DURATION = 300;
        private static Transform mPoolParent;

        public static void Init()
        {
            mPoolParent = new GameObject("GAME_POOL").transform;
            mPoolParent.position = new Vector3(-1000,-1000,-1000);
            UnityObj.DontDestroyOnLoad(mPoolParent);
            mPool = new List<ObjectPool>(POOL_SIZE);
            mPoolIndex = new Dictionary<int, int>(POOL_SIZE);
            mCallBack = new List<LoadCallBack>(CALL_BACK_SIZE);
            mCallBackIndex = new Dictionary<int, int>(CALL_BACK_SIZE);
            mAsyncPath = new List<string>(CALL_BACK_SIZE);
            mAsyncOp = new List<AsyncOperation>(CALL_BACK_SIZE);
            mAsyncBundle = new List<AssetBundle>(CALL_BACK_SIZE);
            mNullAssets = new HashSet<int>();
        }

        public static void LateLoop()
        {
            LoadAsset();
            CollectAsset();
        }

        public static void Exit()
        {
            mPoolParent = null;
            mPool = null;
            mPoolIndex = null;
            mCallBack = null;
            mCallBackIndex = null;
            mAsyncPath = null;
            mAsyncOp = null;
            mAsyncBundle = null;
            mNullAssets = null;
        }

		public static void SetAssetCacheTime(int time)
		{
			CACHE_DURATION = time;
		}

        public static void LoadAsset(string path, Action<UnityObj> callBack, bool sync)
        {
            int hash = path.GetHashCode();
            if(CheckNull(hash))
            {
                callBack(null);
                return;
            }
            int idx = -1;
            //池子存在,直接创建一个返回
            if (mPoolIndex.ContainsKey(hash))
            {
                idx = mPoolIndex[hash];               
                callBack(mPool[idx].CreateObj());
            }
            else
            {
                //添加回调
                LoadCallBack.CreateCallBack(path,hash,callBack,sync);
                BundleMgr.LoadBundle(path, OnBundleLoad);
            }
        }

        public static LuaByteBuffer LoadAsset(string path)
        {
            return BundleMgr.LoadBytes(path);
        }

        public static void DestroyAsset(UnityObj obj)
        {
            //回收LUA脚本不再引用的资源或者C#脚本不再引用的资源
            for(int i = 0;i < mPool.Count;i++)
            {
                if(mPool[i].mUsingObj.Contains(obj))
                {
                    mPool[i].DestroyObj(obj);
                    break;
                }
            }
        }

        private static void CollectAsset()
        {
            for(int i = 0;i < mPool.Count;i++)
            {
                //最后一次使用的时间到现在超过缓存时长,就清理掉
                if(mPool[i].mUsingObj.Count == 0 && mPool[i].mLastUseTime + CACHE_DURATION < Time.time)
                {
                    for(int j = 0;j < mPool[i].mUnUsingObj.Count;j++)
                    {
                        Resources.UnloadAsset(mPool[i].mUnUsingObj[j]);
                    }
                    mPool[i].mUnUsingObj.Clear();
                    Resources.UnloadAsset(mPool[i].mLoadedObj);
                    mPool[i].mLoadedObj = null;
                }
            }
        }

        private static void LoadAsset()
        {
            for(int i = 0;i < mAsyncOp.Count;i++)
            {
                if(mAsyncOp[i].isDone)
                {
                    string path = mAsyncPath[i];
                    AssetBundleRequest req = mAsyncOp[i] as AssetBundleRequest;
                    AssetBundle bundle = mAsyncBundle[i];
                    bundle.Unload(false);
                    mAsyncOp.RemoveAt(i);
                    mAsyncPath.RemoveAt(i);
                    mAsyncBundle.RemoveAt(i);
                    --i;
                    OnObjectLoad(path, req == null ? null : req.asset);
                }
            }
        }

        private static bool CheckNull(int hash)
        {
            return mNullAssets.Contains(hash);
        }

        private static void OnBundleLoad(string path,AssetBundle ab)
        {
            if(ab == null)
            {
                OnObjectLoad(path, null);
            }
            else
            {
                bool sync = false;
                int hash = path.GetHashCode();
                if (mCallBackIndex.ContainsKey(hash))
                {
                    int idx = mCallBackIndex[hash];
                    sync = mCallBack[idx].sync;
                }
                if (sync)
                {
                    UnityObj obj = ab.LoadAsset<UnityObj>(UtilDll.common_md5(path));
                    ab.Unload(false);
                    OnObjectLoad(path, obj);
                }
                else
                {
                    AsyncOperation aop = null;
                    if (ab.isStreamedSceneAssetBundle)
                    {
                        aop = SceneMgr.LoadSceneAsync(UtilDll.common_md5(path));
                    }
                    else
                    {
                        aop = ab.LoadAssetAsync<UnityObj>(UtilDll.common_md5(path));
                    }
                    mAsyncBundle.Add(ab);
                    mAsyncOp.Add(aop);
                    mAsyncPath.Add(path);
                }
            }
        }

        private static void OnObjectLoad(string path,UnityObj obj)
        {
            int hash = path.GetHashCode();
            if(obj == null)
            {
                mNullAssets.Add(hash);
            }
            //缓存资源
            ObjectPool pool = ObjectPool.Create(hash,obj);
            //执行回调
            if (mCallBackIndex.ContainsKey(hash))
            {
                int idx = mCallBackIndex[hash];
                LoadCallBack lc = mCallBack[idx];
                for(int i = 0;i < lc.callBacks.Count;i++)
                {
                    lc.callBacks[i](pool != null ? pool.CreateObj() : null);
                }
                //清空回调
                lc.callBacks.Clear();
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;
using UnityObj = UnityEngine.Object;
using SceneMgr = UnityEngine.SceneManagement.SceneManager;

namespace GameFrameWork
{
    public static class AssetManager
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
                //原始资源加工
                if (obj is AudioClip)
                {
                    GameObject go = new GameObject(obj.name);
                    UnityObj.DontDestroyOnLoad(go);
                    go.transform.parent = mPoolParent;
                    AudioSource source = go.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.clip = obj as AudioClip;

                    obj = go.AddComponent<BehaviourAudio>();
                }
                else if (obj is GameObject)
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
                mLastUseTime = Time.unscaledTime;
                if(mUnUsingObj.Count != 0)
                {
                    obj = mUnUsingObj[mUnUsingObj.Count - 1];
                    mUsingObj.Add(obj);
                    mUnUsingObj.RemoveAt(mUnUsingObj.Count - 1);
                    return obj;
                }
                if(mLoadedObj is Texture || mLoadedObj is Animator || mLoadedObj is Material)
                {
                    obj = mLoadedObj;                   
                }
                else if(obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    //TODO:处理不需要实例化的资源
                    if(go.GetComponent<UIAtlas>() != null)
                    {
                        obj = go;
                    }
                    else
                    {
                        go = UnityObj.Instantiate<GameObject>(go, mPoolParent, false);
                        obj = go;
                    }
                }
                else
                {
                    throw new Exception("unkown object type " + mLoadedObj.GetType().FullName);
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
            public List<int> funcID = null;
            public Action<int, UnityObj> callBack = null;

            public static void CreateCallBack(string path,int hash, int funcID, Action<int, UnityObj> callBack,bool sync)
            {
                int idx = -1;
                if (mCallBackIndex.ContainsKey(hash))
                {
                    idx = mCallBackIndex[hash];
                    mCallBack[idx].funcID.Add(funcID);
                    mCallBack[idx].sync = mCallBack[idx].sync && sync;
                    if (mCallBack[idx].callBack != callBack)
                        throw new Exception("callback not equal with same asset " + path);
                }
                else
                {
                    //回收旧的
                    for (int i = 0; i < mCallBack.Count; i++)
                    {
                        if (mCallBack[i].funcID.Count == 0)
                        {
                            mCallBack[idx].sync = sync;
                            mCallBack[i].callBack = callBack;
                            mCallBack[i].funcID.Add(funcID);
                            mCallBackIndex[hash] = i;
                            return;
                        }
                    }
                    //新建回调
                    {
                        LoadCallBack lc = new LoadCallBack();
                        lc.callBack = callBack;
                        lc.sync = sync;
                        lc.funcID = new List<int>(CALL_BACK_SIZE);
                        lc.funcID.Add(funcID);
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
        //数据加载回调
        private static Action<int, LuaByteBuffer> mBytesCallBack = null;
        private static Dictionary<int, int> mBytesCallBackFuncs = null;
        //异步加载
        private static List<string> mAsyncPath = null;
        private static List<AsyncOperation> mAsyncOp = null;
        private static List<AssetBundle> mAsyncBundle = null;
        private static HashSet<int> mNullAssets = null;
        //池子大小,缓存时间
        private static int CALL_BACK_SIZE = 16;
        private static int POOL_SIZE = 1024;
        private static int CACHE_DURATION = 900;
        private static Transform mPoolParent;
        private static LuaByteBuffer mNullBuffer;

        public static void Init()
        {
            mPoolParent = new GameObject("ASSET_POOL").transform;
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
            mNullBuffer = new LuaByteBuffer(null,0);
            BundleManager.Init();
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
            BundleManager.Exit();
        }

        public static void LoadAsset(string path, int funcID, Action<int, UnityObj> callBack, bool sync)
        {
            int hash = path.GetHashCode();
            if(CheckNull(hash))
            {
                callBack(funcID, null);
                return;
            }
            int idx = -1;
            //池子存在,直接创建一个返回
            if (mPoolIndex.ContainsKey(hash))
            {
                idx = mPoolIndex[hash];               
                callBack(funcID, mPool[idx].CreateObj());
            }
            else
            {
                //添加回调
                LoadCallBack.CreateCallBack(path,hash,funcID,callBack,sync);
                BundleManager.LoadBundle(path, OnBundleLoad);
            }
        }

        public static void LoadAsset(string path, int funcID, Action<int, LuaByteBuffer> callBack, bool sync)
        {
            int hash = path.GetHashCode();
            if (CheckNull(hash))
            {
                callBack(funcID, mNullBuffer);
                return;
            }
            mBytesCallBack = callBack;
            mBytesCallBackFuncs[hash] = funcID;
            OnBytesLoad(path,BundleManager.LoadBytes(path));
        }

        public static LuaByteBuffer LoadAsset(string path)
        {
            return BundleManager.LoadBytes(path);
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
                if(mPool[i].mUsingObj.Count == 0 && mPool[i].mLastUseTime + CACHE_DURATION < Time.unscaledTime)
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
                    UnityObj obj = ab.LoadAsset<UnityObj>(DllMgr.common_md5(path));
                    ab.Unload(false);
                    OnObjectLoad(path, obj);
                }
                else
                {
                    AsyncOperation aop = null;
                    if (ab.isStreamedSceneAssetBundle)
                    {
                        aop = SceneMgr.LoadSceneAsync(DllMgr.common_md5(path));
                    }
                    else
                    {
                        aop = ab.LoadAssetAsync<UnityObj>(DllMgr.common_md5(path));
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
                for(int i = 0;i < lc.funcID.Count;i++)
                {
                    lc.callBack(lc.funcID[i], pool != null ? pool.CreateObj() : null);
                }
                //清空回调
                lc.callBack = null;
                lc.funcID.Clear();
            }
        }

        private static void OnBytesLoad(string path, LuaByteBuffer buffer)
        {
            int hash = path.GetHashCode();
            if (buffer.buffer == null)
            {
                mNullAssets.Add(hash);
            }
            //执行回调
            if (mBytesCallBackFuncs.ContainsKey(hash))
            {
                int funcID = mBytesCallBackFuncs[hash];
                if(mBytesCallBack != null)
                {
                    mBytesCallBack(funcID,buffer);
                }
            }
        }
    }
}
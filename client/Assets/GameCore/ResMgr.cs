using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LycheeSDK;
using cyou.ldj.sdk;
using com.cyou.plugin.resource;
using LuaInterface;
using com.cyou.plugin.res.callback;
using com.cyou.plugin.log;

namespace GameCore
{
    public class ResMgr : BaseMgr<ResMgr>
    {
        private enum CALL_BACK_TYPE
        {
            UNITY_OBJECT = 1,
            SCENE_OBJECT = 2,
            BYTES_OBJECT = 3,
        }

        private bool mInit = false;
        private AssetBundlerLoader mAssetBundlerLoader = null;
        private PackageManager mPackageMgr = null;

        private UpdateManager mUpdateMgr = null;
        private HttpManager mHttpMgr = null;

        private ResourceManager mResMgr = null;
        private LuaFunction mLuaFunc = null;
        private Dictionary<int, string> mResID = null;
        private int mUIAssetBaseID = 100000;

        public override void Init(GameObject owner)
        {
            if (!mInit)
            {
                mInit = true;
                base.Init(owner);

                mAssetBundlerLoader = new AssetBundlerLoader();
                mAssetBundlerLoader.mFileManager = new FileManager();
                mPackageMgr = new PackageManager(mAssetBundlerLoader.mFileManager);

                mHttpMgr = new HttpManager();
                mUpdateMgr = UpdateManager.Create(owner, mAssetBundlerLoader.mFileManager, mHttpMgr);

                mResMgr = ResourceManager.Singleton;
                ResourceManager.m_AssetBundleLoader = mAssetBundlerLoader;
                ResourceManager.Singleton.GetResConfig = GetResConfig;
                ResourceManager.Singleton.GetResConfigArray = GetResConfigArray;
                ResourceManager.Singleton.Init(GameConst.READ_ASSET_FROM_BUNDLE);
                InitResConfig();
            }
        }

        public void InitCallBack(LuaFunction luaFunc)
        {
            mLuaFunc = luaFunc;
        }

        public int InitUIAsset(int assetID,string assetPath)
        {
            mResID[assetID + mUIAssetBaseID] = assetPath;
            return assetID + mUIAssetBaseID;
        }

        public void ClearAsset()
        {
            //更新后重新获取资源ID映射关系
            InitResConfig();
        }

        public override void Update()
        {
            if(mInit)
            {
                mResMgr.Tick((uint)Mathf.RoundToInt(Time.deltaTime * 1000));
            }
        }

        public override void Exit()
        {
            base.Exit();
            mResMgr.Release();
            mPackageMgr.ClearPackageInfo();
        }

        public UnityEngine.Object LoadObject(int resID)
        {
            return mResMgr.LoadResource(resID);
        }

        public int LoadObjectAsync(int resID, int priority)
        {
            int index = mResMgr.LoadResourceAsync(resID, priority);
            if (index != -1)
            {
                mResMgr.RegisterCallBack(index, OnObjectLoad);
            }
            else
            {
                LogSystem.Error("Failed to LoadObjectAsync, Res {0},priority {1}", resID, priority);
            }
            return index;
        }

        public UnityEngine.Object LoadInstantiateObject(int resID, bool isActive)
        {
            return mResMgr.InstantiateGameObject(resID, isActive);
        }

        public int LoadInstantiateObjectAsync(int resID, bool isActive,int priority)
        {            
            int index = mResMgr.InstantiateGameObjectAsync(resID, isActive, priority);            
            if (index != -1)
            {
                mResMgr.RegisterCallBack(index, OnObjectLoad);
            }
            else
            {
                LogSystem.Error("Failed to LoadInstantiateObjectAsync, Res {0},priority {1}", resID, priority);
            }
            
            return index;
        }

        public int LoadInstantiateObjectAsync(int resID, bool isActive, int priority, ResourceCallback.DelegateResourceCallBack callBack)
        {
            int index = mResMgr.InstantiateGameObjectAsync(resID, isActive, priority);
            if (index != -1)
            {
                mResMgr.RegisterCallBack(index, callBack);
            }
            else
            {
                LogSystem.Error("Failed to LoadInstantiateObjectAsync, Res {0},priority {1},isActive{2}", resID, priority,isActive);
            }
            return index;
        }

        public void LoadScene(int resID)
        {
            mResMgr.RegisterSceneLoadedCallBack(OnSceneLoad);
            mResMgr.LoadScene(resID);
        }

        public LuaByteBuffer LoadBytes(string fileName)
        {
            return mPackageMgr.ReadFromPackage(fileName);
        }

        public void StopLoading(int loadIndex)
        {
            mResMgr.StopLoading(loadIndex);
        }

        public void UnloadObject(UnityEngine.Object obj)
        {
            mResMgr.DestroyResource(obj);
        }

        private void InitResConfig()
        {
            LuaByteBuffer buffer = LoadBytes("ResData.bytes");
            using (MemoryStream ms = new MemoryStream(buffer.buffer))
            {
                ms.SetLength(buffer.Length);
                ResConfig.AllResConfig configData = ProtoBuf.Serializer.Deserialize<ResConfig.AllResConfig>(ms);
                if (mResID == null)
                    mResID = new Dictionary<int, string>(configData.datas.Count);
                else
                    mResID.Clear();
                for (int i = 0; i < configData.datas.Count; i++)
                {
                    ResConfig.ResConfig data = configData.datas[i];
                    mResID[data.id] = data.path;
                }
            }
        }

        private IResConfig GetResConfig(int resID)
        {
            if(mResID.ContainsKey(resID))
            {
                string path = mResID[resID];
                if(!mResMgr.IsUseBundle())
                {
                    EditorResConfig config = new EditorResConfig();
                    config.ID = resID;
                    config.MainPath = path;
                    return config;
                }
                else
                {
                    //Debug.Log("GetResConfig   " + path + "   " + resID);
                    var bundleName = mAssetBundlerLoader.GetBundleName(path);
                    if (bundleName == null)
                    {                        
                        LogSystem.Error("mAssetBundlerLoader   GetResCmAssetBundlerLoaderonfig,not find : resID {0},path {1}   ",resID,path);
                        return null;
                    }
                       

                    List<string> bundles = new List<string>();
                    bundles.Add(bundleName);
                    string[] dependes = mAssetBundlerLoader.GetBundleDependencies(bundleName);
                    bundles.AddRange(dependes);

                    BundleResConfig config = new BundleResConfig();
                    config.ID = resID;
                    config.MainName = path;
                    config.BundleNameArray = bundles.ToArray();

                    if (path.EndsWith(".unity"))
                    {
                        //场景mainName是场景名字
                        config.MainName = Path.GetFileNameWithoutExtension(path);
                    }
                    return config;
                }
            }
            else
            {
                LogSystem.Error("GetResConfig,not find : {0} ",resID);
                return null;
            }
        }

        private IResConfig[] GetResConfigArray(int[] resIDArray)
        {
            return null;
        }

        private void OnObjectLoad(int index,object obj)
        {
            if(mLuaFunc != null)
            {
                mLuaFunc.BeginPCall();
                mLuaFunc.Push((int)CALL_BACK_TYPE.UNITY_OBJECT);
                mLuaFunc.Push(index);
                mLuaFunc.Push(obj);
                mLuaFunc.PCall();
                mLuaFunc.EndPCall();
            }else
            {
                LogSystem.Error("OnObjectLoad,not find callback  " );
            }
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (mLuaFunc != null)
            {
                mLuaFunc.BeginPCall();
                mLuaFunc.Push((int)CALL_BACK_TYPE.SCENE_OBJECT);
                mLuaFunc.Push(scene.name);
                mLuaFunc.PCall();
                mLuaFunc.EndPCall();
            }
        }
        
    }
}

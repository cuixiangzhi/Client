using com.cyou.plugin.resource.loader.bundle;
using UnityEngine;

namespace cyou.ldj.sdk
{
    public class AssetBundlerLoader : IAssetBundlerLoader
    {
        public FileManager mFileManager;
        public LycheeSDK.BuildDatabase mBuildDB;

        public void Init()
        {
            if (mFileManager == null)
            {
                mFileManager = new FileManager();
            }

            if (mBuildDB == null)
            {
                mBuildDB = new LycheeSDK.BuildDatabase(mFileManager);
            }
        }

        public string GetBundleName(string assetPath)
        {
            return mBuildDB.GetBundleName(assetPath);
        }

        public string[] GetBundleDependencies(string bundleName)
        {
            return mBuildDB.GetBundleDependencies(bundleName, true);
        }
        

        public AssetBundle LoadBundle(string bundleName)
        {
            LycheeSDK.Location location;

            if (mBuildDB.LocateBundle(bundleName, out location))
            {
                string path = mFileManager.GetPath(location, bundleName);
                return AssetBundle.LoadFromFile(path);
            }
            else
            {
                return null;
            }
        }

        public AssetBundleCreateRequest LoadBundleAsync(string bundleName)
        {
            LycheeSDK.Location location;

            if (mBuildDB.LocateBundle(bundleName, out location))
            {
                string path = mFileManager.GetPath(location, bundleName);
                return AssetBundle.LoadFromFileAsync(path);
            }
            else
            {
                return null;
            }
        }
    }
}

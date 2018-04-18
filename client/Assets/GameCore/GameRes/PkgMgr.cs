using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public static class PkgMgr
    {
        internal static Dictionary<string, AssetBundle> mBundleDic = new Dictionary<string, AssetBundle>(64);
#if UNITY_EDITOR
        internal static string mBundlePath = Application.dataPath + "/../../../assets/";
#else 
        internal static string mBundlePath = Application.streamingAssetsPath + "/";
#endif

        public static AssetBundle LoadBundle(string path)
        {
            AssetBundle bundle = null;
            if (!mBundleDic.TryGetValue(path,out bundle))
            {
                string fullPath = string.Format("{0}{1}", mBundlePath, path);
                bundle = AssetBundle.LoadFromFile(fullPath);
                if(bundle != null)
                {
                    mBundleDic[path] = bundle;
                }               
            }
            return bundle;
        }

        public static int LoadBytes(string path,byte[] buffer)
        {
            bool androidApp = Application.platform == RuntimePlatform.Android;
            string fullPath = string.Format("{0}{1}.bytes", androidApp ? string.Empty : mBundlePath, path);
            if (androidApp)
            {
                IntPtr file = CommonDLL.common_android_open(path);
                if (file != IntPtr.Zero)
                {
                    int len = CommonDLL.common_android_read(file, buffer, buffer.Length);
                    CommonDLL.common_android_close(file);
                    return len;
                }
            }
            else
            {
                IntPtr file = CommonDLL.common_open(fullPath, "rb");
                if(file != IntPtr.Zero)
                {
                    int len = CommonDLL.common_read(file, buffer.Length, buffer);
                    CommonDLL.common_close(file);
                    return len;
                }                
            }
            return 0;
        }

        public static void UnloadBundle(string path)
        {
            AssetBundle bundle = null;
            if(mBundleDic.TryGetValue(path, out bundle))
            {
                bundle.Unload(false);
                mBundleDic.Remove(path);
            }
        }
    }
}


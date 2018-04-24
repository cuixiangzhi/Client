using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    internal class PkgInfo
    {
        internal 
    }

    public static class PkgMgr
    {
        internal static Dictionary<string, AssetBundle> mBundleDic = new Dictionary<string, AssetBundle>(64);
        internal static Dictionary<string, int> mBundleDependDic = new Dictionary<string, int>(64);
#if UNITY_EDITOR
        internal static string mBundlePath = Application.dataPath + "/../../../assets/";
#else 
        internal static string mBundlePath = Application.streamingAssetsPath + "/";
#endif

        public static AssetBundle LoadBundle(string fileName)
        {
            AssetBundle bundle = null;
            if (!mBundleDic.TryGetValue(fileName, out bundle))
            {
                string fullPath = string.Format("{0}{1}", mBundlePath, fileName);
                bundle = AssetBundle.LoadFromFile(fullPath);
                if(bundle != null)
                {
                    mBundleDic[fileName] = bundle;
                }               
            }
            return bundle;
        }

        public static int LoadBytes(string fileName, byte[] buffer)
        {
            bool androidApp = Application.platform == RuntimePlatform.Android;
            string fullPath = string.Format("{0}{1}.bytes", androidApp ? string.Empty : mBundlePath, fileName);
            if (androidApp)
            {
                IntPtr file = CommonDLL.common_android_open(fileName);
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

        public static void UnloadBundle(string fileName)
        {
            AssetBundle bundle = null;
            if(mBundleDic.TryGetValue(fileName, out bundle))
            {
                bundle.Unload(false);
                mBundleDic.Remove(fileName);
            }
        }

        public static void UnloadDependBundle(string fileName)
        {

        }
    }
}


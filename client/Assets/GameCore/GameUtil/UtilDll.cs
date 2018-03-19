using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
using System.Text;

namespace GameCore
{
    public static class UtilDll
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string DLLNAME = "__Internal";
#else
        private const string DLLNAME = "common";
#endif


        [DllImport(DLLNAME)]
        private static extern void common_md5(string data, int startIndex, StringBuilder outdata);
        private static StringBuilder mMD5Buffer = new StringBuilder(64);
        private static Dictionary<int, string> mCacheMD5Result = new Dictionary<int, string>(1024);
        public static string common_md5(string data)
        {
            if (!Application.isPlaying)
            {
                mCacheMD5Result.Clear();
            }
            int hash = data.GetHashCode();
            if (mCacheMD5Result.ContainsKey(hash))
            {
                return mCacheMD5Result[hash];
            }
            int startIndex = data.LastIndexOf("/");
            if (startIndex < 0)
            {
                startIndex = data.LastIndexOf("\\");
            }
            startIndex = startIndex < 0 ? 0 : startIndex + 1;
            mMD5Buffer.Remove(0, mMD5Buffer.Length);
            common_md5(data, startIndex, mMD5Buffer);
            string ret = mMD5Buffer.ToString();
            mCacheMD5Result[hash] = ret;
            return ret;
        }

        [DllImport(DLLNAME)]
        public static extern void common_encode(byte[] data, int len);

        [DllImport(DLLNAME)]
        public static extern void common_decode(byte[] data, int len);

        [DllImport(DLLNAME)]
        public static extern int common_diff(string oldpath, string newpath, string patchpath);

        [DllImport(DLLNAME)]
        public static extern int common_patch(string oldpath, string patchpath, string newpath);

        [DllImport(DLLNAME)]
        private static extern IntPtr common_android_open(string file, IntPtr mgr, int mode);

        private static IntPtr mAssetMgr = IntPtr.Zero;
        public static IntPtr common_android_open(string file)
        {
            if(mAssetMgr == IntPtr.Zero)
            {
                AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
                mAssetMgr = assetMgr.GetRawObject();
            }
            return common_android_open(file, mAssetMgr, 1);
        }

        [DllImport(DLLNAME)]
        public static extern int common_android_read(IntPtr asset, byte[] buffer, int len);

        [DllImport(DLLNAME)]
        public static extern void common_android_seek(IntPtr asset, int offset, int where);

        [DllImport(DLLNAME)]
        public static extern void common_android_close(IntPtr asset);

        public static void Init()
        {
            mCacheMD5Result.Clear();
        }

        public static void Exit()
        {
            mCacheMD5Result.Clear();
        }
    }
}

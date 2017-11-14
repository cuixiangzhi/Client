using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System.Text;

namespace GameFrameWork
{
    public static class UtilDll
    {
#if UNITY_IPHONE
        private const string DLLNAME = "__Internal";
#else
        private const string DLLNAME = "common";
#endif


        [DllImport(DLLNAME)]
        private static extern void common_md5(string data,int startIndex, StringBuilder outdata);
        private static StringBuilder mMD5Buffer = new StringBuilder(64);
        private static Dictionary<int, string> mCacheMD5Result = new Dictionary<int, string>(1024);
        public static string common_md5(string data)
        {
            int hash = data.GetHashCode();
            if (mCacheMD5Result.ContainsKey(hash))
            {
                return mCacheMD5Result[hash];
            }
            int startIndex = data.LastIndexOf("/");
            if(startIndex < 0)
            {
                startIndex = data.LastIndexOf("\\");
            }
            if(startIndex < 0)
            {
                startIndex = 0;
            }
            common_md5(data, startIndex, mMD5Buffer);
            string ret = mMD5Buffer.ToString();
            mCacheMD5Result[hash] = ret;
            return ret;
        }

        [DllImport(DLLNAME)]
        public static extern void common_encode(byte[] data,int len);

        [DllImport(DLLNAME)]
        public static extern void common_decode(byte[] data, int len);

        public static void Init()
        {
            
        }

        public static void Exit()
        {

        }
    }
}

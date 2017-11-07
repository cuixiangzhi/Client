using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System.Text;

namespace GF
{
    public static class UtilDll
    {
#if UNITY_IPHONE
        private const string DLLNAME = "__Internal";
#else
        private const string DLLNAME = "common";
#endif


        [DllImport(DLLNAME)]
        private static extern void common_md5(string data,StringBuilder outdata);
        private static StringBuilder mCacheMD5Result = new StringBuilder(64);
        public static string common_md5(string data)
        {
            common_md5(data, mCacheMD5Result);
            return mCacheMD5Result.ToString();
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

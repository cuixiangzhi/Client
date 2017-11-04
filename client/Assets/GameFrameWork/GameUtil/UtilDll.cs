using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;

namespace GF
{
    public static class UtilDll
    {
#if UNITY_IPHONE
        private const string DLLNAME = "__Internal";
#else
        private const string DLLNAME = "utils";
#endif
        private delegate void LOG_FUNC(string msg);

        [DllImport(DLLNAME)]
        public static extern void dllinit(IntPtr logFunc);

        [DllImport(DLLNAME)]
        public static extern string gethashcode(string filePath);

        public static void Init()
        {
            
        }

        public static void Exit()
        {

        }

        [MonoPInvokeCallback(typeof(LOG_FUNC))]
        private static void Log(string msg)
        {
            Logger.Log(msg);
        }
    }
}

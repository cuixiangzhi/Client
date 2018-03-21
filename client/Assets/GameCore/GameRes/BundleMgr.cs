using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using LuaInterface;
using System.Runtime.InteropServices;

namespace GameCore
{
    public static class BundleMgr
    {
#if UNITY_EDITOR
        private static string persistentPath = Application.dataPath + "/../../assets";
#else
        private static string persistentPath = Application.persistentDataPath;
#endif
        //IO BUFFER
        private static int MAX_BYTE_LEN = 1024 * 1024 * 2;
        private static byte[] mBuffer = null;

        public static void Init()
        {
            mBuffer = new byte[MAX_BYTE_LEN];
        }

        public static void Exit()
        {
            mBuffer = null;
        }

        public static AssetBundle LoadBundle(string relativePath)
        {
            string fullPath = string.Format("{0}/{1}", persistentPath, relativePath);
            AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
            if (bundle == null)
            {
                UtilLog.LogWarning("asset is null {0}", fullPath);
            }
            return bundle;
        }

        public static LuaByteBuffer LoadBytes(string relativePath)
        {
            string fullPath = string.Format("{0}/{1}", persistentPath, relativePath);
            IntPtr file = UtilDll.common_open(fullPath, "rb");
            int len = 0;
            if (file != IntPtr.Zero)
            {
                len = UtilDll.common_read(file, mBuffer.Length, mBuffer);
                UtilDll.common_close(file);
            }
            else
            {
                UtilLog.LogWarning("asset is null {0}", fullPath);
            }
            return new LuaByteBuffer(mBuffer, len);
        }
    }
}

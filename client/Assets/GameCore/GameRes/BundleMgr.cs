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
        private static string persistentPath = Application.streamingAssetsPath;
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
            if (File.Exists(fullPath))
            {
                return AssetBundle.LoadFromFile(fullPath);
            }
            else
            {
                UtilLog.LogWarning("asset is null {0}", relativePath);
                return null;
            }
        }

        public static LuaByteBuffer LoadBytes(string relativePath)
        {
            string fullPath = string.Format("{0}/{1}", persistentPath, relativePath);
            if (File.Exists(fullPath))
            {
                return new LuaByteBuffer(File.ReadAllBytes(fullPath));
            }
            else
            {
                UtilLog.LogWarning("asset is null {0}", relativePath);
                return new LuaByteBuffer(null, 0);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;
using UnityObject = UnityEngine.Object;

namespace GF
{
    public static class ResManager
    {
        public static void Init()
        {

        }

        public static void ReInit()
        {

        }

        public static void LateLoop()
        {

        }

        public static void Exit()
        {
            
        }

        public static UnityObject LoadAsset(string path, int type, Action<string, UnityObject> callBack, bool sync)
        {
            return null;
        }

        public static LuaByteBuffer LoadAsset(string path, Action<string, LuaByteBuffer> callBack = null, bool sync = true)
        {
            return null;
        }

        private static void OnBundleLoad(string path, AssetBundle ab)
        {
            
        }
    }
}
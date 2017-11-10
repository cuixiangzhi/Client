using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;
using UnityObj = UnityEngine.Object;

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

        public static void LoadAsset(string filePath, Action<UnityObj> callValue, bool sync, bool needComponent, Type t)
        {
            
        }

        public static LuaByteBuffer LoadAsset(string filePath, Action<LuaByteBuffer> callValue, bool sync)
        {
            return null;
        }

        private static void LoadBundle(int key,AssetBundle ab)
        {

        }
    }
}
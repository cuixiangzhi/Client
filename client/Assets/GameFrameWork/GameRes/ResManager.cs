using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;

namespace GF
{
    public static class ResManager
    {
        //管理AB,对不同资源的加载做封装,AB出来后转到各个资源的管理器内处理
        //缓存
        private static Dictionary<int, AssetBundle> CACHED_AB = null;
        //回调
        private static Action<AssetBundle> CALL_OF_AB = null;
        private static Action<LuaByteBuffer> CALL_OF_BT = null;
        private static Dictionary<int,List<int>> CALL_KEY_OF_AB = null;
        private static Dictionary<int, List<int>> CALL_KEY_OF_BT = null;

        public static void Init()
        {

        }

        public static void LateLoop()
        {

        }

        public static void Exit()
        {

        }

        public static void LoadUI(string filePath,int callKey,Action<UIBehaviour> callValue,bool sync)
        {

        }

        public static void LoadModel(string filePath, int callKey, Action<UIBehaviour> callValue, bool sync)
        {

        }

        public static void LoadEffect(string filePath, int callKey, Action<UIBehaviour> callValue, bool sync)
        {

        }

        public static void LoadSound(string filePath, int callKey, Action<UIBehaviour> callValue, bool sync)
        {

        }

        public static void LoadAsset(string filePath, int callKey, Action<LuaByteBuffer> callValue, bool sync)
        {

        }

        public static void ClearAssets()
        {

        }
    }
}
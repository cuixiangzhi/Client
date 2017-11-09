using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LuaInterface;
using UnityObj = UnityEngine.Object;
using UnityEngine.Networking;

namespace GF
{
    public static class ResManager
    {
        public static void Init()
        {
            //初始化资源包路径
            //读取资源包信息
            AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player");
        }

        public static void LateLoop()
        {

        }

        public static void Exit()
        {

        }

        public static void Destroy(UnityObj o)
        {

        }

        public static LuaByteBuffer LoadBytes(string filePath)
        {
            return new LuaByteBuffer(Resources.Load<TextAsset>(filePath.Replace(".lua","") + "_32").bytes);
        }
    }
}
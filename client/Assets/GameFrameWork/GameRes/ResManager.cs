using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LuaInterface;
using UnityObj = UnityEngine.Object;

namespace GF
{
    public static class ResManager
    {
        //实例资源
        private static Dictionary<int, Queue<UnityObj>> mCopyCache;
        //加载资源
        private static Dictionary<int, UnityObj> mLoadCache;
        //压缩资源
        private static Dictionary<int, AssetBundle> mBundleCache;

        //字节位置
        private static Dictionary<int, int> mByteIndex;

        private static string PACKAGE_PATH;

        public static void Init()
        {
            //初始化资源包路径
#if UNITY_EDITOR
            PACKAGE_PATH = string.Format("{0}/resources",Application.dataPath.Replace("Assets",""));
#else
            PACKAGE_PATH = string.Format("{0}/resources",Application.persistentDataPath);
#endif
            if (!File.Exists(PACKAGE_PATH))
            {
                PACKAGE_PATH = string.Format("{0}/resources", Application.streamingAssetsPath);
            }
            //读取资源包信息
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
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject am = jo.Call<AndroidJavaObject>("getAssets");

            AndroidJavaObject stream = am.Call<AndroidJavaObject>("open", fileName);

            byte[] buffer = new byte[1024000];
            int len = stream.Call<int>("read", buffer);
            return new LuaByteBuffer(buffer);
        }

//         private static LuaByteBuffer Read(string fileName)
//         {
// #if UNITY_ANDROID && !UNITY_EDITOR
// 
// #else
//             FileStream fs = File.OpenRead(Application.streamingAssetsPath + "/" + fileName);
// 
// #endif
//         }
    }
}
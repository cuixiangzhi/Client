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
        //文件名
        private static string LUA_LIB_NAME = null;
        private static string LUA_LOGIC_NAME = null;
        //缓冲区
        private static byte[] LUA_BUFFER = null;
        //文件流
        private static AndroidJavaObject LUA_LIB_STREAM = null;

        public static void Init()
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/test",FileMode.Create);
            byte[] bbb = new byte[1024 * 1024 * 200];
            fs.Write(bbb, 0, bbb.Length);
            fs.Flush();
            fs.Close();

            fs = new FileStream(Application.persistentDataPath + "/test", FileMode.Open);
            int len = 1;
            while(len != 0)
            {
                Logger.Log("read begin" + Time.realtimeSinceStartup);
                len = fs.Read(bbb, 0, 1024 * 1024 * 10);
                Logger.Log("read finish" + Time.realtimeSinceStartup);
            }



            LUA_LIB_NAME = UtilDll.common_md5(GameConst.LUA_LIB_NAME);
            LUA_LOGIC_NAME = UtilDll.common_md5(GameConst.LUA_LOGIC_NAME);

            
            //读取资源包信息
            AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject assets = context.Call<AndroidJavaObject>("getAssets");
            AndroidJavaObject stream = assets.Call<AndroidJavaObject>("open", "20170621_256");
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
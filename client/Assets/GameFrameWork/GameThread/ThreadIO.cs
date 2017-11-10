using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using LuaInterface;

namespace GF
{
    public sealed class ThreadIO : ThreadBase
    {
        //文件名
        private static string LUA_LIB_NAME = null;
        private static string LUA_LOGIC_NAME = null;
        //缓冲区
        private static byte[] LUA_BUFFER = null;
        //文件流
        private static AndroidJavaObject LUA_LIB_STREAM_ANDROID = null;
        private static FileStream LUA_LIB_STREAM_FILE = null;
        private static AndroidJavaObject LUA_LOGIC_STREAM_ANDROID = null;
        private static AndroidJavaObject LUA_LOGIC_STREAM_FILE = null;

        public void Init()
        {
            FileStream fs = new FileStream(Application.persistentDataPath + "/test", FileMode.Create);
            byte[] bbb = new byte[1024 * 1024 * 200];
            fs.Write(bbb, 0, bbb.Length);
            fs.Flush();
            fs.Close();

            fs = new FileStream(Application.persistentDataPath + "/test", FileMode.Open);
            int len = 1;
            while (len != 0)
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

        public void Loop()
        {

        }

        public void Exit()
        {

        }

        public bool CanRunInThread()
        {
            return false;
        }

        public static void AddIOTask(string path,Action<AssetBundle> callback,bool sync)
        {

        }

        public static void AddIOTask(string path, Action<LuaByteBuffer> callback, bool sync)
        {

        }
    }
}

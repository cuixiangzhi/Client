using GameFrameWork;
using System.IO;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GL
{
    public class GameMain : GameLogic
    {
        [DllImport("android")]
        private static extern IntPtr AAssetManager_fromJava(IntPtr assetManager);

        [DllImport("android")]
        private static extern IntPtr AAssetManager_open(IntPtr assetManager,string filename,int mode);

        AndroidJavaClass player = null;

        private void Awake()
        {
            player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            string appPath = Application.persistentDataPath;
            Thread t = new Thread(() =>
            {
                AndroidJNI.AttachCurrentThread();
                AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
                AndroidJavaObject stream = assetMgr.Call<AndroidJavaObject>("open", "20170621_256");
                byte[] buffer = new byte[1024 * 1024 * 1];
                int total = 0;
                Debug.Log(DateTime.Now.Ticks);
                FileStream fs = new FileStream(appPath + "/tmp", FileMode.Create);
                while(total < 1024 * 1024 * 50)
                {
                    total += stream.Call<int>("read", buffer);
                }
                fs.Close();
                Debug.Log(DateTime.Now.Ticks);
                AndroidJNI.DetachCurrentThread();
            });
            t.Start();

            Thread t1 = new Thread(() =>
            {
                AndroidJNI.AttachCurrentThread();
//                 AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
//                 AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
//                 AndroidJavaObject stream = assetMgr.Call<AndroidJavaObject>("open", "20170621_256");
//                 stream.Call<long>("skip",(long)(1024 * 1024 * 50));
//                 byte[] buffer = new byte[1024 * 1024 * 1];
//                 int len = 1;
//                 Debug.Log(DateTime.Now.Ticks);
//                 while (len > 0)
//                 {
//                     len = stream.Call<int>("read", buffer);
//                 }
//                 Debug.Log(DateTime.Now.Ticks);
                AndroidJNI.DetachCurrentThread();
            });
            t1.Start();

            //框架初始化
            //GameFrameWork.GameFrameWork.Init(this);
            //游戏逻辑初始化
            //LuaManager.DoFile("GameMain");

        }

        public override void Loop()
        {

        }

        public override void LateLoop()
        {

        }

        public override void FixedLoop()
        {

        }

        public override void Exit()
        {

        }
    }
}

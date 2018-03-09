using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using GameCore;
using System.IO;

namespace GameLogic
{
    public class GameTest : MonoBehaviour
    {
        public Animator anim;

		public IntPtr mFilePtr;

		public byte[] mBuffer = new byte[1024 * 1024]; 

		public float totalTime = 0;

        private void Awake()
        {
            //AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
            //AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
            //mFilePtr = UtilDll.common_android_open("c", assetMgr.GetRawObject(), 1);

            //FileStream fs = new FileStream (Application.persistentDataPath + "/file.bundle",FileMode.Create);

            //         if(mFilePtr != IntPtr.Zero)
            //         {
            //             int len = 0;
            //             totalTime = Time.realtimeSinceStartup;
            //             while ((len = UtilDll.common_android_read(mFilePtr, mBuffer, mBuffer.Length)) > 0)
            //             {
            //                 fs.Write(mBuffer, 0, len);
            //             }
            //             totalTime = Time.realtimeSinceStartup - totalTime;
            //             UtilDll.common_android_close(mFilePtr);
            //         }
        }

		void OnGUI()
		{
			GUILayout.Label (totalTime.ToString ());
		}
    }
}

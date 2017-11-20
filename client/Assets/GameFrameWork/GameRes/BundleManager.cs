using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using LuaInterface;

namespace GameFrameWork
{
    public static class BundleManager
    {
#if UNITY_EDITOR
        private static string persistentPath = Application.dataPath + "/../";
#else
        private static string persistentPath = Application.persistentDataPath + "/";
#endif
        //IO BUFFER
        private static int MAX_BYTE_LEN = 1024 * 1024;
        private static byte[] mBuffer = null;
        //资源包信息
        private static FileMap mFileMap = null;
        //文件流
        private static string packageOldPath;
        private static string packageNewPath;
        private static FileStream mPersistStream = null;
        private static FileStream mStreamingStream = null;
        private static AndroidJavaObject mAndroidStreamingStream = null;

        public static void Init()
        {
            mBuffer = new byte[MAX_BYTE_LEN];
            mFileMap = new FileMap();
            //旧包数据
            string fileMapOldPath = Application.streamingAssetsPath + "/" + UtilDll.common_md5(GameConst.FILEMAP_NAME);
            packageOldPath = Application.streamingAssetsPath + "/" + UtilDll.common_md5(GameConst.PACKAGE_NAME);
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
                AndroidJavaObject stream = assetMgr.Call<AndroidJavaObject>("open", UtilDll.common_md5(GameConst.FILEMAP_NAME));
                int len = stream.Call<int>("read",mBuffer);
                stream.Call("close");
                Logger.Log("old filemap read {0} byte",len);
                mFileMap.CreateOldFileMap(mBuffer, len);
                //随机读取块数据
                mAndroidStreamingStream = assetMgr.Call<AndroidJavaObject>("open", UtilDll.common_md5(GameConst.PACKAGE_NAME),1);
                mAndroidStreamingStream.Call("mark", 1024 * 1024 * 1024);
            }
            else
            {
                FileStream fs = File.OpenRead(fileMapOldPath);
                int len = fs.Read(mBuffer, 0, mBuffer.Length);
                fs.Close();
                Logger.Log("old filemap read {0} byte", len);
                mFileMap.CreateOldFileMap(mBuffer, len);
                mStreamingStream = File.OpenRead(packageOldPath);
            }
            //新包数据
            string fileMapNewPath = persistentPath + "/" + UtilDll.common_md5(GameConst.FILEMAP_NAME);
            packageNewPath = persistentPath + "/" + UtilDll.common_md5(GameConst.PACKAGE_NAME);
            if (File.Exists(fileMapNewPath))
            {
                FileStream fs = File.OpenRead(fileMapNewPath);
                int len = fs.Read(mBuffer, 0, mBuffer.Length);
                fs.Close();
                Logger.Log("new filemap read {0} byte", len);
                mFileMap.CreateNewFileMap(mBuffer, len);
                mPersistStream = File.OpenRead(packageNewPath);
            }
        }

        public static void Exit()
        {
            mBuffer = null;
            mFileMap = null;
            if(mPersistStream != null)
                mPersistStream.Close();
            mPersistStream = null;
            if (mStreamingStream != null)
                mStreamingStream.Close();
            mStreamingStream = null;
            mAndroidStreamingStream = null;
        }

        public static void LoadBundle(string path, Action<string, AssetBundle> callBack)
        {
            ByteData data = mFileMap.GetByteData(path);
            if(data.mLength != 0)
            {
                if(mFileMap.IsNewFile(path))
                {
                    callBack(path, AssetBundle.LoadFromFile(packageNewPath, 0, data.mOffset));
                }
                else
                {
                    callBack(path, AssetBundle.LoadFromFile(packageOldPath, 0, data.mOffset));
                }
            }
        }

        public static LuaByteBuffer LoadBytes(string path)
        {
            ByteData data = mFileMap.GetByteData(path);
            if(data.mLength != 0)
            {
                if(mFileMap.IsNewFile(path))
                {
                    mPersistStream.Seek(data.mOffset, SeekOrigin.Begin);
                    mPersistStream.Read(mBuffer, 0, (int)data.mLength);
                }
                else
                {
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        mAndroidStreamingStream.Call("reset");
                        mAndroidStreamingStream.Call<long>("skip", (long)data.mOffset);
                        mAndroidStreamingStream.Call<int>("read", mBuffer,0,(int)data.mLength);
                    }
                    else
                    {
                        mStreamingStream.Seek(data.mOffset, SeekOrigin.Begin);
                        mStreamingStream.Read(mBuffer, 0, (int)data.mLength);
                    }
                }
                UtilDll.common_decode(mBuffer, (int)data.mLength);
                return new LuaByteBuffer(mBuffer, (int)data.mLength);
            }
            else
            {
                return new LuaByteBuffer(null, 0);
            }           
        }
    }
}

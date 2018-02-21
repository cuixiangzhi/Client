using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using LuaInterface;
using System.Runtime.InteropServices;

namespace GameCore
{
    public static class BundleMgr
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
        private static IntPtr mFilePtr;

        public static void Init()
        {
            mBuffer = new byte[MAX_BYTE_LEN];            
            mFileMap = new FileMap();
            //旧包数据
            string fileMapOldPath = Application.streamingAssetsPath + "/" + DllMgr.common_md5(GameConst.FILEMAP_NAME);
            packageOldPath = Application.streamingAssetsPath + "/" + DllMgr.common_md5(GameConst.PACKAGE_NAME);
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject assetMgr = context.Call<AndroidJavaObject>("getAssets");
                mFilePtr = DllMgr.common_android_open(DllMgr.common_md5(GameConst.FILEMAP_NAME), assetMgr.GetRawObject(), 1);
                int len = DllMgr.common_android_read(mFilePtr, mBuffer, mBuffer.Length);
                DllMgr.common_android_close(mFilePtr);
                LogMgr.Log("old filemap read {0} byte",len);
                mFileMap.CreateOldFileMap(mBuffer, len);
                //随机读取块数据
                mFilePtr = DllMgr.common_android_open(DllMgr.common_md5(GameConst.PACKAGE_NAME), assetMgr.GetRawObject(), 1); 
            }
            else
            {
                FileStream fs = File.OpenRead(fileMapOldPath);
                int len = fs.Read(mBuffer, 0, mBuffer.Length);
                fs.Close();
                LogMgr.Log("old filemap read {0} byte", len);
                mFileMap.CreateOldFileMap(mBuffer, len);
                mStreamingStream = File.OpenRead(packageOldPath);
            }
            //新包数据
            string fileMapNewPath = persistentPath + "/" + DllMgr.common_md5(GameConst.FILEMAP_NAME);
            packageNewPath = persistentPath + "/" + DllMgr.common_md5(GameConst.PACKAGE_NAME);
            if (File.Exists(fileMapNewPath))
            {
                FileStream fs = File.OpenRead(fileMapNewPath);
                int len = fs.Read(mBuffer, 0, mBuffer.Length);
                fs.Close();
                LogMgr.Log("new filemap read {0} byte", len);
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
            else
            {
                LogMgr.Log("asset is null {0}", path);
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
                        DllMgr.common_android_seek(mFilePtr,(int)data.mOffset,0);
                        DllMgr.common_android_read(mFilePtr, mBuffer, (int)data.mLength);
                    }
                    else
                    {
                        mStreamingStream.Seek(data.mOffset, SeekOrigin.Begin);
                        mStreamingStream.Read(mBuffer, 0, (int)data.mLength);
                    }
                }
                DllMgr.common_decode(mBuffer, (int)data.mLength);
                return new LuaByteBuffer(mBuffer, (int)data.mLength);
            }
            else
            {
                LogMgr.Log("asset is null {0}", path);
                return new LuaByteBuffer(null, 0);
            }           
        }
    }
}

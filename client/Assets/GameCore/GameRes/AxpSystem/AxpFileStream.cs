using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


namespace AxpTools
{
    class AxpFileStream
    {
        private bool isConst = true;
        private FileStream m_fileStream;

        public int FileLength { get { if (m_fileStream != null) return (int)m_fileStream.Length;
#if UNITY_ANDROID && !UNITY_EDITOR
                else return stream.Call<int>("available"); 
#else
                return -1;
#endif
            } }
#if UNITY_ANDROID && !UNITY_EDITOR
        public static AndroidJavaObject assetManager { get { if (_assetManager == null) { GetAsstesManger(); } return _assetManager; } }
        static AndroidJavaObject _assetManager;
        AndroidJavaObject stream;
        IntPtr METHOD_read;
#endif
#region 实例化部分
        private AxpFileStream() { }
#if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
        public static AxpFileStream OpenFile(string url,bool isConst)    //IOS
        {
             return Init(url,isConst);
        }
#elif UNITY_ANDROID
        private static void GetAsstesManger()
        {
            AndroidJavaObject activityJO = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            if (activityJO == null)
            {

                Games.TLBB.Log.LogSystem.Error("Initial Activity Failed.");
            }

            //从Activity取得AssetManager实例
            _assetManager = activityJO.Call<AndroidJavaObject>("getAssets");
            if (_assetManager == null)
            {
                Games.TLBB.Log.LogSystem.Error("assetManager is null");
            }
        }
        public static AxpFileStream OpenFile(string url, bool isConst, bool isStreaming)   //Android
        {
            AxpFileStream stream = null;
            if (isStreaming)
                stream = Init_Streaming(url);
            else
                stream = Init(url, isConst);
            return stream;
        }
        private static AxpFileStream Init_Streaming(string url)
        {
            AxpFileStream stream = new AxpFileStream();

            //打开文件流
            stream.stream = assetManager.Call<AndroidJavaObject>("open", url);
            if (stream.stream == null)
            {
                Games.TLBB.Log.LogSystem.Error("Open Axp File Failed. {0}" , url);
                return null;
            }

            //取得InputStream.read的MethodID
            IntPtr clsPtr = AndroidJNI.FindClass("java/io/InputStream");
            if (clsPtr == IntPtr.Zero)
            {
                Games.TLBB.Log.LogSystem.Error("Find Class Failed.");
                return null;
            }
            stream.METHOD_read = AndroidJNIHelper.GetMethodID(clsPtr, "read", "([B)I");
            if (stream.METHOD_read == IntPtr.Zero)
            {
                Games.TLBB.Log.LogSystem.Error("read ([B)I Failed.");
                AndroidJNI.DeleteLocalRef(clsPtr);
                return null;
            }
            AndroidJNI.DeleteLocalRef(clsPtr);
            return stream;
        }
#endif
        private static AxpFileStream Init(string url, bool isConst)
        {
            AxpFileStream stream = new AxpFileStream();
            if (isConst)
                stream.m_fileStream = File.OpenRead(url);
            else
                stream.m_fileStream = File.Open(url, FileMode.OpenOrCreate);
            if (stream.m_fileStream == null)
            {
                Games.TLBB.Log.LogSystem.Info("Open Axp Failed.");
                return null;
            }
            stream.isConst = isConst;
            return stream;
        }
#endregion
        public void Seek(long offset)
        {
            if (m_fileStream != null && m_fileStream.CanSeek)
                m_fileStream.Seek(offset, SeekOrigin.Begin);
#if UNITY_ANDROID && !UNITY_EDITOR
            else if (stream != null)
            {
                stream.Call("reset");
                while (offset > 0)
                {
                    long rsp = stream.Call<long>("skip", offset); //AndroidJNI.CallLongMethod(stream.GetRawObject(), Method_skip, new[] { new jvalue() { j = offset } });
                    if (rsp == -1)
                    {
                        return;
                    }
                    offset -= rsp;
                }
            }
#endif
        }
        public void Flush()
        {
            if (m_fileStream != null && !isConst && m_fileStream.CanWrite)
                m_fileStream.Flush();
        }
        public byte[] ReadBytes(int count)
        {
            byte[] buffer;
            if (m_fileStream != null && m_fileStream.CanRead)
            {
                buffer = new byte[count];
                int length = m_fileStream.Read(buffer, 0, count);
                if (length > 0)
                {
                    byte[] ret = new byte[length];
                    Array.Copy(buffer, ret, length);
                    return ret;
                }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            else if (stream != null && METHOD_read != IntPtr.Zero)
            {
                //申请一个Java ByteArray对象句柄
                IntPtr byteArray = AndroidJNI.NewByteArray(count);
                if (byteArray == IntPtr.Zero)
                    return null;
                //调用方法
                int length = AndroidJNI.CallIntMethod(stream.GetRawObject(), METHOD_read, new[] { new jvalue() { l = byteArray } });
                if (length > 0)
                {
                    buffer = AndroidJNI.FromByteArray(byteArray);  //从Java ByteArray中得到C# byte数组
                    AndroidJNI.DeleteLocalRef(byteArray);
                    return buffer;
                }
                else
                    AndroidJNI.DeleteLocalRef(byteArray);
            }
#endif
            return null;
        }
        public void Write(byte[] buffer)
        {
            if (m_fileStream != null && !isConst && m_fileStream.CanWrite)
            {
                m_fileStream.Write(buffer, 0, buffer.Length);
            }
        }
        public void Close()
        {
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
                m_fileStream = null;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            else if (stream != null)
            {
                //关闭文件流
                stream.Call("close");
                stream.Dispose();
            }
#endif
        }
        public static uint getDiskFileSize(string strFileName)
        {
            if (string.IsNullOrEmpty(strFileName))
            {
                return 0;
            }

            System.IO.FileInfo file = new System.IO.FileInfo(strFileName);

            if (file != null)
            {
                return (uint)file.Length;
            }

            return 0;
        }
    }

}

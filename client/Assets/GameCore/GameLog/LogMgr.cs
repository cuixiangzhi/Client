using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace GameCore
{
    public static class LogMgr
    {
        //日志文件流
        private static FileStream LOG_STREAM = null;
        //字符串转换byte数组buffer
        private static byte[] STRING_BYTE_BUFFER = null;
        //日志单条最大长度10KB
        private static int MAX_BYTE_LEN_OF_MSG = 10240;

        [Conditional("ENABLE_LOG")]
        public static void Init()
        {
#if !UNITY_EDITOR
            if (!Directory.Exists(GameConst.LOG_PATH))
                Directory.CreateDirectory(GameConst.LOG_PATH);
            LOG_STREAM = new FileStream(string.Format("{0}/{1}.log", GameConst.LOG_PATH, DateTime.Now.ToString("yyyy_MM_dd_HH_mm")), FileMode.Create);
            STRING_BYTE_BUFFER = new byte[MAX_BYTE_LEN_OF_MSG];
#endif
        }

        [Conditional("ENABLE_LOG")]
        public static void Exit()
        {
            if(LOG_STREAM != null)
            {
                LOG_STREAM.Flush();
                LOG_STREAM.Close();
                LOG_STREAM.Dispose();
                LOG_STREAM = null;
            }
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(string msg)
        {
            string message = SaveToFile(msg);
            UnityEngine.Debug.Log(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(string fmt,params object[] args)
        {
            Log(string.Format(fmt, args));
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(string msg)
        {
            string message = SaveToFile(msg);
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(string fmt, params object[] args)
        {
            LogError(string.Format(fmt, args));
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(string msg)
        {
            string message = SaveToFile(msg);
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(string fmt, params object[] args)
        {
            LogWarning(string.Format(fmt,args));
        }

        private static string SaveToFile(string msg)
        {
            string message = string.Format("{0:00000.000} {1} ", Time.realtimeSinceStartup,msg);

            if(LOG_STREAM != null)
            {
                int totalBytes = Encoding.UTF8.GetBytes(message,0,message.Length, STRING_BYTE_BUFFER,0);

                StackTrace trace = new StackTrace(2,true);
                StackFrame[] frames = trace.GetFrames();
                for(int i = 0;i < frames.Length;i++)
                {
                    string fileName = frames[i].GetFileName();
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    fileName = fileName.Replace("\\", "/").Replace(GameConst.PROJECT_PATH, "");
                    string methodName = frames[i].GetMethod().Name;
                    string lineName = frames[i].GetFileLineNumber().ToString();

                    try
                    {
                        totalBytes += Encoding.UTF8.GetBytes(fileName, 0, fileName.Length, STRING_BYTE_BUFFER, totalBytes);
                        totalBytes += Encoding.UTF8.GetBytes(":", 0, 1, STRING_BYTE_BUFFER, totalBytes);
                        totalBytes += Encoding.UTF8.GetBytes(methodName, 0, methodName.Length, STRING_BYTE_BUFFER, totalBytes);
                        totalBytes += Encoding.UTF8.GetBytes(":", 0, 1, STRING_BYTE_BUFFER, totalBytes);
                        totalBytes += Encoding.UTF8.GetBytes(lineName, 0, lineName.Length, STRING_BYTE_BUFFER, totalBytes);
                        totalBytes += Encoding.UTF8.GetBytes("  ", 0, 2, STRING_BYTE_BUFFER, totalBytes);
                    }
                    catch(Exception)
                    {
                        UnityEngine.Debug.LogError(string.Format("log stack trace too long!!max len is {0} byte", MAX_BYTE_LEN_OF_MSG));
                        return message;
                    }
                }
                totalBytes += Encoding.UTF8.GetBytes("\n", 0, 1, STRING_BYTE_BUFFER, totalBytes);
                LOG_STREAM.Write(STRING_BYTE_BUFFER,0,totalBytes);
                LOG_STREAM.Flush();
            }

            return message;
        }
    }
}


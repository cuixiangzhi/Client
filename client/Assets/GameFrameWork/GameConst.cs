using System.IO;
using System.Reflection;
using UnityEngine;

namespace GameFrameWork
{
    public static class GameConst
    {
        //项目路径
        public static string PROJECT_PATH = ((string)typeof(StackTraceUtility).GetField("projectFolder", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic).GetValue(null)).Replace('\\', '/');

        //分析结果保存路径
        public static string PROFILER_PATH = Path.GetFullPath(Application.dataPath + "/../Profiler");
        //日志文件路径
#if UNITY_EDITOR
        public static string LOG_PATH = Path.GetFullPath(Application.dataPath + "/../Log");
#else
        public static string LOG_PATH = Path.GetFullPath(Application.persistentDataPath + "/Log");
#endif
        public static string PACKAGE_LUA_NAME = "luapackage";
    }
}

using System.IO;
using System.Reflection;
using UnityEngine;

namespace GF
{
    public static class GameConst
    {
        //项目路径
        public static string PROJECT_PATH = ((string)typeof(StackTraceUtility).GetField("projectFolder", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic).GetValue(null)).Replace('\\', '/');

        //分析结果保存路径
        public static string PROFILER_PATH = Path.GetFullPath(Application.dataPath + "/../profiler");
        //日志文件路径
#if UNITY_EDITOR
        public static string LOG_PATH = Path.GetFullPath(Application.dataPath + "/../log");
#else
        public static string LOG_PATH = Path.GetFullPath(Application.persistentDataPath + "/log");
#endif
        public static string LUA_LIB_NAME = "lualib";
        public static string LUA_LOGIC_NAME = "lualogic";
    }
}

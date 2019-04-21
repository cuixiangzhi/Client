using System.IO;
using System.Reflection;
using UnityEngine;

namespace GameCore
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
        //资源包
        public static string PKG_ASSET_NAME = "package_asset";
        //数据包
        public static string PKG_DATA_NAME = "package_data";
        //脚本包
        public static string PKG_LUA_NAME = "package_lua";
        //启动包
        public static string PKG_START_NAME = "package_start";
    }
}

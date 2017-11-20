using UnityEditor;

namespace GameFrameWork
{
    public static class ExportManager
    {
        [MenuItem("工具/导出资源/脚本")]
        public static void Export_Lua()
        {
            ExportLua.Export();
        }

        [MenuItem("工具/导出资源/打包")]
        public static void Export_Package()
        {
            ExportPackage.Export();
        }
    }
}

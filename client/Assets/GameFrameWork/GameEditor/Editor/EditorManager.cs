using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace GameFrameWork
{
    public static class EditorManager
    {
        [MenuItem("工具/导出资源/LUA脚本")]
        public static void ExportLuaPackage()
        {
            ExportLua.Export();
        }

        [MenuItem("工具/导出资源/打包")]
        public static void ExportPackage()
        {

        }
    }
}

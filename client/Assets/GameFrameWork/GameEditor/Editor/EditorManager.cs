using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace GameFrameWork
{
    //编辑器代码不能直接调用运行时的模块
    public static class EditorManager
    {
        [MenuItem("工具/导出资源/LUA脚本")]
        public static void ExportLuaPackage()
        {
            ExportLua.Export();
        }
    }
}

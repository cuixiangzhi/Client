using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using System.Text;
using System;

namespace GameCore
{
    public sealed class ExportLua
    {
        private static string LUA_LIB_PATH = Path.GetFullPath("Assets/GameCore/GameLua/ToLua/Lua");
        private static string LUA_LOGIC_PATH = Path.GetFullPath("Assets/GameLogic/Lua");
        private static string LUA_JIT_PATH = Path.GetFullPath("Assets/../../../tools/luajit/bytecode.py");
        private static string LUA_EXP_PATH = Path.GetFullPath("Assets/../../../assets/export/bytes/lua");
        

        public static void Export()
        {
            if(Directory.Exists(LUA_EXP_PATH)) Directory.Delete(LUA_EXP_PATH, true);
            MakeByteCode(LUA_JIT_PATH, LUA_LIB_PATH + " " + LUA_EXP_PATH);
            MakeByteCode(LUA_JIT_PATH, LUA_LOGIC_PATH + " " + LUA_EXP_PATH);
			EditorUtility.DisplayDialog("提示", "导出完成", "确定");
        }

        private static void MakeByteCode(string command,string args)
        {
			Process process = Process.Start("python",command + " " + args);
            process.WaitForExit();
            process.Close();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace GF
{
    public sealed class ExportLua
    {
        private static string LUA_LIB_PATH = Path.GetFullPath("Assets/GameFrameWork/GameLua/ToLua/Lua");
        private static string LUA_LOGIC_PATH = Path.GetFullPath("Assets/GameLogic/Lua");

        private static string LUA32_JIT_PATH = Path.GetFullPath("Assets/../../tools/jit/luajit32/build.py");
        private static string LUA32_EXPORT_PATH = Path.GetFullPath("Assets/../../res/export/lua/32");

        private static string LUA64_JIT_PATH = Path.GetFullPath("Assets/../../tools/jit/luajit64/build.py");
        private static string LUA64_EXPORT_PATH = Path.GetFullPath("Assets/../../res/export/lua/64");

        public static void Export()
        {
            if (Directory.Exists(LUA32_EXPORT_PATH))
                Directory.Delete(LUA32_EXPORT_PATH, true);
            Export(LUA32_JIT_PATH, LUA_LIB_PATH + " " + LUA32_EXPORT_PATH);
            Export(LUA32_JIT_PATH, LUA_LOGIC_PATH + " " + LUA32_EXPORT_PATH);

            if (Directory.Exists(LUA64_EXPORT_PATH))
                Directory.Delete(LUA64_EXPORT_PATH, true);
            Export(LUA64_JIT_PATH, LUA_LIB_PATH + " " + LUA64_EXPORT_PATH);
            Export(LUA64_JIT_PATH, LUA_LOGIC_PATH + " " + LUA64_EXPORT_PATH);
        }

        private static void Export(string command,string args)
        {
            Process process = Process.Start(command, args);
            process.WaitForExit();
            process.Close();
        }
    }
}
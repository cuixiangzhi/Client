using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using System.Runtime.InteropServices;

namespace GF
{
    public sealed class ExportLua
    {
        private static string LUA_LIB_PATH = Path.GetFullPath("Assets/GameFrameWork/GameLua/ToLua/Lua");
        private static string LUA_LOGIC_PATH = Path.GetFullPath("Assets/GameLogic/Lua");
        private static string LUA_JIT_PATH = Path.GetFullPath("Assets/../../tools/luajit/bytecode.py");
        private static string LUA_TMP_PATH = Path.GetFullPath("Assets/../tmp");
        

        public static void ExportLib()
        {
            if(Directory.Exists(LUA_TMP_PATH))
            {
                Directory.Delete(LUA_TMP_PATH, true);
            }
            MakeByteCode(LUA_JIT_PATH, LUA_LIB_PATH + " " + LUA_TMP_PATH);
            //MakePackage(LUA_TMP_PATH,GameConst.LUA_LIB_NAME);
        }

        public static void ExportLogic()
        {
            if (Directory.Exists(LUA_TMP_PATH))
            {
                Directory.Delete(LUA_TMP_PATH, true);
            }
            MakeByteCode(LUA_JIT_PATH, LUA_LOGIC_PATH + " " + LUA_TMP_PATH);
            //MakePackage(LUA_TMP_PATH, GameConst.LUA_LOGIC_NAME);
        }

        private static void MakePackage(string path,string name)
        {
            FileStream package = new FileStream(Application.streamingAssetsPath + "/" + UtilDll.common_md5(name), FileMode.Create);
            //32位字节码写入文件并记录byte信息
            string[] files = Directory.GetFiles(path, "*.bytes", SearchOption.AllDirectories);
            List<FileByteData> fileInfo = new List<FileByteData>(files.Length);
            List<string> fileUUID = new List<string>(files.Length / 2);

            int offset = 0;
            for(int i = 0;i < files.Length;i++)
            {
                //读取数据并加密,写入包内
                byte[] tmp = File.ReadAllBytes(files[i]);
                UtilDll.common_encode(tmp, tmp.Length);
                package.Write(tmp, 0, tmp.Length);
                package.Flush();
                //记录字节信息
                string fileName = Path.GetFileNameWithoutExtension(files[i]).Replace("_32","");
                string uuid = UtilDll.common_md5(fileName);
                int index = fileUUID.IndexOf(uuid);
                FileByteData data = new FileByteData();
                if (files[i].EndsWith("32"))
                {
                    data.mOffset32 = offset;
                    data.mLength32 = tmp.Length;
                    data.mOffset64 = index < 0 ? 0 : fileInfo[index].mOffset64;
                    data.mLength64 = index < 0 ? 0 : fileInfo[index].mLength64;
                }
                else
                {
                    data.mOffset32 = index < 0 ? 0 : fileInfo[index].mOffset32;
                    data.mLength32 = index < 0 ? 0 : fileInfo[index].mLength32;
                    data.mOffset64 = offset;
                    data.mLength64 = tmp.Length;
                }
                if (index < 0)
                {                    
                    fileUUID.Add(uuid);
                    fileInfo.Add(data);
                }
                else
                {
                    fileInfo[index] = data;
                }
                offset += tmp.Length;             
            }
            //写入文件信息
            for(int i = 0;i < fileUUID.Count;i++)
            {

            }
        }

        private static void MakeByteCode(string command,string args)
        {
            Process process = Process.Start(command, args);
            process.WaitForExit();
            process.Close();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using System.Text;
using System;

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
            if(Directory.Exists(LUA_TMP_PATH)) Directory.Delete(LUA_TMP_PATH, true);
            MakeByteCode(LUA_JIT_PATH, LUA_LIB_PATH + " " + LUA_TMP_PATH);
            MakePackage(LUA_TMP_PATH,GameConst.LUA_LIB_NAME);
            if (Directory.Exists(LUA_TMP_PATH)) Directory.Delete(LUA_TMP_PATH, true);
        }

        public static void ExportLogic()
        {
            if (Directory.Exists(LUA_TMP_PATH)) Directory.Delete(LUA_TMP_PATH, true);
            MakeByteCode(LUA_JIT_PATH, LUA_LOGIC_PATH + " " + LUA_TMP_PATH);
            MakePackage(LUA_TMP_PATH, GameConst.LUA_LOGIC_NAME);
            if (Directory.Exists(LUA_TMP_PATH)) Directory.Delete(LUA_TMP_PATH, true);
        }

        private static void MakePackage(string path,string name)
        {
            FileStream package = new FileStream(Application.streamingAssetsPath + "/" + UtilDll.common_md5(name), FileMode.Create);
            //字节码写入文件并记录byte信息
            List<string> files = new List<string>(Directory.GetFiles(path, "*.bytes", SearchOption.AllDirectories));
            files.Sort((a,b)=>
            {
                string aName = Path.GetFileNameWithoutExtension(a).ToLower();
                string bName = Path.GetFileNameWithoutExtension(b).ToLower();
                if (aName != bName || a == b)
                    return aName.CompareTo(bName);
                throw new Exception("file name equal!\n" + a + "\n" + b);
            });
            List<FileByteData> fileInfo = new List<FileByteData>(files.Count);
            List<string> fileUUID = new List<string>(files.Count / 2);

            uint offset = 0;
            for(int i = 0;i < files.Count; i++)
            {
                //读取数据并加密,写入包内
                byte[] tmp = File.ReadAllBytes(files[i]);
                UtilDll.common_encode(tmp, tmp.Length);
                package.Write(tmp, 0, tmp.Length);
                //记录字节信息
                string fileName = Path.GetFileNameWithoutExtension(files[i]).Replace("_32","").Replace("_64","");
                string uuid = UtilDll.common_md5(fileName);
                int index = fileUUID.IndexOf(uuid);
                FileByteData data = new FileByteData();
                if (files[i].Contains("_32.bytes"))
                {
                    data.mOffset32 = offset;
                    data.mLength32 = (uint)tmp.Length;
                    data.mOffset64 = index < 0 ? 0 : fileInfo[index].mOffset64;
                    data.mLength64 = index < 0 ? 0 : fileInfo[index].mLength64;
                }
                else
                {
                    data.mOffset32 = index < 0 ? 0 : fileInfo[index].mOffset32;
                    data.mLength32 = index < 0 ? 0 : fileInfo[index].mLength32;
                    data.mOffset64 = offset;
                    data.mLength64 = (uint)tmp.Length;
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
                offset += (uint)tmp.Length;             
            }
            //写入文件信息
            byte[] file_byte_info = new byte[52];
            for (int i = 0;i < fileUUID.Count;i++)
            {
                Encoding.UTF8.GetBytes(fileUUID[i],0,fileUUID[i].Length,file_byte_info,0);
                ByteUtil.ToBytes(file_byte_info, fileUUID[i].Length, fileInfo[i].mOffset32);
                ByteUtil.ToBytes(file_byte_info, fileUUID[i].Length + 4, fileInfo[i].mLength32);
                ByteUtil.ToBytes(file_byte_info, fileUUID[i].Length + 8, fileInfo[i].mOffset64);
                ByteUtil.ToBytes(file_byte_info, fileUUID[i].Length + 12, fileInfo[i].mLength64);
                UtilDll.common_encode(file_byte_info, file_byte_info.Length);
                package.Write(file_byte_info,0,file_byte_info.Length);
            }
            //写入文件个数
            ByteUtil.ToBytes(file_byte_info, 0, (uint)fileUUID.Count);
            package.Write(file_byte_info, 0, 4);
            package.Flush();
            package.Close();
        }

        private static void MakeByteCode(string command,string args)
        {
            Process process = Process.Start(command, args);
            process.WaitForExit();
            process.Close();
        }
    }
}
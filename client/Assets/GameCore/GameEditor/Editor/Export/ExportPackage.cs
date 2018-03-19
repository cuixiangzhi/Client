using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GameCore
{
    public sealed class ExportPackage
    {
        private static string EXPORT_PATH = Path.GetFullPath("Assets/StreamingAssets/");
        private static string BUNDLE_PATH = Path.GetFullPath("Assets/../../../assets/export/bundles");
        private static string BYTES_PATH = Path.GetFullPath("Assets/../../../assets/export/bytes");

        public static void Export()
        {
            ExportAllBundles();
            ExportAllBytes();
            ExportFileMap();
            EditorUtility.DisplayDialog("提示", "打包完成", "确定");
        }

        private static void ExportAllBundles()
        {
            //FileStream package = new FileStream(EXPORT_PATH + "/" + UtilDll.common_md5(GameConst.PACKAGE_NAME), FileMode.Create);
            //FileStream filemap = new FileStream(EXPORT_PATH + "/" + UtilDll.common_md5(GameConst.FILEMAP_NAME), FileMode.Create);
            //uint offset = 0;
            //List<string> files = GetFiles(BUNDLE_PATH);
            //byte[] file_byte_info = new byte[44];
            //for (int i = 0; i < files.Count; i++)
            //{
            //    //写入随机长度的加密字符串
            //    int random = UnityEngine.Random.Range(1, 36);
            //    string randomuuid = UtilDll.common_md5(random.ToString());
            //    Encoding.UTF8.GetBytes(randomuuid, 0, random, file_byte_info,0);
            //    UtilDll.common_encode(file_byte_info,random);
            //    offset += (uint)random;
            //    package.Write(file_byte_info,0,random);
            //    //写入AB数据
            //    byte[] bytes = File.ReadAllBytes(files[i]);
            //    package.Write(bytes, 0, bytes.Length);
            //    //写入偏移信息
            //    string uuid = UtilDll.common_md5(files[i]);
            //    Encoding.UTF8.GetBytes(uuid, 0, uuid.Length, file_byte_info, 0);
            //    UtilByte.ToBytes(file_byte_info, uuid.Length, offset);
            //    UtilByte.ToBytes(file_byte_info, uuid.Length + 4, (uint)bytes.Length);
            //    filemap.Write(file_byte_info,0,file_byte_info.Length);
            //    offset += (uint)bytes.Length;
            //}
            //package.Close();
            //filemap.Close();
        }

        private static void ExportAllBytes()
        {
            //FileStream package = new FileStream(EXPORT_PATH + "/" + UtilDll.common_md5(GameConst.PACKAGE_NAME), FileMode.Append);
            //FileStream filemap = new FileStream(EXPORT_PATH + "/" + UtilDll.common_md5(GameConst.FILEMAP_NAME), FileMode.Append);
            //uint offset = 0;
            //List<string> files = GetFiles(BYTES_PATH);
            //byte[] file_byte_info = new byte[44];
            //for (int i = 0; i < files.Count; i++)
            //{
            //    //写入加密后的数据
            //    byte[] bytes = File.ReadAllBytes(files[i]);
            //    UtilDll.common_encode(bytes, bytes.Length);
            //    package.Write(bytes, 0, bytes.Length);
            //    //写入偏移信息
            //    string uuid = UtilDll.common_md5(files[i].Replace("_32","").Replace("_64",""));
            //    Encoding.UTF8.GetBytes(uuid, 0, uuid.Length, file_byte_info, 0);
            //    UtilByte.ToBytes(file_byte_info, uuid.Length, offset);
            //    UtilByte.ToBytes(file_byte_info, uuid.Length + 4, (uint)bytes.Length);
            //    filemap.Write(file_byte_info, 0, file_byte_info.Length);
            //    offset += (uint)bytes.Length;
            //}
            //package.Close();
            //filemap.Close();
        }

        private static void ExportFileMap()
        {
            ////加密偏移信息
            //string fileMapPath = EXPORT_PATH + "/" + UtilDll.common_md5(GameConst.FILEMAP_NAME);
            //byte[] bytes = File.ReadAllBytes(fileMapPath);
            //UtilDll.common_encode(bytes, bytes.Length);
            //FileStream filemap = new FileStream(fileMapPath, FileMode.Create);
            //filemap.Write(bytes, 0, bytes.Length);
            //filemap.Close();
        }

        private static List<string> GetFiles(string path)
        {
            if(!Directory.Exists(path))
            {
                return new List<string>();
            }
            List<string> files = new List<string>(Directory.GetFiles(path, "*", SearchOption.AllDirectories));
            files.Sort((a, b) =>
            {
                string aName = Path.GetFileNameWithoutExtension(a).ToLower();
                string bName = Path.GetFileNameWithoutExtension(b).ToLower();
                if (aName != bName || a == b)
                    return aName.CompareTo(bName);
                throw new Exception("file name equal!\n" + a + "\n" + b);
            });
            return files;
        }
    }
}

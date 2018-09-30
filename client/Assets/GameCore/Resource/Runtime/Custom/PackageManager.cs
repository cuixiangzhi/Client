using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ResConfig;
using System.IO;
using GameCore;
using LycheeSDK;
using cyou.ldj.sdk;

namespace GameCore
{
    public class PackageManager
    {
        private Dictionary<string, BytesConfig> mBytesInfoDic = null;
        private static readonly string PACKAGE_NAME = "bundle_package";
        private byte[] mByteBuffer = null;
        private FileManager mFileManager = null;
        private Stream mFileStream = null;

        public PackageManager(FileManager fileManager)
        {
            mFileManager = fileManager;
            mByteBuffer = new byte[1024 * 1024];
            mBytesInfoDic = new Dictionary<string, BytesConfig>(2048);
        }

        private void InitPackageInfo()
        {
            if (mBytesInfoDic != null && mBytesInfoDic.Count != 0) return;
            if (mFileManager.Exists(Location.Download, PACKAGE_NAME))
            {
                mFileStream = mFileManager.Open(Location.Download, PACKAGE_NAME, FileMode.Open, FileAccess.Read);
            }
            else if (mFileManager.Exists(Location.Initial, PACKAGE_NAME))
            {
                mFileStream = mFileManager.Open(Location.Initial, PACKAGE_NAME, FileMode.Open, FileAccess.Read);
            }
            else
            {
                Debug.LogError("bytes package not exists! " + PACKAGE_NAME);
            }
            if (mFileStream != null)
            {
                mFileStream.Seek(-4, SeekOrigin.End);
                mFileStream.Read(mByteBuffer, 0, 4);
                byte a = mByteBuffer[0];
                byte b = mByteBuffer[1];
                byte c = mByteBuffer[2];
                byte d = mByteBuffer[3];
                int dataLength = (a << 24) | (b << 16) | (c << 8) | d;
                int fileLength = (int)mFileStream.Length;
                int configLength = fileLength - dataLength - 4;
                mFileStream.Seek(dataLength, SeekOrigin.Begin);
                if(mByteBuffer.Length <= configLength)
                {
                    mByteBuffer = new byte[configLength];
                    Debug.LogError("config data length too large: " + configLength / 1024 + " KB");
                }
                mFileStream.Read(mByteBuffer, 0, configLength);

                using (MemoryStream ms = new MemoryStream(mByteBuffer, true))
                {
                    ms.SetLength(configLength);
                    AllBytesConfig allConfigs = ProtoBuf.Serializer.Deserialize<AllBytesConfig>(ms);
                    for (int i = 0; i < allConfigs.datas.Count; i++)
                    {
                        BytesConfig config = allConfigs.datas[i];
                        mBytesInfoDic[config.name] = config;
                    }
                }
            }
        }

        public void ClearPackageInfo()
        {
            mBytesInfoDic = null;
            mByteBuffer = null;
            mFileManager = null;
            if (mFileStream != null) mFileStream.Close();
            mFileStream = null;
        }

        public static string CreatePackage(string bytesRootPath, string luaRootPath, string packagePath)
        {
            string[] allBytes = Directory.GetFiles(bytesRootPath, "*.bytes", SearchOption.AllDirectories);
            string[] allLuas = Directory.GetFiles(luaRootPath, "*.lua", SearchOption.AllDirectories);
            packagePath += "/" + PACKAGE_NAME;
            AllBytesConfig allConfigs = new AllBytesConfig();
            FileStream packageStream = new FileStream(packagePath, FileMode.OpenOrCreate, FileAccess.Write);

            int allDataLength = 0;
            byte[] buffer = new byte[1024 * 1024 * 4];

            for (int i = 0; i < allBytes.Length; i++)
            {
                FileStream fs = File.OpenRead(allBytes[i]);
                int dataLength = fs.Read(buffer, 0, buffer.Length);
                packageStream.Write(buffer, 0, dataLength);
                fs.Close();

                BytesConfig config = new BytesConfig();
                config.name = Path.GetFileName(allBytes[i]);
                config.offset = allDataLength;
                config.length = dataLength;
                allConfigs.datas.Add(config);

                allDataLength += dataLength;
            }

            for (int i = 0; i < allLuas.Length; i++)
            {
                FileStream fs = File.OpenRead(allLuas[i]);
                int dataLength = fs.Read(buffer, 0, buffer.Length);
                packageStream.Write(buffer, 0, dataLength);
                fs.Close();

                BytesConfig config = new BytesConfig();
                config.name = allLuas[i].Replace("\\", "/").Replace("Assets/", "");
                config.offset = allDataLength;
                config.length = dataLength;
                allConfigs.datas.Add(config);

                allDataLength += dataLength;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms,allConfigs);
                packageStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
            }

            buffer[0] = (byte)((allDataLength & 0xFF000000) >> 24);
            buffer[1] = (byte)((allDataLength & 0x00FF0000) >> 16);
            buffer[2] = (byte)((allDataLength & 0x0000FF00) >> 8);
            buffer[3] = (byte)((allDataLength & 0x000000FF));
            packageStream.Write(buffer, 0, 4);
            packageStream.Close();

            return packagePath;
        }

        public LuaByteBuffer ReadFromPackage(string fileName)
        {
            if (!GameConst.READ_BYTES_FROM_BUNDLE)
            {
                string fullPath = string.Empty;
                if (fileName.EndsWith(".bytes"))
                {
                    fullPath = string.Format("{0}/Res/Data/{1}", Application.dataPath, fileName);
                }
                else if(fileName.EndsWith(".lua"))
                {
                    fullPath = string.Format("{0}/{1}", LuaConst.luaDir, fileName);
                    if(!File.Exists(fullPath)) fullPath = string.Format("{0}/{1}", LuaConst.toluaDir, fileName);
                }
                if (File.Exists(fullPath))
                {
                    FileStream fs = File.OpenRead(fullPath);
                    int length = (int)fs.Length;
                    if (fs.Length > mByteBuffer.Length)
                    {
                        mByteBuffer = new byte[fs.Length];
                        Debug.LogError("file size too large,consider other load type " + fileName);
                    }
                    fs.Read(mByteBuffer, 0, length);
                    return new LuaByteBuffer(mByteBuffer, length);
                }
                else
                {
                    if(fullPath.EndsWith(".bytes")) UnityEngine.Debug.LogError("file not exists " + fullPath);
                    return new LuaByteBuffer(null, 0);
                }
            }
            else
            {
                InitPackageInfo();
                BytesConfig config = null;
                if (mFileStream != null)
                {
                    if(fileName.EndsWith(".lua"))
                    {
                        string fullPath = string.Format("Lua/{0}", fileName);
                        if(mBytesInfoDic.ContainsKey(fullPath))
                        {
                            fileName = fullPath;
                        }
                        else
                        {
                            fileName = string.Format("Lua/Logic/Framework/ToLua/{0}", fileName);
                        }
                    }
                    if(mBytesInfoDic.TryGetValue(fileName, out config))
                    {
                        if (config.length > mByteBuffer.Length)
                        {
                            mByteBuffer = new byte[config.length];
                            Debug.LogError("file size too large,consider other load type " + fileName);
                        }
                        mFileStream.Seek(config.offset, SeekOrigin.Begin);
                        mFileStream.Read(mByteBuffer, 0, config.length);
                        return new LuaByteBuffer(mByteBuffer, config.length);
                    }
                }
                if(fileName.EndsWith(".bytes")) UnityEngine.Debug.LogError("file not exists " + fileName);
                return new LuaByteBuffer(null, 0);
            }
        }
    }
}

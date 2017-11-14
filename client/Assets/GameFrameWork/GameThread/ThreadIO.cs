using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using LuaInterface;

namespace GameFrameWork
{
    public sealed class ThreadIO : ThreadBase
    {
#if UNITY_EDITOR
        private string persistentPath = Application.dataPath + "/../";
#else
        private string persistentPath = Application.persistentDataPath + "/";
#endif
        //LUA脚本包信息
        private byte[] mBuffer = new byte[1024 * 1024];
        private FileStream mLuaPackageFile = null;
        private Dictionary<int, FileByteData> mLuaPackageData = new Dictionary<int, FileByteData>(1024);

        public void Init()
        {
            if (File.Exists(persistentPath + UtilDll.common_md5(GameConst.PACKAGE_LUA_NAME)))
            {
                mLuaPackageFile = File.OpenRead(persistentPath + UtilDll.common_md5(GameConst.PACKAGE_LUA_NAME));
            }
            else
            {
                mLuaPackageFile = File.OpenRead(Application.streamingAssetsPath + "/" + UtilDll.common_md5(GameConst.PACKAGE_LUA_NAME));
            }
            if(mLuaPackageFile != null)
            {
                mLuaPackageFile.Seek(4, SeekOrigin.End);
                mLuaPackageFile.Read(mBuffer, 0, 4);
                int fileDataLength = (int)ByteUtil.ToUInt32(mBuffer, 0) * 52;
                mLuaPackageFile.Seek(fileDataLength, SeekOrigin.Current);
                mLuaPackageFile.Read(mBuffer, 0, fileDataLength);
                for(int i = 0;i < mBuffer.Length;i += 52)
                {
                    string md5 = Encoding.UTF8.GetString(mBuffer, i, 36);
                    uint offset32 = ByteUtil.ToUInt32(mBuffer, i + 36);
                    uint length32 = ByteUtil.ToUInt32(mBuffer, i + 36);
                    uint offset64 = ByteUtil.ToUInt32(mBuffer, i + 36);
                    uint length64 = ByteUtil.ToUInt32(mBuffer, i + 36);
                }
            }
        }

        public void Loop()
        {

        }

        public void Exit()
        {

        }

        public bool CanRunInThread()
        {
            return false;
        }

        public static void LoadBundle(string path,Action<string,AssetBundle> callBack, bool sync)
        {

        }

        public static LuaByteBuffer LoadBytes(string path, Action<string,LuaByteBuffer> callBack, bool sync)
        {
            return null;
        }
    }
}

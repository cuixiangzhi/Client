using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

namespace GameCore
{
    public struct ByteData
    {
        public uint mOffset;
        public uint mLength;

        public ByteData(uint offset,uint length)
        {
            mOffset = offset;
            mLength = length;
        }
    }

    public class FileMap
    {
        private Dictionary<string, ByteData> mOldFile32 = null;
        private Dictionary<string, ByteData> mOldFile64 = null;
        private Dictionary<string, ByteData> mNewFile32 = null;
        private Dictionary<string, ByteData> mNewFile64 = null;
        private ByteData mNullByteData = new ByteData(0,0);
        private int TOTAL_FILE_COUNT = 1024 * 4;

        public FileMap()
        {
            mOldFile32 = new Dictionary<string, ByteData>(TOTAL_FILE_COUNT);
            mOldFile64 = new Dictionary<string, ByteData>(TOTAL_FILE_COUNT);
            mNewFile32 = new Dictionary<string, ByteData>(TOTAL_FILE_COUNT);
            mNewFile64 = new Dictionary<string, ByteData>(TOTAL_FILE_COUNT);
        }

        private void CreateFileMap(byte[] data,int len,bool old)
        {
            Dictionary<string, ByteData> map32 = old ? mOldFile32 : mNewFile32;
            Dictionary<string, ByteData> map64 = old ? mOldFile64 : mNewFile64;
            DllMgr.common_decode(data, len);
            for (int i = 0; i < len; i += 44)
            {
                string name = Encoding.UTF8.GetString(data, i, 36);
                uint offset = ByteUtil.ToUInt32(data, i + 36);
                uint length = ByteUtil.ToUInt32(data, i + 40);
                if(map32.ContainsKey(name))
                {
                    map64[name] = new ByteData(offset,length);
                }
                else
                {
                    map32[name] = new ByteData(offset, length);
                }
            }
        }

        public void CreateOldFileMap(byte[] data,int len)
        {
            CreateFileMap(data,len,true);
        }

        public void CreateNewFileMap(byte[] data,int len)
        {
            CreateFileMap(data, len,false);
        }

        public bool IsNewFile(string path)
        {
            string name = DllMgr.common_md5(path);
            return (IntPtr.Size == 8 && mNewFile64.ContainsKey(name)) || (mNewFile32.ContainsKey(name));
        }

        public ByteData GetByteData(string path)
        {
            string name = DllMgr.common_md5(path);
            if (IntPtr.Size == 8 && mNewFile64.ContainsKey(name))
            {
                return mNewFile64[name];
            }
            if (IntPtr.Size == 8 && mOldFile64.ContainsKey(name))
            {
                return mOldFile64[name];
            }
            if (mNewFile32.ContainsKey(name))
            {
                return mNewFile32[name];
            }
            if (mOldFile32.ContainsKey(name))
            {
                return mOldFile32[name];
            }
            return mNullByteData;
        }
    }

    public class UIData
    {
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameWork
{
    public static class ByteUtil
    {
        public static void ToBytes(byte[] buffer,int offset,uint data)
        {
            buffer[offset + 0] = (byte)(data >> 24 & 0x8);
            buffer[offset + 1] = (byte)(data >> 16 & 0x8);
            buffer[offset + 2] = (byte)(data >> 8 & 0x8);
            buffer[offset + 3] = (byte)(data >> 0 & 0x8);
        }

        public static uint ToUInt32(byte[] data,int offset)
        {
            uint ret = 0;
            ret |= (uint)(data[offset + 0]) << 24;
            ret |= (uint)(data[offset + 1]) << 16;
            ret |= (uint)(data[offset + 2]) << 8;
            ret |= (uint)(data[offset + 3]) << 0;
            return ret;
        }

        public static void WriteBytes(byte[] dst,int offset,byte[] src)
        {
            for(int i = 0;i < src.Length;i++)
            {
                dst[offset + i] = src[i];
            }
        }
    }
}

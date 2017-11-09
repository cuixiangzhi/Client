using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityObj = UnityEngine.Object;

namespace GF
{
    public struct FileByteData
    {
        public uint mOffset32;
        public uint mLength32;
        public uint mOffset64;
        public uint mLength64;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public sealed class CpkMgr : BaseMgr<CpkMgr>
    {
        public AssetBundle LoadBundle(string fileName)
        {
            throw new Exception("undefine api");
        }

        public int LoadBytes(string fileName, byte[] buffer)
        {
            throw new Exception("undefine api");
        }

        public void UnloadBundle(string fileName)
        {
            throw new Exception("undefine api");
        }

        public void UnloadDependBundle(string fileName)
        {
            throw new Exception("undefine api");
        }
    }
}


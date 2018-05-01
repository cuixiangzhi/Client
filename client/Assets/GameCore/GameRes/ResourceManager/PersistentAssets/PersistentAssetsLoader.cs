/********************************************************************************
 *	创建人：	 李彬
 *	创建时间：   2016-05-12
 *
 *	功能说明：  
 *	
 *	修改记录：
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Games.TLBB.Manager.IO
{
    class PersistentAssetsLoader
    {
        public byte[] Load(string name)
        {
            string fullPath = ResourceManager.PersistentAssetsPath + name;
            if (File.Exists(fullPath))
                return File.ReadAllBytes(fullPath);
            else
                return null;
//             if( File.Exists(fullPath))
//             {
//                 FileStream fs = File.OpenRead(m_strPersistentAssetsPath + name);
//                 if (fs != null)
//                 {
//                     byte[] bt = null;
//                     if (fs.Length > 0)
//                     {
//                         bt = new byte[fs.Length];
//                         fs.Read(bt, 0, (Int32)fs.Length);
//                     }
//                     fs.Close();
//                     fs.Dispose();
//                     return bt;
//                 }
//                 else
//                 {
//                     return null;
//                 }
//             }
//             else
//             {
//                 return null;
//             }
        }
    }
}

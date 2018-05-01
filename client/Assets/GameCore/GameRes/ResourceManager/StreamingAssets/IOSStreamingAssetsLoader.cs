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
    class IOSStreamingAssetsLoader : StreamingAssetsLoader
    {
        private string m_streamingAssetsPath = Application.streamingAssetsPath + "/";

        public override byte[] Load(string name, Int32 offset, Int32 length)
        {
            String strFullPath = m_streamingAssetsPath + name;
            if (File.Exists(strFullPath) == false)
                return null;

            FileStream fin = File.OpenRead(strFullPath);
            if (fin != null)
            {
                fin.Seek(offset, SeekOrigin.Begin);
                Byte[] dateByte = new Byte[length];
                fin.Read(dateByte, 0, length);
                fin.Close();
                fin.Dispose();
                return dateByte;
            }
            else
            {
                return null;
            }
        }

        public override byte[] Load(string name)
        {
            String strFullPath = m_streamingAssetsPath + name;
            if (File.Exists(strFullPath) == false)
                return null;
            return System.IO.File.ReadAllBytes(strFullPath);
//             if (bydata == null || bydata.Length == 0)
//             {
//                 return null;
//             }
//             else
//             {
//                 return bydata;
//             }
        }
    }

}

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
using UnityEngine;

namespace Games.TLBB.Manager.IO
{
    public abstract class StreamingAssetsLoader
    {
        public String StreamingAssetsPath
        {
            get
            {
                return m_strStreamingAssetsPath;
            }
        }
        protected static String m_strStreamingAssetsPath;
        static StreamingAssetsLoader()
        {
// #if UNITY_EDITOR
// 	    string streamingAssetsPath = Application.dataPath +"/StreamingAssets"; //file io
// #elif UNITY_IPHONE
// 	    string streamingAssetsPath = Application.dataPath +"/Raw"; //file io
// #elif UNITY_ANDROID
// 	    string streamingAssetsPath = 
// #endif
            if ( Application.platform == RuntimePlatform.Android)
            {
                
                //m_strStreamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets/";  //www
                m_strStreamingAssetsPath = Application.dataPath + "!assets/";
            }
            else
            {
                m_strStreamingAssetsPath = Application.streamingAssetsPath + "/";
            }
        }
        public abstract byte[] Load(string name);
        public abstract byte[] Load(string name, Int32 offset, Int32 length);

    }

}

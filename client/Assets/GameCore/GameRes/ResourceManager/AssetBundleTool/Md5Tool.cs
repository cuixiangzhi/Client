/********************************************************************************
 *	创建人：	 
 *	创建时间：   2015-06-11   作废
 *
 *	功能说明： 测试使用
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

namespace GetMD5
{
	public class Md5Tool 
	{
		public static string GetFileHash(string path)
		{
			string fileMd5 = "";
			try
			{
				FileStream f = new FileStream(path, FileMode.Open,FileAccess.Read);
				int length = (int)f.Length;
				byte[] data = new byte[length];
				f.Read(data,0,length);
				f.Close();
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] result = md5.ComputeHash(data);
				foreach(byte b in result)
				{
					fileMd5 += Convert.ToString(b,16);
				}
			}
			catch(FileNotFoundException ex)
			{
				Debug.Log(ex.Message);
			}
			return fileMd5;
		}
	}
}


/********************************************************************************
 *	创建人：	 
 *	创建时间：   2015-06-11   作废
 *
 *	功能说明： 测试使用
 *	
 *	修改记录：
*********************************************************************************/
using UnityEngine;
using System.Collections;

namespace GetMD5
{
	public class BundleInfoJson
	{
		public string ResourceName;
		public string md5;
	}

	public class BundleVersionInfoJson
	{
		public string ResourceName;
		public int version;
	}

    public class TotalBundleJson
    {
        public string ResourceName;
        public string md5;
        public int version;
    }
}

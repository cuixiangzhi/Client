using System;
using UnityEngine;

namespace GameCore
{
	public class BaseMgr<T> where T:class,new()
	{
		private static T mInstance = null;

		public static T Instance
		{
			get
			{
                if(mInstance == null)
                {
                    mInstance = new T();
                }
                return mInstance;
            }	
		}
	}
}


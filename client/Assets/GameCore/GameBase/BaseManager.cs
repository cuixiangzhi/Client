using System;
using UnityEngine;

namespace GameCore
{
	public class BaseManager<T> : MonoBehaviour where T:class
	{
		private T mInstance = null;

		public T Instance
		{
			get
			{
				if (mInstance != null)
				{
					return mInstance;
				} 
				else
				{
					GameObject go = new GameObject (typeof(T).Name);	
				}
			}	
		}
	}
}


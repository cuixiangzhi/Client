using System;
using UnityEngine;

namespace GameCore
{
	public class BaseMgr<T> : MonoBehaviour where T:Component
	{
		private T mInstance = null;

        private static GameObject GAME_MAIN = null;

		public T Instance
		{
			get
			{
                if(mInstance == null)
                {
                    if(GAME_MAIN == null)
                    {
                        GAME_MAIN = GameObject.Find("GAME_MAIN");
                    }
                    mInstance = GAME_MAIN.AddComponent(typeof(T)) as T;
                }
                return mInstance;
            }	
		}
	}
}


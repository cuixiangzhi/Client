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

        private GameObject mOwner;

        public virtual void Init(GameObject owner)
        {
            mOwner = owner;
        }

        public virtual void Update()
        {

        }

        public virtual void LateUpdate()
        {

        }

        public virtual void FixedUpdate()
        {

        }

        public virtual void Exit()
        {
            mOwner = null;
        }
	}
}


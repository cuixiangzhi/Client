using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public class BTBaseWindow
	{
		protected int mID;
		protected Rect mRect;
		protected string mName;
		
		public void OnEnable()
		{
			OnInit();
		}

		public void OnGUI(int id,Rect rect,string windowName)
		{
			mID = id;
			mRect = rect;
			mName = windowName;
			if (id > 0)
			{
				GUI.Window (id, rect, OnDraw, windowName);
			}
		}

		public void OnDisable()
		{
			OnExit();
		}

		protected virtual void OnInit()
		{
			
		}

		protected virtual void OnDraw(int id)
		{

		}

		protected virtual void OnExit()
		{
		}
	}
}


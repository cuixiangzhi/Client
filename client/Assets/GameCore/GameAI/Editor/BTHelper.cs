using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
    public static class BTHelper
    {
		
		public static readonly float WINDOW_MIN_FLOAT = 0.001f;

		#region NODE窗口配置

		public static Rect NodeWindowRect()
		{
			return new Rect(Screen.width - 190, 0, 190, Screen.height);
		}

		public static Rect NodeWindowClipRect()
		{
			return new Rect(0, 18, 190, Screen.height - 48);
		}

		public static Rect NodeWindowScrollRect(float nodeHeight,float clipOffset,float clipHeight)
		{
			float scrollOffset = 22;
			float scrollHeight = 0;
			if(nodeHeight > clipHeight)
			{
				scrollHeight = clipHeight * clipHeight / nodeHeight;
				scrollOffset = (clipHeight - scrollHeight) * (- clipOffset / (nodeHeight - clipHeight)) + 22;
			}
			else
			{
				scrollHeight = clipHeight;
			}
			return new Rect(180, scrollOffset, 6, scrollHeight);
		}

		public static Rect NodeWindowBoxRect()
		{
			return new Rect(0,0,190,Screen.height);
		}

		public static float NodeWindowScrollDelta()
		{
			return Event.current.delta.y * 15;
		}

		#endregion

		#region NODE结点配置



		#endregion

		public static void DrawRect(Rect rect,string content)
		{
			Handles.BeginGUI();
			Vector2 lt = new Vector2(rect.xMin, rect.yMax);
			Vector2 lb = new Vector2(rect.xMin, rect.yMin);
			Vector2 rt = new Vector2(rect.xMax, rect.yMax);
			Vector2 rb = new Vector2(rect.xMax, rect.yMin);
			Handles.DrawAAPolyLine(1, lt, rt, rb, lb, lt);
			GUI.Box(rect, content);
			Handles.EndGUI();
		}
    }
}

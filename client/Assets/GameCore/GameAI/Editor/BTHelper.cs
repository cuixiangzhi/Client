using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
    public static class BTHelper
    {
		
		public static float WINDOW_MIN_FLOAT { get { return 0.001f; } }

		public static Rect NODE_WINDOW_RECT { get { return new Rect (Screen.width - 190, 0, 190, Screen.height);  } }

		public static Rect NODE_WINDOW_CLIP_RECT { get { return new Rect(0, 18, 190, Screen.height - 48); } }

		public static Rect NODE_WINDOW_BOX_RECT { get { return new Rect(0,0,190,Screen.height); } }

		public static float NODE_WINDOW_SCROLL_DELTA { get { return Event.current.delta.y * 15; } }

		public static Rect NODE_WINDOW_SCROLL_RECT(float nodeHeight,float clipOffset,float clipHeight)
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

		private static GUIStyle _NODE_GROUP_STYLE = null;
		public static GUIStyle NODE_GROUP_STYLE 
		{ 
			get 
			{ 
				if(_NODE_GROUP_STYLE == null) 
				{ 
					_NODE_GROUP_STYLE = new GUIStyle(GUI.skin.box);
					_NODE_GROUP_STYLE.normal.textColor = Color.cyan;
					_NODE_GROUP_STYLE.fontSize = 16;
				}
				return _NODE_GROUP_STYLE;
			} 
		}

		private static GUIStyle _NODE_CHILD_STYLE = null;
		public static GUIStyle NODE_CHILD_STYLE
		{
			get
			{
				if (_NODE_CHILD_STYLE == null)
				{
					_NODE_CHILD_STYLE = new GUIStyle (GUI.skin.box);
					_NODE_CHILD_STYLE.normal.textColor = Color.white;
					_NODE_CHILD_STYLE.fontSize = 13;
				}
				return _NODE_CHILD_STYLE;
			}
		}

		private static GUIStyle _NODE_DRAG_STYLE = null;
		public static GUIStyle NODE_DRAG_STYLE
		{
			get
			{
				if (_NODE_DRAG_STYLE == null)
				{
					_NODE_DRAG_STYLE = new GUIStyle (GUI.skin.box);
					_NODE_DRAG_STYLE.normal.textColor = Color.green;
					_NODE_DRAG_STYLE.fontStyle = FontStyle.BoldAndItalic;
					_NODE_DRAG_STYLE.fontSize = 13;
				}
				return _NODE_DRAG_STYLE;
			}
		}

		public static Rect NODE_GROUP_RECT(float x,float y)
		{
			return new Rect(x, y + 4, 178, 24);
		}

		public static float NODE_GROUP_HEIGHT { get { return 24; } } 

		public static float NODE_GROUP_SPACE { get { return 4; } }

		public static float NODE_GROUP_HEIGHT_WITH_SPACE { get { return 28; } } 

		public static float NODE_CHILD_HEIGHT_WITH_SPACE { get { return 24; } }

		public static float NODE_CHILD_WIDTH { get { return 150; } } 

		public static float NODE_CHILD_HEIGHT { get { return 20; } } 

		public static float NODE_CHILD_OFFSET { get { return 15; } }

		public static float NODE_HEIGHT_OFFSET { get { return 18; }}

		public static Rect NODE_DRAG_RECT { get { return new Rect (0, 0, 130, 20); } }

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

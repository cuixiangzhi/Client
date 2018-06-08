using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
    public static class BTHelper
    {
		
		public static float WINDOW_MIN_FLOAT { get { return 0.001f; } }

        #region NODE WINDOW

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

        #endregion

        #region NODE LIST

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

        #region ZOOM WINDOW

        public static Rect ZOOM_WINDOW_RECT(float scale) { return new Rect(190, 22, (Screen.width - 380) / scale, (Screen.height - 22) / scale); }

        public static Vector2 ZOOM_WINDOW_CENTER_CUR(float scale) { return new Vector2((Screen.width - 380) / scale / 2, (Screen.height - 22) / scale / 2); }

        public static Vector2 ZOOM_WINDOW_CENTER_RAW { get { return new Vector2((Screen.width - 380) / 2, (Screen.height - 22) / 2); } }

        public static Matrix4x4 ZOOM_WINDOW_TRS(Rect rect, float scale)
        {
            Matrix4x4 tM = Matrix4x4.Translate(new Vector3(190, 22, 0));
            Matrix4x4 sM = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            return tM * sM * tM.inverse;
        }

        public static Rect ZOOM_WINDOW_OLD_RECT { get { return new Rect(0, 22, Screen.width, Screen.height - 22); } }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.AI.Editor
{
    public class BTNodeBase : ScriptableObject
    {
        public static GUIStyle NODE_GROUP_STYLE = null;
        public static int NODE_START_OFFSET = 18;
        public static int NODE_GROUP_WIDTH = 178;
        public static int NODE_GROUP_SPACE = 4;
        public static int NODE_GROUP_HEIGHT = 24;

        public static GUIStyle NODE_CHILD_STYLE = null;
        public static int NODE_CHILD_WIDTH = 150;
        public static int NODE_CHILD_HEIGHT = 20;
        public static int NODE_CHILD_OFFSET = 15;

        public static GUIStyle NODE_DRAG_STYLE = null;
        public static int NODE_DRAG_WIDTH = 130;
        public static int NODE_DRAG_HEIGHT = 20;

        public string mNodeName;

        public virtual void OnPreDraw()
        {
            if (NODE_GROUP_STYLE == null)
            {
                //设置结点组格式
                NODE_GROUP_STYLE = new GUIStyle(GUI.skin.box);
                NODE_GROUP_STYLE.alignment = TextAnchor.MiddleCenter;
                NODE_GROUP_STYLE.normal.textColor = Color.cyan;
                NODE_GROUP_STYLE.fontStyle = FontStyle.Normal;
                NODE_GROUP_STYLE.fontSize = 16;
                NODE_GROUP_STYLE.wordWrap = false;
            }
            if (NODE_CHILD_STYLE == null)
            {
                NODE_CHILD_STYLE = new GUIStyle(GUI.skin.box);
                NODE_CHILD_STYLE.alignment = TextAnchor.MiddleCenter;
                NODE_CHILD_STYLE.normal.textColor = Color.white;
                NODE_CHILD_STYLE.fontStyle = FontStyle.Normal;
                NODE_CHILD_STYLE.fontSize = 13;
                NODE_CHILD_STYLE.wordWrap = false;
            }
            if (NODE_DRAG_STYLE == null)
            {
                NODE_DRAG_STYLE = new GUIStyle(GUI.skin.box);
                NODE_DRAG_STYLE.alignment = TextAnchor.MiddleCenter;
                NODE_DRAG_STYLE.normal.textColor = Color.green;
                NODE_DRAG_STYLE.fontStyle = FontStyle.BoldAndItalic;
                NODE_DRAG_STYLE.fontSize = 13;
                NODE_DRAG_STYLE.wordWrap = false;
            }
        }

        public virtual void OnDraw(float x = 0,float y = 0)
        {

        }

        public virtual void OnPostDraw()
        {

        }

        public virtual bool OnMouseDown(Vector2 position)
        {
            throw new Exception("can't process OnMouseDown event in base class");
        }

        public virtual bool OnMouseDrag(Vector2 position)
        {
            throw new Exception("can't process OnMouseDrag event in base class");
        }

        public virtual bool OnMouseUp(Vector2 position)
        {
            throw new Exception("can't process OnMouseUp event in base class");
        }

        public virtual bool OnMouseIgnore()
        {
            throw new Exception("can't process OnMouseIgnore event in base class");
        }
    }
}

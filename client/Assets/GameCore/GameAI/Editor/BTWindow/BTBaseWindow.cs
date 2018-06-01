using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public class BTBaseWindow
	{
        public static float WINDOW_MIN_FLOAT = 0.001f;

        public static int NODE_WINDOW_WIDTH = 190;
        public static int NODE_WINDOW_CLIP_END_OFF = 48;
        public static int NODE_WINDOW_CLIP_START_OFF = 18;
        public static int NODE_WINDOW_SCROLL_START_OFF_Y = 22;
        public static int NODE_WINDOW_SCROLL_START_OFF_X = 180;
        public static int NODE_WINDOW_SCROLL_WIDTH = 6;
        public static int NODE_WINDOW_SCROLL_SPEED = 15;

        public static int UNIT_WINDOW_WIDTH = 190;

        public static int ZOOM_WINDOW_OFF_Y = 22;

        public Rect mWindowRect = Rect.zero;
        public string mWindowName = string.Empty;
        public bool mIsDirty = false;

        public void OnGUI()
		{
            mIsDirty = false;
            OnPreDraw();
            OnDraw();
            OnPostDraw();
        }

        public virtual void OnEvent()
        {
            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                    OnScrollWheel();
                    break;
                case EventType.MouseUp:
                    OnMouseUp(Event.current.mousePosition);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current.mousePosition);
                    break;
                case EventType.MouseDown:
                    OnMouseDown(Event.current.mousePosition);
                    break;
                case EventType.KeyUp:
                    OnKeyUp();
                    break;
                case EventType.KeyDown:
                    OnKeyDown();
                    break;
                case EventType.ValidateCommand:
                    OnValidateCommand();
                    break;
                case EventType.Ignore:
                    OnMouseIgnore();
                    break;
                case EventType.ContextClick:
                    OnContextClick();
                    break;
            }
        }

        public virtual void OnEnable()
		{
			
		}

        public virtual void OnPreDraw()
        {

        }

        public virtual void OnDraw()
		{

		}

        public virtual void OnPostDraw()
        {

        }

        public virtual void OnDisable()
		{
        }

        public virtual void OnScrollWheel()
        {
            
        }

        public virtual void OnMouseUp(Vector2 position)
        {
            
        }

        public virtual void OnMouseDrag(Vector2 position)
        {
            
        }

        public virtual void OnMouseDown(Vector2 position)
        {
            
        }

        public virtual void OnKeyUp()
        {
            
        }

        public virtual void OnKeyDown()
        {
            
        }

        public virtual void OnContextClick()
        {
            
        }

        public virtual void OnValidateCommand()
        {
            
        }

        public virtual void OnMouseIgnore()
        {

        }
    }
}


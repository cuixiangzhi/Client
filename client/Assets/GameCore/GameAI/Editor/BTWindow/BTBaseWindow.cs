using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public class BTBaseWindow
	{
        public int mID = -1;
        public Rect mRect = Rect.zero;
        public string mName = string.Empty;
        public bool mIsDirty = false;

		public void OnGUI(int id,Rect rect,string windowName)
		{
			mID = id;
			mRect = rect;
			mName = windowName;
            mIsDirty = false;

            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                    OnScrollWheel();
                    break;
                case EventType.MouseUp:
                    OnMouseUp();
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag();
                    break;
                case EventType.MouseDown:
                    OnMouseDown();
                    break;
                case EventType.KeyUp:
                    OnKeyUp();
                    break;
                case EventType.KeyDown:
                    OnKeyDown();
                    break;
                case EventType.ContextClick:
                    OnContextClick();
                    break;
                case EventType.ValidateCommand:
                    OnValidateCommand();
                    break;
            }

            OnPreDraw();

            if (id > 0)
			{
				GUI.Window (id, rect, (idw)=> { OnDraw(); }, windowName);
			}
            else
            {
                OnDraw();
            }

            OnPostDraw();
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

        public virtual void OnMouseUp()
        {
            
        }

        public virtual void OnMouseDrag()
        {
            
        }

        public virtual void OnMouseDown()
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
    }
}


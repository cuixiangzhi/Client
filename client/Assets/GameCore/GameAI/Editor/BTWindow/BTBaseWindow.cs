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

            if (id > 0)
			{
				GUI.Window (id, rect, (idw)=> { OnDraw(); }, windowName);
			}
            else
            {
                OnDraw();
            }

            switch(Event.current.type)
            {
                case EventType.ScrollWheel:
                    OnScrollWheel();
                    break;
                case EventType.MouseEnterWindow:
                    OnMouseEnterWindow();
                    break;
                case EventType.MouseLeaveWindow:
                    OnMouseLeaveWindow();
                    break;
                case EventType.MouseUp:
                    OnMouseUp();
                    break;
                case EventType.MouseMove:
                    OnMouseMove();
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
                case EventType.DragUpdated:
                    OnDragUpdated();
                    break;
                case EventType.DragPerform:
                    OnDragPerform();
                    break;
                case EventType.DragExited:
                    OnDragExited();
                    break;
                case EventType.ContextClick:
                    OnContextClick();
                    break;
                case EventType.ExecuteCommand:
                    OnExecuteCommand();
                    break;
                case EventType.ValidateCommand:
                    OnValidateCommand();
                    break;
            }
		}

        public virtual void OnEnable()
		{
			
		}

        public virtual void OnDraw()
		{

		}

        public virtual void OnDisable()
		{
        }

        public virtual void OnScrollWheel()
        {
            LogMgr.LogError("OnScrollWheel " + mName);
        }

        public virtual void OnMouseEnterWindow()
        {
            
        }

        public virtual void OnMouseLeaveWindow()
        {
            
        }

        public virtual void OnMouseUp()
        {
            LogMgr.LogError("OnMouseUp " + mName);
        }

        public virtual void OnMouseMove()
        {
            LogMgr.LogError("OnMouseUp " + mName);
        }

        public virtual void OnMouseDrag()
        {
            LogMgr.LogError("OnMouseDrag " + mName);
        }

        public virtual void OnMouseDown()
        {
            LogMgr.LogError("OnMouseDown " + mName);
        }

        public virtual void OnKeyUp()
        {
            LogMgr.LogError("OnKeyUp " + mName);
        }

        public virtual void OnKeyDown()
        {
            LogMgr.LogError("OnKeyDown " + mName);
        }

        public virtual void OnDragUpdated()
        {
            LogMgr.LogError("OnDragUpdated " + mName);
        }

        public virtual void OnDragPerform()
        {
            LogMgr.LogError("OnDragPerform " + mName);
        }

        public virtual void OnDragExited()
        {
            LogMgr.LogError("OnDragExited " + mName);
        }

        public virtual void OnContextClick()
        {
            LogMgr.LogError("OnContextClick " + mName);
        }

        public virtual void OnExecuteCommand()
        {
            LogMgr.LogError("OnExecuteCommand " + mName);
        }

        public virtual void OnValidateCommand()
        {
            LogMgr.LogError("OnValidateCommand " + mName);
        }
    }
}


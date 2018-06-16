using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public class BTBaseWindow
	{
        public Rect mWindowRect = Rect.zero;
        public string mWindowName = string.Empty;
        public bool mIsDirty = false;

        public virtual void OnEnable()
		{
			
		}

        public virtual void OnRepaint()
        {
            
        }

        public virtual void OnDisable()
		{

        }

        public virtual void OnMouseDown(Vector2 position)
        {

        }

        public virtual void OnMouseDrag(Vector2 position)
        {
            
        }

        public virtual void OnMouseUp(Vector2 position)
        {

        }

        public virtual void OnMouseIgnore()
        {

        }

        public virtual void OnContextClick()
        {

        }

        public virtual void OnScrollWheel()
        {

        }
    }
}


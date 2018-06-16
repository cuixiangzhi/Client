using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.AI.Editor
{
    public class BTNodeBase
    {
        public string mNodeName;

        public virtual void OnPreDraw()
        {

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

        public virtual bool OnContextClick()
        {
            throw new Exception("can't process OnMouseIgnore event in base class");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.AI.Editor
{
    public sealed class BTUnitInfo : BTNodeBase
    {
        public override void OnDraw(float x = 0, float y = 0)
        {
            
        }

        public override bool OnMouseDown(Vector2 position)
        {
            return false;
        }

        public override bool OnMouseDrag(Vector2 position)
        {
            return false;
        }

        public override bool OnMouseUp(Vector2 position)
        {
            return false;
        }

        public override bool OnMouseIgnore()
        {
            return false;
        }
    }
}

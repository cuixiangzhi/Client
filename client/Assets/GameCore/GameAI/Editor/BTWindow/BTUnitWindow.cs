using System;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
	public sealed class BTUnitWindow : BTBaseWindow
    {
        public override void OnEnable()
        {
            mWindowName = "BTUnitWindow";
        }

        public override void OnPreDraw()
        {
            mWindowRect = new Rect(0, 0, 180, Screen.height);
            GUI.BeginGroup(new Rect(mWindowRect.x, mWindowRect.y, mWindowRect.width, mWindowRect.height));
            GUI.Box(new Rect(0,0,mWindowRect.width,mWindowRect.height),mWindowName,GUI.skin.window);
        }

        public override void OnDraw()
        {
            
        }

        public override void OnPostDraw()
        {
            GUI.matrix = Matrix4x4.identity;
            GUI.EndGroup();
        }
    }
}


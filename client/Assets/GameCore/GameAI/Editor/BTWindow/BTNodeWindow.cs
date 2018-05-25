using System;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
	public sealed class BTNodeWindow : BTBaseWindow
	{
        private Vector2 mScrollPosition = Vector2.zero;

        public override void OnDraw()
        {
            mScrollPosition = GUI.BeginScrollView(new Rect(0, 20, mRect.width, mRect.height), mScrollPosition, mRect, false, false);
            GUI.EndScrollView();
        }
    }
}


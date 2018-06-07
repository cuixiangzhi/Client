using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public sealed class BTZoomWindow : BTBaseWindow
	{
		private float mZoomScaleCur = 1f;
        private float mZoomScaleMax = 1f;

        private Rect mZoomRect = Rect.zero;
        private Vector2 mZoomCenterRaw = Vector2.zero;
        private Vector2 mZoomCenterCur = Vector2.zero;

        public override void OnEnable()
        {
            mWindowName = "BTZoomWindow";
        }

        public override void OnPreDraw()
        {
            mWindowRect = BTHelper.ZOOM_WINDOW_RECT(mZoomScaleCur);
            mZoomCenterRaw = BTHelper.ZOOM_WINDOW_CENTER_RAW;
            mZoomCenterCur = BTHelper.ZOOM_WINDOW_CENTER_CUR(mZoomScaleCur);
            GUI.EndGroup();
            GUI.BeginGroup(mWindowRect);
            GUI.matrix = BTHelper.ZOOM_WINDOW_TRS(mWindowRect,mZoomScaleCur);
        }

        public override void OnDraw()
		{
            Vector2 rawPos = new Vector2(mCurPosition.x, mCurPosition.y);
            Vector2 finalPos = mZoomCenterCur - mZoomCenterRaw + rawPos;
            GUI.Box(new Rect(finalPos.x, finalPos.y, 200, 200), "", GUI.skin.box);
        }

        public override void OnPostDraw()
        {
            GUI.matrix = Matrix4x4.identity;
			GUI.EndGroup ();
            GUI.BeginGroup(BTHelper.ZOOM_WINDOW_OLD_RECT);
        }

        private Vector2 mCurPosition = Vector2.zero;
        private bool mDraging = false;

        public override void OnMouseDown(Vector2 position)
        {
            if(mWindowRect.Contains(position))
            {
                mDraging = true;
                mIsDirty = true;
            }
        }

        public override void OnMouseDrag(Vector2 position)
        {
            if(mDraging)
            {
                mCurPosition += Event.current.delta;
                mIsDirty = true;
            }
        }

        public override void OnMouseUp(Vector2 position)
        {
            mDraging = false;
            mIsDirty = true;
        }

        public override void OnMouseIgnore()
        {
            mDraging = false;
            mIsDirty = true;
        }

        public override void OnScrollWheel()
        {
            //缩放变化
            if (mWindowRect.Contains(Event.current.mousePosition))
            {
                mZoomScaleMax = Mathf.Clamp(mZoomScaleMax * (1f - Event.current.delta.y * 0.05f), 0.4f, 1f);
                mIsDirty = true;
            }
        }

        public override void OnRepaint()
        {
            //缩放插值
            if (Mathf.Abs(mZoomScaleCur - mZoomScaleMax) > BTHelper.WINDOW_MIN_FLOAT)
            {
                mZoomScaleCur = Mathf.Lerp(mZoomScaleCur, mZoomScaleMax, 0.1f);
                mIsDirty = true;
            }
        }
    }
}


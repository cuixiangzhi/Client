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
        private Vector3 mZoomTransform = Vector3.zero;

        public override void OnEnable()
        {
            mWindowName = "BTZoomWindow";
        }

        public override void OnPreDraw()
        {
            //计算窗口大小及缩放后大小
            mWindowRect = new Rect(190, 0, Screen.width - 190 * 2, Screen.height);
            if (Mathf.Abs(mZoomScaleCur - mZoomScaleMax) > BTHelper.WINDOW_MIN_FLOAT)
            {
                mIsDirty = true;
                mZoomScaleCur = Mathf.Lerp(mZoomScaleCur, mZoomScaleMax, 0.1f);
            }
            mZoomRect = new Rect(mWindowRect.x, mWindowRect.y, 1.07374182E+09f, 1.07374182E+09f);
            mZoomTransform = new Vector3(190 + mWindowRect.width / 2, mWindowRect.height / 2);
            //计算TRS矩阵
            Matrix4x4 transform = Matrix4x4.TRS(mZoomTransform, Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(mZoomScaleCur, mZoomScaleCur, 1f));
            Matrix4x4 trs = transform * scale * transform.inverse * GUI.matrix;
            GUI.matrix = trs;
            //绘制窗口
            GUI.BeginGroup(mZoomRect);
        }

        public override void OnDraw()
		{          
            GUI.Box(new Rect(mCurPosition.x, mCurPosition.y, 200,200), "", GUI.skin.window);
        }

        public override void OnPostDraw()
        {           
            GUI.EndGroup();

            GUI.matrix = Matrix4x4.identity;
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
            //检查是否滚动了结点列表
            if (mWindowRect.Contains(Event.current.mousePosition))
            {
                mZoomScaleMax = Mathf.Clamp(mZoomScaleMax * (1f - Event.current.delta.y * 0.05f), 0.4f, 1f);
                mIsDirty = true;
            }
        }
    }
}


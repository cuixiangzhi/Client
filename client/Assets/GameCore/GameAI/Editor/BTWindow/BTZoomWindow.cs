using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public sealed class BTZoomWindow : BTBaseWindow
	{
		private float mZoomScaleCur = 1f;
        private float mZoomScaleMax = 1f;

		public override void OnDraw()
		{
            GUI.BeginGroup(new Rect(mRect.x, mRect.y, mRect.width / mZoomScaleCur, mRect.height / mZoomScaleMax));

            Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(180,22), Quaternion.identity, Vector3.one);
            Matrix4x4 matrix4x2 = Matrix4x4.Scale(new Vector3(mZoomScaleCur, mZoomScaleCur, 1f));
            GUI.matrix = (matrix4x * matrix4x2 * matrix4x.inverse * GUI.matrix);     
            GUI.Box(new Rect(100,100,200,200), "", GUI.skin.window);      
            GUI.matrix = Matrix4x4.identity;

            GUI.EndGroup();

            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                    mZoomScaleMax = Mathf.Clamp(mZoomScaleMax * (1f - Event.current.delta.y * 0.05f), 0.1f, 1.5f);
                    break;
            }
            if (Mathf.Abs(mZoomScaleCur - mZoomScaleMax) > 1e-3)
            {
                mIsDirty = true;
                mZoomScaleCur = Mathf.Lerp(mZoomScaleCur, mZoomScaleMax, 0.1f);
            }
        }
	}
}


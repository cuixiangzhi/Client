using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public sealed class BTZoomWindow : BTBaseWindow
	{
		private float mScale = 1;

		protected override void OnDraw(int id)
		{
			
		}

		private void BeginDraw()
		{
			//结束上一分组
			GUI.EndGroup();
			//开启外部分组
			GUI.BeginGroup(new Rect(mRect.x,mRect.y + 22,mRect.width / mScale, mRect.height / mScale));

			//开启内部分组
			Matrix4x4 matrixOld = GUI.matrix;
			//Matrix4x4 matrix4x = Matrix4x4.TRS(rect.TopLeft(), Quaternion.identity, Vector3.one);
			//Matrix4x4 matrix4x2 = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1f));
			//GUI.matrix = (matrix4x * matrix4x2 * matrix4x.inverse * GUI.matrix);
			//Rect rect2 = rect;
			//rect2.width = 1.07374182E+09f;
			//rect2.height = 1.07374182E+09f;
			//rect2.x -= cameraPos.x;
			//rect2.y -= cameraPos.y;
			//GUI.BeginGroup(rect2);
		}

		private void EndDraw()
		{
			//结束内部分组
			GUI.EndGroup();
			//结束外部分组
			//GUI.matrix = prevGuiMatrix;
			GUI.EndGroup();
			GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
		}
	}
}


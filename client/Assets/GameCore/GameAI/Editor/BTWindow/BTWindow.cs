using System;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
	public sealed class BTWindow : EditorWindow
	{
		//单位窗口
		private BTBaseWindow mUnitWindow = new BTUnitWindow();
		//缩放窗口
		private BTBaseWindow mZoomWindow = new BTZoomWindow();
		//结点窗口
		private BTBaseWindow mNodeWindow = new BTNodeWindow();

		void OnEnable()
		{
			mUnitWindow.OnEnable();
			mZoomWindow.OnEnable();
			mNodeWindow.OnEnable();
		}

		void OnDisable()
		{
			mUnitWindow.OnDisable();
			mZoomWindow.OnDisable();
			mNodeWindow.OnDisable();
		}

		void OnGUI()
		{
			Rect rectUnit = new Rect (0, 0, 180, Screen.height);
			Rect rectNode = new Rect (Screen.width - 180, 0, 180, Screen.height);
			Rect rectZoom = new Rect (180, 0, Screen.width - 360, Screen.height);

			BeginWindows();
			mUnitWindow.OnGUI (2, rectUnit, "UnitWindow");
            mNodeWindow.OnGUI (3, rectNode, "NodeWindow");
            mZoomWindow.OnGUI (0, rectZoom, "ZoomWindow");
			EndWindows();

            if(mUnitWindow.mIsDirty || mNodeWindow.mIsDirty || mZoomWindow.mIsDirty)
            {
                Repaint();
            }            
		}
	}
}


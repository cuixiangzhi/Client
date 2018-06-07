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
        //控制单击
        private bool mIsMouseDown = false;

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
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Used:
                case EventType.MouseEnterWindow:
                case EventType.MouseLeaveWindow:
                case EventType.DragPerform:
                case EventType.DragUpdated:
                case EventType.DragExited:
                    //忽略
                    break;
                default:
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            //重绘
                            BeginWindows();
                            mUnitWindow.OnGUI();
                            mNodeWindow.OnGUI();
                            mZoomWindow.OnGUI();
                            EndWindows();
                        }
                    }
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            //重绘
                            BeginWindows();
                            mUnitWindow.OnGUI();
                            mNodeWindow.OnGUI();
                            mZoomWindow.OnGUI();
                            EndWindows();
                        }
                    }
                    {
                        if (!Event.current.isMouse || Event.current.button == 0 || (!mIsMouseDown && Event.current.type == EventType.ContextClick))
                        {
                            mUnitWindow.OnEvent();
                            mNodeWindow.OnEvent();
                            mZoomWindow.OnEvent();
                        }
                    }
                    break;
            }

            if (mUnitWindow.mIsDirty || mNodeWindow.mIsDirty || mZoomWindow.mIsDirty)
            {
                Repaint();
            }            
		}
	}
}


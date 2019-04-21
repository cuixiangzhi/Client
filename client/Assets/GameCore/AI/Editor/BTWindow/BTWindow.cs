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

        UnityEngine.Object mObject;

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
            //DragAndDrop.PrepareStartDrag();
            //DragAndDrop.activeControlID = idx;
            //DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            //GameObject GO = new GameObject(mNodeDatas[idx].nodeName);
            //GO.hideFlags = HideFlags.DontSave;
            //ServantEditData IG = GO.AddComponent<ServantEditData>();
            //List<ServantEditData> ls = new List<ServantEditData>();
            //ls.Add(IG);
            //DragAndDrop.objectReferences = ls.ToArray();
            //DragAndDrop.StartDrag(mNodeDatas[idx].nodeName);
            mObject = EditorGUI.ObjectField(new Rect(200, 200, 180, 20), mObject, typeof(IEditEventSender), true);
            switch (Event.current.type)
            {
                case EventType.Layout:
                case EventType.Used:
                case EventType.MouseEnterWindow:
                case EventType.MouseLeaveWindow:
                case EventType.KeyUp:
                case EventType.KeyDown:
                case EventType.DragUpdated:
                case EventType.DragExited:
                case EventType.DragPerform:
                    break;
                case EventType.MouseUp:
                    if (Event.current.button != 0)
                        break;
                    mIsMouseDown = false;
                    mUnitWindow.OnMouseUp(Event.current.mousePosition);
                    mNodeWindow.OnMouseUp(Event.current.mousePosition);
                    mZoomWindow.OnMouseUp(Event.current.mousePosition);
                    break;
                case EventType.MouseDrag:
                    if (!mIsMouseDown)
                        break;
                    mUnitWindow.OnMouseDrag(Event.current.mousePosition);
                    mNodeWindow.OnMouseDrag(Event.current.mousePosition);
                    mZoomWindow.OnMouseDrag(Event.current.mousePosition);
                    break;
                case EventType.MouseDown:
                    if (Event.current.button != 0)
                        break;
                    mIsMouseDown = true;
                    mUnitWindow.OnMouseDown(Event.current.mousePosition);
                    mNodeWindow.OnMouseDown(Event.current.mousePosition);
                    mZoomWindow.OnMouseDown(Event.current.mousePosition);
                    break;
                case EventType.Ignore:
                    mUnitWindow.OnMouseIgnore();
                    mNodeWindow.OnMouseIgnore();
                    mZoomWindow.OnMouseIgnore();
                    break;
                case EventType.ContextClick:
                    mUnitWindow.OnContextClick();
                    mNodeWindow.OnContextClick();
                    mZoomWindow.OnContextClick();
                    break;
                case EventType.ScrollWheel:
                    mUnitWindow.OnScrollWheel();
                    mNodeWindow.OnScrollWheel();
                    mZoomWindow.OnScrollWheel();
                    break;
                case EventType.Repaint:
                    mUnitWindow.OnRepaint();
                    mNodeWindow.OnRepaint();
                    mZoomWindow.OnRepaint();
                    break;
            }

            if (mUnitWindow.mIsDirty || mNodeWindow.mIsDirty || mZoomWindow.mIsDirty)
            {
                Repaint();
            }            
		}
	}
}


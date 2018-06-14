using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace GameCore.AI.Editor
{
	public sealed class BTZoomWindow : BTBaseWindow
	{
		private float mZoomScaleCur = 1f;
        private float mZoomScaleMax = 1f;

        private Rect mZoomRect = Rect.zero;
        private Vector2 mZoomCenterRaw = Vector2.zero;
        private Vector2 mZoomCenterCur = Vector2.zero;
        private Vector2 mZoomRawOffset = Vector2.zero;
        private Vector2 mZoomRealOffset = Vector2.zero;
        private bool mZoomDraging = false;

        private List<BTNodeBase> mUnitNodes = new List<BTNodeBase>();

        public override void OnEnable()
        {
            mWindowName = "BTZoomWindow";
        }

        public override void OnPreDraw()
        {
            mWindowRect = BTHelper.ZOOM_WINDOW_RECT(mZoomScaleCur);
            mZoomCenterRaw = BTHelper.ZOOM_WINDOW_CENTER_RAW;
            mZoomCenterCur = BTHelper.ZOOM_WINDOW_CENTER_CUR(mZoomScaleCur);
            mZoomRealOffset = mZoomCenterCur - mZoomCenterRaw + mZoomRawOffset;
            GUI.EndGroup();
            GUI.BeginGroup(mWindowRect);
            GUI.matrix = BTHelper.ZOOM_WINDOW_TRS(mWindowRect,mZoomScaleCur);
        }

        public override void OnDraw()
		{
            GUI.Box(new Rect(mZoomRealOffset.x, mZoomRealOffset.y, 100,100),"",GUI.skin.window);
        }

        public override void OnPostDraw()
        {
            GUI.matrix = Matrix4x4.identity;
			GUI.EndGroup ();
            GUI.BeginGroup(BTHelper.ZOOM_WINDOW_OLD_RECT);
        }

        public override void OnMouseDown(Vector2 position)
        {
            if(mWindowRect.Contains(position))
            {
                mZoomDraging = true;
                for (int i = 0;i < mUnitNodes.Count;i++)
                {
                    if(mUnitNodes[i].OnMouseDown(position))
                    {
                        mIsDirty = true;
                        Event.current.Use();
                        return;
                    }
                }
            }
        }

        public override void OnMouseDrag(Vector2 position)
        {
            if(mZoomDraging)
            {
                for (int i = 0; i < mUnitNodes.Count; i++)
                {
                    if (mUnitNodes[i].OnMouseDrag(position))
                    {
                        mIsDirty = true;
                        Event.current.Use();
                        return;
                    }
                }
                mZoomRawOffset += Event.current.delta;
                mIsDirty = true;
            }
        }

        public override void OnMouseUp(Vector2 position)
        {
            if (mZoomDraging)
            {
                mZoomDraging = false;
                for (int i = 0; i < mUnitNodes.Count; i++)
                {
                    if (mUnitNodes[i].OnMouseUp(position))
                    {
                        mIsDirty = true;
                        Event.current.Use();
                        return;
                    }
                }
            }
        }

        public override void OnMouseIgnore()
        {
            if (mZoomDraging)
            {
                mZoomDraging = false;
                for (int i = 0; i < mUnitNodes.Count; i++)
                {
                    if (mUnitNodes[i].OnMouseIgnore())
                    {
                        mIsDirty = true;
                        Event.current.Use();
                        return;
                    }
                }
            }
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

        public override void OnAddNode()
        {
            
        }

        public override void OnContextClick()
        {
            if(mWindowRect.Contains(Event.current.mousePosition))
            {
                for (int i = 0; i < mUnitNodes.Count; i++)
                {
                    if (mUnitNodes[i].OnContextClick())
                    {
                        mIsDirty = true;
                        Event.current.Use();
                        return;
                    }
                }
                OnContextClickNull();
            }
        }

        private void OnContextClickNull()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("帮助"), false, () => { EditorUtility.DisplayDialog("帮助", "开发期,暂无帮助信息", "确定"); });
            menu.AddItem(new GUIContent("关于"), false, () => { EditorUtility.DisplayDialog("关于", "AI编辑器,可编辑角色、关卡AI\n制作人:崔祥志", "确定"); });
            menu.ShowAsContext();
            Event.current.Use();
        }
    }
}


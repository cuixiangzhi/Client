using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GameCore.AI.Editor
{
	public sealed class BTUnitWindow : BTBaseWindow
    {
        private Rect mClipRect = Rect.zero;
        private Rect mScrollRect = Rect.zero;
        private Rect mWindowBoxRect = Rect.zero;
        private float mClipOffset = 0;
        private List<BTUnitGroup> mUnitList = new List<BTUnitGroup>();
        private BTUnitInfo mUnitInfo = new BTUnitInfo();     

        public override void OnEnable()
        {
            mWindowName = "BTUnitWindow";
        }

        public override void OnRepaint()
        {
            //计算窗口、裁剪、滚动大小
            mWindowRect = BTHelper.UNIT_WINDOW_RECT;
            mWindowBoxRect = BTHelper.UNIT_WINDOW_BOX_RECT;
            mClipRect = BTHelper.UNIT_WINDOW_CLIP_RECT;
            mScrollRect = BTHelper.UNIT_WINDOW_SCROLL_RECT(GetUnitHeight(), mClipOffset, mClipRect.height);

            //绘制背景窗口
            GUI.BeginGroup(mWindowRect);
            GUI.Box(mWindowBoxRect, mWindowName, GUI.skin.window);
            //绘制滚动条            
            GUI.Box(mScrollRect, string.Empty);
            //绘制裁剪区域            
            GUI.BeginClip(mClipRect, Vector2.zero, Vector2.zero, false);

            //TODO绘制
            mUnitInfo.OnDraw();
            for(int i = 0;i < mUnitList.Count;i++)
            {
                mUnitList[i].OnDraw();
            }           

            //结束裁剪区域
            GUI.EndClip();
            //结束绘制组
            GUI.EndGroup();
        }

        public override void OnMouseDown(Vector2 position)
        {
            //检查是否点中了某个结点
            if (mWindowRect.Contains(position))
            {
                if(!mUnitInfo.OnMouseDown(position))
                {
                    for (int i = 0; i < mUnitList.Count; i++)
                    {
                        if (mUnitList[i].OnMouseDown(position))
                        {
                            mIsDirty = true;
                            Event.current.Use();
                            return;
                        }
                    }
                }
            }
        }

        public override void OnMouseDrag(Vector2 position)
        {

        }

        public override void OnMouseUp(Vector2 position)
        {

        }

        public override void OnMouseIgnore()
        {

        }

        public override void OnScrollWheel()
        {
            //检查是否滚动了结点列表
            if (mWindowRect.Contains(Event.current.mousePosition))
            {
                float oldOffset = mClipOffset;
                mClipOffset -= BTHelper.UNIT_WINDOW_SCROLL_DELTA;

                float nodeHeight = GetUnitHeight();
                float maxOffset = nodeHeight > mClipRect.height ? mClipRect.height - nodeHeight : 0;

                mClipOffset = Mathf.Clamp(mClipOffset, maxOffset, 0);
                if (Mathf.Abs(oldOffset - mClipOffset) > BTHelper.WINDOW_MIN_FLOAT)
                {
                    mIsDirty = true;
                }
                Event.current.Use();
            }
        }

        private float GetUnitHeight()
        {
            //获取已绘制结点总高度
            float nodeHeight = 0;
            for (int i = 0; i < mUnitList.Count; i++)
            {
                nodeHeight += mUnitList[i].mNodeHeight;
            }
            return nodeHeight;
        }
    }
}


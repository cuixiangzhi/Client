using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GameCore.AI.Editor
{
	public sealed class BTNodeWindow : BTBaseWindow
	{
        private Rect mClipRect = Rect.zero;
        private Rect mScrollRect = Rect.zero;
        private Rect mWindowBoxRect = Rect.zero;
        private float mClipOffset = 0;
        private List<BTNodeGroup> mNodeList = new List<BTNodeGroup>();

        public override void OnEnable()
        {
            mWindowName = "BTNodeWindow";
            //根据结点数据创建结点组
            List<NodeData> datas = BTEditor.GetNode();
            for(int i = 0;i < datas.Count;i++)
            {
                BTNodeGroup group = new BTNodeGroup();
                for (int j = i;j < datas.Count;j++)
                {
                    if(datas[j].groupID == datas[i].groupID)
                    {
                        group.mNodeName = datas[j].groupName;
                        group.mNodeDatas.Add(datas[j]);
                    }
                    else
                    {
                        break;
                    }
                }
                i += group.mNodeDatas.Count;
                mNodeList.Add(group);
            }
        }

        public override void OnPreDraw()
        {
            //计算窗口、裁剪、滚动大小
			mWindowRect = BTHelper.NODE_WINDOW_RECT;
			mWindowBoxRect = BTHelper.NODE_WINDOW_BOX_RECT;
			mClipRect = BTHelper.NODE_WINDOW_CLIP_RECT;
			mScrollRect = BTHelper.NODE_WINDOW_SCROLL_RECT(GetNodeHeight(),mClipOffset,mClipRect.height);

            //绘制背景窗口
            GUI.BeginGroup(mWindowRect);
			GUI.Box(mWindowBoxRect, mWindowName, GUI.skin.window);
            //绘制滚动条            
            GUI.Box(mScrollRect, string.Empty);
            //绘制裁剪区域            
			GUI.BeginClip(mClipRect, Vector2.zero, Vector2.zero, false);
            //绘制前处理
            for (int i = 0; i < mNodeList.Count; i++)
            {
                mNodeList[i].OnPreDraw();
            }
        }

        public override void OnDraw()
        {
            //绘制结点组
            for(int i = 0;i < mNodeList.Count; i++)
            {
                mNodeList[i].OnDraw(0, i == 0 ? mClipOffset : mNodeList[i - 1].mNodeRect.y);
            }
        }

        public override void OnPostDraw()
        {
            GUI.EndClip();
            GUI.EndGroup();
            //绘制后处理
            for (int i = 0; i < mNodeList.Count; i++)
            {
                mNodeList[i].OnPostDraw();
            }
        }

        public override void OnMouseDown(Vector2 position)
        {
            //检查是否点中了某个结点
            if (mWindowRect.Contains(position))
            {
                for(int i = 0;i < mNodeList.Count;i ++)
                {
                    if(mNodeList[i].OnMouseDown(position))
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
            //检查是否拖拽了某个结点
            for (int i = 0; i < mNodeList.Count; i++)
            {
                if (mNodeList[i].OnMouseDrag(position))
                {
                    mIsDirty = true;
                    Event.current.Use();
                    return;
                }
            }
        }

        public override void OnMouseUp(Vector2 position)
        {
            //检查某个结点是否拖拽结束
            for (int i = 0; i < mNodeList.Count; i++)
            {
                if (mNodeList[i].OnMouseUp(position))
                {
                    mIsDirty = true;
                    Event.current.Use();
                    return;
                }
            }
        }

        public override void OnMouseIgnore()
        {
            //检查某个结点是否拖拽离开了窗口
            for (int i = 0; i < mNodeList.Count; i++)
            {
                if (mNodeList[i].OnMouseIgnore())
                {
                    mIsDirty = true;
                    Event.current.Use();
                    return;
                }
            }
        }

        public override void OnScrollWheel()
        {
            //检查是否滚动了结点列表
            if(mWindowRect.Contains(Event.current.mousePosition))
            {
                float oldOffset = mClipOffset;
				mClipOffset -= BTHelper.NODE_WINDOW_SCROLL_DELTA;

                float nodeHeight = GetNodeHeight();
				float maxOffset = nodeHeight > mClipRect.height ? mClipRect.height - nodeHeight : 0;

                mClipOffset = Mathf.Clamp(mClipOffset, maxOffset, 0);
                if(Mathf.Abs(oldOffset - mClipOffset) > BTHelper.WINDOW_MIN_FLOAT)
                {
                    mIsDirty = true;
                }
                Event.current.Use();             
            }
        }

        private float GetNodeHeight()
        {
            //获取已绘制结点总高度
            float nodeHeight = 0;
            for (int i = 0; i < mNodeList.Count; i++)
            {
                nodeHeight += mNodeList[i].mNodeHeight;
            }
            return nodeHeight;
        }
    }
}


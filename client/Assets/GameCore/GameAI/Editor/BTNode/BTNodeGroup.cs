using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameCore.AI.Editor
{
    public sealed class BTNodeGroup : BTNodeBase
    {
        public List<NodeData> mNodeDatas = new List<NodeData>();
        public bool mFoldOut = false;
        public Rect mNodeRect = Rect.zero;
        public float mNodeHeight = 0;
        public float mNodeOffset = 0;

        public int mDragingIdx = -1;
		public Rect mDragRect = BTHelper.NODE_DRAG_RECT;

        public override void OnDraw(float x = 0,float y = 0)
        {
            //计算当前结点组偏移
			mNodeRect = BTHelper.NODE_GROUP_RECT(x,y);
            mNodeOffset = mNodeRect.y;

            //绘制折叠按钮
            GUI.color = Color.cyan;
			GUI.Box(mNodeRect, mNodeName, BTHelper.NODE_GROUP_STYLE);
            EditorGUI.Foldout(mNodeRect, mFoldOut, "", true);
            GUI.color = Color.white;
			mNodeRect.y += BTHelper.NODE_GROUP_HEIGHT;
			mNodeHeight = BTHelper.NODE_GROUP_HEIGHT_WITH_SPACE;

            //绘制子结点
            if (mFoldOut)
            {
				mNodeRect.width = BTHelper.NODE_CHILD_WIDTH;
				mNodeRect.height = BTHelper.NODE_CHILD_HEIGHT;
				mNodeRect.x = BTHelper.NODE_CHILD_OFFSET;
                for (int i = 0;i < mNodeDatas.Count;i++)
                {                  
					mNodeRect.y += BTHelper.NODE_GROUP_SPACE;
                    GUI.Box(mNodeRect, new GUIContent(string.Empty, mNodeDatas[i].nodeTip));
					GUI.Label(mNodeRect, mNodeDatas[i].nodeName, i == mDragingIdx ? BTHelper.NODE_DRAG_STYLE : BTHelper.NODE_CHILD_STYLE);
					mNodeRect.y += BTHelper.NODE_CHILD_HEIGHT;
					mNodeHeight += BTHelper.NODE_GROUP_SPACE + BTHelper.NODE_CHILD_HEIGHT;
                }
            }
        }

        public override void OnPostDraw()
        {
            if (mDragingIdx != -1)
            {
                BTHelper.DrawRect(mDragRect,mNodeDatas[mDragingIdx].nodeName);
            }
        }

        public override bool OnMouseDown(Vector2 position)
        {
			float y = position.y - BTHelper.NODE_HEIGHT_OFFSET;
			if(y >= mNodeOffset && y <= mNodeOffset + BTHelper.NODE_GROUP_HEIGHT)
            {
                mFoldOut = !mFoldOut;
                return true;
            }
			else if (y >= mNodeOffset + BTHelper.NODE_GROUP_HEIGHT_WITH_SPACE && y <= mNodeOffset + mNodeHeight)
            {
				float offset = y - mNodeOffset - BTHelper.NODE_GROUP_HEIGHT_WITH_SPACE;
				int idx = Mathf.CeilToInt(offset / BTHelper.NODE_CHILD_HEIGHT_WITH_SPACE) - 1;
                if (idx < mNodeDatas.Count)
                {
                    mDragingIdx = idx;
                    mDragRect.center = position;
                    return true;
                }
            }
            return false;
        }

        public override bool OnMouseDrag(Vector2 position)
        {
            if(mDragingIdx != -1)
            {
                mDragRect.center = position;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool OnMouseUp(Vector2 position)
        {
            if(mDragingIdx != -1)
            {
                BTEditor.AddNode(mNodeDatas[mDragingIdx], position);
                mDragingIdx = -1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool OnMouseIgnore()
        {
            if (mDragingIdx != -1)
            {
                mDragingIdx = -1;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

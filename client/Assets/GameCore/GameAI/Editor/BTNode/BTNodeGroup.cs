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
        public Rect mDragRect = new Rect(0,0,NODE_DRAG_WIDTH,NODE_DRAG_HEIGHT);

        public override void OnDraw(float x = 0,float y = 0)
        {
            //计算当前结点组偏移
            mNodeRect = new Rect(x, y + NODE_GROUP_SPACE, NODE_GROUP_WIDTH, NODE_GROUP_HEIGHT);
            mNodeOffset = mNodeRect.y;

            //绘制折叠按钮
            GUI.color = Color.cyan;
            GUI.Box(mNodeRect, mNodeName, NODE_GROUP_STYLE);
            EditorGUI.Foldout(mNodeRect, mFoldOut, "", true);
            GUI.color = Color.white;
            mNodeRect.y += NODE_GROUP_HEIGHT;
            mNodeHeight = NODE_GROUP_HEIGHT + NODE_GROUP_SPACE;

            //绘制子结点
            if (mFoldOut)
            {
                mNodeRect.width = NODE_CHILD_WIDTH;
                mNodeRect.height = NODE_CHILD_HEIGHT;
                mNodeRect.x = NODE_CHILD_OFFSET;
                for (int i = 0;i < mNodeDatas.Count;i++)
                {                  
                    mNodeRect.y += NODE_GROUP_SPACE;
                    GUI.Box(mNodeRect, new GUIContent(string.Empty, mNodeDatas[i].nodeTip));
                    GUI.Label(mNodeRect, mNodeDatas[i].nodeName, i == mDragingIdx ? NODE_DRAG_STYLE : NODE_CHILD_STYLE);
                    mNodeRect.y += NODE_CHILD_HEIGHT;
                    mNodeHeight += NODE_GROUP_SPACE + NODE_CHILD_HEIGHT;
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
            float y = position.y - NODE_START_OFFSET;
            if(y >= mNodeOffset && y <= mNodeOffset + NODE_GROUP_HEIGHT)
            {
                mFoldOut = !mFoldOut;
                return true;
            }
            else if (y >= mNodeOffset + NODE_GROUP_HEIGHT + NODE_GROUP_SPACE && y <= mNodeOffset + mNodeHeight)
            {
                float offset = y - mNodeOffset - NODE_GROUP_HEIGHT - NODE_GROUP_SPACE;
                int idx = Mathf.CeilToInt(offset / (NODE_CHILD_HEIGHT + NODE_GROUP_SPACE)) - 1;
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

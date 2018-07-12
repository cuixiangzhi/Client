using UnityEngine;
using System.Collections.Generic;

public class ContentData : MonoBehaviour
{
    public int mWrapIndex = -1;
    public int mRealIndex = -1;
}

public class UIWrapContentEx : MonoBehaviour
{
    public enum Align
    {
        Top,
        Bottom,
    }
    public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);
    private Align mItemAlign = Align.Bottom;
    private Align mDataAlign = Align.Bottom;

    private bool mInited = false;
    private UIScrollView mScrollView;
    private SpringPanel mSpringPanel;
    private Transform mTrans;
    private Transform mDragTrans;
    private UIPanel mPanel;
    private Transform mPanelTrans;
    private float mPanelHalfHeight;
    private OnInitializeItem mOnInit;
    private UIPanel.OnClippingMoved mOnClipMove = null;
    private List<UIWidget> mChildWidgets = new List<UIWidget>();
    private List<Transform> mChildTranss = new List<Transform>();
    private List<GameObject> mChildObjss = new List<GameObject>();
    private List<ContentData> mChildContents = new List<ContentData>();
    private int mMaxCount = 0;

    public void ResetWrapContent(int count, OnInitializeItem onInit,Align itemAlign,Align dataAlign, bool updateAll)
    {
        CacheComponent();
        mMaxCount = count;
        mPanel.onClipMove = null;
        mItemAlign = itemAlign;
        mDataAlign = dataAlign;
        mOnInit = onInit;
        mPanel.clipOffset = Vector2.zero;
        mPanelTrans.localPosition = Vector3.zero;
        mScrollView.InvalidateBounds();

        bool reInit = mChildContents.Count != 0;

        //Cache组件
        for (int i = 0; i < mTrans.childCount; i++)
        {
            if (!reInit)
            {
                mChildTranss.Add(mTrans.GetChild(i));
                mChildObjss.Add(mChildTranss[i].gameObject);
                mChildWidgets.Add(mChildTranss[i].GetComponent<UIWidget>());
                mChildContents.Add(mChildObjss[i].AddMissingComponent<ContentData>());
                mChildContents[i].mRealIndex = -1;
                mChildContents[i].mWrapIndex = i;
            }
        }

        if(updateAll)
        {
            for(int i = 0;i < mChildContents.Count;i++)
            {
                mChildContents[i].mRealIndex = -1;
            }
        }

        if(mItemAlign == Align.Top)
        {
            InitContentTop();
        }
        else if(mItemAlign == Align.Bottom)
        {
            InitContentBottom();
        }

        for (int i = 0; i < mChildContents.Count; i++)
        {
            if (mChildContents[i].mRealIndex == -1 && mChildObjss[i].activeSelf)
            {
                mChildObjss[i].SetActive(false);
            }
        }

        mPanel.onClipMove = mOnClipMove;
    }

    private void InitContentTop()
    {
        int tmpRealIndex = mDataAlign == Align.Top ? mMaxCount - 1 : 0;
        UIWidget w = null;
        Transform t = null;
        //更新内容
        for (int i = 0; i < mChildContents.Count; i++)
        {
            if (mMaxCount == 0)
            {
                mChildObjss[i].SetActive(false);
            }
            else
            {
                //每次更新可能只有有限的几个有变化,根据优先级找出一个可用ITEM
                int wrapIndex = GetWrapIndexInit(tmpRealIndex, mDataAlign == Align.Top);
                ContentData cd = mChildContents[wrapIndex];
                if (cd.mRealIndex != tmpRealIndex)
                {
                    cd.mRealIndex = tmpRealIndex;
                    UnityEngine.Profiling.Profiler.BeginSample("INIT_CONTENT_STRING");
                    mOnInit(mChildObjss[wrapIndex], wrapIndex, cd.mRealIndex);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                //更新ITEM位置
                if (tmpRealIndex == (mDataAlign == Align.Top ? mMaxCount - 1 : 0))
                {
                    mChildTranss[wrapIndex].localPosition = new Vector3(0, -mChildWidgets[wrapIndex].height / 2.0f, 0);
                }
                else
                {
                    float y = t.localPosition.y - w.height / 2.0f - mChildWidgets[wrapIndex].height / 2.0f;
                    mChildTranss[wrapIndex].localPosition = new Vector3(0, y, 0);
                }
                w = mChildWidgets[wrapIndex];
                t = mChildTranss[wrapIndex];

                //将未被使用的ITEM id全部改为-1
                if(mDataAlign == Align.Top)
                {
                    --tmpRealIndex;
                    if (tmpRealIndex < 0)
                    {
                        for (int j = 0; j < mChildContents.Count; j++)
                        {
                            if (mChildContents[i].mRealIndex > mMaxCount - 1)
                            {
                                mChildContents[i].mRealIndex = -1;
                                mChildObjss[i].SetActive(false);
                            }
                        }
                        break;
                    }
                }
                else
                {
                    ++tmpRealIndex;
                    if(tmpRealIndex >= mMaxCount)
                    {
                        for (int j = 0; j < mChildContents.Count; j++)
                        {
                            if (mChildContents[i].mRealIndex > mMaxCount - 1)
                            {
                                mChildContents[i].mRealIndex = -1;
                                mChildObjss[i].SetActive(false);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    private void InitContentBottom()
    {
        int tmpRealIndex = mDataAlign == Align.Bottom ? mMaxCount - 1 : 0;
        int totalHeight = 0;
        UIWidget w = null;
        Transform t = null;
        //更新内容
        for (int i = 0; i < mChildContents.Count; i++)
        {
            if (mMaxCount == 0)
            {
                mChildObjss[i].SetActive(false);
            }
            else
            {
                //每次更新可能只有有限的几个有变化,根据优先级找出一个可用ITEM
                int wrapIndex = GetWrapIndexInit(tmpRealIndex, mDataAlign == Align.Bottom);
                ContentData cd = mChildContents[wrapIndex];
                if (cd.mRealIndex != tmpRealIndex)
                {
                    cd.mRealIndex = tmpRealIndex;
                    UnityEngine.Profiling.Profiler.BeginSample("INIT_CONTENT_STRING");
                    mOnInit(mChildObjss[wrapIndex], wrapIndex, cd.mRealIndex);
                    UnityEngine.Profiling.Profiler.EndSample();
                }

                totalHeight += mChildWidgets[wrapIndex].height;

                //更新ITEM位置,从下往上排
                if (tmpRealIndex == (mDataAlign == Align.Bottom ? mMaxCount - 1 : 0))
                {
                    mChildTranss[wrapIndex].localPosition = new Vector3(0, -mChildWidgets[wrapIndex].height / 2.0f, 0);
                }
                else
                {
                    float y = t.localPosition.y + w.height / 2.0f + mChildWidgets[wrapIndex].height / 2.0f;
                    mChildTranss[wrapIndex].localPosition = new Vector3(0, y, 0);
                }
                w = mChildWidgets[wrapIndex];
                t = mChildTranss[wrapIndex];

                //将未被使用的ITEM id全部改为-1
                if(mDataAlign == Align.Bottom)
                {
                    --tmpRealIndex;
                    if (tmpRealIndex < 0)
                    {
                        for (int j = 0; j < mChildContents.Count; j++)
                        {
                            if (mChildContents[i].mRealIndex > mMaxCount - 1)
                            {
                                mChildContents[i].mRealIndex = -1;
                                mChildObjss[i].SetActive(false);
                            }
                        }
                        break;
                    }
                }
                else
                {
                    ++tmpRealIndex;
                    if (tmpRealIndex >= mMaxCount)
                    {
                        for (int j = 0; j < mChildContents.Count; j++)
                        {
                            if (mChildContents[i].mRealIndex > mMaxCount - 1)
                            {
                                mChildContents[i].mRealIndex = -1;
                                mChildObjss[i].SetActive(false);
                            }
                        }
                        break;
                    }
                }
            }
        }
        if(mMaxCount > 0)
        {
            //计算整体偏移距离,超出Panel范围则以最大的值来取距离
            float y = 0;
            if(totalHeight > mPanelHalfHeight * 2)
            {
                int bottomRealIndex = mDataAlign == Align.Bottom ? mMaxCount - 1 : 0;
                int wrapIndex = GetWrapIndexInit(bottomRealIndex, false);
                y = mPanelHalfHeight * 2 - mChildWidgets[wrapIndex].height;
            }
            else
            {
                int topRealIndex = mDataAlign == Align.Bottom ? (mMaxCount > mChildContents.Count ? mMaxCount - mChildContents.Count : 0) : (mMaxCount > mChildContents.Count ? (mChildContents.Count - 1) : mMaxCount - 1);
                int wrapIndex = GetWrapIndexInit(topRealIndex,false);
                y = mChildTranss[wrapIndex].localPosition.y + mChildWidgets[wrapIndex].height / 2.0f;
            }
            for (int i = 0; i < mChildContents.Count; i++)
            {
                if (mChildContents[i].mRealIndex != -1)
                {
                    t = mChildTranss[i];
                    t.localPosition = new Vector3(0, t.localPosition.y - y, 0);
                }
            }
        }
    }

    public void UpdateContent(int oldCount,int newCount)
    {
        UnityEngine.Profiling.Profiler.BeginSample("UpdateContent");
        //隐藏old到new之间的item
        for (int i = 0; i < mChildContents.Count; i++)
        {
            if (mChildContents[i].mRealIndex > oldCount)
            {
                mChildContents[i].mRealIndex = -1;
                mChildObjss[i].SetActive(false);
            }
        }
        mMaxCount = newCount;
        while(true)
        {         
            if (mMaxCount == 0)
            {
                ResetWrapContent(mMaxCount, mOnInit, mItemAlign,mDataAlign, true);
                break;
            }
                
            //检查是否上下有空白并且上下仍有数据
            bool hasSpace = false;
            if(mDataAlign == Align.Top)
            {
                int maxIndex = GetWrapIndexSide(true);
                float y = mDragTrans.InverseTransformPoint(mChildTranss[maxIndex].position).y;
                if (y < mPanelHalfHeight && mChildContents[maxIndex].mRealIndex != mMaxCount - 1)
                    hasSpace = true;

                int minIndex = GetWrapIndexSide(false);
                y = mDragTrans.InverseTransformPoint(mChildTranss[minIndex].position).y;
                if (y > -mPanelHalfHeight && mChildContents[minIndex].mRealIndex > 0)
                    hasSpace = true;
            }
            else
            {
                int maxIndex = GetWrapIndexSide(true);
                float y = mDragTrans.InverseTransformPoint(mChildTranss[maxIndex].position).y;
                if (y > -mPanelHalfHeight && mChildContents[maxIndex].mRealIndex != mMaxCount - 1)
                    hasSpace = true;

                int minIndex = GetWrapIndexSide(false);
                y = mDragTrans.InverseTransformPoint(mChildTranss[minIndex].position).y;
                if (y < mPanelHalfHeight && mChildContents[minIndex].mRealIndex > 0)
                    hasSpace = true;
            }
            if (!hasSpace)
                break;
            UpdateContent(null);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public int GetMaxRealIndex()
    {
        int wrapIndex = GetWrapIndexSide(true);
        if(wrapIndex != -1)
        {
            return mChildContents[wrapIndex].mRealIndex;
        }
        return -1;
    }

    private void CacheComponent()
    {
        if (!mInited)
        {
            mInited = true;
            mTrans = transform;
            mDragTrans = transform.parent.parent;
            mPanel = GetComponentInParent<UIPanel>();
            mScrollView = GetComponentInParent<UIScrollView>();
            mPanelTrans = mPanel.transform;
            mTrans.localPosition = new Vector3(mTrans.localPosition.x, mPanel.baseClipRegion.w / 2.0f, 0);
            mOnClipMove = UpdateContent;
            mPanel.onClipMove = mOnClipMove;
            mSpringPanel = mPanel.gameObject.AddMissingComponent<SpringPanel>();
        }
        mSpringPanel.enabled = false;
        mScrollView.currentMomentum = Vector3.zero;
        mPanelHalfHeight = mPanel.baseClipRegion.w / 2.0f;
    }

    private void UpdateContent(UIPanel panel)
    {
        UnityEngine.Profiling.Profiler.BeginSample("UpdateContentMove");
        if (mMaxCount == 0)
            return;
        {
            //找出上边界的ITEM
            int maxWrapIndex = GetWrapIndexSide(true);
            if(mChildContents[maxWrapIndex].mRealIndex != mMaxCount - 1)
            {
                Transform t = mChildTranss[maxWrapIndex];
                UIWidget w = mChildWidgets[maxWrapIndex];
                ContentData c = mChildContents[maxWrapIndex];
                float y = mDragTrans.InverseTransformPoint(t.position).y;

                if(mDataAlign == Align.Top)
                {
                    if (y < mPanelHalfHeight)
                    {
                        //找出ID最小的移上去
                        int minIndex = GetWrapIndexForMove(false);
                        Transform t1 = mChildTranss[minIndex];
                        UIWidget w1 = mChildWidgets[minIndex];
                        ContentData c1 = mChildContents[minIndex];
                        GameObject g1 = mChildObjss[minIndex];

                        c1.mRealIndex = c.mRealIndex + 1;
                        mOnInit(g1, minIndex, c1.mRealIndex);

                        t1.localPosition = t.localPosition + new Vector3(0, w.height / 2.0f + w1.height / 2.0f, 0);
                    }
                }
                else
                {
                    if (y > -mPanelHalfHeight)
                    {
                        //找出ID最小的移下来
                        int minIndex = GetWrapIndexForMove(false);
                        Transform t1 = mChildTranss[minIndex];
                        UIWidget w1 = mChildWidgets[minIndex];
                        ContentData c1 = mChildContents[minIndex];
                        GameObject g1 = mChildObjss[minIndex];

                        c1.mRealIndex = c.mRealIndex + 1;
                        mOnInit(g1, minIndex, c1.mRealIndex);

                        t1.localPosition = t.localPosition - new Vector3(0, w.height / 2.0f + w1.height / 2.0f, 0);
                    }
                }
            }
        }

        {
            //找出下边界的ITEM
            int minWrapIndex = GetWrapIndexSide(false);
            if (mChildContents[minWrapIndex].mRealIndex > 0)
            {
                Transform t = mChildTranss[minWrapIndex];
                UIWidget w = mChildWidgets[minWrapIndex];
                ContentData c = mChildContents[minWrapIndex];
                float y = mDragTrans.InverseTransformPoint(t.position).y;

                if(mDataAlign == Align.Top)
                {
                    if (y > -mPanelHalfHeight)
                    {
                        //找出ID最大的移下来
                        int maxIndex = GetWrapIndexForMove(true);
                        Transform t1 = mChildTranss[maxIndex];
                        UIWidget w1 = mChildWidgets[maxIndex];
                        ContentData c1 = mChildContents[maxIndex];
                        GameObject g1 = mChildObjss[maxIndex];

                        c1.mRealIndex = c.mRealIndex - 1;
                        mOnInit(g1, maxIndex, c1.mRealIndex);

                        t1.localPosition = t.localPosition - new Vector3(0, w.height / 2.0f + w1.height / 2.0f, 0);
                    }
                }
                else
                {
                    if (y < mPanelHalfHeight)
                    {
                        //找出ID最大的移上去
                        int maxIndex = GetWrapIndexForMove(true);
                        Transform t1 = mChildTranss[maxIndex];
                        UIWidget w1 = mChildWidgets[maxIndex];
                        ContentData c1 = mChildContents[maxIndex];
                        GameObject g1 = mChildObjss[maxIndex];

                        c1.mRealIndex = c.mRealIndex - 1;
                        mOnInit(g1, maxIndex, c1.mRealIndex);

                        t1.localPosition = t.localPosition + new Vector3(0, w.height / 2.0f + w1.height / 2.0f, 0);
                    }
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
        ResetTargetPosition();
    }

    private void ResetTargetPosition()
    {
        UnityEngine.Profiling.Profiler.BeginSample("ResetTargetPosition");
        mScrollView.InvalidateBounds();
        if (mSpringPanel != null && mSpringPanel.enabled)
        {
            UnityEngine.Profiling.Profiler.BeginSample("bounds");
            Bounds b = mScrollView.bounds;
            UnityEngine.Profiling.Profiler.EndSample();
            Vector3 constraint = mPanel.CalculateConstrainOffset(b.min, b.max);

            if (!mScrollView.canMoveHorizontally) constraint.x = 0f;
            if (!mScrollView.canMoveVertically) constraint.y = 0f;

            if (constraint.sqrMagnitude > 0.1f)
            {
                Vector3 pos = mPanelTrans.localPosition + constraint;
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                mSpringPanel.target = pos;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private int GetWrapIndexForMove(bool max)
    {
        for(int i = 0;i < mChildContents.Count;i++)
        {
            if(mChildContents[i].mRealIndex == -1)
            {
                return i;
            }
        }
        int wrapIndex = 0;
        for (int i = 1; i < mChildContents.Count; i++)
        {
            if(max)
            {
                wrapIndex = mChildContents[i].mRealIndex > mChildContents[wrapIndex].mRealIndex ? i : wrapIndex;
            }
            else
            {
                wrapIndex = mChildContents[i].mRealIndex < mChildContents[wrapIndex].mRealIndex ? i : wrapIndex;
            }
        }
        return wrapIndex;
    }

    private int GetWrapIndexSide(bool max)
    {
        if(max)
        {
            int maxIndex = -1;
            for(int i = 0;i < mChildContents.Count;i++)
            {
                if (mChildContents[i].mRealIndex == -1)
                    continue;
                if(maxIndex == -1)
                {
                    maxIndex = i;
                }
                else
                {
                    maxIndex = mChildContents[i].mRealIndex > mChildContents[maxIndex].mRealIndex ? i : maxIndex;
                }
            }
            return maxIndex;
        }
        else
        {
            int minIndex = -1;
            for (int i = 0; i < mChildContents.Count; i++)
            {
                if (mChildContents[i].mRealIndex == -1)
                    continue;
                if (minIndex == -1)
                {
                    minIndex = i;
                }
                else
                {
                    minIndex = mChildContents[i].mRealIndex < mChildContents[minIndex].mRealIndex ? i : minIndex;
                }
            }
            return minIndex;
        }
    }

    private int GetWrapIndexInit(int realIndex,bool findMin)
    {
        //优先找出当前id相等的ITEM
        for(int i = 0;i < mChildContents.Count;i++)
        {
            if(mChildContents[i].mRealIndex == realIndex)
            {
                return i;
            }
        }
        //找不到则找出id为-1,即没被使用的ITEM
        for (int i = 0; i < mChildContents.Count; i++)
        {
            if (mChildContents[i].mRealIndex == -1)
            {
                return i;
            }
        }
        if(findMin)
        {
            //找出id最小的ITEM
            int minID = -1;
            for (int i = 0; i < mChildContents.Count; i++)
            {
                if (minID == -1)
                {
                    minID = i;
                }
                else
                {
                    minID = mChildContents[i].mRealIndex < mChildContents[minID].mRealIndex ? i : minID;
                }
            }
            return minID;
        }
        else
        {
            //找出id最大的ITEM
            int maxID = -1;
            for (int i = 0; i < mChildContents.Count; i++)
            {
                if (maxID == -1)
                {
                    maxID = i;
                }
                else
                {
                    maxID = mChildContents[i].mRealIndex > mChildContents[maxID].mRealIndex ? i : maxID;
                }
            }
            return maxID;
        }
    }
}

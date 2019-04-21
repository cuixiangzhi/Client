using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    [System.Serializable]
    public class UIFrame
    {
        [SerializeField] private int mUIID;
        [SerializeField] private int mResID;
        [SerializeField] private int mLoadIndex;

        [SerializeField] private int mLayer;
        [SerializeField] private int mGroup;

        [SerializeField] private UIState mState;
        [SerializeField] private GameObject mRootGo;
        [SerializeField] private Transform mRootTrans;

        [SerializeField] private bool mSortedPanel = false;
        [SerializeField] private bool mSortedToggle = false;
        [SerializeField] private List<UIPanel> mPanels = null;
        [SerializeField] private List<UIToggle> mToggles = null;
        [SerializeField] private List<UIEvent> mEvents = null;

        [SerializeField] private bool mFocus = false;
        [SerializeField] private int mFocusID = -1;

        [System.Serializable]
        private enum UIState
        {
            InValid = -1,
            None = 0,
            Create = 1,
            Enable = 2,
            Disable = 3,
            Loading = 4,
        }

        public UIFrame(int uiID, int resID)
        {
            mUIID = uiID;
            mResID = resID;
            mState = UIState.None;
        }

        public void OnOpen()
        {
            if (mState == UIState.InValid) return;
            if (mRootGo != null)
            {
                mState = UIState.Enable;
                mRootGo.SetActive(true);
                UIMgr.Instance.OnEnable(this);
            }
            else
            {
                if (mState != UIState.Loading)
                {
                    mState = UIState.Loading;
                    mLoadIndex = ResMgr.Instance.LoadInstantiateObjectAsync(mResID, false, 1, OnLoad);
                }
            }
        }

        public void OnClose()
        {
            if (mState == UIState.InValid || mState == UIState.Disable) return;
            if (mState == UIState.Loading)
            {
                mState = UIState.None;
                ResMgr.Instance.StopLoading(mLoadIndex);
            }
            else
            {
                mState = UIState.Disable;
                mRootGo.SetActive(false);
                UIMgr.Instance.OnDisable(this);
            }
        }

        public void OnFocus(bool focus, int btnID)
        {
            mFocus = focus;
            mFocusID = btnID;
            if (mFocus)
            {
                for (int i = 0; i < mEvents.Count; i++)
                {
                    mEvents[i].col.enabled = mEvents[i].id == btnID;
                }
            }
            else
            {
                for (int i = 0; i < mEvents.Count; i++)
                {
                    mEvents[i].col.enabled = true;
                }
            }
        }

        public void OnLoad(int index, System.Object obj)
        {
            if (obj != null)
            {
                mState = UIState.Enable;
                mRootGo = obj as GameObject;
                mRootTrans = mRootGo.transform;
                mRootTrans.parent = UIMgr.Instance.UIRoot;
                mRootTrans.localPosition = Vector3.zero;
                mRootTrans.localRotation = Quaternion.identity;
                mRootTrans.localScale = Vector3.one;
                mPanels = new List<UIPanel>(mRootGo.GetComponentsInChildren<UIPanel>(true));
                mToggles = new List<UIToggle>(mRootGo.GetComponentsInChildren<UIToggle>(true));
                mEvents = new List<UIEvent>(mRootGo.GetComponentsInChildren<UIEvent>(true));
                for (int i = 0; i < mEvents.Count; i++)
                {
                    mEvents[i].frame = this;
                }
                mRootGo.name = mRootGo.name.Substring(0, mRootGo.name.IndexOf('('));
                mRootGo.SetActive(true);
                UIMgr.Instance.OnCreate(this);
                UIMgr.Instance.OnEnable(this);
            }
            else
            {
                mState = UIState.InValid;
            }
        }

        public Transform GetRoot()
        {
            return mRootTrans;
        }

        public int GetUIID()
        {
            return mUIID;
        }

        public int GetUILayer()
        {
            return mLayer;
        }

        public int GetUIGroup()
        {
            return mGroup;
        }

        public void SetUILayer(int layer, int group)
        {
            mLayer = layer;
            mGroup = group;
        }

        public void SetPanelDepth(int baseDepth)
        {
            //对panel排序
            if (!mSortedPanel)
            {
                mSortedPanel = true;
                mPanels.Sort((a, b) =>
                {
                    if (a.depth != b.depth) return a.depth.CompareTo(b.depth);
                    else if (a != b) return a.GetInstanceID().CompareTo(b.GetInstanceID());
                    else return -1;
                });
            }
            //重置panel深度
            for (int i = 0; i < mPanels.Count; i++)
            {
                mPanels[i].useSortingOrder = true;
                mPanels[i].depth = baseDepth + i * 5;
                mPanels[i].sortingOrder = mPanels[i].depth;
                mRootTrans.localPosition = new Vector3(0, 0, 2000 - mPanels[i].depth);
            }
        }

        public void SetToggleGroup(int baseGroup)
        {
            //对toggle排序
            if (!mSortedToggle)
            {
                mSortedToggle = true;
                mToggles.Sort((a, b) =>
                {
                    if (a.group != b.group) return a.group.CompareTo(b.group);
                    else if (a != b) return a.GetInstanceID().CompareTo(b.GetInstanceID());
                    else return -1;
                });
            }
            //重置toggle组
            for (int i = 0; i < mToggles.Count; i++)
            {
                if (mToggles[i].group != 0)
                {
                    mToggles[i].group += baseGroup;
                }
            }
        }

        public Transform Find(string childPath)
        {
            return mRootTrans.Find(childPath);
        }

        public Component FindComponent(string comType, string comPath)
        {
            Transform trans = mRootTrans.Find(comPath);
            if (trans == null) return null;
            return trans.GetComponent(comType);
        }

        public Transform DuplicateAndAdd(Transform prefab,Transform parent,int id)
        {
            GameObject dup = GameObject.Instantiate(prefab.gameObject, parent, true);
            return dup.transform;
        }
    }
}

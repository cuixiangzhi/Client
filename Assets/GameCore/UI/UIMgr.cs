using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class UIMgr : BaseMgr<UIMgr>
    {
        #region 全局信息
        private Dictionary<int, UIFrame> mUIFrames = new Dictionary<int, UIFrame>();

        private Transform mUIRoot;
        public Transform UIRoot
        {
            get
            {
                if(mUIRoot == null)
                {
                    mUIRoot = GameObject.Find("UI Root").transform;
                    GameObject.DontDestroyOnLoad(mUIRoot.gameObject);
                }
                return mUIRoot;
            }
        }
        #endregion

        #region UI开关

        public void ShowUI(int uiID, int resID)
        {
            UIFrame frame = null;
            if (!mUIFrames.TryGetValue(uiID, out frame))
            {
                frame = new UIFrame(uiID,resID);
                mUIFrames[uiID] = frame;
            }
            frame.OnOpen();
        }

        public void UnShowUI(int uiID)
        {
            UIFrame frame = null;
            if (mUIFrames.TryGetValue(uiID, out frame))
            {
                frame.OnClose();
            }
        }

        public void UnShowAllUI()
        {
            foreach(var frameKV in mUIFrames)
            {
                frameKV.Value.OnClose();
            }
        }
        #endregion

        #region UI事件

        private LuaFunction mOnCreate;
        private LuaFunction mOnEnable;
        private LuaFunction mOnDisable;

        private LuaFunction mOnPress;
        private LuaFunction mOnSelect;
        private LuaFunction mOnClick;

        private LuaFunction mOnDragStart;
        private LuaFunction mOnDrag;
        private LuaFunction mOnDragEnd;

        private LuaFunction mOnDoubleClick;

        public void Init(LuaFunction onCreate, LuaFunction onEnable, LuaFunction onDisable,
                         LuaFunction onPress, LuaFunction onSelect, LuaFunction onClick,
                         LuaFunction onDragStart, LuaFunction onDrag, LuaFunction onDragEnd,
                         LuaFunction onDoubleClick)
        {
            mOnCreate = onCreate;
            mOnEnable = onEnable;
            mOnDisable = onDisable;

            mOnPress = onPress;
            mOnSelect = onSelect;
            mOnClick = onClick;

            mOnDragStart = onDragStart;
            mOnDrag = onDrag;
            mOnDragEnd = onDragEnd;

            mOnDoubleClick = onDoubleClick;

            mUIRoot = UIRoot;
        }

        public void FocusEvent(bool focus, int uiID, int btnID)
        {
            UIFrame frame = null;
            if (mUIFrames.TryGetValue(uiID, out frame))
            {
                frame.OnFocus(focus, btnID);
            }
        }

        public void OnCreate(UIFrame frame)
        {
            mOnCreate.BeginPCall();
            mOnCreate.Push(frame.GetUIID());
            mOnCreate.Push(frame);
            mOnCreate.PCall();
            mOnCreate.EndPCall();
        }

        public void OnEnable(UIFrame frame)
        {
            int layer = frame.GetUILayer();
            int group = frame.GetUIGroup();
            if (layer > 0)
            {
                foreach(var ui in mUIFrames)
                {
                    int uiLayer = ui.Value.GetUILayer();
                    int uiGroup = ui.Value.GetUIGroup();
                    //关闭非同组 自动控制开关的高级界面
                    if (ui.Value != frame && uiLayer >= layer && (group != uiGroup || uiGroup == 0))
                    {
                        ui.Value.OnClose();
                    }
                }
            }
            mOnEnable.BeginPCall();
            mOnEnable.Push(frame.GetUIID());
            mOnEnable.PCall();
            mOnEnable.EndPCall();
        }

        public void OnDisable(UIFrame frame)
        {
            mOnDisable.BeginPCall();
            mOnDisable.Push(frame.GetUIID());
            mOnDisable.PCall();
            mOnDisable.EndPCall();
        }

        public void OnPress(UIFrame frame, GameObject go, bool press, int btnID)
        {
            mOnPress.BeginPCall();
            mOnPress.Push(frame.GetUIID());
            mOnPress.Push(go);
            mOnPress.Push(press);
            mOnPress.Push(btnID);
            mOnPress.PCall();
            mOnPress.EndPCall();
        }

        public void OnSelect(UIFrame frame, GameObject go, bool select, int btnID)
        {
            mOnSelect.BeginPCall();
            mOnSelect.Push(frame.GetUIID());
            mOnSelect.Push(go);
            mOnSelect.Push(select);
            mOnSelect.Push(btnID);
            mOnSelect.PCall();
            mOnSelect.EndPCall();
        }

        public void OnClick(UIFrame frame, GameObject go, int btnID)
        {
            mOnClick.BeginPCall();
            mOnClick.Push(frame.GetUIID());
            mOnClick.Push(go);
            mOnClick.Push(btnID);
            mOnClick.PCall();
            mOnClick.EndPCall();
        }

        public void OnDragStart(UIFrame frame, GameObject go, int btnID)
        {
            mOnDragStart.BeginPCall();
            mOnDragStart.Push(frame.GetUIID());
            mOnDragStart.Push(go);
            mOnDragStart.Push(btnID);
            mOnDragStart.PCall();
            mOnDragStart.EndPCall();
        }

        public void OnDrag(UIFrame frame, GameObject go, Vector2 delta, int btnID)
        {
            mOnDrag.BeginPCall();
            mOnDrag.Push(frame.GetUIID());
            mOnDrag.Push(go);
            mOnDrag.Push(delta);
            mOnDrag.Push(btnID);
            mOnDrag.PCall();
            mOnDrag.EndPCall();
        }

        public void OnDragEnd(UIFrame frame, GameObject go, int btnID)
        {
            mOnDragEnd.BeginPCall();
            mOnDragEnd.Push(frame.GetUIID());
            mOnDragEnd.Push(go);
            mOnDragEnd.Push(btnID);
            mOnDragEnd.PCall();
            mOnDragEnd.EndPCall();
        }

        public void OnDoubleClick(UIFrame frame, GameObject go, int btnID)
        {
            mOnDoubleClick.BeginPCall();
            mOnDoubleClick.Push(frame.GetUIID());
            mOnDoubleClick.Push(go);
            mOnDoubleClick.Push(btnID);
            mOnDoubleClick.PCall();
            mOnDoubleClick.EndPCall();
        }
        #endregion
    }
}


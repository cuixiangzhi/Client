using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

namespace GameCore
{
    public sealed class UIMgr : BaseMgr<UIMgr>
    {
        private Dictionary<int, UIBehaviour> mUIOpened = new Dictionary<int, UIBehaviour>(512);

        public void Init()
        {

        }

        public void Exit()
        {
            
        }

        public void Show(int uiID,string uiName)
        {
            UIBehaviour ui = null;
            if(!mUIOpened.TryGetValue(uiID,out ui))
            {
                mUIOpened[uiID] = new UIBehaviour(uiID,uiName);
                ui.OnCreate();
            }
            else
            {
                ui.OnEnable();
            }
        }

        public void UnShow(int uiID)
        {
            UIBehaviour ui = null;
            if (mUIOpened.TryGetValue(uiID, out ui))
            {
                ui.OnDisable();
            }
        }

        public void FocusEvent()
        {

        }

        public void UnFocusEvent()
        {

        }

        public void LockEvent()
        {
            UICamera.ignoreAllEvents = true;
        }

        public void UnLockEvent()
        {
            UICamera.ignoreAllEvents = false;
        }
    }
}

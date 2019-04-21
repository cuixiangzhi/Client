using UnityEngine;
using cyou.ldj.sdk;

namespace GameCore
{
	public class GameStart : MonoBehaviour
	{
        private GameObject mGo;

        private void Start()
        {
            mGo = gameObject;
            DontDestroyOnLoad(mGo);
            ResMgr.Instance.Init(mGo);
            LuaMgr.Instance.Init(mGo);
            NetMgr.Instance.Init(mGo);
            LuaMgr.Instance.DoFile("GameStart.lua");
            LuaMgr.Instance.CallLuaFunc("StartGame");
        }

        private void Update()
        {
            ResMgr.Instance.Update();
            LuaMgr.Instance.Update();
            NetMgr.Instance.Update();
        }

        private void LateUpdate()
        {
            ResMgr.Instance.LateUpdate();
            LuaMgr.Instance.LateUpdate();
            NetMgr.Instance.LateUpdate();
        }

        private void FixedUpdate()
        {
            ResMgr.Instance.FixedUpdate();
            LuaMgr.Instance.FixedUpdate();
            NetMgr.Instance.FixedUpdate();
        }

        private void OnApplicationFocus(bool focus)
        {
            
        }

        private void OnApplicationPause(bool pause)
        {
            
        }

        private void OnApplicationQuit()
        {
            LuaMgr.Instance.CallLuaFunc("StopGame");
            ResMgr.Instance.Exit();
            LuaMgr.Instance.Exit();
        }
    }
}

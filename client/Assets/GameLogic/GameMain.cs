using GameFrameWork;
using System.IO;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using SceneMgr = UnityEngine.SceneManagement.SceneManager;

namespace GL
{
    public class GameMain : GameLogic
    {
        private void Awake()
        {
            GameFrameWork.GameFrameWork.Init(this);
            LuaManager.DoFile("GameMain");
        }

        public override void Loop()
        {

        }

        public override void LateLoop()
        {

        }

        public override void FixedLoop()
        {

        }

        public override void Exit()
        {

        }
    }
}

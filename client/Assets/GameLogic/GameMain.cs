using GameFrameWork;
using System.IO;
using UnityEngine;

namespace GL
{
    public class GameMain : GameLogic
    {
        private void Awake()
        {
            //框架初始化
            GameFrameWork.GameFrameWork.Init(this);
            //游戏逻辑初始化
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

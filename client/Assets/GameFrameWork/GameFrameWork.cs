using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UEP = UnityEngine.Profiling.Profiler;

namespace GF
{
    public sealed class GameFrameWork : MonoBehaviour
    {
        private GameLogic mLogic = null;

        public static void Init(GameLogic logic)
        {
#if UNITY_EDITOR
            System.GC.Collect();
#endif          
            DontDestroyOnLoad(logic.gameObject);
            GameFrameWork framework = logic.gameObject.AddMissingComponent<GameFrameWork>();
            framework.mLogic = logic;
            framework.Init();         
        }

        private void Init()
        {
            //工具类初始化
            UEP.BeginSample("Profiler.Init();");
            Profiler.Init();
            UEP.EndSample();

            UEP.BeginSample("Logger.Init();");
            Logger.Init();
            UEP.EndSample();

            UEP.BeginSample("UIFollow.Init();");
            UIFollow.Init();
            UEP.EndSample();

            UEP.BeginSample("Timer.Init();");
            Timer.Init();
            UEP.EndSample();

            UEP.BeginSample("UtilDll.Init();");
            UtilDll.Init();
            UEP.EndSample();

            Logger.Log("game start: manager init");

            //运行环境初始化
            UEP.BeginSample("ResManager.Init();");
            ResManager.Init();
            UEP.EndSample();

            UEP.BeginSample("NetManager.Init();");
            NetManager.Init();
            UEP.EndSample();

            UEP.BeginSample("LuaManager.Init();");
            LuaManager.Init();
            UEP.EndSample();
        }

        private void Update()
        {
            //运行环境循环
            UEP.BeginSample("NetManager.Loop();");
            NetManager.Loop();
            UEP.EndSample();

            UEP.BeginSample("LuaManager.Loop();");
            LuaManager.Loop();
            UEP.EndSample();

            //游戏逻辑循环
            UEP.BeginSample("GameLogic.Loop();");
            mLogic.Loop();
            UEP.EndSample();
        }

        private void LateUpdate()
        {
            UEP.BeginSample("LuaManager.LateLoop();");
            LuaManager.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("ResManager.LateLoop();");
            ResManager.LateLoop();
            UEP.EndSample();

            //游戏逻辑循环后处理
            UEP.BeginSample("GameLogic.LateLoop();");
            mLogic.LateLoop();
            UEP.EndSample();

            //工具类循环
            UEP.BeginSample("Profiler.LateLoop();");
            Profiler.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("Logger.LateLoop();");
            Logger.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UIFollow.LateLoop();");
            UIFollow.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("Timer.LateLoop();");
            Timer.LateLoop();
            UEP.EndSample();
        }

        private void FixedUpdate()
        {
            UEP.BeginSample("LuaManager.FixedLoop();");
            LuaManager.FixedLoop();
            UEP.EndSample();

            //游戏逻辑循环定时处理
            UEP.BeginSample("GameLogic.FixedLoop();");
            mLogic.FixedLoop();
            UEP.EndSample();
        }

        private void OnApplicationQuit()
        {
            Logger.Log("game quit: manager exit");

            //游戏逻辑退出
            UEP.BeginSample("GameLogic.Exit();");
            mLogic.Exit();
            mLogic = null;
            UEP.EndSample();

            //运行环境退出
            UEP.BeginSample("NetManager.Exit();");
            NetManager.Exit();
            UEP.EndSample();

            UEP.BeginSample("LuaManager.Exit();");
            LuaManager.Exit();
            UEP.EndSample();

            UEP.BeginSample("ResManager.Exit();");
            ResManager.Exit();
            UEP.EndSample();

            //工具类退出
            UEP.BeginSample("Logger.Exit();");
            Logger.Exit();
            UEP.EndSample();

            UEP.BeginSample("UIFollow.Exit();");
            UIFollow.Exit();
            UEP.EndSample();

            UEP.BeginSample("Profiler.Exit();");
            Profiler.Exit();
            UEP.EndSample();

            UEP.BeginSample("Timer.Exit();");
            Timer.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilDll.Exit();");
            UtilDll.Exit();
            UEP.EndSample();
#if UNITY_EDITOR
            System.GC.Collect();
#endif
        }
    }
}

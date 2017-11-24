using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UEP = UnityEngine.Profiling.Profiler;

namespace GameCore
{
    public sealed class GameMain : MonoBehaviour
    {
        private void Start()
        {
#if UNITY_EDITOR
            System.GC.Collect();
#endif          
            DontDestroyOnLoad(gameObject);
            //工具类初始化
            UEP.BeginSample("Profiler.Init();");
            Profiler.Init();
            UEP.EndSample();

            UEP.BeginSample("LogMgr.Init();");
            LogMgr.Init();
            UEP.EndSample();

            UEP.BeginSample("UIFollow.Init();");
            UIFollow.Init();
            UEP.EndSample();

            UEP.BeginSample("Timer.Init();");
            Timer.Init();
            UEP.EndSample();

            UEP.BeginSample("DllMgr.Init();");
            DllMgr.Init();
            UEP.EndSample();

            LogMgr.Log("game start: manager init");

            //运行环境初始化
            UEP.BeginSample("ThreadManager.Init();");
            ThreadManager.Init();
            UEP.EndSample();

            UEP.BeginSample("ResManager.Init();");
            AssetManager.Init();
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

            //游戏逻辑循环
            UEP.BeginSample("LuaManager.Loop();");
            LuaManager.Loop();
            UEP.EndSample();
        }

        private void LateUpdate()
        {
            //游戏逻辑循环后处理
            UEP.BeginSample("LuaManager.LateLoop();");
            LuaManager.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("ResManager.LateLoop();");
            AssetManager.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("ThreadManager.LateLoop();");
            ThreadManager.LateLoop();
            UEP.EndSample();          

            //工具类循环
            UEP.BeginSample("Profiler.LateLoop();");
            Profiler.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("LogMgr.LateLoop();");
            LogMgr.LateLoop();
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
            //游戏逻辑循环定时处理
            UEP.BeginSample("LuaManager.FixedLoop();");
            LuaManager.FixedLoop();
            UEP.EndSample();          
        }

        private void OnApplicationQuit()
        {
            LogMgr.Log("game quit: manager exit");
            //游戏逻辑退出
            //运行环境退出
            UEP.BeginSample("NetManager.Exit();");
            NetManager.Exit();
            UEP.EndSample();

            UEP.BeginSample("LuaManager.Exit();");
            LuaManager.Exit();
            UEP.EndSample();

            UEP.BeginSample("ResManager.Exit();");
            AssetManager.Exit();
            UEP.EndSample();

            UEP.BeginSample("ThreadManager.Exit();");
            ThreadManager.Exit();
            UEP.EndSample();

            //工具类退出
            UEP.BeginSample("LogMgr.Exit();");
            LogMgr.Exit();
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

            UEP.BeginSample("DllMgr.Exit();");
            DllMgr.Exit();
            UEP.EndSample();
#if UNITY_EDITOR
            System.GC.Collect();
#endif
        }
    }
}

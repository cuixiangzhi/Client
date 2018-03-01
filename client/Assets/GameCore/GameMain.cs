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
            DontDestroyOnLoad(gameObject);
            InitCoreUtil();
            InitCoreLua();
        }

        private void Update()
        {
            UEP.BeginSample("LuaMgr.Loop();");
            LuaMgr.Loop();
            UEP.EndSample();
        }

        private void LateUpdate()
        {
            UEP.BeginSample("LuaMgr.LateLoop();");
            LuaMgr.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UtilUIFollow.LateLoop();");
            UtilUIFollow.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UtilTimer.LateLoop();");
            UtilTimer.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UtilProfiler.LateLoop();");
            UtilProfiler.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("AssetMgr.LateLoop();");
            AssetMgr.LateLoop();
            UEP.EndSample();
        }

        private void OnApplicationQuit()
        {
            ExitCoreLua();
            ExitCoreUtil();
        }

        private void InitCoreUtil()
        {
            UEP.BeginSample("UtilDll.Init();");
            UtilDll.Init();
            UEP.EndSample();

            UEP.BeginSample("UtilLog.Init();");
            UtilLog.Init();
            UEP.EndSample();

            UEP.BeginSample("UtilUIFollow.Init();");
            UtilUIFollow.Init();
            UEP.EndSample();

            UEP.BeginSample("UtilTimer.Init();");
            UtilTimer.Init();
            UEP.EndSample();

            UEP.BeginSample("UtilProfiler.Init();");
            UtilProfiler.Init();
            UEP.EndSample();

            UEP.BeginSample("AssetMgr.Init();");
            AssetMgr.Init();
            UEP.EndSample();
        }

        private void InitCoreLua()
        {
            UEP.BeginSample("LuaMgr.Init();");
            LuaMgr.Init();
            UEP.EndSample();
        }

        private void ExitCoreUtil()
        {
            UEP.BeginSample("AssetMgr.Exit();");
            AssetMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilUIFollow.Exit();");
            UtilUIFollow.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilTimer.Exit();");
            UtilTimer.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilLog.Exit();");
            UtilLog.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilDll.Exit();");
            UtilDll.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilProfiler.Exit();");
            UtilProfiler.Exit();
            UEP.EndSample();
        }

        private void ExitCoreLua()
        {
            UEP.BeginSample("LuaMgr.Exit();");
            LuaMgr.Exit();
            UEP.EndSample();
        }
    }
}

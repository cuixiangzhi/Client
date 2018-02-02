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

            UEP.BeginSample("DllMgr.Init();");
            DllMgr.Init();
            UEP.EndSample();

            UEP.BeginSample("LogMgr.Init();");
            LogMgr.Init();
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

            LogMgr.Log("game init begin");

            UEP.BeginSample("AssetMgr.Init();");
            AssetMgr.Init();
            UEP.EndSample();

            UEP.BeginSample("LuaMgr.Init();");
            LuaMgr.Init();
            UEP.EndSample();

            LogMgr.Log("game init finish");
        }

        private void Update()
        {
            UEP.BeginSample("LuaMgr.Loop();");
            LuaMgr.Loop();
            UEP.EndSample();
        }

        private void LateUpdate()
        {
            UEP.BeginSample("UtilUIFollow.LateLoop();");
            UtilUIFollow.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UtilTimer.LateLoop();");
            UtilTimer.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("UtilProfiler.LateLoop();");
            UtilProfiler.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("LuaMgr.LateLoop();");
            LuaMgr.LateLoop();
            UEP.EndSample();

            UEP.BeginSample("AssetMgr.LateLoop();");
            AssetMgr.LateLoop();
            UEP.EndSample();     

        }

        private void OnApplicationQuit()
        {
            LogMgr.Log("game exit begin");

            UEP.BeginSample("LuaMgr.Exit();");
            LuaMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("AssetMgr.Exit();");
            AssetMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilUIFollow.Exit();");
            UtilUIFollow.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilTimer.Exit();");
            UtilTimer.Exit();
            UEP.EndSample();

            UEP.BeginSample("LogMgr.Exit();");
            LogMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("DllMgr.Exit();");
            DllMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("UtilProfiler.Exit();");
            UtilProfiler.Exit();
            UEP.EndSample();

            LogMgr.Log("game exit finish");

#if UNITY_EDITOR
            System.GC.Collect();
#endif
        }
    }
}

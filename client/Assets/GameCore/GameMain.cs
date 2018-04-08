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

            UEP.BeginSample("LuaMgr.Init();");
			LuaMgr.Init();
			UEP.EndSample();
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
        }

        private void OnApplicationQuit()
        {
            UEP.BeginSample("LuaMgr.Exit();");
			LuaMgr.Exit();
			UEP.EndSample();
        }
    }
}

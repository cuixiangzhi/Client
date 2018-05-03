using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UEP = UnityEngine.Profiling.Profiler;

namespace GameCore
{
    public sealed class GameMain : MonoBehaviour
    {
        public static bool mInstance = false;

        private void Start()
        {
            if (mInstance)
            {
                Destroy(gameObject);
            }
            else
            {
                mInstance = true;

                DontDestroyOnLoad(gameObject);

                UEP.BeginSample("ResMgr.Init();");
                ResMgr.Init();
                UEP.EndSample();

                UEP.BeginSample("LuaMgr.Init();");
                UEP.EndSample();
            }
        }

        private void Update()
        {
            UEP.BeginSample("ResMgr.Loop();");
            ResMgr.Loop();
            UEP.EndSample();

            UEP.BeginSample("LuaMgr.Loop();");
            UEP.EndSample();
        }

        private void LateUpdate()
        {
            UEP.BeginSample("LuaMgr.LateLoop();");
            UEP.EndSample();
        }

        private void OnApplicationQuit()
        {
            UEP.BeginSample("ResMgr.Exit();");
            ResMgr.Exit();
            UEP.EndSample();

            UEP.BeginSample("LuaMgr.Exit();");
			UEP.EndSample();
        }
    }
}

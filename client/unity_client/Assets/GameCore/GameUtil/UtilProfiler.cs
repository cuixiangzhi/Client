using UnityEngine;
using System.IO;
using System.Diagnostics;
using UEP = UnityEngine.Profiling.Profiler;

namespace GameCore
{
    public static class UtilProfiler
    {
        [Conditional("ENABLE_PROFILER_FILE")]
        public static void Init()
        {
            if (Directory.Exists(GameConst.PROFILER_PATH))
                Directory.Delete(GameConst.PROFILER_PATH, true);
            Directory.CreateDirectory(GameConst.PROFILER_PATH);
        }

        [Conditional("ENABLE_PROFILER_FILE")]
        public static void LateLoop()
        {
            if (Time.frameCount % 300 == 1)
            {
                if (!UEP.enabled)
                {
                    UEP.enabled = true;
                }
                UEP.enableBinaryLog = false;
                UEP.logFile = string.Format("{0}/profiler.{1}_{2}", GameConst.PROFILER_PATH, Time.frameCount, Time.frameCount + 299);
                UEP.enableBinaryLog = true;
            }
        }

        [Conditional("ENABLE_PROFILER_FILE")]
        public static void Exit()
        {
            UEP.logFile = string.Empty;
            UEP.enableBinaryLog = false;
            UEP.enabled = false;
        }
    }
}


using com.cyou.plugin.log;
using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace com.cyou.plugin.resource.loader.bundle
{
    class CleanupTask : ICleanupTask
    {
        
        public static Int32 MemCleanSize = 240;

        public static float MaxMono = 1024;

        
        public static Int32 MemoryTimeInterval = 300;


        
        public static Int32 MonoCleanSize = 100;//MB 超过这个size不在清理。
        public static String GCMonoKey = "GCMono";//MB
        public static bool GCMono = true;//MB
        public static float DropFrameTime = 600;//second
        public static Int32 DropFrame = 20;




        private bool memClean = false;
        private float memCleanTime = -1;
        private bool forceMemClean = false;


        private Int32 TotalReservedMemory = -1;
        private Int32 TotalReservedMemoryTime = -1;
        //private Int32 SampTime = -1;
        //protected int InvTime = 10;
        protected int waitMonoFrame = -1;

        private float lastGCTime = -1;

        private ResourceLoader m_ResourceLoader;
        public void SetResourceLoader(ResourceLoader resourceLoader)
        {
            m_ResourceLoader = resourceLoader;
        }
        public bool TickCleanup(uint uDeltaTimeMS)
        {
            if (forceMemClean)
            {
                m_ResourceLoader.ForceAsyncEnd();
                forceMemClean = false;
                //CachePool.CleanAllCacheData();
                BundlePool.CleanAllUnusedCacheData();
                return true;
            }

            Int32 time = (Int32)Time.realtimeSinceStartup;
            if (time - memCleanTime > 10)
            {
                memClean = true;
                memCleanTime = time;
            }
            TotalReservedMemory = (Int32)(Profiler.GetTotalReservedMemory() / (1048576.0f));
            Int32 TotalMonoMemory = (Int32)(GC.GetTotalMemory(false) / (1048576.0f));
            TotalReservedMemory += TotalMonoMemory;
            
            if (TotalReservedMemory > MemCleanSize)
            {
                if (((time - TotalReservedMemoryTime) > MemoryTimeInterval))
                {
                    Cleanup();
                    TotalReservedMemoryTime = (Int32)Time.realtimeSinceStartup;
                    return true;
                }
            }
            if (waitMonoFrame > 0)
            {
                waitMonoFrame--;
                if (waitMonoFrame == 0)
                {
                    waitMonoFrame = -1;
                    if (TotalMonoMemory > MaxMono)
                    {
                        if (Debug.unityLogger.logEnabled == false)
                        {
                            LogSystem.Error("Mono Memory!");
                            // Application.Quit();
                        }
                    }
                }
                return true;
            }
            else
            {
                if (TotalMonoMemory > MaxMono)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    waitMonoFrame = 10;
                    return true;
                }
            }
            if (GCMono)
            {
                if (Time.realtimeSinceStartup - lastGCTime > 60)
                {
                    lastGCTime = Time.realtimeSinceStartup;
                    int size = MonoCleanSize;
                    if (size <= 0)
                    {
                        return true;
                    }
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                }
            }
            return false;
        }
        public void Cleanup()
        {
            forceMemClean = true;
            memClean = true;
            memCleanTime = Time.realtimeSinceStartup;
        }
        public void ForceCleanup()
        {

        }

        public void Release()
        {
            waitMonoFrame = -1;
            lastGCTime = Time.realtimeSinceStartup + 60;
            Cleanup();
        }
    }
}

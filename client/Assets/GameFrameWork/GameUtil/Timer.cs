using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GF
{
    public static class Timer
    {
        private class TimerInfo
        {
            public int leftCount = 0;
            public float duration = 0;
            public float leftDuration = 0;
            public int id = 0;
            public bool pause = false;
            public bool removeFlag = false;
        }

        private static List<TimerInfo> mUsed = new List<TimerInfo>();

        private static Queue<TimerInfo> mUnUsed = new Queue<TimerInfo>();

        private static int mTimerID = 0;

        private static bool mUpdateFlag = false;

        public delegate void TimerCallBack(int id);

        public static TimerCallBack ON_RM_TIMER = null;

        public static TimerCallBack ON_TICK_FINISH = null;

        public static void Init()
        {
            mTimerID = 0;
            mUpdateFlag = false;
        }

        public static void LateLoop()
        {
            if (mUsed.Count <= 0)
                return;

            mUpdateFlag = true;
            int totalCount = mUsed.Count;
            for (int i = 0; i < totalCount; i++)
            {
                if (!mUsed[i].pause && !mUsed[i].removeFlag)
                {
                    mUsed[i].leftDuration -= Time.deltaTime;
                    if (mUsed[i].leftDuration <= 0)
                    {
                        if (ON_TICK_FINISH != null)
                            ON_TICK_FINISH(mUsed[i].id);
                        --mUsed[i].leftCount;
                        if (mUsed[i].leftCount <= 0)
                            mUsed[i].removeFlag = true;
                        else
                            mUsed[i].leftDuration = mUsed[i].duration;
                    }
                }
            }
            mUpdateFlag = false;
            for (int i = 0; i < mUsed.Count; i++)
            {
                if (mUsed[i].removeFlag)
                {
                    RemoveTimer(mUsed[i]);
                    mUsed.RemoveAt(i--);
                }
            }
        }

        public static void Exit()
        {
            mUsed.Clear();
            mUnUsed.Clear();
            ON_RM_TIMER = null;
            ON_TICK_FINISH = null;
        }

        public static int AddTimer(float duration, int count)
        {
            TimerInfo curInfo = null;
            if (mUnUsed.Count != 0)
            {
                curInfo = mUnUsed.Dequeue();
            }
            else
            {
                curInfo = new TimerInfo();
            }
            curInfo.leftCount = count;
            curInfo.duration = duration;
            curInfo.leftDuration = duration;
            curInfo.id = mTimerID++;
            curInfo.pause = false;
            curInfo.removeFlag = false;
            mUsed.Add(curInfo);
            return curInfo.id;
        }

        public static void DeleteTimer(int id)
        {
            for (int i = 0; i < mUsed.Count; i++)
            {
                if (mUsed[i].id == id)
                {
                    if (mUpdateFlag)
                    {
                        mUsed[i].removeFlag = true;
                    }
                    else
                    {
                        RemoveTimer(mUsed[i]);
                        mUsed.RemoveAt(i);
                    }
                    break;
                }
            }
        }

        public static void PauseTimer(int id, bool pause)
        {
            for (int i = 0; i < mUsed.Count; i++)
            {
                if (mUsed[i].id == id)
                {
                    mUsed[i].pause = true;
                    break;
                }
            }
        }

        private static void RemoveTimer(TimerInfo tm)
        {
            mUnUsed.Enqueue(tm);
            if (ON_RM_TIMER != null)
            {
                ON_RM_TIMER(tm.id);
            }
        }
    }
}
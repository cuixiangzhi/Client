using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public static class UtilTimer
    {
        private class TimerInfo
        {
            public int leftCount = 0;
            public float duration = 0;
            public float leftDuration = 0;
            public int id = 0;
            public bool pause = false;
            public bool removeFlag = false;
            public TimerCallBack callBack = null;
        }

        private static List<TimerInfo> mUsed = new List<TimerInfo>();

        private static Queue<TimerInfo> mUnUsed = new Queue<TimerInfo>();

        private static int mTimerID = 0;

        private static bool mUpdateFlag = false;

        public delegate void TimerCallBack(int id);

        public static void Init()
        {
            mTimerID = 0;
            mUpdateFlag = false;
            mUsed.Clear();
            mUnUsed.Clear();
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
        }

        public static int AddTimer(float duration, int count, TimerCallBack callBack)
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
            curInfo.callBack = callBack;
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

        public static void DeleteTimer(TimerCallBack callBack)
        {
            for (int i = 0; i < mUsed.Count; i++)
            {
                if (mUsed[i].callBack == callBack)
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

        public static void PauseTimer(TimerCallBack callBack, bool pause)
        {
            for (int i = 0; i < mUsed.Count; i++)
            {
                if (mUsed[i].callBack == callBack)
                {
                    mUsed[i].pause = pause;
                    break;
                }
            }
        }

        private static void RemoveTimer(TimerInfo tm)
        {
            tm.callBack = null;
            mUnUsed.Enqueue(tm);
        }
    }
}
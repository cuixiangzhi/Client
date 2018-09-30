using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LuaInterface;

namespace TimeUtil
{
    public class Timer : MonoBehaviour
    {
        private static List<TimerInfo> used = new List<TimerInfo>();

        private static Queue<TimerInfo> unused = new Queue<TimerInfo>();

        private static int TIMER_ID = 0;

        private static bool UPDATE_FLAG = false;

        public delegate void TimerCallBack(int id);

        public static TimerCallBack ON_RM_TIMER = null;

        public static TimerCallBack ON_TICK_FINISH = null;

        private class TimerInfo
        {
            public int leftCount = 0;
            public float duration = 0;
            public float leftDuration = 0;
            public int id = 0;
            public bool pause = false;
            public bool removeFlag = false;
        }

        public static int AddTimer(float duration, int count)
        {
            TimerInfo curInfo = null;
            if (unused.Count != 0)
            {
                curInfo = unused.Dequeue();
            }
            else
            {
                curInfo = new TimerInfo();
            }
            curInfo.leftCount = count;
            curInfo.duration = duration;
            curInfo.leftDuration = duration;
            curInfo.id = ++TIMER_ID;
            curInfo.pause = false;
            curInfo.removeFlag = false;
            used.Add(curInfo);
            return curInfo.id;
        }

        public static void DeleteTimer(int id)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].id == id)
                {
                    if (UPDATE_FLAG)
                    {
                        used[i].removeFlag = true;
                    }
                    else
                    {
                        RemoveTimer(used[i]);
                        used.RemoveAt(i);
                    }
                    break;
                }
            }
        }

        public static void PauseTimer(int id, bool pause)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].id == id)
                {
                    used[i].pause = true;
                    break;
                }
            }
        }

        void Update()
        {
            if (used.Count <= 0)
                return;

            UPDATE_FLAG = true;
            int totalCount = used.Count;
            for (int i = 0; i < totalCount; i++)
            {
                if (!used[i].pause && !used[i].removeFlag)
                {
                    used[i].leftDuration -= Time.deltaTime;
                    if (used[i].leftDuration <= 0)
                    {
                        if (ON_TICK_FINISH != null)
                            ON_TICK_FINISH(used[i].id);
                        --used[i].leftCount;
                        if (used[i].leftCount <= 0)
                            used[i].removeFlag = true;
                        else
                            used[i].leftDuration = used[i].duration;
                    }
                }
            }
            UPDATE_FLAG = false;
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].removeFlag)
                {
                    RemoveTimer(used[i]);
                    used.RemoveAt(i--);
                }
            }
        }

        private static void RemoveTimer(TimerInfo tm)
        {
            unused.Enqueue(tm);
            if(ON_RM_TIMER != null)
            {
                ON_RM_TIMER(tm.id);
            }
        }

        private void OnDestroy()
        {
            used.Clear();
            ON_RM_TIMER = null;
            ON_TICK_FINISH = null;
        }
    }
}
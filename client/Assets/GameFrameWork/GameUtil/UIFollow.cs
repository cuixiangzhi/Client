using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameWork
{
    public static class UIFollow
    {
        private static Camera mGameCamera = null;

        private static Camera mUICamera = null;

        private static List<FollowData> mFollowList = null;

        private struct FollowData
        {
            public Transform mSource;
            public GameObject mSourceObj;
            public Vector3 mSourceOffset;
            public Transform mTarget;
            public Vector3 mTargetOffset;
        }

        public static void Init()
        {
            mFollowList = new List<FollowData>(32);
        }

        public static void LateLoop()
        {
            if (mFollowList == null || mFollowList.Count == 0)
            {
                return;
            }
            if(mGameCamera == null)
            {
                mGameCamera = Camera.main;
            }
            if(mUICamera == null && UICamera.current != null)
            {
                mUICamera = UICamera.current.cachedCamera;
            }

            if(mGameCamera == null || mUICamera == null)
            {
                return;
            }

            for(int i = 0;i < mFollowList.Count;i++)
            {
                if(mFollowList[i].mSource == null || mFollowList[i].mSourceObj == null)
                {
                    LogMgr.LogError("follow source is null!");
                    mFollowList.RemoveAt(i--);
                    continue;
                }
                if(mFollowList[i].mTarget == null)
                {
                    LogMgr.LogError("follow target is null {0}!", mFollowList[i].mSource.name);
                    mFollowList.RemoveAt(i--);
                    continue;
                }

                if(!mFollowList[i].mSourceObj.activeInHierarchy)
                {
                    continue;
                }

                Vector3 pos = mGameCamera.WorldToScreenPoint(mFollowList[i].mTarget.position + mFollowList[i].mTargetOffset);
                pos.z = 0;
                mFollowList[i].mSource.position = mUICamera.ScreenToWorldPoint(pos);
                mFollowList[i].mSource.localPosition += mFollowList[i].mSourceOffset;
            }
        }

        public static void Exit()
        {
            mGameCamera = null;
            mUICamera = null;
            mFollowList.Clear();
        }

        public static void AddFollow(Transform source,Transform target,Vector3 sourceOffset,Vector3 targetOffset)
        {
            //结构体分配的是栈内存,给Listadd数据时,会拷贝到List里,list的数据是在堆上创建的
            FollowData data = new FollowData();
            data.mSource = source;
            data.mSourceObj = source.gameObject;
            data.mSourceOffset = sourceOffset;
            data.mTarget = target;
            data.mTargetOffset = targetOffset;

            mFollowList.Add(data);
        }

        public static void RemoveFollow(Transform source)
        {
            for (int i = 0; i < mFollowList.Count; i++)
            {
                if (mFollowList[i].mSource == source)
                {
                    mFollowList.RemoveAt(i--);
                    break;
                }
            }
        }
    }
}

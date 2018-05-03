using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Cache
{
    //public class CacheObject
    //{
    //    private string mFileName;
    //    private WeakReference mWeakLoadObj = null;
    //    private GameObject mLoadObj = null;
    //    private List<UnityObj> mSpawnObjs = new List<UnityObj>(16);
    //    private List<UnityObj> mDeSpawnObjs = new List<UnityObj>(16);
    //    private float mLastUseTime = 0;
    //    private float MAX_CACHE_TIME = 60;

    //    public bool IsActive()
    //    {
    //        if (mLoadObj != null)
    //        {
    //            for (int i = 0; i < mSpawnObjs.Count; i++)
    //            {
    //                if (mSpawnObjs[i] != null)
    //                    return true;
    //                else
    //                    mSpawnObjs.RemoveAt(i--);
    //            }
    //            float passedTime = Time.time - mLastUseTime;
    //            return passedTime >= MAX_CACHE_TIME;
    //        }
    //        else
    //        {
    //            return mWeakLoadObj.IsAlive;
    //        }
    //    }

    //    public bool Contains(UnityObj obj)
    //    {
    //        if (mLoadObj != null)
    //        {
    //            return mSpawnObjs.Contains(obj) || mDeSpawnObjs.Contains(obj);
    //        }
    //        else
    //        {
    //            return (mWeakLoadObj.Target as UnityObj) == obj;
    //        }
    //    }

    //    public UnityObj GetObj()
    //    {
    //        if (mLoadObj != null)
    //        {
    //            GameObject obj = null;
    //            for (int i = 0; i < mDeSpawnObjs.Count; i++)
    //            {
    //                if (mDeSpawnObjs[i] != null)
    //                {
    //                    obj = mDeSpawnObjs[i] as GameObject;
    //                    mDeSpawnObjs.RemoveAt(i);
    //                    break;
    //                }
    //                else
    //                {
    //                    mDeSpawnObjs.RemoveAt(i--);
    //                }
    //            }
    //            if (obj == null)
    //            {
    //                obj = UnityObj.Instantiate<GameObject>(mLoadObj);
    //            }
    //            obj.transform.parent = ResMgr.mPoolInstance;
    //            mSpawnObjs.Add(obj);
    //            return obj;
    //        }
    //        else
    //        {
    //            return mWeakLoadObj.Target as UnityObj;
    //        }
    //    }

    //    public void SetObj(UnityObj obj)
    //    {
    //        if (obj is GameObject)
    //        {
    //            mLoadObj = obj as GameObject;
    //            mLoadObj.transform.parent = ResMgr.mPoolLoad;
    //            mLoadObj.SetActive(false);
    //        }
    //        else
    //        {
    //            mWeakLoadObj = new WeakReference(obj);
    //        }
    //    }

    //    public void ClearObj()
    //    {
    //        if (mLoadObj != null)
    //        {
    //            for (int i = 0; i < mDeSpawnObjs.Count; i++)
    //            {
    //                if (mDeSpawnObjs[i] != null)
    //                {
    //                    UnityObj.Destroy(mDeSpawnObjs[i]);
    //                }
    //            }
    //            if (mSpawnObjs.Count != 0)
    //            {
    //                LogMgr.LogError("pool has spawned object when clear !!");
    //            }
    //            mDeSpawnObjs.Clear();
    //            mSpawnObjs.Clear();
    //            mLoadObj = null;
    //            PkgMgr.UnloadDependBundle(mFileName);
    //        }
    //    }

    //    public void RecycleObj(UnityObj obj)
    //    {
    //        if (mLoadObj != null)
    //        {
    //            if (mSpawnObjs.Remove(obj))
    //                mDeSpawnObjs.Add(obj);
    //        }
    //        mLastUseTime = Time.time;
    //    }
    //}
}

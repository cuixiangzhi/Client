//using UnityEngine;
//using System.Collections;
//using System;

namespace GameCore.Action
{

    //public class SoundPlayerMonitor : MonoBehaviour 
    //{
    //	public AudioSource source;

    //    public string mSoundPath;

    //    public int mguid = -1;

    //    private bool mFadeIn;
    //    private bool mFadeOut;

    //    private float mVolume = 1;
    //    private float mFadeTime = 1.0f;

    //    private Coroutine mPlayCor;
    //    private Coroutine mStopCor;

    //    public int SoundID
    //    {
    //        get;
    //        private set;
    //    }

    //    void Awake()
    //    {
    //        source = GetComponent<AudioSource>();
    //    }

    //    //在某个点播放3D音效
    //    public void Init(SoundData.SoundData sdata, Vector3 pos, int guid, bool mute)
    //    {
    //        SoundID = sdata.ID;
    //        mguid = guid;
    //        mFadeIn = sdata.IsFadeIn;
    //        mFadeOut = sdata.IsFadeOut;
    //        mVolume = sdata.Volume / 100.0f;
    //        source.mute = mute;
    //        source.loop = sdata.IsLoop;
    //        source.rolloffMode = AudioRolloffMode.Linear;
    //        source.dopplerLevel = 0;
    //        transform.position = pos;
    //        transform.localEulerAngles = Vector3.zero;
    //        transform.localScale = Vector3.one;
    //        if (transform.parent == null || transform.parent.gameObject.activeSelf)
    //        {
    //            mPlayCor = StartCoroutine(Play());
    //        }
    //    }

    //    //挂在某个物体下的3D音效
    //    public void Init(SoundData.SoundData sdata, Transform parent, int guid, bool mute)
    //    {
    //        SoundID = sdata.ID;
    //        mguid = guid;
    //        mFadeIn = sdata.IsFadeIn;
    //        mFadeOut = sdata.IsFadeOut;
    //        mVolume = sdata.Volume / 100.0f;
    //        source.mute = mute;
    //        source.loop = sdata.IsLoop;
    //        source.rolloffMode = AudioRolloffMode.Linear;
    //        source.dopplerLevel = 0;
    //        transform.parent = parent;
    //        transform.localPosition = Vector3.zero;
    //        transform.localEulerAngles = Vector3.zero;
    //        transform.localScale = Vector3.one;
    //        if (transform.parent == null || transform.parent.gameObject.activeSelf)
    //        {
    //            mPlayCor = StartCoroutine(Play());
    //        }
    //    }

    //    //2D音效或背景音乐
    //    public void Init(SoundData.SoundData sdata,int guid,bool mute)
    //    {
    //        SoundID = sdata.ID;
    //        mguid = guid;
    //        mSoundPath = sdata.SoundPath;
    //        mFadeIn = sdata.IsFadeIn;
    //        mFadeOut = sdata.IsFadeOut;
    //        mVolume = sdata.Volume / 100.0f;
    //        source.mute = mute;
    //        source.loop = sdata.IsLoop;
    //        source.rolloffMode = AudioRolloffMode.Linear;
    //        source.dopplerLevel = 0;
    //        transform.localPosition = Vector3.zero;
    //        transform.localEulerAngles = Vector3.zero;
    //        transform.localScale = Vector3.one;
    //        if (transform.parent == null || transform.parent.gameObject.activeSelf)
    //        {
    //            mPlayCor = StartCoroutine(Play());
    //        }
    //    }

    //    private IEnumerator Play()
    //    {
    //        if (mStopCor != null)
    //        {
    //            StopCoroutine(mStopCor);
    //            mStopCor = null;
    //        }

    //        if (mFadeIn)
    //        {
    //            //从0开始淡入,根据淡入时长计算递增值
    //            float tmpVol = 0;
    //            float deltaVol = mVolume / mFadeTime;

    //            source.volume = tmpVol;
    //            source.Play();
    //            while (tmpVol < mVolume)
    //            {
    //                tmpVol += Time.deltaTime * deltaVol;
    //                source.volume = tmpVol;
    //                yield return null;
    //            }
    //        }
    //        else
    //        {
    //            source.volume = mVolume;
    //            source.Play();
    //        }

    //        while (source != null && source.isPlaying)
    //        {
    //            yield return null;
    //        }

    //        mPlayCor = null;

    //        SoundManager.Instance.StopSound(this);
    //    }

    //    //停止播放
    //    public void Stop(Action cb = null)
    //    {
    //        if (this != null && this.gameObject != null && this.gameObject.activeInHierarchy)
    //            mStopCor = StartCoroutine(Stop(cb,null));
    //        else
    //        {
    //            if(this!=null && source!=null)
    //                source.Stop();
    //            if (cb != null)
    //            {
    //                cb();
    //            }
    //        }
    //    }

    //    private IEnumerator Stop(Action cb,Action nul)
    //    {
    //        if (mPlayCor != null)
    //        {
    //            StopCoroutine(mPlayCor);
    //            mPlayCor = null;
    //        }

    //        if (mFadeOut)
    //        {
    //            //从mvol开始淡出,根据淡出时长计算递增值
    //            float deltaVol = mVolume / mFadeTime;

    //            while (mVolume > 0)
    //            {
    //                mVolume -= Time.deltaTime * deltaVol;
    //                source.volume = mVolume;
    //                yield return null;
    //            }
    //        }

    //        if (this != null && source != null)
    //        {
    //            source.Stop();
    //        }

    //        mStopCor = null;

    //        if (cb != null)
    //        {
    //            cb();
    //        }
    //    }

    //    void OnDestroy()
    //    {
    //        source = null;
    //    }
    //}

}

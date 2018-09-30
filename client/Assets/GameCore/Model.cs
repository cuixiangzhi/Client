using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class Model : MonoBehaviour
    {
        private GameObject mMainModel;
        private List<GameObject> mMainModelBodys;

        private Animator mAnimator;
        private int mBaseLayerIndex = -1;
        private RuntimeAnimatorController mAnimatorController;
        private Dictionary<string, AnimationClip> mAnimationDic;

        private string mAnimName = string.Empty;
        private float mFadeInTime = 0;
        private bool mCrossFade = true;
        private float mTimeOffset = 0;

        private void PlayAnimation()
        {
            if (mAnimator == null) return;
            if (!mAnimationDic.ContainsKey(mAnimName)) return;
            AnimatorStateInfo stateInfo = mAnimator.GetCurrentAnimatorStateInfo(mBaseLayerIndex);
            if (!mCrossFade || stateInfo.normalizedTime <= 0.2f)
            {
                mAnimator.Play(mAnimName, mBaseLayerIndex, mTimeOffset);
            }
            else
            {
                mAnimator.CrossFade(mAnimName, mFadeInTime, mBaseLayerIndex, mTimeOffset);
            }
        }

        public void SetMainModel(GameObject mainModel)
        {
            mMainModel = mainModel;
        }

        public void SetMainModelBody(GameObject mainModelBody)
        {
            if (mMainModelBodys == null) mMainModelBodys = new List<GameObject>(6);
            mMainModelBodys.Add(mainModelBody);
        }

        public void SetMainModelAnimatorController(RuntimeAnimatorController animatorController)
        {
            mAnimatorController = animatorController;
            if (mAnimationDic == null) mAnimationDic = new Dictionary<string, AnimationClip>();
            AnimationClip[] clips = animatorController.animationClips;
            if(clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    AnimationClip clip = clips[i];
                    mAnimationDic[clip.name] = clip;
                }
            }
        }

        public void Combine()
        {
            if(mMainModelBodys != null && mMainModelBodys.Count > 0)
            {
                CombineMgr.Instance.CombineMesh(mMainModel, mMainModelBodys);
            }
            mAnimator = mMainModel.GetComponent<Animator>();
            if(mAnimator != null)
            {
                mAnimator.runtimeAnimatorController = mAnimatorController;
                mBaseLayerIndex = mAnimator.GetLayerIndex("Base Layer");
                PlayAnimation();
            }
        }

        public void Clear()
        {
            mMainModel = null;
            if (mMainModelBodys != null) mMainModelBodys.Clear();
            mAnimator = null;
            mAnimatorController = null;
            if (mAnimationDic != null) mAnimationDic.Clear();
        }

        public void PlayAnimation(string animName, float fadeInTime, bool crossFade, float timeOffset)
        {
            mAnimName = animName;
            mFadeInTime = fadeInTime;
            mCrossFade = crossFade;
            mTimeOffset = timeOffset;
            PlayAnimation();
        }

        public float GetAnimationLength(string animName)
        {
            AnimationClip clip = null;
            if(mAnimationDic.TryGetValue(animName,out clip))
            {
                return clip.length;
            }
            return 0;
        }
    }
}

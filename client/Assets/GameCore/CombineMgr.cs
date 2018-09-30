using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class CombineMgr : BaseMgr<CombineMgr>
    {
        private List<SkinnedMeshRenderer> mSkinList = new List<SkinnedMeshRenderer>(8);
        private List<Material> mMaterials = new List<Material>(8);
        private List<CombineInstance> mCombineInstances = new List<CombineInstance>(8);
        private List<Transform> mBoneList = new List<Transform>(256);
        private Dictionary<string, Transform> mBoneDic = new Dictionary<string, Transform>(256);

        private GameObject mMainModel;
        private Transform mMainModelTransform;
        private Transform mMainModelParent;
        private Vector3 mLocalPosition;
        private Vector3 mLocalScale;
        private Quaternion mLocalRotation;
        private List<GameObject> mMainModelBodys = new List<GameObject>(8);

        private void ClearCombineDatas()
        {
            mMainModel = null;
            mMainModelTransform = null;
            mMainModelParent = null;
            mMainModelBodys.Clear();
            mSkinList.Clear();
            mMaterials.Clear();
            mCombineInstances.Clear();
            mBoneList.Clear();
            mBoneDic.Clear();
        }
        private void FillModelDatas(GameObject main, List<GameObject> bodys)
        {
            mMainModel = main;
            mMainModelTransform = main.transform;

            mMainModelParent = main.transform.parent;
            mLocalPosition = mMainModelTransform.localPosition;
            mLocalScale = mMainModelTransform.localScale;
            mLocalRotation = mMainModelTransform.localRotation;

            mMainModelTransform.parent = null;
            mMainModelTransform.localPosition = Vector3.zero;
            mMainModelTransform.localScale = Vector3.one;
            mMainModelTransform.localRotation = Quaternion.identity;

            mMainModelBodys.AddRange(bodys);
        }

        private void FillBoneDatas(Transform parentBone = null)
        {
            if (parentBone == null) parentBone = mMainModelTransform;
            for (int i = 0; i < parentBone.childCount; i++)
            {
                Transform bone = parentBone.GetChild(i);
                mBoneDic[bone.name] = bone;
                FillBoneDatas(bone);
            }
        }

        private void FillSkinDatas()
        {
            for (int i = 0; i < mMainModelBodys.Count; i++)
            {
                SkinnedMeshRenderer skinMesh = mMainModelBodys[i].GetComponentInChildren<SkinnedMeshRenderer>();
                CombineInstance combineInstance = new CombineInstance()
                {
                    mesh = skinMesh.sharedMesh,
                };
                Transform[] skinBones = skinMesh.bones;
                for (int j = 0; j < skinBones.Length; j++)
                {
                    Transform bone = mBoneDic[skinBones[j].name];
                    mBoneList.Add(bone);
                }

                mSkinList.Add(skinMesh);
                mCombineInstances.Add(combineInstance);
                mMaterials.Add(skinMesh.sharedMaterial);
            }
        }

        private void Combine()
        {
            SkinnedMeshRenderer skinMeshRender = mMainModel.AddMissingComponent<SkinnedMeshRenderer>();

            skinMeshRender.sharedMesh = new Mesh();
            skinMeshRender.sharedMesh.CombineMeshes(mCombineInstances.ToArray(), false, false);
            skinMeshRender.sharedMaterials = mMaterials.ToArray();
            skinMeshRender.bones = mBoneList.ToArray();
            skinMeshRender.sharedMesh.RecalculateBounds();
            skinMeshRender.sharedMesh.UploadMeshData(true);

            mMainModelTransform.parent = mMainModelParent;
            mMainModelTransform.localPosition = mLocalPosition;
            mMainModelTransform.localScale = mLocalScale;
            mMainModelTransform.localRotation = mLocalRotation;
        }

        public void CombineMesh(GameObject main,List<GameObject> bodys)
        {
            ClearCombineDatas();
            FillModelDatas(main, bodys);
            FillBoneDatas();
            FillSkinDatas();
            Combine();
            ClearCombineDatas();
        }
    }
}

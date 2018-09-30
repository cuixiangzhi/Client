using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Action
{
    public class ActionCombine : MonoBehaviour
    {
        public class SkinMeshCombiner
        {
            public class CombineResult
            {
                public GameObject EachTransformRoot;
                public List<Transform> EachTransform = new List<Transform>();
                public Transform CombinedMesh = null;
                public string ModelName;

                public void Destroy()
                {
                    if (null != EachTransformRoot)
                    {
                        GameObject.Destroy(EachTransformRoot);
                        EachTransformRoot = null;
                    }
                    else
                    {
                        for (int i = 0; i < EachTransform.Count; i++)
                        {
                            Transform tr = EachTransform[i];
                            if (null != tr)
                            {
                                GameObject.Destroy(tr.gameObject);
                            }
                        }
                    }

                    if (CombinedMesh != null)
                    {
                        GameObject.Destroy(CombinedMesh.gameObject);
                        CombinedMesh = null;
                    }

                    EachTransform.Clear();
                }

                public void Destroy(bool isCache)
                {
                    if (isCache)
                    {
                        CombineInstancePool.Despawn(this);
                    }
                    else
                    {
                        Destroy();
                    }

                }
            }

            static string COMBINEMESH_ROOT_NAME = "CombineMeshRoot";
            static public GameObject root;

            static List<SkinnedMeshRenderer> smList = new List<SkinnedMeshRenderer>();
            static List<Transform> boneList = new List<Transform>();
            static List<BoneWeight> boneWeightList = new List<BoneWeight>();
            static List<CombineInstance> combines = new List<CombineInstance>();
            static List<Matrix4x4> bindposes = new List<Matrix4x4>();

            public static GameObject GetCombineMeshRoot()
            {
                if (root == null)
                {
                    root = new GameObject(COMBINEMESH_ROOT_NAME);
                    root.AddComponent<CombineMeshRoot>();
                }
                return root;
            }

            //合并骨骼,骨骼权重,共享材质,MESH实例,计算权重绑定骨骼偏移
            public static CombineResult CreateOneMesh(Object prefab, int number, GameObject parent = null)
            {
                Debug.Log(string.Format("combine mesh ({0})", prefab.name));

                smList.Clear();
                boneList.Clear();
                boneWeightList.Clear();
                combines.Clear();
                bindposes.Clear();

                if (null == parent)
                {
                    parent = GetCombineMeshRoot();
                }

                CombineResult ret = new CombineResult();
                ret.ModelName = prefab.name;

                try
                {
                    //实例化N个物体,获取物体蒙皮数据
                    for (int i = 0; i < number; i++)
                    {
                        GameObject tf = GameObject.Instantiate(prefab) as GameObject;
                        SkinnedMeshRenderer[] allsmr = tf.GetComponentsInChildren<SkinnedMeshRenderer>();
                        smList.AddRange(allsmr);

                        ret.EachTransform.Add(tf.transform);
                    }

                    Material sharedMaterial = smList[0].sharedMaterial;
                    int num = 0;
                    for (int j = 0; j < smList.Count; j++)
                    {
                        //遍历蒙皮数据,修改蒙皮顶点绑定的骨骼偏移(因为是相同的MESH，所以直接加上当前骨骼数)
                        SkinnedMeshRenderer skinnedMeshRenderer = smList[j];
                        BoneWeight[] boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
                        for (int k = 0; k < boneWeights.Length; k++)
                        {
                            BoneWeight boneWeight = boneWeights[k];
                            BoneWeight item = boneWeight;

                            item.boneIndex0 = item.boneIndex0 + num;
                            item.boneIndex1 = item.boneIndex1 + num;
                            item.boneIndex2 = item.boneIndex2 + num;
                            item.boneIndex3 = item.boneIndex3 + num;
                            //加入合并权重列表
                            boneWeightList.Add(item);
                        }
                        num += skinnedMeshRenderer.bones.Length;
                        //加入合并骨骼列表
                        for (int l = 0; l < skinnedMeshRenderer.bones.Length; l++)
                        {
                            Transform item2 = skinnedMeshRenderer.bones[l];
                            boneList.Add(item2);
                        }
                        //创建当前模型的绑定MESH
                        CombineInstance item3 = new CombineInstance
                        {
                            mesh = skinnedMeshRenderer.sharedMesh,
                            transform = skinnedMeshRenderer.transform.localToWorldMatrix
                        };
                        combines.Add(item3);
                        skinnedMeshRenderer.gameObject.SetActive(false);
                    }
                    GameObject gameObject = new GameObject(string.Format("{0}_{1}", prefab.name, "SingleMesh"));
                    //计算每根骨骼的模型到骨骼的转换矩阵bindpose
                    //bindpose:蒙皮顶点由模型空间转换到bone空间的矩阵
                    //bindpose用于计算T-POSE时的顶点位置
                    for (int m = 0; m < boneList.Count; m++)
                    {
                        Transform transform = boneList[m];
                        bindposes.Add(transform.worldToLocalMatrix * gameObject.transform.localToWorldMatrix);
                    }
                    SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject.AddComponent<SkinnedMeshRenderer>();
                    skinnedMeshRenderer2.updateWhenOffscreen = true;
                    skinnedMeshRenderer2.localBounds = new Bounds(new Vector3(0f, 1f, 0f), new Vector3(0, 4f, 0));

                    skinnedMeshRenderer2.sharedMesh = new Mesh();
                    skinnedMeshRenderer2.sharedMesh.CombineMeshes(combines.ToArray(), true, true);
                    skinnedMeshRenderer2.sharedMaterial = sharedMaterial;
                    skinnedMeshRenderer2.bones = boneList.ToArray();
                    skinnedMeshRenderer2.sharedMesh.boneWeights = boneWeightList.ToArray();
                    skinnedMeshRenderer2.sharedMesh.bindposes = bindposes.ToArray();
                    skinnedMeshRenderer2.sharedMesh.RecalculateBounds();

                    ret.CombinedMesh = gameObject.transform;
                    gameObject.transform.parent = parent.transform;
                    gameObject.transform.position = parent.transform.position;
                    gameObject.transform.localScale = Vector3.one;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("excption occur when combine mesh ({0}), msg:({1})", prefab.name, e.Message));
                }
                finally
                {
                }

                return ret;
            }

            public static CombineResult CreateOneMesh(List<GameObject> models, string modelName, GameObject eachTransformRoot, GameObject parent = null)
            {
                if (models.Count <= 0) return null;

                Debug.Log(string.Format("combine mesh ({0})", modelName));

                smList.Clear();
                boneList.Clear();
                boneWeightList.Clear();
                combines.Clear();
                bindposes.Clear();

                if (null == parent)
                {
                    parent = GetCombineMeshRoot();
                }

                CombineResult ret = new CombineResult();
                ret.ModelName = modelName;
                ret.EachTransformRoot = eachTransformRoot;
                try
                {
                    for (int i = 0; i < models.Count; i++)
                    {
                        GameObject tf = models[i];
                        SkinnedMeshRenderer[] allsmr = tf.GetComponentsInChildren<SkinnedMeshRenderer>();
                        smList.AddRange(allsmr);

                        ret.EachTransform.Add(tf.transform);
                    }

                    Material sharedMaterial = smList[0].sharedMaterial;
                    int num = 0;
                    for (int j = 0; j < smList.Count; j++)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = smList[j];
                        BoneWeight[] boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;
                        for (int k = 0; k < boneWeights.Length; k++)
                        {
                            BoneWeight boneWeight = boneWeights[k];
                            BoneWeight item = boneWeight;

                            item.boneIndex0 = item.boneIndex0 + num;
                            item.boneIndex1 = item.boneIndex1 + num;
                            item.boneIndex2 = item.boneIndex2 + num;
                            item.boneIndex3 = item.boneIndex3 + num;

                            boneWeightList.Add(item);
                        }
                        num += skinnedMeshRenderer.bones.Length;
                        for (int l = 0; l < skinnedMeshRenderer.bones.Length; l++)
                        {
                            Transform item2 = skinnedMeshRenderer.bones[l];
                            boneList.Add(item2);
                        }
                        CombineInstance item3 = new CombineInstance
                        {
                            mesh = skinnedMeshRenderer.sharedMesh,
                            transform = skinnedMeshRenderer.transform.localToWorldMatrix
                        };
                        combines.Add(item3);
                        skinnedMeshRenderer.gameObject.SetActive(false);
                    }
                    GameObject gameObject = new GameObject(string.Format("{0}_{1}", modelName, "SingleMesh"));

                    for (int m = 0; m < boneList.Count; m++)
                    {
                        Transform transform = boneList[m];
                        bindposes.Add(transform.worldToLocalMatrix * gameObject.transform.worldToLocalMatrix);
                    }
                    SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject.AddComponent<SkinnedMeshRenderer>();
                    skinnedMeshRenderer2.updateWhenOffscreen = true;
                    skinnedMeshRenderer2.localBounds = new Bounds(new Vector3(0f, 1f, 0f), new Vector3(0, 4f, 0));

                    skinnedMeshRenderer2.sharedMesh = new Mesh();
                    skinnedMeshRenderer2.sharedMesh.CombineMeshes(combines.ToArray(), true, true);
                    skinnedMeshRenderer2.sharedMaterial = sharedMaterial;
                    skinnedMeshRenderer2.bones = boneList.ToArray();
                    skinnedMeshRenderer2.sharedMesh.boneWeights = boneWeightList.ToArray();
                    skinnedMeshRenderer2.sharedMesh.bindposes = bindposes.ToArray();
                    skinnedMeshRenderer2.sharedMesh.RecalculateBounds();

                    ret.CombinedMesh = gameObject.transform;
                    gameObject.transform.parent = parent.transform;
                    gameObject.transform.position = parent.transform.position;
                    gameObject.transform.localScale = Vector3.one;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("excption occur when combine mesh ({0}), msg:({1})", modelName, e.Message));
                }
                finally
                {
                }

                return ret;
            }
        }


        public static class CombineInstancePool
        {
            public static Dictionary<string, List<SkinMeshCombiner.CombineResult>> pool;

            public static SkinMeshCombiner.CombineResult Spawn(string modelName, int num)
            {
                if (pool == null) return null;

                List<SkinMeshCombiner.CombineResult> crs;
                if (pool.TryGetValue(modelName, out crs))
                {
                    for (int i = 0; i < crs.Count; i++)
                    {
                        SkinMeshCombiner.CombineResult cr = crs[i];
                        if (cr.EachTransform.Count == num)
                        {
                            if (null != cr.EachTransformRoot)
                            {
                                cr.EachTransformRoot.SetActive(true);
                            }
                            cr.CombinedMesh.gameObject.SetActive(true);
                            crs.RemoveAt(i);
                            return cr;
                        }
                    }
                }

                return null;
            }

            public static void Despawn(SkinMeshCombiner.CombineResult cr)
            {
                List<SkinMeshCombiner.CombineResult> crs;
                if (!pool.TryGetValue(cr.ModelName, out crs))
                {
                    crs = new List<SkinMeshCombiner.CombineResult>();
                    pool[cr.ModelName] = crs;
                }
                crs.Add(cr);
                cr.CombinedMesh.gameObject.SetActive(false);
                if (null != cr.EachTransformRoot)
                {
                    cr.EachTransformRoot.transform.SetParent(SkinMeshCombiner.root.transform);
                    cr.EachTransformRoot.SetActive(false);
                }

            }
        }
        public class CombineMeshRoot : MonoBehaviour
        {
            void Awake()
            {
                CombineInstancePool.pool = new Dictionary<string, List<SkinMeshCombiner.CombineResult>>();
            }
            void OnDestroy()
            {
                var e = CombineInstancePool.pool.GetEnumerator();
                while (e.MoveNext())
                {
                    List<SkinMeshCombiner.CombineResult> crs = e.Current.Value;
                    for (int i = 0; i < crs.Count; i++)
                    {
                        crs[i].Destroy();
                    }
                    crs.Clear();
                }
                e.Dispose();
                CombineInstancePool.pool.Clear();
                CombineInstancePool.pool = null;
            }

#if UNITY_EDITOR
            public List<string> cacheds = new List<string>();
            void Update()
            {
                cacheds.Clear();
                var e = CombineInstancePool.pool.GetEnumerator();
                while (e.MoveNext())
                {
                    List<SkinMeshCombiner.CombineResult> crs = e.Current.Value;
                    if (crs.Count > 0)
                    {
                        cacheds.Add(e.Current.Key);
                    }
                }
                e.Dispose();
            }
#endif
        }
    }
}

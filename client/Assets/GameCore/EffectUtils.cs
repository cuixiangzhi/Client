using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EffectUtils
{
    private static HashSet<int> mCacheEffectSetFlag = new HashSet<int>();

    public static void SetRendererSortOrder(GameObject curObj, int sortOrderBase)
    {
        if(curObj != null)
        {
            if(!mCacheEffectSetFlag.Contains(curObj.GetInstanceID()))
            {
                mCacheEffectSetFlag.Add(curObj.GetInstanceID());
                Renderer[] renderAll = curObj.GetComponentsInChildren<Renderer>(true);
                if (renderAll != null)
                {
                    for (int i = 0; i < renderAll.Length; i++)
                    {
                        renderAll[i].sortingOrder += sortOrderBase;
                    }
                }
            }
        }
    }

    public static void SetGameObjectLayer(Transform curTransform, int layer)
    {
        curTransform.gameObject.layer = layer;
        for (int i = 0; i < curTransform.childCount; i++)
        {
            SetGameObjectLayer(curTransform.GetChild(i), layer);
        }
    }

    public static void SetParticleScale(GameObject curObj)
    {
        if (curObj != null)
        {
            ParticleSystem[] renderAll = curObj.GetComponentsInChildren<ParticleSystem>();
            if (renderAll != null)
            {
                float globalScale = GetGlobalScale();
                if(globalScale != 1)
                {
                    for (int i = 0; i < renderAll.Length; i++)
                    {
                        ParticleSystem.MainModule mm = renderAll[i].main;
                        Transform trans = renderAll[i].transform;
                        Vector3 localScale = trans.localScale;
                        trans.localScale = new Vector3(globalScale * localScale.x, globalScale * localScale.y, globalScale * localScale.z);
                        if (!mm.startSize3D)
                        {
                            ParticleSystem.MinMaxCurve mmc = mm.startSize;
                            mmc.constant /= globalScale;
                            mm.startSize = mmc;
                        }
                        else
                        {
                            ParticleSystem.MinMaxCurve mmcX = mm.startSizeX;
                            mmcX.constant /= globalScale;
                            mm.startSizeX = mmcX;

                            ParticleSystem.MinMaxCurve mmcY = mm.startSizeY;
                            mmcY.constant /= globalScale;
                            mm.startSizeY = mmcY;

                            ParticleSystem.MinMaxCurve mmcZ = mm.startSizeZ;
                            mmcZ.constant /= globalScale;
                            mm.startSizeZ = mmcZ;
                        }
                    }
                }
            }
        }
    }

    private static float GetGlobalScale()
    {
        Vector2 screenSize = NGUITools.screenSize;
        float aspect = screenSize.x / screenSize.y;
        float activeHeight = UIRoot.list[0].manualWidth / aspect;
        float scale = 2 / activeHeight;

        float activeHeight169 = UIRoot.list[0].manualWidth / (16.0f / 9.0f);
        float scale169 = 2 / activeHeight169;
        return scale169 / scale;
    }
}

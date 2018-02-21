using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System;

public static class NGUITextExt
{
    private static UILabel HELP_LABEL = null;

    private static LuaFunction HELP_FUNC = null;

    private static int INT_CHAR_0 = Convert.ToInt32('0');

    public static void InitParam(UILabel lb)
    {
        HELP_LABEL = lb;
    }

    public static void InitParam(LuaFunction luaFunc)
    {
        HELP_FUNC = luaFunc;
    }

    public static Vector2 CalculatePrintedSizeDynamicFont(string text)
    {
        UnityEngine.Profiling.Profiler.BeginSample("CALCULATE PRINTEDSIZE DYNAMICFONT");
        HELP_LABEL.text = text;
        HELP_LABEL.ProcessText();
        UnityEngine.Profiling.Profiler.EndSample();
        return HELP_LABEL.printedSize;
    }

    public static void SetSpriteName(UISprite sp,int id)
    {
        UnityEngine.Profiling.Profiler.BeginSample("SET SPRITE NAME");
        for(int i = 0;i < sp.atlas.spriteList.Count;i++)
        {
            UISpriteData data = sp.atlas.spriteList[i];
            if (data.name.Length == 3 && char.IsDigit(data.name[0]) && char.IsDigit(data.name[1]) && char.IsDigit(data.name[2]))
            {
                int id0 = Convert.ToInt32(data.name[0]) - INT_CHAR_0;
                int id1 = Convert.ToInt32(data.name[1]) - INT_CHAR_0;
                int id2 = Convert.ToInt32(data.name[2]) - INT_CHAR_0;
                if(id == id0 * 100 + id1 * 10 + id2)
                {
                    sp.spriteName = data.name;
                    break;
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public static void SetAnimationName(UISpriteAnimation spAnim,UISprite sp)
    {
        spAnim.namePrefix = sp.spriteName;
    }

    public static void GetSpriteSize(UIAtlas atlas,int atlasIndex)
    {
        UnityEngine.Profiling.Profiler.BeginSample("CACHE SPRITE SIZE");
        List<UISpriteData> list = atlas.spriteList;
        for(int i = 0;i < list.Count;i++)
        {
            if(list[i].name.Length >= 3 && char.IsDigit(list[i].name[0]) && char.IsDigit(list[i].name[1]) && char.IsDigit(list[i].name[2]))
            {
                int id0 = Convert.ToInt32(list[i].name[0]) - INT_CHAR_0;
                int id1 = Convert.ToInt32(list[i].name[1]) - INT_CHAR_0;
                int id2 = Convert.ToInt32(list[i].name[2]) - INT_CHAR_0;
                int id = id0 * 100 + id1 * 10 + id2;

                int x = Mathf.RoundToInt(atlas.pixelSize * (list[i].width + list[i].paddingLeft + list[i].paddingRight));
                int y = Mathf.RoundToInt(atlas.pixelSize * (list[i].height + list[i].paddingTop + list[i].paddingBottom));

                if ((x & 1) == 1) ++x;
                if ((y & 1) == 1) ++y;

                HELP_FUNC.BeginPCall();
                HELP_FUNC.Push(atlasIndex);
                HELP_FUNC.Push(id);
                HELP_FUNC.Push(x);
                HELP_FUNC.Push(y);
                HELP_FUNC.PCall();
                HELP_FUNC.EndPCall();
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

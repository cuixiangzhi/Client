﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInputEx : UIInput
{
    private CharacterInfo mTempChar;

    //是否包含无法显示的字符
    public bool HasIllegalChar()
    {
        int finalSize = Mathf.RoundToInt(label.defaultFontSize / label.root.pixelSizeAdjustment);
        label.trueTypeFont.RequestCharactersInTexture(mValue, finalSize, label.fontStyle);
        for (int i = 0; i < mValue.Length; i++)
        {
            if (!label.trueTypeFont.GetCharacterInfo(mValue[i], out mTempChar, finalSize, label.fontStyle))
            {
                return true;
            }
        }
        return false;
    }

    //失去焦点后显示最后的文字,左对齐
    protected override void OnSelect(bool isSelected)
    {
        base.OnSelect(isSelected);
        if(!isSelected)
        {
            int offset = label.CalculateOffsetToFit(mValue);
            if (offset > 0)
            {
                label.alignment = NGUIText.Alignment.Left;
                label.text = mValue.Substring(offset, mValue.Length - offset);
            }
        }
    }

    //赋值之后显示最后的文字,左对齐
    public void SetValue(string value)
    {
        base.Set(value,false);
        int offset = label.CalculateOffsetToFit(mValue);
        if (offset > 0)
        {
            label.alignment = NGUIText.Alignment.Left;
            label.text = mValue.Substring(offset, mValue.Length - offset);
        }
    }
}

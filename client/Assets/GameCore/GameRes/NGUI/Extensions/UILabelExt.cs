using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LuaInterface;

public sealed class UILabelExt : MonoBehaviour
{
    public enum AlignType
    {
        LEFT,
        RIGHT,
    }

    private struct ShareData
    {
        public int shareID;
        public int shareType;
        public string shareName;
        public string shareColor;
        public bool replaceFlag;
    }

    private struct LineData
    {
        public int index;
        public float curWidth;
    }

    private struct ColorData
    {
        public int index;
        public int length;
    }

    private struct ClickData
    {
        public int index;
        public int length;
        public float width;

        public string key;
        public float startOffset;
        public float endOffset;
    }

    private struct EmojiData
    {
        public string emojiName;
        public float emojiWidth;
        public float emojiHeight;
        public float emojiOffset;
    }

    private static string[] mNGUISymbol = new string[]
    {
        "[b]", "[/b]",
        "[i]", "[/i]",
        "[u]", "[/u]",
        "[s]", "[/s]",
        "[c]", "[/c]",
        "[sub]","[/sub]",
        "[sup]","[/sup]",
        "[url=]","[/url]",
        "[url=","[-]",
    };

    private static Dictionary<char,string> mColorSymbol = new Dictionary<char, string>(64);

    private static Dictionary<char, string> mFontSymbol = new Dictionary<char, string>();

    private static Dictionary<int, string> mEmojiSymbol = new Dictionary<int, string>(256);

    private static Dictionary<int, Vector2> mEmojiSymbolSize = new Dictionary<int, Vector2>(256);

    private static StringBuilder mBuilder = new StringBuilder(1024);

    private static StringBuilder mLineBuilder = new StringBuilder(128);

    private static List<ColorData> mColorDatas = new List<ColorData>(32);

    private static List<ClickData> mClickDatas = new List<ClickData>(32);

    private static LineData mLineData = new LineData();

    private static ClickData mNextLineClickData = new ClickData();

    private static List<ClickData> mLineClicks = new List<ClickData>(16);

    private static List<EmojiData> mLineEmojis = new List<EmojiData>(32);

    private Dictionary<string, ShareData> mShareSymbols = new Dictionary<string, ShareData>(8);

    private Dictionary<string, ShareData> mReplaceSymbols = new Dictionary<string, ShareData>(8);

    private Dictionary<char, float> mCharWidthCache = new Dictionary<char, float>(1024);

    private Queue<UISprite> mSpriteCache = new Queue<UISprite>(32);

    private Dictionary<UISprite,UISpriteAnimation> mSpriteAnimCache = new Dictionary<UISprite, UISpriteAnimation>(32);

    private Queue<UILabel> mLabelCache = new Queue<UILabel>(32);

    private float mCurHeight = 0;
    private float mCurWidth = 0;
    private Transform mLabelRoot = null;
    private LuaFunction mLuaFunc = null;
    private UILabel mLabel = null;
    private UISprite mSprite = null;

    private float mMaxWidth = 0;
    private float mLineSpace = 0;
    private AlignType mAlign = AlignType.LEFT;
    private GameCore.Component.UIComponent mUIComponent;
    private int mLabelCount = 0;
    private int mSpriteCount = 0;
    private int mItemIndex = 0;
    private int mStartStringLength = 0;
    private int mEndStringLength = 0;

    private bool mInit = false;

    /// <summary>
    /// 1.初始化当前UI图文混排基本信息
    /// </summary>
    /// <param name="lb">label prefab,用于复制</param>
    /// <param name="sp">sprite prefab,用于复制</param>
    /// <param name="luaFunc">初始化每一行数据时调用,传递点击信息和复制的Label和Sprite</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="lineSpace">行间距</param>
    /// <param name="defaultLabelCount"></param>
    /// <param name="defaultSpriteCount"></param>
    /// <param name="frame"></param>
    public void Init(UILabel lb,UISprite sp, LuaFunction luaFunc,float maxWidth,float lineSpace,int defaultLabelCount,int defaultSpriteCount,GameCore.Component.UIComponent uiComponent)
    {
        if(!mInit)
        {
            mLabel = lb;
            mSprite = sp;
            mInit = true;
            mUIComponent = uiComponent;
            lb.gameObject.SetActive(false);
            sp.gameObject.SetActive(false);
            mLuaFunc = luaFunc;
            mMaxWidth = maxWidth;
            mLineSpace = lineSpace;
            for (int i = 0; i < defaultLabelCount; i++)
            {
                NewLabel(true);
            }
            for (int i = 0; i < defaultSpriteCount; i++)
            {
                NewSprite(true,string.Empty);
            }
            if(mEmojiSymbol.Count == 0)
            {
                int code0 = Convert.ToInt32('0');
                List<UISpriteData> list = sp.atlas.spriteList;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].name.Length >= 3)
                    {
                        if (char.IsDigit(list[i].name[0]) && char.IsDigit(list[i].name[1]) && char.IsDigit(list[i].name[2]))
                        {
                            int id0 = Convert.ToInt32(list[i].name[0]) - code0;
                            int id1 = Convert.ToInt32(list[i].name[1]) - code0;
                            int id2 = Convert.ToInt32(list[i].name[2]) - code0;
                            int id = id0 * 100 + id1 * 10 + id2;
                            //记录该ID为表情
                            if (list[i].name.Length == 3)
                            {
                                mEmojiSymbol[id] = list[i].name;
                            }

                            //记录该ID表情大小
                            UISpriteData sd = list[i];
                            int x = Mathf.RoundToInt(mSprite.atlas.pixelSize * (sd.width + sd.paddingLeft + sd.paddingRight));
                            int y = Mathf.RoundToInt(mSprite.atlas.pixelSize * (sd.height + sd.paddingTop + sd.paddingBottom));
                            if ((x & 1) == 1) ++x;
                            if ((y & 1) == 1) ++y;

                            if(mEmojiSymbolSize.ContainsKey(id))
                            {
                                Vector2 size = mEmojiSymbolSize[id];
                                size.x = Mathf.Max(size.x, x);
                                size.y = Mathf.Max(size.y, y);
                                mEmojiSymbolSize[id] = size;
                            }
                            else
                            {
                                mEmojiSymbolSize[id] = new Vector2(x, y);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 2.初始化ITEM之前需要传入当前字符串内可点击的信息
    /// </summary>
    /// <param name="shareID">用于标记LUA内哪条数据</param>
    /// <param name="shareType">用于区分是不是@ 0是@</param>
    /// <param name="shareName">当前点击信息的字符串标记</param>
    /// <param name="shareColor">当前分享信息的颜色</param>
    /// <param name="isReplace">是否是服务器推送的模板数据</param>
    public void ProcessSymbols(int shareID,int shareType,string shareName,string shareColor,bool isReplace)
    {
        ShareData data = new ShareData();
        data.shareID = shareID;
        data.shareType = shareType;
        data.shareName = shareName;
        data.shareColor = shareColor;
        data.replaceFlag = false;
        mShareSymbols[data.shareName] = data;
        if(isReplace)
        {
            mReplaceSymbols[data.shareName] = data;
        }
    }

    /// <summary>
    /// 3.回收不使用的Label和Sprite,隐藏或者重新初始化时必须调用
    /// </summary>
    /// <param name="lb"></param>
    /// <param name="sp"></param>
    public void ProcessRelease(UILabel lb,UISprite sp)
    {
        if(lb != null)
        {
            lb.enabled = false;
            lb.transform.parent = transform;
            mLabelCache.Enqueue(lb);
        }
        if (sp != null)
        {
            sp.enabled = false;
            sp.transform.parent = transform;
            UISpriteAnimation spa = mSpriteAnimCache[sp];
            spa.enabled = false;
            mSpriteCache.Enqueue(sp);
        }
    }

    /// <summary>
    /// 4.初始化当前图文混排文本
    /// </summary>
    /// <param name="lineRoot">文本根结点,左上角或者右上角锚点</param>
    /// <param name="align">左对齐还是右对齐</param>
    /// <param name="value">要处理的字符串</param>
    /// <param name="defaultColor">UI的默认颜色值,不填会读取初始化时的label的默认值</param>
    /// <param name="defaultString">默认字符串前缀,例如#000[张三]说:</param>
    /// <param name="shieldNGUISymbol">是否需要屏蔽NGUI语法标记</param>
    public void ProcessText(int itemIndex,Transform lineRoot,AlignType align, string value, bool shieldNGUISymbol, string startColor, string startString,string endColor,string endString)
    {
        mItemIndex = itemIndex;
        mAlign = align;
        mLabelRoot = lineRoot;
        if (mLabelRoot.childCount != 0)
            throw new Exception("you must call ProcessRelease before call this func!");
        UnityEngine.Profiling.Profiler.BeginSample("UILabelExt.ProcessText");
        Prepare(value);
        ProcessNGUISymbol(shieldNGUISymbol);
        ProcessDefaultSymbol(startColor, startString, endColor, endString);
        ProcessCustomSymbol();
        ProcessClickSymbol();
        ProcessTextGraphics();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void Prepare(string value)
    {
        mCurHeight = 0;
        mCurWidth = 0;
        mBuilder.Remove(0, mBuilder.Length);
        mBuilder.Append(value);
        mLineBuilder.Remove(0, mLineBuilder.Length);
        mColorDatas.Clear();
        mClickDatas.Clear();
        mLineData.curWidth = 0;
        mLineData.index = 0;
        mNextLineClickData.index = -1;
        mLineClicks.Clear();
        mLineEmojis.Clear();
    }

    private void ProcessNGUISymbol(bool shieldNGUISymbol)
    {
        if(shieldNGUISymbol)
        {
            for(int i = 0; i < mBuilder.Length; i++)
            {
                bool isNGUISymbol = false;
                for(int j = 0;j < mNGUISymbol.Length; j++)
                {
                    if(IsEqualValue(mBuilder,i,i + mNGUISymbol[j].Length - 1,mNGUISymbol[j]))
                    {
                        mBuilder.Insert(i + 1, "[000000][-]");
                        i = i + 11 + mNGUISymbol[j].Length - 1;
                        isNGUISymbol = true;
                        break;
                    }
                }
                if(!isNGUISymbol && mBuilder[i] == '[' && (IsColor(mBuilder,i,i + 7) || IsColor(mBuilder, i, i + 9)))
                {
                    mBuilder.Insert(i + 1, "[000000][-]");
                    i += 11;
                }
            }
        }
    }

    private void ProcessCustomSymbol()
    {
        for(int i = 0;i < mBuilder.Length;i++)
        {
            if(mBuilder.Length > i + 1)
            {
                if (mBuilder[i] == '#')
                {
                    i = ProcessFontSymbol(i);
                }
                else if (mBuilder[i] == '[')
                {
                    i = ProcessShareSymbol(i);
                }
                else if (mBuilder[i] == '%')
                {
                    i = ProcessReplaceSymbol(i);
                }
                else if (mBuilder[i] == '@')
                {
                    i = ProcessAtSymbol(i);
                }
            }
        }
    }

    private void ProcessDefaultSymbol(string startColor, string startString, string endColor, string endString)
    {
        if (string.IsNullOrEmpty(startColor))
        {
            startColor = string.Format("[{0:X2}{1:X2}{2:X2}]", (int)(mLabel.color.r * 255), (int)(mLabel.color.g * 255), (int)(mLabel.color.b * 255));
        }
        mBuilder.Insert(0, startColor);
        mBuilder.Append("[-]");
        if (!string.IsNullOrEmpty(startString))
        {
            mStartStringLength = startString.Length;
            mBuilder.Insert(startColor.Length, startString);
        }
        else
        {
            mStartStringLength = 0;
        }
        if (string.IsNullOrEmpty(endColor))
        {
            endColor = string.Format("[{0:X2}{1:X2}{2:X2}]", (int)(mLabel.color.r * 255), (int)(mLabel.color.g * 255), (int)(mLabel.color.b * 255));
        }
        mBuilder.Append(endColor);
        if (!string.IsNullOrEmpty(endString))
        {
            mEndStringLength = endString.Length;
            mBuilder.Append(endString);
        }
        else
        {
            mEndStringLength = 0;
        }
        mBuilder.Append("[-]");
    }

    private int ProcessFontSymbol(int i)
    {
        char symbol = mBuilder[i + 1];
        if (mColorSymbol.ContainsKey(symbol))
        {
            //用户输入特定颜色
            string color = mColorSymbol[symbol];
            mBuilder.Remove(i, 2);
            mBuilder.Insert(i, color);
            i += color.Length - 1;
        }
        else if (mFontSymbol.ContainsKey(symbol))
        {
            //用户输入特定字体     
            string font = mFontSymbol[symbol];
            mBuilder.Remove(i, 2);
            mBuilder.Insert(i, font);
            i += font.Length - 1;
        }
        else if (symbol == 'c')
        {
            //用户输入指定颜色
            mBuilder.Remove(i, 2);
            for (int j = 0; j < 8; j++)
            {
                if (mBuilder.Length <= i + j || !IsHex(mBuilder[i + j]))
                {
                    mBuilder.Insert(Mathf.Min(mBuilder.Length, i + j), j >= 6 ? 'f' : '0');
                }
            }
            mBuilder.Insert(i, '[');
            mBuilder.Insert(i + 9, ']');
            i += 9;
        }
        else if (IsEmoji(mBuilder,i,i + 3))
        {
            //用户输入特定表情
            mBuilder.Remove(i, 1);
            mBuilder.Insert(i, '[');
            mBuilder.Insert(i + 4, ']');
            i += 4;
        }
        else
        {
            //独立的#
        }
        return i;
    }

    private int ProcessShareSymbol(int i)
    {
        int startIndex = i + 1;
        int endIndex = -1;
        for (int j = startIndex; j < mBuilder.Length; j++)
        {
            if (mBuilder[j] == ']')
            {
                endIndex = j;
                break;
            }
        }
        if (endIndex != -1 && endIndex > startIndex)
        {
            foreach (var item in mShareSymbols)
            {
                if (IsEqualValue(mBuilder, startIndex, endIndex - 1, item.Key))
                {
                    mBuilder.Insert(endIndex + 1, "[-]");
                    mBuilder.Insert(i, item.Value.shareColor);
                    i += item.Value.shareColor.Length + item.Value.shareName.Length + 2 + 3 - 1;
                    break;
                }
            }
        }
        return i;
    }

    private int ProcessReplaceSymbol(int i)
    {
        if(mBuilder[i + 1] == 's')
        {
            ShareData minData = new ShareData();
            minData.shareID = -1;

            foreach (var item in mReplaceSymbols)
            {
                if(!item.Value.replaceFlag && (minData.shareID == -1 || minData.shareID > item.Value.shareID))
                {
                    minData = item.Value;
                }
            }

            if(minData.shareID != -1)
            {
                minData.replaceFlag = true;
                mReplaceSymbols[minData.shareName] = minData;
                mBuilder.Remove(i, 2);
                mBuilder.Insert(i, "[-]");
                mBuilder.Insert(i, ']');
                mBuilder.Insert(i, minData.shareName);
                mBuilder.Insert(i, '[');
                mBuilder.Insert(i, minData.shareColor);
                i += minData.shareColor.Length + 5 + minData.shareName.Length - 1;
            }
        }
        return i;
    }

    private int ProcessAtSymbol(int i)
    {
        foreach (var item in mShareSymbols)
        {
            if (item.Value.shareType == 0 && IsEqualValue(mBuilder,i + 1,i + item.Value.shareName.Length,item.Value.shareName))
            {
                mBuilder.Insert(i + item.Value.shareName.Length + 1, "[-]");
                mBuilder.Insert(i, item.Value.shareColor);
                i += item.Value.shareColor.Length + item.Value.shareName.Length + 3;
                break;
            }
        }
        return i;
    }

    private void ProcessClickSymbol()
    {
        for (int i = 0; i < mBuilder.Length; i++)
        {
            if (mBuilder[i] == '[' || mBuilder[i] == '@')
            {
                foreach(var item in mShareSymbols)
                {
                    if(IsEqualValue(mBuilder,i + 1, i + item.Value.shareName.Length,item.Value.shareName))
                    {
                        if(mBuilder[i] != '@' || item.Value.shareType == 0)
                        {
                            ClickData data = new ClickData();
                            data.index = i;
                            data.key = item.Value.shareName;
                            data.length = 1 + item.Value.shareName.Length + (mBuilder[i] == '[' ? 1 : 0);
                            data.width = GetCharWidth(mBuilder, i, i + data.length - 1);
                            i += data.length - 1;
                            mClickDatas.Add(data);
                        }
                        break;
                    }
                }
            }
        }
    }

    private void ProcessTextGraphics()
    {
        for (int i = 0;i < mBuilder.Length;i++)
        {
            bool isSpecial = false;
            if (mBuilder[i] == '[')
            {
                NewClick(i);
                //处理颜色和表情
                if (IsColor(mBuilder, i, i + 7))
                {
                    isSpecial = true;
                    GetRangeChar(mLineBuilder, mBuilder, i, i + 7);
                    ColorData data = new ColorData();
                    data.index = i;
                    data.length = 8;
                    mColorDatas.Add(data);
                    i += 7;
                }
                else if (IsColor(mBuilder, i, i + 9))
                {
                    isSpecial = true;
                    GetRangeChar(mLineBuilder, mBuilder, i, i + 9);
                    ColorData data = new ColorData();
                    data.index = i;
                    data.length = 10;
                    mColorDatas.Add(data);
                    i += 9;
                }
                else if (IsEmoji(mBuilder, i, i + 4))
                {
                    isSpecial = true;
                    EmojiData data = new EmojiData();
                    data.emojiName = mEmojiSymbol[GetEmojiID(mBuilder, i + 1)];
                    Vector2 size = GetEmojiSize(data.emojiName);
                    data.emojiHeight = size.y;
                    float spaceWidth = GetCharWidth(' ');
                    int spaceCount = Mathf.CeilToInt(size.x / spaceWidth);
                    data.emojiWidth = spaceWidth * spaceCount;

                    if (mLineData.curWidth + data.emojiWidth > mMaxWidth)
                    {
                        NewLine((i + 4) == (mBuilder.Length - 1));
                    }
                    for(int j = 0;j < spaceCount;j++)
                    {
                        mLineBuilder.Append(' ');
                    }
                    data.emojiOffset = mLineData.curWidth + data.emojiWidth / 2;
                    mLineEmojis.Add(data);
                    i += 4;
                    mLineData.curWidth += data.emojiWidth;
                }
                else if (IsNGUIEndSymbol(mBuilder, i, i + 2))
                {
                    isSpecial = true;
                    mColorDatas.RemoveAt(mColorDatas.Count - 1);
                    mLineBuilder.Append("[-]");
                    i += 2;
                }
            }
            if(!isSpecial)
            {
                //其他字符
                float width = GetCharWidth(mBuilder[i]);
                if(mLineData.curWidth + width > mMaxWidth)
                {
                    NewLine(i == (mBuilder.Length - 1));
                }
                mLineData.curWidth += width;
                mLineBuilder.Append(mBuilder[i]);
            }
            if((i == mBuilder.Length - 1) && mLineData.index != -1)
            {
                NewLine(true);
            }
        }
    }

    private bool IsEqualValue(StringBuilder builder, int startIndex, int endIndex, string value)
    {
        if (builder.Length <= startIndex || builder.Length <= endIndex)
            return false;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (builder[i] != value[i - startIndex])
                return false;
        }
        return true;
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsHex(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }

    private bool IsColor(StringBuilder builder,int startIndex,int endIndex)
    {
        for(int i = startIndex + 1;i < endIndex;i ++)
        {
            if (builder.Length <= i || !IsHex(builder[i]))
                return false;
        }
        return builder.Length > endIndex && builder[endIndex] == ']';
    }

    private bool IsEmoji(StringBuilder builder, int startIndex, int endIndex)
    {
        if (builder[startIndex] == '[' && builder.Length > endIndex)
        {
            if (IsDigit(builder[startIndex + 1]) && IsDigit(builder[startIndex + 2]) && IsDigit(builder[startIndex + 3]) && builder[endIndex] == ']')
            {
                int emojiID = GetEmojiID(builder,startIndex + 1);
                return mEmojiSymbol.ContainsKey(emojiID);
            }
        }
        else if (builder[startIndex] == '#' && builder.Length > endIndex)
        {
            if (IsDigit(builder[startIndex + 1]) && IsDigit(builder[startIndex + 2]) && IsDigit(builder[startIndex + 3]))
            {
                int emojiID = GetEmojiID(builder, startIndex + 1);
                return mEmojiSymbol.ContainsKey(emojiID) && (emojiID < 900 || startIndex <= (8 + mStartStringLength) || startIndex >= (mBuilder.Length - mEndStringLength - 3));
            }
        }
        return false;
    }

    private bool IsNGUIEndSymbol(StringBuilder builder, int startIndex, int endIndex)
    {
        return builder.Length > endIndex && builder[startIndex] == '[' && builder[startIndex + 1] == '-' && builder[endIndex] == ']';
    }

    private int GetEmojiID(StringBuilder builder, int startIndex)
    {
        int code0 = Convert.ToInt32('0');
        int a = Convert.ToInt32(builder[startIndex]) - code0;
        int b = Convert.ToInt32(builder[startIndex + 1]) - code0;
        int c = Convert.ToInt32(builder[startIndex + 2]) - code0;
        return 100 * a + 10 * b + c;
    }

    private Vector2 GetEmojiSize(string emojiName)
    {
        UISpriteData sd = mSprite.atlas.GetSprite(emojiName);
        int x = Mathf.RoundToInt(mSprite.atlas.pixelSize * (sd.width + sd.paddingLeft + sd.paddingRight));
        int y = Mathf.RoundToInt(mSprite.atlas.pixelSize * (sd.height + sd.paddingTop + sd.paddingBottom));
        if ((x & 1) == 1) ++x;
        if ((y & 1) == 1) ++y;

        return new Vector2(x,y);
    }

    private float GetCharWidth(char c)
    {
        if(!mCharWidthCache.ContainsKey(c))
        {
            mLabel.text = c.ToString();
            mLabel.ProcessText();
            mCharWidthCache[c] = mLabel.printedSize.x;
        }
        return mCharWidthCache[c];
    }

    private float GetCharWidth(StringBuilder builder,int startIndex,int endIndex)
    {
        float width = 0;
        for(int i = startIndex;i <= endIndex;i++)
        {
            width += GetCharWidth(builder[i]);
        }
        return width;
    }

    private void GetRangeChar(StringBuilder dstBuilder,StringBuilder srcBuilder,int startIndex,int endIndex)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            dstBuilder.Append(srcBuilder[i]);
        }
    }

    private void NewLine(bool isLastLine)
    {
        //处理旧行数据
        mLuaFunc.BeginPCall();

        UILabel lb = NewLabel(false);
        lb.enabled = true;
        lb.text = mLineBuilder.ToString();
        lb.transform.parent = mLabelRoot;
        float curLineHeight = lb.height;

        mLuaFunc.Push(lb);
        mLuaFunc.Push(mLineEmojis.Count);

        for (int i = 0; i < mLineEmojis.Count; i++)
        {
            UISprite sp = NewSprite(false, mLineEmojis[i].emojiName);
            sp.MakePixelPerfect();
            sp.transform.parent = lb.transform;
            sp.transform.localPosition = new Vector3(mLineEmojis[i].emojiOffset, 0, 0);
            curLineHeight = Mathf.Max(curLineHeight, mLineEmojis[i].emojiHeight);

            mLuaFunc.Push(sp);
        }
        if (mAlign == AlignType.LEFT)
        {
            mCurWidth = Mathf.Max(mLineData.curWidth, mCurWidth);
            if (mLineData.index == 0)
            {
                lb.transform.localPosition = new Vector3(0, -curLineHeight / 2, 0);
            }
            else
            {
                lb.transform.localPosition = new Vector3(0, mCurHeight - mLineSpace - curLineHeight / 2, 0);
            }
        }
        else if (mAlign == AlignType.RIGHT)
        {
            if (mLineData.index == 0 && isLastLine)
            {
                lb.transform.localPosition = new Vector3(-mLineData.curWidth, -curLineHeight / 2, 0);
                mCurWidth = mLineData.curWidth;
            }
            else
            {
                mCurWidth = mMaxWidth;
                if (mLineData.index == 0)
                {
                    lb.transform.localPosition = new Vector3(mMaxWidth, -curLineHeight / 2, 0);
                }
                else
                {
                    lb.transform.localPosition = new Vector3(mMaxWidth, mCurHeight - mLineSpace - curLineHeight / 2, 0);
                }
            }
        }
        mCurHeight = lb.transform.localPosition.y - curLineHeight / 2;

        mLuaFunc.Push(mLineData.index);
        mLuaFunc.Push(mCurWidth);
        mLuaFunc.Push(mCurHeight);
        mLuaFunc.Push(isLastLine);
        for (int i = 0; i < mLineClicks.Count; i++)
        {
            ClickData click = mLineClicks[i];
            ShareData share = mShareSymbols[click.key];
            mLuaFunc.Push(share.shareID);
            mLuaFunc.Push(click.startOffset);
            mLuaFunc.Push(click.endOffset);
        }
        mLuaFunc.Push(mUIComponent);
        mLuaFunc.Push(mItemIndex);
        mLuaFunc.PCall();
        mLuaFunc.EndPCall();

        if (!isLastLine)
        {
            //清理旧行数据
            mLineData.curWidth = 0;
            mLineData.index = mLineData.index + 1;
            mLineBuilder.Remove(0, mLineBuilder.Length);
            mLineClicks.Clear();
            mLineEmojis.Clear();
            //记录点击数据
            if (mNextLineClickData.index != -1)
            {
                mLineClicks.Add(mNextLineClickData);
            }
            mNextLineClickData.index = -1;
            //记录颜色数据
            for (int j = 0; j < mColorDatas.Count; j++)
            {
                ColorData data = mColorDatas[j];
                for (int k = data.index; k < data.index + data.length; k++)
                {
                    mLineBuilder.Append(mBuilder[k]);
                }
            }
        }
        else
        {
            mLineData.index = -1;
        }
    }

    private void NewClick(int index)
    {
        //分割点击信息
        for (int j = 0; j < mClickDatas.Count; j++)
        {
            ClickData data = mClickDatas[j];
            if (index == data.index)
            {
                data.startOffset = mLineData.curWidth;
                if (mLineData.curWidth + data.width > mMaxWidth)
                {
                    data.endOffset = mMaxWidth;

                    mNextLineClickData.index = index;
                    mNextLineClickData.width = 0;
                    mNextLineClickData.startOffset = 0;
                    mNextLineClickData.endOffset = mLineData.curWidth + data.width - mMaxWidth;
                    mNextLineClickData.key = data.key;
                }
                else
                {
                    data.endOffset = mLineData.curWidth + data.width;
                }
                mLineClicks.Add(data);
                break;
            }
        }
    }

    private UILabel NewLabel(bool createNew)
    {
        if(createNew || mLabelCache.Count == 0)
        {
            Transform t = mUIComponent.Duplicate(mLabel.transform, transform, ++mLabelCount);
            t.name = string.Format("lb_{0}", mLabelCount);
            t.gameObject.SetActive(true);
            UILabel lb = t.GetComponent<UILabel>();
            lb.enabled = false;
            lb.color = Color.white;
            mLabelCache.Enqueue(lb);
        }
        if (!createNew)
            return mLabelCache.Dequeue();
        else
            return null;
    }

    private UISprite NewSprite(bool createNew,string spName)
    {
        if (createNew || mSpriteCache.Count == 0)
        {
            Transform t = mUIComponent.Duplicate(mSprite.transform, transform, ++mSpriteCount);
            t.name = string.Format("sp_{0}", mSpriteCount);
            t.gameObject.SetActive(true);
            UISprite sp = t.GetComponent<UISprite>();
            sp.enabled = false;
            mSpriteCache.Enqueue(sp);

            UISpriteAnimation spa = t.GetComponent<UISpriteAnimation>();
            spa.enabled = false;
            mSpriteAnimCache[sp] = spa;
        }
        if (!createNew)
        {
            UISprite sp = mSpriteCache.Dequeue();
            UISpriteAnimation spa = mSpriteAnimCache[sp];
            sp.enabled = true;
            spa.enabled = true;
            sp.spriteName = spName;
            spa.namePrefix = spName;
            return sp;
        }            
        else
        {
            return null;
        }           
    }
}

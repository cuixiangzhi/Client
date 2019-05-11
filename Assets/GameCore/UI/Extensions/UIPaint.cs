using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UIPaintPoint
{
    public int x;
    public int y;
    public int frame;
    public int prev;
    public int next;
    public int diameter;
    public Color color;
}

public class UIPaintConfig
{
    public int pointdiameter = 11;
    public Color pointColor = Color.cyan;
    public UIPaintPoint pointNew;
    public int pointType = -1;

    public int invalidTouchID = -100;
    public bool multiTouch = false;

    public int leftTouchID = -100;
    public int leftTouchLastOffset = -1;

    public int rightTouchID = -100;
    public int rightTouchLastOffset = -1;

    public int currentPressID = -100;

    public int leftPressID = -100;
    public int leftPressLastOffset = -1;

    public int rightPressID = -100;
    public int rightressLastOffset = -1;

    public bool repaint = false;
    public bool frameChanged = false;
    public int paintFrame = -1;
    public int paintOffset = -1;

    public Color[] clearBlock = new Color[128];

    public UIPaintConfig()
    {
        for (int i = 0; i < clearBlock.Length; i++)
        {
            clearBlock[i] = Color.clear;
        }
    }
}

public sealed class UIPaint : UITexture
{
    public UIPaintConfig mConfig = new UIPaintConfig();
    public List<UIPaintPoint> mPoints = new List<UIPaintPoint>(1024);
    public Texture2D mPaintBoard = null;

    private void OnGUI()
    {
        if (GUILayout.Button("重播", GUILayout.Width(300), GUILayout.Height(150)))
        {
            PlayPoints();
        }
        if (GUILayout.Button("重画", GUILayout.Width(300), GUILayout.Height(150)))
        {
            ClearAll();
        }
    }

    public void ClearAll()
    {
        clear_pixels();
        clear_points();
    }

    public void PlayPoints()
    {
        if(mPoints.Count != 0)
        {
            clear_pixels();
            mConfig.repaint = true;
            mConfig.paintFrame = mPoints[0].frame;
            mConfig.paintOffset = 0;
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if(mConfig.repaint && mConfig.paintFrame > 0 && mConfig.paintOffset >= 0 && mConfig.paintOffset < mPoints.Count)
        {
            mConfig.frameChanged = false;
            for (; mConfig.paintOffset < mPoints.Count; ++mConfig.paintOffset)
            {
                UIPaintPoint point = mPoints[mConfig.paintOffset];
                if (point.frame <= mConfig.paintFrame) { draw_line(point, false); mConfig.frameChanged = true; }
                else {  break; }
            }
            ++mConfig.paintFrame;
            if (mConfig.frameChanged) mPaintBoard.Apply();
            if (mConfig.paintOffset >= mPoints.Count) { mConfig.repaint = false;mConfig.paintFrame = -1;mConfig.paintOffset = -1; }
        }
    }

    private void LateUpdate()
    {
        if(mConfig.leftPressID != mConfig.invalidTouchID)
        {
            UICamera.MouseOrTouch touch = UICamera.GetTouch(mConfig.leftPressID, false);
            if(touch != null && !touch.dragStarted && touch.delta.sqrMagnitude > 0.001f)
            {
                mConfig.currentPressID = mConfig.leftPressID;
                add_new_point(false, false, false, false, true, false);
                mConfig.currentPressID = mConfig.invalidTouchID;
            }
        }
        if(mConfig.rightPressID != mConfig.invalidTouchID)
        {
            UICamera.MouseOrTouch touch = UICamera.GetTouch(mConfig.rightPressID, false);
            if (touch != null && !touch.dragStarted && touch.delta.sqrMagnitude > 0.001f)
            {
                mConfig.currentPressID = mConfig.rightPressID;
                add_new_point(false, false, false, false, true, false);
                mConfig.currentPressID = mConfig.invalidTouchID;
            }
        }
    }

    private void OnPress(bool press)
    {
        if (!mConfig.repaint)
            add_new_point(false, false, false, press, false, !press);
    }

    private void OnDragStart()
    {
        if (!mConfig.repaint)
            add_new_point(true, false, false, false, false, false);
    }

    private void OnDrag(Vector2 delta)
    {
        if (!mConfig.repaint)
            add_new_point(false, true, false, false, false, false);
    }

    private void OnDragEnd()
    {
        if (!mConfig.repaint)
            add_new_point(false, false, true, false, false, false);
    }

    private bool is_valid_touch(bool drag_start, bool drag_update, bool drag_end, bool press_start, bool press_update, bool press_end)
    {
        //multi touch control
        if ((drag_update || drag_end) && mConfig.leftTouchID != UICamera.currentTouchID && mConfig.rightTouchID != UICamera.currentTouchID)
            return false;
        if ((press_update || press_end) && mConfig.leftPressID != UICamera.currentTouchID && mConfig.rightPressID != UICamera.currentTouchID)
            return false;
        if (mConfig.multiTouch)
        {
            if (drag_start && mConfig.leftTouchID != mConfig.invalidTouchID && mConfig.rightTouchID != mConfig.invalidTouchID)
                return false;
            if (press_start && mConfig.leftPressID != mConfig.invalidTouchID && mConfig.rightPressID != mConfig.invalidTouchID)
                return false;
        }
        else
        {
            if (drag_start && (mConfig.leftTouchID != mConfig.invalidTouchID || mConfig.rightTouchID != mConfig.invalidTouchID))
                return false;
            if (press_start && (mConfig.leftPressID != mConfig.invalidTouchID || mConfig.rightPressID != mConfig.invalidTouchID))
                return false;
        }
        return true;
    }

    private bool is_valid_point(bool drag_start, bool drag_update, bool drag_end, bool press_start, bool press_update, bool press_end)
    {
        if(mConfig.pointNew.prev >= 0 && mConfig.pointNew.prev < mPoints.Count)
        {
            UIPaintPoint point = mPoints[mConfig.pointNew.prev];
            if(point.x == mConfig.pointNew.x && point.y == mConfig.pointNew.y)
            {
                return true;
            }
        }
        if (press_end) return false;
        return true;
    }

    private void add_new_point(bool drag_start,bool drag_update,bool drag_end,bool press_start,bool press_update,bool press_end)
    {
        if(is_valid_touch(drag_start, drag_update, drag_end, press_start, press_update, press_end))
        {
            new_point();
            new_left_drag(drag_start, drag_update, drag_end);
            new_right_drag(drag_start, drag_update, drag_end);
            new_left_press(press_start, press_update, press_end);
            new_right_press(press_start, press_update, press_end);
            if(is_valid_point(drag_start, drag_update, drag_end, press_start, press_update, press_end))
            {
                mPoints.Add(mConfig.pointNew);
                draw_line(mConfig.pointNew);
            }
            else
            {
                switch(mConfig.pointType)
                {
                    case -1:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                }
            }
        }
    }

    private void new_point()
    {
        //convert point to local space
        Vector2 worldPos = UICamera.currentCamera.ScreenToWorldPoint(UICamera.lastEventPosition);
        Vector2 localPos = cachedTransform.InverseTransformPoint(worldPos);
        mConfig.pointNew = new UIPaintPoint();
        //point position in local space
        mConfig.pointNew.x = Mathf.RoundToInt(localPos.x);
        mConfig.pointNew.y = Mathf.RoundToInt(localPos.y);
        //point render frame,cache to repaint
        mConfig.pointNew.frame = Time.frameCount;
        //diameter and color
        mConfig.pointNew.diameter = mConfig.pointdiameter;
        mConfig.pointNew.color = mConfig.pointColor;
        //link to last and next
        mConfig.pointNew.prev = -1;
        mConfig.pointNew.next = -1;
    }

    private void new_left_drag(bool drag_start, bool drag_update, bool drag_end)
    {
        if(mConfig.leftTouchID == UICamera.currentTouchID && drag_update)
        {
            //link prev->next = cur
            if (mConfig.leftTouchLastOffset >= 0 && mConfig.leftTouchLastOffset < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.leftTouchLastOffset];
                prev.next = mPoints.Count;
                mPoints[mConfig.leftTouchLastOffset] = prev;
            }
            //link cur->prev = last
            mConfig.pointNew.prev = mConfig.leftTouchLastOffset;
            mConfig.leftTouchLastOffset = mPoints.Count;
            mConfig.pointType = 1;
        }
        else if(mConfig.leftTouchID == mConfig.invalidTouchID && drag_start)
        {
            //new drag point
            mConfig.leftTouchID = UICamera.currentTouchID;
            mConfig.leftTouchLastOffset = mPoints.Count;
            if(mConfig.leftTouchID == mConfig.leftPressID)
            {
                mConfig.pointNew.prev = mConfig.leftPressLastOffset;
            }
            else if(mConfig.leftTouchID == mConfig.rightPressID)
            {
                mConfig.pointNew.prev = mConfig.rightressLastOffset;
            }            
            //link prev->next = cur
            if (mConfig.pointNew.prev >= 0 && mConfig.pointNew.prev < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.pointNew.prev];
                prev.next = mPoints.Count;
                mPoints[mConfig.pointNew.prev] = prev;
            }
            mConfig.pointType = 1;
        }
        else if(mConfig.leftTouchID == UICamera.currentTouchID && drag_end)
        {
            //end drag point
            mConfig.pointNew.prev = mConfig.leftTouchLastOffset;
            mConfig.leftTouchID = mConfig.invalidTouchID;
            mConfig.leftTouchLastOffset = -1;
            mConfig.pointType = 1;
        }
    }

    private void new_right_drag(bool drag_start, bool drag_update, bool drag_end)
    {
        if(mConfig.leftTouchID == UICamera.currentTouchID)
        {
            //do nothing
        }
        else if (mConfig.rightTouchID == UICamera.currentTouchID && drag_update)
        {
            //link prev->next = cur
            if (mConfig.rightTouchLastOffset >= 0 && mConfig.rightTouchLastOffset < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.rightTouchLastOffset];
                prev.next = mPoints.Count;
                mPoints[mConfig.rightTouchLastOffset] = prev;
            }
            //link cur->prev = last
            mConfig.pointNew.prev = mConfig.rightTouchLastOffset;
            mConfig.rightTouchLastOffset = mPoints.Count;
            mConfig.pointType = 2;
        }
        else if (mConfig.rightTouchID == mConfig.invalidTouchID && drag_start)
        {
            //new drag point                
            mConfig.rightTouchID = UICamera.currentTouchID;
            mConfig.rightTouchLastOffset = mPoints.Count;
            if (mConfig.rightTouchID == mConfig.leftPressID)
            {
                mConfig.pointNew.prev = mConfig.leftPressLastOffset;
            }
            else if (mConfig.rightTouchID == mConfig.rightPressID)
            {
                mConfig.pointNew.prev = mConfig.rightressLastOffset;
            }
            //link prev->next = cur
            if (mConfig.pointNew.prev >= 0 && mConfig.pointNew.prev < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.pointNew.prev];
                prev.next = mPoints.Count;
                mPoints[mConfig.pointNew.prev] = prev;
            }
            mConfig.pointType = 2;
        }
        else if (mConfig.rightTouchID == UICamera.currentTouchID && drag_end)
        {
            //end drag point
            mConfig.pointNew.prev = mConfig.rightTouchLastOffset;
            mConfig.rightTouchID = mConfig.invalidTouchID;
            mConfig.rightTouchLastOffset = -1;
            mConfig.pointType = 2;
        }
    }

    private void new_left_press(bool press_start, bool press_update, bool press_end)
    {
        if (mConfig.leftPressID == mConfig.currentPressID && press_update)
        {
            //link prev->next = cur
            if (mConfig.leftPressLastOffset >= 0 && mConfig.leftPressLastOffset < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.leftPressLastOffset];
                prev.next = mPoints.Count;
                mPoints[mConfig.leftPressLastOffset] = prev;
            }
            //link cur->prev = last
            mConfig.pointNew.prev = mConfig.leftPressLastOffset;
            mConfig.leftPressLastOffset = mPoints.Count;
            mConfig.pointType = 3;
        }
        else if (mConfig.leftPressID == mConfig.invalidTouchID && press_start)
        {
            //new drag point
            mConfig.leftPressID = UICamera.currentTouchID;
            mConfig.leftPressLastOffset = mPoints.Count;
            mConfig.pointType = 3;
        }
        else if (mConfig.leftPressID == UICamera.currentTouchID && press_end)
        {
            //end press point
            mConfig.pointNew.prev = mConfig.leftPressLastOffset;
            mConfig.leftPressID = mConfig.invalidTouchID;
            mConfig.leftPressLastOffset = -1;
            mConfig.pointType = -1;
        }
    }

    private void new_right_press(bool press_start, bool press_update, bool press_end)
    {
        if (mConfig.leftPressID == UICamera.currentTouchID && press_start)
        {
            //do nothing
        }
        else if(mConfig.rightPressID == mConfig.currentPressID && press_update)
        {
            //link prev->next = cur
            if (mConfig.rightressLastOffset >= 0 && mConfig.rightressLastOffset < mPoints.Count)
            {
                UIPaintPoint prev = mPoints[mConfig.rightressLastOffset];
                prev.next = mPoints.Count;
                mPoints[mConfig.rightressLastOffset] = prev;
            }
            //link cur->prev = last
            mConfig.pointNew.prev = mConfig.rightressLastOffset;
            mConfig.rightressLastOffset = mPoints.Count;
            mConfig.pointType = 4;
        }
        else if (mConfig.rightPressID == mConfig.invalidTouchID && press_start)
        {
            //new press point
            mConfig.rightPressID = UICamera.currentTouchID;
            mConfig.rightressLastOffset = mPoints.Count;
            mConfig.pointType = 4;
        }
        else if (mConfig.rightPressID == UICamera.currentTouchID && press_end)
        {
            //end press point
            mConfig.pointNew.prev = mConfig.rightressLastOffset;
            mConfig.rightPressID = mConfig.invalidTouchID;
            mConfig.rightressLastOffset = -1;
            mConfig.pointType = -1;
        }
    }

    private void draw_line(UIPaintPoint point,bool apply = true)
    {
        if (point.prev >= 0 && point.prev < mPoints.Count)
        {
            UnityEngine.Profiling.Profiler.BeginSample("COST FOR DRAW_LINE");
            draw_line(mPoints[point.prev], point);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        else
        {
            draw_pixels_circle(point.x, point.y, (point.diameter - 1) / 2, point.color);
        }

        if (apply && mPaintBoard != null) mPaintBoard.Apply();
    }

    private void draw_line(UIPaintPoint start, UIPaintPoint end)
    {
        if (mPaintBoard != null)
        {
            if (start.x == end.x && start.y == end.y)
            {
                draw_pixels_circle(end.x, end.y, (end.diameter - 1) / 2, end.color);
            }
            else if (start.y == end.y)
            {
                draw_pixels_horizontal(start.x < end.x ? start.x : end.x, start.x < end.x ? end.x : start.x, end.y, end.diameter, end.color);
            }
            else if (start.x == end.x)
            {
                draw_pixels_vertical(start.y < end.y ? start.y : end.y, start.y < end.y ? end.y : start.y, end.x, end.diameter, end.color);
            }
            else
            {
                int x0 = 0, y0 = 0, x1 = Mathf.Abs(start.x - end.x), y1 = Mathf.Abs(start.y - end.y);
                int quadrant = start.x < end.x ? (start.y < end.y ? 1 : 4) : (start.y < end.y ? 2 : 3);
                draw_pixels_lean(x0, x1, y0, y1, start.x, start.y, quadrant, end.diameter, end.color);
            }
        }
    }

    private void draw_pixels_lean(int x0, int x1, int y0, int y1, int startx,int starty, int quadrant, int diameter,Color color)
    {
        UnityEngine.Profiling.Profiler.BeginSample("COST FOR draw_pixels_lean");
        if (mPaintBoard != null)
        {
            bool kIsZeroToOne = true;
            if (y1 > x1) { int tmp = x1; x1 = y1; y1 = tmp; kIsZeroToOne = false; }
            int radius = (diameter - 1) / 2;
            int x = x0, y = y0, a = (y0 - y1), b = (x1 - x0);
            int d = 2 * a + b, d1 = 2 * a, d2 = 2 * (a + b);
            bool minusl = (quadrant == 1 || quadrant == 4) ? false : true;
            bool minusr = (quadrant == 1 || quadrant == 2) ? false : true;
            int xt, yt;
            while (x < x1)
            {
                if (d < 0) { d += d2; ++x; ++y; }
                else { d += d1; ++x; }
                xt = !kIsZeroToOne ? (x0 + (minusl ? -y : y)) : (x0 + (minusl ? -x : x));
                yt = !kIsZeroToOne ? (y0 + (minusr ? -x : x)) : (y0 + (minusr ? -y : y));
                draw_pixels_circle(startx + xt, starty + yt, radius, color);
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void draw_pixels_horizontal(int xmin, int xmax, int y,int diameter, Color color)
    {
        UnityEngine.Profiling.Profiler.BeginSample("COST FOR draw_pixels_horizontal");
        if (mPaintBoard != null)
        {
            int radius = (diameter - 1) / 2;
            for (int x = xmin; x <= xmax; ++x)
            {
                for (int cy = y - radius; cy <= y + radius; ++cy)
                {
                    mPaintBoard.SetPixel(x, cy, color);
                }
            }
            draw_pixels_circle(xmin, y, radius, color);
            draw_pixels_circle(xmax, y, radius, color);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void draw_pixels_vertical(int ymin, int ymax, int x, int diameter, Color color)
    {
        UnityEngine.Profiling.Profiler.BeginSample("COST FOR draw_pixels_vertical");
        if (mPaintBoard != null)
        {
            int radius = (diameter - 1) / 2;
            for (int y = ymin; y <= ymax; ++y)
            {
                for (int cx = x - radius; cx <= x + radius; ++cx)
                {
                    mPaintBoard.SetPixel(cx, y, color);
                }
            }
            draw_pixels_circle(x, ymin, radius, color);
            draw_pixels_circle(x, ymax, radius, color);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void draw_pixels_circle(int xc,int yc,int radius,Color color)
    {
        if (mPaintBoard != null)
        {
            if (radius == 0)
            {
                mPaintBoard.SetPixel(xc, yc, color);
            }
            else
            {
                int x = 0, y = radius;
                int d = 5 - 4 * y;
                draw_pixels(xc + x, xc + x, yc - y, yc + y, color);
                while (x < y)
                {
                    if (d <= 0) { d += (2 * x + 3) * 4; ++x; }
                    else { d += (2 * (x - y) + 5) * 4; ++x; --y; }
                    draw_pixels(xc + x, xc + x, yc - y, yc + y, color);
                    draw_pixels(xc - x, xc - x, yc - y, yc + y, color);
                    draw_pixels(xc + y, xc + y, yc - x, yc + x, color);
                    draw_pixels(xc - y, xc - y, yc - x, yc + x, color);
                }
            }
        }
    }

    private void draw_pixels(int xmin,int xmax,int ymin,int ymax,Color color)
    {
        if (mPaintBoard != null)
        {
            for (int x = xmin; x <= xmax; ++x)
            {
                for (int y = ymin; y <= ymax; ++y)
                {
                    mPaintBoard.SetPixel(x, y, color);
                }
            }
        }
    }

    private void clear_pixels()
    {
        if (mainTexture == null)
        {
            mainTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            mainTexture.wrapMode = TextureWrapMode.Clamp;
            mPaintBoard = mainTexture as Texture2D;
        }
        else
        {
            mainTexture.wrapMode = TextureWrapMode.Clamp;
            mPaintBoard = mainTexture as Texture2D;
        }
        UnityEngine.Profiling.Profiler.BeginSample("COST FOR CLEAR SET");
        int blockWidth = mConfig.clearBlock.Length;
        bool enoughWidth = false;int realBlockWidth = 0;
        for (int x = 0; x < width; x += blockWidth)
        {
            enoughWidth = x + blockWidth < width;
            realBlockWidth = enoughWidth ? blockWidth : width - x;
            for (int y = 0; y < height; y++)
            {
                mPaintBoard.SetPixels(x, y, realBlockWidth, 1, mConfig.clearBlock);
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("COST FOR CLEAR APPLY");
        mPaintBoard.Apply();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void clear_points()
    {
        mConfig.repaint = false;
        mConfig.paintFrame = -1;
        mConfig.paintOffset = -1;

        mConfig.leftTouchID = mConfig.invalidTouchID;
        mConfig.rightTouchID = mConfig.invalidTouchID;

        mPoints.Clear();
    }
}

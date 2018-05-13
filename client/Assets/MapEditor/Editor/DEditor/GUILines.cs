using UnityEngine;
using System.Collections;
using UnityEditor;

public class GUILines
{
    public enum ConnectionLineStyle
    {
        Bezier,
        Linear,
        Rectilinear
    }
    private const float partOffsetFactor = 10f;
    private const float connectionWidth = 2f;
    public static Color connectionColor = new Color(1f, 1f, 1f, 0.3f);
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
    {
        Handles.BeginGUI();
        Handles.color = (color);
        if (antiAlias)
        {
            Handles.DrawAAPolyLine(width, new Vector3[]
				{
					pointA,
					pointB
				});
        }
        else
        {
            Handles.DrawPolyLine(new Vector3[]
				{
					pointA,
					pointB
				});
        }
        Handles.EndGUI();
    }
    public static void DrawDisc(Vector2 center, float radius, Color color)
    {
        Handles.BeginGUI();
        Handles.color = (color);
        Handles.DrawWireDisc(center, Vector3.forward, radius);
        Handles.EndGUI();
    }
    public static void DrawDashed(Vector2[] points, Color color, float width, bool antiAlias, int dashSeg)
    {
        Handles.BeginGUI();
        Handles.color = (color);
        Vector3[] array;
        float dashStep = points.Length / (float)dashSeg;
        float steps = 0;
        for (int i = 0; i < points.Length;)
        {
            int length = Mathf.Min((int)dashStep, points.Length - i);
            array = new Vector3[length];
            for (int j = 0; j < length; j++)
            {
                array[j] = new Vector3(points[i + j].x, points[i + j].y);
            }
            steps += dashStep * 2;
            i = (int)steps;
            if (antiAlias)
            {
                Handles.DrawAAPolyLine(width, array);
            }
            else
            {
                Handles.DrawPolyLine(array);
            }
        }
        Handles.EndGUI();
    }
    public static void DrawLines(Vector2[] points, Color color, float width, bool antiAlias)
    {
        Handles.BeginGUI();
        Handles.color = (color);
        Vector3[] array = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            array[i] = new Vector3(points[i].x, points[i].y);
        }
        if (antiAlias)
        {
            Handles.DrawAAPolyLine(width, array);
        }
        else
        {
            Handles.DrawPolyLine(array);
        }
        Handles.EndGUI();
    }
    public static void DrawLines(Color color, float width, bool antiAlias, params Vector2[] points)
    {
        DrawLines(points, color, width, antiAlias);
    }
    public static void Highlight(Rect r, float offset, int strength = 1)
    {
        r.xMax = (r.xMax + 1f);
        r = GetExpanded(r, offset);
        for (int i = 0; i < strength; i++)
        {
            GUI.Box(r, string.Empty);
        }
    }
    public static void DrawCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color color, float width, bool antiAlias, int segments)
    {
        Vector2 pointA = GUILines.CubicBezier(p0, p1, p2, p3, 0f);
        for (int i = 1; i <= segments; i++)
        {
            Vector2 vector = GUILines.CubicBezier(p0, p1, p2, p3, (float)i / (float)segments);
            GUILines.DrawLine(pointA, vector, color, width, antiAlias);
            pointA = vector;
        }
    }
    public static void DrawCubicBezierOffset(float offset, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color color, float width, bool antiAlias, int segments)
    {
        Vector2 pointA = GUILines.CubicBezierOffset(offset, p0, p1, p2, p3, 0f);
        for (int i = 1; i <= segments; i++)
        {
            Vector2 vector = GUILines.CubicBezierOffset(offset, p0, p1, p2, p3, (float)i / (float)segments);
            GUILines.DrawLine(pointA, vector, color, width, antiAlias);
            pointA = vector;
        }
    }
    public static Vector2[] ConnectionBezierOffsetArray(float offset, BaseNode startCon, BaseNode endCon, int segments)
    {
        Vector2 connectionPoint = endCon.leftHand_Rect.center;
        Vector2 connectionPoint2 = startCon.rightHand_Rect.center;
        bool flag = connectionPoint.x < connectionPoint2.x;
        int num = segments + 1;
        Vector2[] array;
        if (flag)
        {
            array = new Vector2[num * 2];
        }
        else
        {
            array = new Vector2[num];
        }
        if (flag)
        {
            float num2;
            if (startCon.body_Rect.center.y > endCon.body_Rect.center.y)
            {
                num2 = (startCon.body_Rect.yMin + endCon.body_Rect.yMax) / 2f;
            }
            else
            {
                num2 = (startCon.body_Rect.yMax + endCon.body_Rect.yMin) / 2f;
            }
            float num3 = Mathf.Abs(connectionPoint.x - connectionPoint2.x);
            float num4 = Mathf.InverseLerp(0f, 100f, num3);
            num4 = Smoother(num4) * 0.707106769f;
            Vector2 vector = connectionPoint;
            Vector2 vector2 = new Vector2(connectionPoint.x, num2);
            float num5 = Mathf.Abs(vector.y - vector2.y) * num4;
            Vector2 vector3 = new Vector2(num5, 0f);
            Vector2 p = vector - vector3;
            Vector2 p2 = vector2 - vector3;
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i] = GUILines.CubicBezierOffset(offset, vector, p, p2, vector2, t);
            }
            Vector2 vector4 = new Vector2(connectionPoint2.x, num2);
            Vector2 vector5 = connectionPoint2;
            num5 = Mathf.Abs(vector4.y - vector5.y) * num4;
            vector3 = new Vector2(num5, 0f);
            Vector2 p3 = vector4 + vector3;
            Vector2 p4 = vector5 + vector3;
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i + num] = GUILines.CubicBezierOffset(offset, vector4, p3, p4, vector5, t);
            }
        }
        else
        {
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i] = GUILines.ConnectionBezierOffset(offset, connectionPoint, connectionPoint2, t);
            }
        }
        return array;
    }
    public static Vector2[] ConnectionBezierOffsetArray(float offset, Rect start, Rect s_body, Rect end, Rect e_body, int segments)
    {
        Vector2 connectionPoint = start.center;
        Vector2 connectionPoint2 = end.center;
        bool flag = connectionPoint.x < connectionPoint2.x;
        int num = segments + 1;
        Vector2[] array;
        if (flag)
        {
            array = new Vector2[num * 2];
        }
        else
        {
            array = new Vector2[num];
        }
        if (flag)
        {
            float num2;
            if (start.center.y > end.center.y)
            {
                num2 = (s_body.yMin + e_body.yMax) / 2f;
            }
            else
            {
                num2 = (s_body.yMax + e_body.yMin) / 2f;
            }
            float num3 = Mathf.Abs(connectionPoint.x - connectionPoint2.x);
            float num4 = Mathf.InverseLerp(0f, 100f, num3);
            num4 = Smoother(num4) * 0.707106769f;
            Vector2 vector = connectionPoint;
            Vector2 vector2 = new Vector2(connectionPoint.x, num2);
            float num5 = Mathf.Abs(vector.y - vector2.y) * num4;
            Vector2 vector3 = new Vector2(num5, 0f);
            Vector2 p = vector - vector3;
            Vector2 p2 = vector2 - vector3;
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i] = GUILines.CubicBezierOffset(offset, vector, p, p2, vector2, t);
            }
            Vector2 vector4 = new Vector2(connectionPoint2.x, num2);
            Vector2 vector5 = connectionPoint2;
            num5 = Mathf.Abs(vector4.y - vector5.y) * num4;
            vector3 = new Vector2(num5, 0f);
            Vector2 p3 = vector4 + vector3;
            Vector2 p4 = vector5 + vector3;
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i + num] = GUILines.CubicBezierOffset(offset, vector4, p3, p4, vector5, t);
            }
        }
        else
        {
            for (int i = 0; i < num; i++)
            {
                float t = (float)i / (float)segments;
                array[i] = GUILines.ConnectionBezierOffset(offset, connectionPoint, connectionPoint2, t);
            }
        }

        return array;
    }
    public static void DrawConnectionBezierArray(Rect start, Rect s_body, Rect end, Rect e_body, int segments, float width, Color color, bool antiAlias)
    {
        Vector2[] array = ConnectionBezierOffsetArray(0, start, s_body, end, e_body, segments);
        GUILines.DrawLines(array, color, width, antiAlias);
    }
    public static void DrawDashedConnectionBezierArray(Rect start, Rect s_body, Rect end, Rect e_body, int segments, float width, Color color, bool antiAlias, int dashSeg)
    {
        Vector2[] array = ConnectionBezierOffsetArray(0, start, s_body, end, e_body, segments);
        GUILines.DrawDashed(array, color, width, antiAlias, dashSeg);
    }
    public static Vector2 ConnectionBezierOffset(float offset, Vector2 start, Vector2 end, float t)
    {
        float num = (start.x > end.x) ? 1f : 4f;
        float num2 = Mathf.Abs(end.x - start.x) * 0.5f * num;
        Vector2 p = new Vector2(start.x - num2, start.y);
        Vector2 p2 = new Vector2(end.x + num2, end.y);
        return GUILines.CubicBezierOffset(offset, start, p, p2, end, t);
    }
    public static Vector2 CubicBezierOffset(float offset, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 vector = GUILines.QuadBezier(p0, p1, p2, t);
        Vector2 vector2 = GUILines.QuadBezier(p1, p2, p3, t);
        Vector2 vector3 = GUILines.Lerp(vector, vector2, t);
        Vector2 normalized = (vector2 - vector).normalized;
        Vector2 vector4 = new Vector2(-normalized.y, normalized.x);
        return vector3 + vector4 * offset;
    }
    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 v = GUILines.QuadBezier(p0, p1, p2, t);
        Vector2 v2 = GUILines.QuadBezier(p1, p2, p3, t);
        return GUILines.Lerp(v, v2, t);
    }
    public static Vector2 QuadBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float num = t * t;
        float num2 = t * 2f;
        return p0 * (num - num2 + 1f) + p1 * (num2 - 2f * num) + p2 * num;
    }
    public static Vector2 Lerp(Vector2 v0, Vector2 v1, float t)
    {
        return v0 * (1f - t) + t * v1;
    }
    public static void QuickBezier(Vector2 p0, Vector2 p1, Color color, int detail = 12, int width = 2)
    {
        Vector2 pointA = p0;
        for (float num = 1f; num < (float)(detail - 1); num += 1f)
        {
            float num2 = num / (float)(detail - 2);
            Vector2 vector = new Vector2(Mathf.Lerp(p0.x, p1.x, num2), Mathf.Lerp(p0.y, p1.y, Smooth(num2)));
            GUILines.DrawLine(pointA, vector, color, (float)width, true);
            pointA = vector;
        }
    }
    public static void DrawStyledConnection(ConnectionLineStyle style, Vector2 a, Vector2 b, int cc, Color col)
    {
        switch (style)
        {
            case ConnectionLineStyle.Bezier:
                GUILines.DrawMultiBezierConnection(a, b, cc, col);
                break;
            case ConnectionLineStyle.Linear:
                GUILines.DrawMultiLinearConnection(a, b, cc, col);
                break;
            case ConnectionLineStyle.Rectilinear:
                GUILines.DrawMultiRectilinearConnection(a, b, cc, col);
                break;
        }
    }
    public static void DrawMultiRectilinearConnection(Vector2 p0, Vector2 p1, int count, Color col)
    {
        float num = 10f / (float)count;
        float num2 = (float)(-(float)(count - 1)) * 0.5f * num;
        for (int i = 0; i < count; i++)
        {
            float offset = num2 + num * (float)i;
            GUILines.DrawRectilinearConnection(p0, p1, offset, col);
        }
    }
    public static void DrawMultiLinearConnection(Vector2 p0, Vector2 p1, int count, Color col)
    {
        float num = 10f / (float)count;
        float num2 = (float)(-(float)(count - 1)) * 0.5f * num;
        for (int i = 0; i < count; i++)
        {
            float offset = num2 + num * (float)i;
            GUILines.DrawLinearConnection(p0, p1, offset, col);
        }
    }
    public static void DrawMultiBezierConnection(Vector2 p0, Vector2 p1, int count, Color col)
    {
        float num = 10f / (float)count;
        float num2 = (float)(-(float)(count - 1)) * 0.5f * num;
        for (int i = 0; i < count; i++)
        {
            float offset = num2 + num * (float)i;
            GUILines.DrawBezierConnection(p0, p1, offset, col);
        }
    }
    public static void DrawRectilinearConnection(Vector2 p0, Vector2 p1, float offset, Color col)
    {
        p0 += new Vector2(0f, offset);
        p1 += new Vector2(0f, offset);
        Vector2 vector = new Vector2((p0.x + p1.x) / 2f + ((p0.y < p1.y) ? (-offset) : offset), p0.y);
        Vector2 vector2 = new Vector2(vector.x, p1.y);
        GUILines.DrawLine(p0, vector, col, 2f, true);
        GUILines.DrawLine(vector, vector2, col, 2f, true);
        GUILines.DrawLine(vector2, p1, col, 2f, true);
    }
    public static void DrawLinearConnection(Vector2 p0, Vector2 p1, float offset, Color col)
    {
        p0 += new Vector2(0f, offset);
        p1 += new Vector2(0f, offset);
        GUILines.DrawLine(p0, p1, col, 2f, true);
    }
    public static void DrawDashedLine(Vector2 p0, Vector2 p1, Color col, float dashLength)
    {
        float num = dashLength / (p0 - p1).magnitude;
        for (float num2 = 0f; num2 < 1f; num2 += num * 2f)
        {
            float num3 = Mathf.Min(1f, num2 + num);
            GUILines.DrawLine(Vector2.Lerp(p0, p1, num2), Vector2.Lerp(p0, p1, num3), col, 2f, true);
        }
    }
    public static void DrawBezierConnection(Vector2 p0, Vector2 p1, float offset, Color col)
    {
        Vector2 p2 = p0;
        Vector2 p3 = p1;
        bool flag = p0.x < p1.x;
        float num = Mathf.Max(20f, Mathf.Abs(p0.x - p1.x) * 0.5f);
        p2.x = p0.x - num;
        p3.x = p1.x + num;
        int segments = 25;
        if (!flag)
        {
            if (offset == 0f)
            {
                GUILines.DrawCubicBezier(p0, p2, p3, p1, col, 2f, true, segments);
            }
            else
            {
                GUILines.DrawCubicBezierOffset(offset, p0, p2, p3, p1, col, 2f, true, segments);
            }
        }
        else
        {
            Vector2 vector = (p0 + p1) * 0.5f;
            Vector2 p4 = new Vector2(p2.x, vector.y);
            Vector2 p5 = new Vector2(p3.x, vector.y);
            if (offset == 0f)
            {
                GUILines.DrawCubicBezier(p0, p2, p4, vector, col, 2f, true, segments);
                GUILines.DrawCubicBezier(vector, p5, p3, p1, col, 2f, true, segments);
            }
            else
            {
                GUILines.DrawCubicBezierOffset(offset, p0, p2, p4, vector, col, 2f, true, segments);
                GUILines.DrawCubicBezierOffset(offset, vector, p5, p3, p1, col, 2f, true, segments);
            }
        }
    }
    public static float Smoother(float x)
    {
        return x * x * x * (x * (x * 6f - 15f) + 10f);
    }
    public static float Smooth(float x)
    {
        return x * x * (3f - 2f * x);
    }
    public static Rect GetExpanded(Rect r, float px)
    {
        r.y = (r.y - px);
        r.x = (r.x - px);
        r.width = (r.width + 2f * px);
        r.height = (r.height + 2f * px);
        return r;
    }

    public static void DrawRect(Rect r, float width = 1)
    {
        Vector2 v_lt = new Vector2(r.xMin, r.yMin);
        Vector2 v_rt = new Vector2(r.xMax, v_lt.y);
        Vector2 v_rb = new Vector2(v_rt.x, r.yMax);
        Vector2 v_lb = new Vector2(v_lt.x, v_rb.y);
        Handles.DrawAAPolyLine(width, v_lt, v_rt, v_rb, v_lb, v_lt);
    }

    public static void DrawRect(Vector2 center, Vector2 size, float width = 1)
    {
        Rect r = new Rect();
        r.width = size.x;
        r.height = size.y;
        r.center = center;
        DrawRect(r, width);
    }

    public static void DrawHighLightRect(Rect r, float width, bool antiAlias, Color outLineColor)
    {
        Color org = GUI.color;
        GUI.color = outLineColor;
        GUI.Box(GetExpanded(r, width), "", GUI.skin.button);
        GUI.color = org;
    }

    public static void DrawHandRect(Rect r, Color color)
    {
        Color org = GUI.color;
        GUI.color = color;
        GUI.Box(r, "", GUI.skin.button);
        GUI.color = org;
    }

    public static void DrawHexagon(Rect rect, float strength, Color color)     //                       v0               v1
    {                                                                          //                        *  *  *  *  *  *
        Vector2 v0 = new Vector2(rect.xMin + rect.width * 0.1f, rect.yMin);    //                      *                  *
        Vector2 v1 = new Vector2(rect.xMax - rect.width * 0.1f, v0.y);         //                    *                      *
        Vector2 v2 = new Vector2(rect.xMax, rect.center.y);                    //                  *                          *
        Vector2 v3 = new Vector2(v1.x, rect.yMax);                             //             v5 *                              * v2
        Vector2 v4 = new Vector2(v0.x, v3.y);                                  //                  *                          *
        Vector2 v5 = new Vector2(rect.xMin, v2.y);                             //                    *                      *
                                                                               //                      *                  *
        Handles.color = color;                                                 //                       v4  *  *  *  *  * v3
        Handles.DrawAAPolyLine(strength, v0, v1, v2, v3, v4, v5, v0);
    }

    public static void DrawDiamond(Vector2 center, int width, int height, float strength, Color color)
    {
        Handles.color = color;
        Vector2 point_l = new Vector2(center.x - width * 0.5f, center.y);
        Vector2 point_t = new Vector2(center.x, center.y + height * 0.5f);
        Vector2 point_r = new Vector2(center.x + width * 0.5f + 2, center.y);
        Vector2 point_d = new Vector2(center.x, center.y - height * 0.5f);

        Handles.DrawAAPolyLine(strength, point_l, point_t, point_r, point_d, point_l);
    }

    public static void DrawRoundedCornerRect(Rect r, float radius, Color color)
    {
        Handles.color = color;
        //上边
        Handles.DrawAAPolyLine(new Vector2(r.xMin + radius, r.yMax), new Vector2(r.xMax - radius, r.yMax));
        //右边
        Handles.DrawAAPolyLine(new Vector2(r.xMax, r.yMax - radius), new Vector2(r.xMax, r.yMin + radius));
        //下边
        Handles.DrawAAPolyLine(new Vector2(r.xMax - radius, r.yMin), new Vector2(r.xMin + radius, r.yMin));
        //左边
        Handles.DrawAAPolyLine(new Vector2(r.xMin, r.yMin + radius), new Vector2(r.xMin, r.yMax - radius));

        DrawRoundedCorner(new Vector2(r.xMin, r.yMax), Vector2.right, -Vector2.up, radius, color);
        DrawRoundedCorner(new Vector2(r.xMax, r.yMax), -Vector2.right, -Vector2.up, radius, color);
        DrawRoundedCorner(new Vector2(r.xMax, r.yMin), -Vector2.right, Vector2.up, radius, color);
        DrawRoundedCorner(new Vector2(r.xMin, r.yMin), Vector2.right, Vector2.up, radius, color);
    }

    /// <summary>
    /// 圆角线条
    /// </summary>
    /// <param name="center">圆角顶点</param>
    /// <param name="xDir">X方向</param>
    /// <param name="yDir">Y方向</param>
    /// <param name="radius"></param>
    static void DrawRoundedCorner(Vector2 center, Vector2 xDir, Vector3 yDir, float radius, Color color)
    {
        Vector2 realCenter = Vector2.zero;
        Vector2 coordinate = new Vector2(xDir.x > 0 ? 1 : -1, yDir.y > 0 ? 1 : -1);
        realCenter = new Vector2(center.x + radius * coordinate.x, center.y + radius * coordinate.y);

        Handles.color = color;
        bool flag = false;
        Vector2 s = Vector2.zero;
        Vector2 e = Vector2.zero;
        for (int i = 180; i <= 270; i += 3)
        {
            float angle = i * Mathf.Deg2Rad;
            if (flag)
            {
                e = new Vector2(Mathf.Cos(angle) * radius * coordinate.x, Mathf.Sin(angle) * radius * coordinate.y) + realCenter;
                Handles.DrawAAPolyLine(s, e);
                s = e;
            }
            else
            {
                flag = true;
                s = new Vector2(Mathf.Cos(angle) * radius * coordinate.x, Mathf.Sin(angle) * radius * coordinate.y) + realCenter;
            }
        }
    }

}
using UnityEngine;
using System.Collections;

public static class ZoomArea
{
    private static float kEditorWindowTabHeight = 0;
    private static Matrix4x4 prevGuiMatrix;
    public static Rect Begin(float zoomScale, Rect rect, Vector2 cameraPos)
    {
        GUI.EndGroup();
		GUI.BeginGroup(new Rect (rect.x, rect.y + 22, rect.width / zoomScale, rect.height / zoomScale));
        prevGuiMatrix = GUI.matrix;
        Matrix4x4 matrix4x = Matrix4x4.TRS(rect.TopLeft(), Quaternion.identity, Vector3.one);
        Matrix4x4 matrix4x2 = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1f));
        Matrix4x4 scale = matrix4x * matrix4x2 * matrix4x.inverse;
        GUI.matrix = scale * GUI.matrix;

        Rect rect2 = rect;
        rect2.width = 1.07374182E+09f;
        rect2.height = 1.07374182E+09f;
        rect2.x -= cameraPos.x;
        rect2.y -= cameraPos.y;

        GUI.BeginGroup(rect2);

        return rect;
    }
    public static void End(float zoomScale)
    {
        GUI.EndGroup();
        GUI.matrix = prevGuiMatrix;
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0f, kEditorWindowTabHeight, Screen.width, Screen.height));
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x = (result.x - pivotPoint.x);
        result.y = (result.y - pivotPoint.y);
        result.xMin = (result.xMin * scale);
        result.xMax = (result.xMax * scale);
        result.yMin = (result.yMin * scale); 
        result.yMax = (result.yMax * scale);
        result.x = (result.x + pivotPoint.x);
        result.y = (result.y + pivotPoint.y);
        return result;
    }

    public static bool Incapsule(this Rect rect, Rect test)
    {
        return rect.Contains(test.TopLeft()) && rect.Contains(test.BottomRight());
    }

    public static Vector2 TopLeft(this Rect r)
    {
        return new Vector2(r.x, r.y);
    }

    public static Vector2 BottomRight(this Rect r)
    {
        return new Vector2(r.xMax, r.yMax);
    }
}

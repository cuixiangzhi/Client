using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameCore.AI.Editor
{
    public static class BTHelper
    {
        public static void DrawRect(Rect rect,string content)
        {
            Handles.BeginGUI();
            Vector2 lt = new Vector2(rect.xMin, rect.yMax);
            Vector2 lb = new Vector2(rect.xMin, rect.yMin);
            Vector2 rt = new Vector2(rect.xMax, rect.yMax);
            Vector2 rb = new Vector2(rect.xMax, rect.yMin);
            Handles.DrawAAPolyLine(1, lt, rt, rb, lb, lt);
            GUI.Box(rect, content);
            Handles.EndGUI();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.cyou.plugin.res.debuger
{
    class GUI_ListView
    {
        private Vector2 leftTop;
        private List<GUI_ListView_Title> titles = new List<GUI_ListView_Title>();
        private List<List<GUI_ListView_Item>> rows = new List<List<GUI_ListView_Item>>();
        private float rowHeight = 30;
        private float titleHeight = 30;

        private float listHeight = 0;

        public delegate void OnRowClickedListener(int row, int column);

        private OnRowClickedListener m_OnRowClicked = null;

        public void SetPosition(int l, int t)
        {
            leftTop = new Vector2(l, t);
        }

        public void AddTitle(string title, float w = 100)
        {
            titles.Add(new GUI_ListView_Title(title, w, titleHeight));
        }

        public void AddRow(params string[] items)
        {
            if (items == null || items.Length != titles.Count)
            {
                Debug.LogError("items == null || items.Length != titles.Count");
                return;
            }
            List<GUI_ListView_Item> row = new List<GUI_ListView_Item>();
            for (int i = 0; i < items.Length; i++)
            {
                row.Add(new GUI_ListView_Item(this, this.rows.Count, i, items[i]));
            }
            this.rows.Add(row);
        }
        public void SetRowHeight(float height)
        {
            rowHeight = height;
        }
        public void SetTitleHeight(float height)
        {
            titleHeight = height;
        }
        public void ClearRowData()
        {
            this.rows.Clear();
        }
        public void AddRowClickedListener(OnRowClickedListener listener)
        {
            m_OnRowClicked = listener;
        }
        public Vector2 scrollPosition = Vector2.zero;
        public void Draw()
        {

            scrollPosition = GUI.BeginScrollView(new Rect(leftTop.x, leftTop.y, 500, 200), scrollPosition, new Rect(0, 0, 510, listHeight));
            float titleLPos =  0 /*leftTop.x*/;
            for (int i = 0; i < titles.Count; i++)
            {
                titleLPos += GetTitleWidth(i - 1);
                float tPos = 0;
                titles[i].Draw(titleLPos, tPos);
                
            }
            listHeight = titleHeight;
            for (int i = 0; i < rows.Count; i++)
            {
                List<GUI_ListView_Item> items = rows[i];
                for (int j = 0; j < items.Count; j++)
                {
                    
                    float lPos = GetItemlPosX(j);
                    float tPos =i * rowHeight + titleHeight;
                    float w = GetTitleWidth(j);
                    float h = rowHeight;
                    items[j].Draw(lPos, tPos, w, h);
                }
                listHeight += rowHeight;
            }
            listHeight += 100;
            GUI.EndScrollView();
        }
        private float GetTitleWidth(int i)
        {
            if (i < 0)
                return 0;
            return titles[i].width;
        }
        
        private float GetItemlPosX(int colume)
        {
            float w = 0;
            for(int i = 0; i< colume; i++)
            {
                w += GetTitleWidth(i);
            }
            float lPos = /*leftTop.x + */w;
            return lPos;
        }

        public class GUI_ListView_Title
        {
            public string title;
            public float width;
            public float height;
            public GUI_ListView_Title(string title, float w, float h)
            {
                this.title = title;
                this.width = w;
                this.height = h;
            }
            public void Draw(float l, float t)
            {
                if (GUI.Button(new Rect(l, t, width, height), title))
                {

                }
            }
        }
        public class GUI_ListView_Item
        {
            public string context;
            private int column;
            private int row;
            private GUI_ListView listView;
            public GUI_ListView_Item(GUI_ListView listView, int row, int column, string context)
            {
                this.listView = listView;
                this.context = context;
                this.column = column;
                this.row = row;

            }
            public void Draw(float l, float t, float w, float h)
            {
                if (GUI.Button(new Rect(l, t, w, h), context))
                {
                    if(this.listView.m_OnRowClicked != null)
                        this.listView.m_OnRowClicked(row, column);
                }
            }
        }

    }

}

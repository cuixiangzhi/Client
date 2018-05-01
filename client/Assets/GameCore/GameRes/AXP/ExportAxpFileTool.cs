#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using AxpTools;


public class ExportAxpFileTool : EditorWindow
{
    public static ExportAxpFileTool m_CurExportPanel;

    private static string m_AxpMapFilePath = null;
    private static string m_AxpFilePath = null;
    private static string m_AxpFileExportPath = null;
    private static int m_AxpFileID = -1;
    private static AxpMap m_AxpMapFile = null;

    [MenuItem("Tools/ExportAxpFileTool")]
    static void ExportAxpFile()
    {
        m_CurExportPanel = GetWindow<ExportAxpFileTool>();
        m_CurExportPanel.Init();
        m_CurExportPanel.Show();

        if(m_AxpMapFile == null)
        {
            m_AxpMapFile = AxpMap.Init();
        }
    }


    void Init()
    {

    }


    void OnGUI()
    {
        DrawAxpFileTool();
    }

    void DrawAxpFileTool()
    {
        //EditorGUILayout.BeginScrollView(new Vector2(0,0), GUILayout.Width(100));
        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        

        m_AxpMapFilePath = string.IsNullOrEmpty(m_AxpMapFilePath) ? m_AxpMapFilePath : m_AxpMapFilePath;
        
        EditorGUILayout.LabelField("AxpMap 存放目录:", m_AxpMapFilePath);

        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            m_AxpMapFilePath = EditorUtility.OpenFolderPanel("Select folder of the tables", m_AxpMapFilePath, "");
            if (!string.IsNullOrEmpty(m_AxpMapFilePath))
            {
                //config.ConfigRoot = basePath;

                EditorGUILayout.LabelField("AxpMap 存放目录:", m_AxpMapFilePath);
            }
        }      
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        m_AxpFilePath = string.IsNullOrEmpty(m_AxpFilePath) ? m_AxpFilePath : m_AxpFilePath;
        
        EditorGUILayout.LabelField("AxpFile 存放目录:", m_AxpFilePath);

        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            string[] filterarray = { "AxpType","axp.so" };
            m_AxpFilePath = EditorUtility.OpenFilePanelWithFilters("Select folder of the tables", m_AxpFilePath, filterarray);
            if (!string.IsNullOrEmpty(m_AxpFilePath))
            {
                //config.ConfigRoot = basePath;

                //OnRepaint();
            }
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        m_AxpFileExportPath = string.IsNullOrEmpty(m_AxpFileExportPath) ? m_AxpFileExportPath : m_AxpFileExportPath;

        EditorGUILayout.LabelField("导出AxpFile存放目录:", m_AxpFileExportPath);

        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            m_AxpFileExportPath = EditorUtility.OpenFolderPanel("Select folder of the tables", m_AxpFileExportPath, "");
            if (!string.IsNullOrEmpty(m_AxpFileExportPath))
            {
                //config.ConfigRoot = basePath;

                //OnRepaint();
            }
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        //m_AxpFilePath = string.IsNullOrEmpty(m_AxpFilePath) ? m_AxpFilePath : m_AxpFilePath;
        //m_AxpFilePath = "hello";
        //GUILayout.Label("设置AxpFileID:", GUILayout.Width(80));      
        //m_AxpFileID = EditorGUILayout.IntField(m_AxpFileID);
        if (GUILayout.Button("开始导出AxpFile", GUILayout.MaxWidth(120)))
        {
            ExportAxpFileInternal();
        }
        EditorGUILayout.EndHorizontal();
        //EditorGUILayout.EndScrollView();

        this.Repaint();
    }


    void ExportAxpFileInternal()
    {
        if (string.IsNullOrEmpty(m_AxpMapFilePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "AxpMap文件路径为空？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }
        if (string.IsNullOrEmpty(m_AxpFilePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "AxpFile文件路径为空？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }

        if(string.IsNullOrEmpty(m_AxpFileExportPath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "导出AxpFile文件路径为空？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }
        //if (m_AxpFileID < 0)
        //{
        //    if (EditorUtility.DisplayDialog("提示!!!", "索引为负值？\n请确索引值是否正确！!!", "确定", "取消"))
        //        return;
        //}

        if (!File.Exists(m_AxpMapFilePath + "/AxpMap.db"))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "AxpMap文件不存在？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }

        if (!File.Exists(m_AxpFilePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "AxpFile文件路径不存在？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }

        if (!Directory.Exists(m_AxpFileExportPath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "导出AxpFile文件路径不存在？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }

        if(m_AxpFilePath.Contains("NewAxp"))
        {
            int strLength = m_AxpFilePath.IndexOf(".axp.so") - m_AxpFilePath.IndexOf("NewAxp") - 6;
            string str = m_AxpFilePath.Substring(m_AxpFilePath.IndexOf("NewAxp") + 6, strLength);
            int.TryParse(str, out m_AxpFileID);
        }
        else if(m_AxpFilePath.Contains("Data"))
        {
            int strLength = m_AxpFilePath.IndexOf(".axp.so") - m_AxpFilePath.IndexOf("Data") - 4;
            string str = m_AxpFilePath.Substring(m_AxpFilePath.IndexOf("Data") + 4, strLength);
            int.TryParse(str, out m_AxpFileID);
        }
        if(m_AxpMapFile == null)
        {
            m_AxpMapFile = AxpMap.Init();
        }
        m_AxpMapFile.Clear();
        if(!m_AxpMapFile.Load(m_AxpMapFilePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "加载AxpMap失败！!!", "确定", "取消"))
                return;
        }


        AxpFile_Editor t_AxpFile = new AxpFile_Editor();
        bool t_result = t_AxpFile.ExportAxpFile(m_AxpFilePath, m_AxpMapFile, m_AxpFileExportPath, m_AxpFileID, AxpFilePath.PersistentDataPath);

        t_AxpFile.closePakFile();

        if(t_result == true)
        {
            if (EditorUtility.DisplayDialog("提示!!!", "导出AxpFile成功！!!", "确定", "取消"))
                return;
        }
        else
        {
            if (EditorUtility.DisplayDialog("提示!!!", "导出AxpFile失败！!!", "确定", "取消"))
                return;
        }
    }
}


#endif
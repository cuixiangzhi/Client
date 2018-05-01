/********************************************************************************
 *	创建人：	 woonam
 *	创建时间：   2017-07-17
 *
 *	功能说明：没有什么恰当的名字来形容这个工具了，这个工具要做的事情就是从SVN上Down下来的资源
 *	         生成的AssetBundle看看那些引起了复杂的引用关系，导致更新包会变大
 *	
 *	修改记录：
*********************************************************************************/
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GameCore;

public class AutoCompareAssetBundleTool : EditorWindow
{
    public static AutoCompareAssetBundleTool m_CurExportPanel;
    private static List<string> m_TableList = null;
    private static Dictionary<string, List<string>> m_DicBundleAssetNames = null;
    private static Dictionary<string, List<string>> m_Result = null;
    private static string m_ABUnCompressedPath = null;          //the update asset bundle compressed that uncompressed in the folder path.
    private static string m_FromVersion = null;
    private static string m_ToVersion = null;

    [MenuItem("Tools/AutoCompareAssetBundleTool")]
    static void ExportAxpFile( )
    {
        m_CurExportPanel = GetWindow<AutoCompareAssetBundleTool>();
        m_CurExportPanel.Init();
        m_CurExportPanel.Show();


    }
    void Init( )
    {
        m_TableList = new List<string>();
        m_DicBundleAssetNames = new Dictionary<string, List<string>>();
        m_Result = new Dictionary<string, List<string>>();
    }


    void OnGUI( )
    {
        DrawAutoCompareAssetBundleTool();
    }


    void DrawAutoCompareAssetBundleTool( )
    {

        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        m_ABUnCompressedPath = string.IsNullOrEmpty(m_ABUnCompressedPath) ? m_ABUnCompressedPath : m_ABUnCompressedPath;
        m_ABUnCompressedPath = EditorGUILayout.TextField("更新包解压AssetBundle路径", m_ABUnCompressedPath);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        m_FromVersion = EditorGUILayout.TextField("FromVersion", m_FromVersion);
        m_ToVersion = EditorGUILayout.TextField("ToVersion", m_ToVersion);
        EditorGUILayout.EndHorizontal();

        ////////////////////////////////////////////////////////////////////
        //生成SVN资源列表
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("开始生成你想要的表", GUILayout.MaxWidth(120)))
        {
            GenerateSVNFileListTable();
        }
        EditorGUILayout.EndHorizontal();

        this.Repaint();
    }


    void GenerateSVNFileListTable()
    {
        if (string.IsNullOrEmpty(m_ABUnCompressedPath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "解压AssetBundle路径为空？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }
        if(string.IsNullOrEmpty(m_FromVersion) || string.IsNullOrEmpty(m_ToVersion))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "SVN版本号为空？\n请确定是否正确！!!", "确定", "取消"))
                return;
        }
        string t_StrArguments =  m_FromVersion + " " + m_ToVersion;
        Process p = new Process();
        string path = Application.dataPath + "/../../SvnVersionDiffFiles.bat";
        ProcessStartInfo pi = new ProcessStartInfo(path,t_StrArguments);
        pi.UseShellExecute = false;
        pi.RedirectStandardOutput = true;
        p.StartInfo = pi;
        p.Start();
        p.WaitForExit();


        string resultpath = Application.dataPath + "/../../file_list.txt";
        if(!File.Exists(resultpath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "生成file_list.txt失败!!!", "确定", "取消"))
                return;
        }
        if(m_TableList == null)
        {
            m_TableList = new List<string>();
        }
        m_TableList.Clear();
        MemoryStream ms = new MemoryStream(CusEncoding.EncodingUtil.FileByteToLocal(File.ReadAllBytes(resultpath)));
        StreamReader sr = new StreamReader(ms);

        string line = null;
        while(true)
        {
            line = sr.ReadLine();

            if(string.IsNullOrEmpty(line))
                break;
            int length = line.LastIndexOf(".cs");
            if(line.LastIndexOf(".cs") > 0 || line.LastIndexOf(".meta") > 0)
                continue;
            if(line.Contains(".") && line.Contains("Assets") )
            {
                line = line.Substring(line.IndexOf("Assets"), line.Length - line.IndexOf("Assets"));
                line = line.Replace("\\", "/").ToLower();
                m_TableList.Add(line);
            }
        }
        sr.Close();
        sr.Dispose();
        ms.Close();
        ms.Dispose();

        //load the uncompressed asset bundle.

        if(!Directory.Exists(m_ABUnCompressedPath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "哈哈 解压AssetBundle路径不存在啊？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }


        if(m_DicBundleAssetNames == null)
        {
            m_DicBundleAssetNames = new Dictionary<string, List<string>>();
        }
        m_DicBundleAssetNames.Clear();
        string[] t_FileNames = Directory.GetFiles(m_ABUnCompressedPath);

        for (int index = 0; index < t_FileNames.Length; index++)
        {
            string t_tempName = t_FileNames[index].Replace("\\", "/");
            string t_guidName = t_tempName.Substring(t_tempName.LastIndexOf('/') + 1, t_tempName.Length - t_tempName.LastIndexOf('/') - 1);

            //string[] t_TempDep = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(t_guidName));

            AssetBundle t_Bundle = AssetBundle.LoadFromFile(t_FileNames[index]);
            string[] t_TempDep = t_Bundle.GetAllAssetNames();
            if(!m_DicBundleAssetNames.ContainsKey(t_guidName))
            {
                m_DicBundleAssetNames.Add(t_guidName, new List<string>(t_TempDep));
            } 
            else
            {
                throw new System.Exception("为什么AssetBundle重复了？");
            }
            t_Bundle.Unload(true);
        }



        //开始比对了
        if(m_Result == null)
        {
            m_Result = new Dictionary<string, List<string>>();
        }
        m_Result.Clear();

        //for(int index = 0; index < m_TableList.Count; index++)
        //{
        //    string t_file = m_TableList[index];
        //    string[] t_TempDep = AssetDatabase.GetDependencies(t_file);
        //    List<string> t_ListResult = new List<string>();
        //    for(int j = 0; j < t_TempDep.Length; j++)
        //    {
        //        int t_referenceValue = 0;
        //        foreach(KeyValuePair<string, List<string>> data in m_DicBundleAssetNames)
        //        {
        //            if(data.Value.Contains(t_TempDep[j]))
        //                t_referenceValue++;
        //        }

        //        if(t_referenceValue != 0)
        //        {
        //            string t_ReferenceValueData = t_TempDep[j] + " " + t_referenceValue.ToString();
        //            t_ListResult.Add(t_ReferenceValueData);
        //        }
        //    }

        //    m_Result.Add(m_TableList[index], t_ListResult);
        //}

        foreach(KeyValuePair<string, List<string>> data in m_DicBundleAssetNames)
        {
            List<string> t_IncludeAsset = new List<string>();
            foreach(string str in data.Value)
            {
                if(m_TableList.Contains(str))
                {
                    t_IncludeAsset.Add(str);
                }
            }

            m_Result.Add(data.Key, t_IncludeAsset);
        }

        //output result in the txt file.

        StreamWriter sw = File.CreateText(Application.dataPath + "/../../CompareSVN_AssetBundle_TableResult.txt");
        foreach(KeyValuePair<string, List<string>> data in m_Result)
        {
            string strline = data.Key;
            string str_guid = strline.Substring(0, strline.Length - 2);
            string str_path = AssetDatabase.GUIDToAssetPath(str_guid);
            strline = strline + "@" + str_path + "@";
            if(data.Value.Count > 0)
            {
                foreach(string name in data.Value)
                {
                    strline += "@" + name;
                }
            }

            sw.WriteLine(strline);
        }
        sw.Close();
        sw.Dispose();
    }

}
#endif
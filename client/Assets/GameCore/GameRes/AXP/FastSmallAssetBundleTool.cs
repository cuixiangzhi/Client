using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class FastSmallAssetBundleTool : EditorWindow
{
    // 自定义类型，用于显示
    public enum CustomTarget
    {
        None,
        WindowsPC,
        OSXUniversal,
        iPhone,
        Android,
        WinPhone
    }

    public static FastSmallAssetBundleTool m_FastSmallAssetBundlePanel;

    private static string m_ResourcesTablePath = null;
    private static string m_ExportAssetBundlePath = null;
    private static string m_DepTablePath = null;
    private static List<string> m_ResourcesNameList = null;
    private static Dictionary<string, UnityEngine.Object> m_ObjDic = null;
    private static Dictionary<string, string> m_DependentTableDic = null;
    private static List<AssetBundleBuild> m_AssetBundleBuildList = null;
    private static Dictionary<string, List<string>> m_TableDepBinDic = null;
    private static CustomTarget m_CustomTarget = CustomTarget.WindowsPC;
    private static BuildTarget m_BuildTarget = BuildTarget.StandaloneWindows;
    private static BuildAssetBundleOptions buildOp = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle  | BuildAssetBundleOptions.DisableWriteTypeTree;//


    [MenuItem("Tools/FastSmallAssetBundleTool")]
    static void ExportFastSamllAssetBundle()
    {
        m_FastSmallAssetBundlePanel = GetWindow<FastSmallAssetBundleTool>();
        m_FastSmallAssetBundlePanel.Init();
        m_FastSmallAssetBundlePanel.Show();


    }


    void Init()
    {
        m_ResourcesNameList = new List<string>();
        m_ObjDic = new Dictionary<string, UnityEngine.Object>();
        m_DependentTableDic = new Dictionary<string, string>();
        m_AssetBundleBuildList = new List<AssetBundleBuild>();
        m_TableDepBinDic = new Dictionary<string, List<string>>();
    }


    void OnGUI()
    {
        DrawFastSmallAssetBUndlePanel();
    }

    void DrawFastSmallAssetBUndlePanel()
    {
        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("资源列表文件:", m_ResourcesTablePath);

        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            string[] filterarray = { "文本文件", "txt" };
            m_ResourcesTablePath = EditorUtility.OpenFilePanelWithFilters("Select folder of the tables", m_ResourcesTablePath, filterarray);
            if (!string.IsNullOrEmpty(m_ResourcesTablePath))
            {
                //config.ConfigRoot = basePath;

                //EditorGUILayout.LabelField("AxpMap 存放目录:", m_AxpMapFilePath);
            }
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        m_ExportAssetBundlePath = string.IsNullOrEmpty(m_ExportAssetBundlePath) ? m_ExportAssetBundlePath : m_ExportAssetBundlePath;
        EditorGUILayout.LabelField("导出AssetBundle存放目录:", m_ExportAssetBundlePath);
        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            m_ExportAssetBundlePath = EditorUtility.OpenFolderPanel("Select folder of the tables", m_ExportAssetBundlePath, "");
            if (!string.IsNullOrEmpty(m_ExportAssetBundlePath))
            {
                //config.ConfigRoot = basePath;

                EditorGUILayout.LabelField("AssetBundle 存放目录为空:", m_ExportAssetBundlePath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        m_CustomTarget = (CustomTarget)EditorGUILayout.EnumPopup("导出平台", m_CustomTarget, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("开始资源列表中的AssetBundle", GUILayout.MaxWidth(200)))
        {
            StartBuildAssetBundle();
        }
        EditorGUILayout.EndHorizontal();



        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("TableDep.bin:", m_DepTablePath);

        if (GUILayout.Button("更改", GUILayout.MaxWidth(50)))
        {
            string[] filterarray = { "二进制文件", "bin" };
            m_DepTablePath = EditorUtility.OpenFilePanelWithFilters("Select folder of the tables", m_DepTablePath, filterarray);
            if (!string.IsNullOrEmpty(m_DepTablePath))
            {
            }
        }

        if (GUILayout.Button("更新TableDep.bin文件", GUILayout.MaxWidth(200)))
        {
            ReplaceDepdentTable();
        }
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Build update table's resource to assetbundle.
    /// </summary>
    void StartBuildAssetBundle()
    {
        if(!File.Exists(m_ResourcesTablePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "导出AxpFile文件路径为空？\n请确定路径是否正确！!!", "确定", "取消"))
                return;
        }

        if(!ReadTableContent(m_ResourcesTablePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "解析表错误？\n请确定表格式是否正确！!!", "确定", "取消"))
                return;
        }

        RefreshBuildTarget();

        BuildAssetBundle();
    }


    void RefreshBuildTarget()
    {
        m_BuildTarget = 0;
        switch (m_CustomTarget)
        {
            case CustomTarget.WindowsPC:
                {
                    m_BuildTarget = BuildTarget.StandaloneWindows;
                }
                break;
            case CustomTarget.OSXUniversal:
                {
                    m_BuildTarget = BuildTarget.StandaloneOSXUniversal;
                }
                break;
            case CustomTarget.iPhone:
                {
                    m_BuildTarget = BuildTarget.iOS;
                }
                break;
            case CustomTarget.Android:
                {
                    m_BuildTarget = BuildTarget.Android;
                }
                break;
            case CustomTarget.WinPhone:
                {
                    m_BuildTarget = BuildTarget.WP8Player;
                }
                break;
        }
    }

    bool ReadTableContent(string filepath)
    {
        m_ResourcesNameList.Clear();
        m_ObjDic.Clear();
        m_DependentTableDic.Clear();
        m_AssetBundleBuildList.Clear();


        MemoryStream ms = new MemoryStream(CusEncoding.EncodingUtil.FileByteToLocal(File.ReadAllBytes(m_ResourcesTablePath)));
        StreamReader sr = new StreamReader(ms);



        string line;
        while (true)
        {
            line = sr.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
            ;
            if ((line.Length > 1) && (line[0] != '#'))
            {
                m_ResourcesNameList.Add(line);


                string t_ResourcePath = line;
                string t_StrResource = null;
                t_ResourcePath = t_ResourcePath.Replace('\\', '/');
                if (t_ResourcePath.IndexOf("Resources/") >= 0)
                {
                    t_StrResource = t_ResourcePath.Substring(10);
                }

                UnityEngine.Object o = Resources.Load(t_StrResource);
                if (o == null)
                {
                    Debug.LogError(string.Format("资源{0}不存在!配置ID:{1}", t_StrResource, line));
                }
                else
                {
                    if (o is TextAsset)
                    {
                        //if (jsonObjectDic.ContainsKey(strPathResource.Replace('/', '_')))
                        //{
                        //    throw new System.Exception(strPathResource + " 配置资源重复！！");
                        //}
                        //jsonObjectDic.Add(strPathResource.Replace('/', '_'), o);

                        Debug.LogError(string.Format("资源{0}是Json文本文件 不能打包:{1}", t_StrResource, line));
                    }
                    else
                    {
                        if(!m_ObjDic.ContainsKey(t_StrResource))
                        {
                            m_ObjDic.Add(t_ResourcePath, o);
                        }
                        else
                        {
                            Debug.LogError("资源配置重复：" + t_ResourcePath);
                        }
                    }
                }
            }
            else
            {
                continue;
            }
        }
        sr.Close();
        sr.Dispose();
        ms.Close();
        ms.Dispose();

        return true;
    }


    void BuildAssetBundle()
    {
        if(m_ObjDic.Count <= 0)
        {
            Debug.LogError("资源表数据为空");
            return;
        }
        if (!Directory.Exists(m_ExportAssetBundlePath + "/Bundles"))
        {
            Directory.CreateDirectory(m_ExportAssetBundlePath + "/Bundles");
        }

        foreach (KeyValuePair<string, UnityEngine.Object> data in m_ObjDic)
        {
            m_AssetBundleBuildList.Clear();
            string t_ObjPath = AssetDatabase.GetAssetPath(data.Value.GetInstanceID());
            string t_ObjGuid = AssetDatabase.AssetPathToGUID(t_ObjPath);

            string t_AssetBundleName = t_ObjGuid + ".t";
            List<string> t_ArrayList = new List<string>();

            if(AssetDatabase.GetDependencies(t_ObjPath).Length > 1)
            {
                string t_filename = t_ObjPath.Substring(t_ObjPath.LastIndexOf('/')  + 1, t_ObjPath.Length - t_ObjPath.LastIndexOf('/') -1);
                t_filename = "Bundles/" + t_AssetBundleName + "|" + t_filename;
                m_DependentTableDic.Add(data.Key, t_filename);
                string[] t_internalAssetsArray = AssetDatabase.GetDependencies(t_ObjPath);
                for(int index = 0; index < t_internalAssetsArray.Length; index++)
                {
                    if (t_internalAssetsArray[index].Contains(".cs"))
                        continue;
                    t_ArrayList.Add(t_internalAssetsArray[index]);
                }
                 
            }
            else
            {
                string t_filename = "Bundles/" + t_AssetBundleName ;
                m_DependentTableDic.Add(data.Key, t_filename);
                t_ArrayList.Add(data.Key);
            }

            string[] t_AssetsArray = { t_ObjPath };
            AssetBundleBuild t_AssetBundleBuild = new AssetBundleBuild();
            t_AssetBundleBuild.assetBundleName = t_AssetBundleName;
            t_AssetBundleBuild.assetNames = t_ArrayList.ToArray();

            m_AssetBundleBuildList.Add(t_AssetBundleBuild);

            BuildPipeline.BuildAssetBundles(m_ExportAssetBundlePath + "/Bundles", m_AssetBundleBuildList.ToArray(), buildOp, m_BuildTarget);

        }




        string[] t_manifestFiles = Directory.GetFiles(m_ExportAssetBundlePath + "/Bundles", "*.manifest");
        if(t_manifestFiles.Length != 0)
        {
            for(int index = 0; index < t_manifestFiles.Length; index++)
            {
                if(File.Exists(t_manifestFiles[index]))
                {
                    File.Delete(t_manifestFiles[index]);
                }
            }
        }

        if(File.Exists(m_ExportAssetBundlePath + "/Bundles/Bundles"))
        {
            File.Delete(m_ExportAssetBundlePath + "/Bundles/Bundles");
        }

        StreamWriter sw = File.CreateText(m_ExportAssetBundlePath + "/ResourceDepTable.txt" );
        foreach (KeyValuePair<string, string> data in m_DependentTableDic)
        {
            sw.WriteLine(data.Key + "@" + data.Value);
        }
        sw.Close();
        sw.Dispose();

        return;
    }

    void ReplaceDepdentTable()
    {
        if(!File.Exists(m_DepTablePath))
        {
            if (EditorUtility.DisplayDialog("提示!!!", "TableDep.bin不存在？\n请确定表是否存在！!!", "确定", "取消"))
                return;
        }
        byte[] byteData = File.ReadAllBytes(m_DepTablePath);
        MemoryStream memStream = new MemoryStream(byteData);
        BinaryFormatter binFormat = new BinaryFormatter();
        m_TableDepBinDic = (Dictionary<String, List<String>>)binFormat.Deserialize(memStream);
        memStream.Close();
        memStream.Dispose();


        //replace the TableDep.bin content.
        foreach(KeyValuePair<string, string> data in m_DependentTableDic)
        {
            if(m_TableDepBinDic.ContainsKey(data.Key))
            {
                m_TableDepBinDic[data.Key].Clear();
                m_TableDepBinDic[data.Key].Add(data.Value);
                
            }
        }

        if(!Directory.Exists(m_ExportAssetBundlePath + "/Depends"))
        {
            Directory.CreateDirectory(m_ExportAssetBundlePath + "/Depends");
        }
        //serialize the TableDep.bin file
        Stream stream = new FileStream(m_ExportAssetBundlePath + "/Depends/TableDep.bin", FileMode.Create, FileAccess.ReadWrite);
        BinaryFormatter BFormat = new BinaryFormatter();
        BFormat.Serialize(stream, m_TableDepBinDic);
        stream.Close();
        stream.Dispose();


        //out put text TableDep.txt file
        StreamWriter sw = File.CreateText(m_ExportAssetBundlePath + "/TableDep.txt");
        foreach (KeyValuePair<string, List<string>> pair in m_TableDepBinDic)
        {
            string line = pair.Key;
            if (pair.Value.Count > 0)
            {
                foreach (string name in pair.Value)
                {
                    line += "@" + name;
                }
            }
            sw.WriteLine(line);
        }
        sw.Close();
        sw.Dispose();

    }
}

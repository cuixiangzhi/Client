#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

//namespace AxpTools
//{

//    public class AxpBuildTool : EditorWindow
//    {

//        private static Vector2 m_WinMinSize = new Vector2(800, 800);
//        private static string DEFAULT_PHOTO_SAVE_PATH = Application.dataPath + "/../../../Public/Resource/Public/Scenes";

//        private string m_AxpNewFilePath = null;
//        private string m_AxpOpenFilePath = null;
//        private string m_AddAxpFolderPath = null;
//        private string m_AxpSuffixName = ".unity3d|.tab";

//        public static string prefixName = "";
//        public static string suffixName = "";

//        private int m_PerAxpFileSize = 100;          //MB.
//        //private AxpFile m_AxpFile;
//        private AxpPackMaker m_AxpPackMaker;

//        private List<string> m_FoldersList = new List<string>();



//        [MenuItem("AssetBundleTool/Build/AxpBuilder")]
//        public static void AxpBuilder()
//        {
//            AxpBuildTool window = EditorWindow.GetWindow<AxpBuildTool>("AxpTool", true, typeof(EditorWindow));
//            window.minSize = m_WinMinSize;
//            window.wantsMouseMove = true;
//            window.Show();
//            window.Initialize();
//        }



//        private void Initialize()
//        {
//            //m_AxpFile = null;
//            m_AxpPackMaker = new AxpPackMaker();
//#if UNITY_ANDROID
//            prefixName = "lib";
//            suffixName = "axp.so";
//#elif UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
//            prefixName = "";
//            suffixName = "axp.so";
//#endif
//        }


//        private void OnGUI()
//        {
//            //create new axp file.
//            EditorGUILayout.BeginHorizontal();
//            m_AxpNewFilePath = EditorGUILayout.TextField("创建Axp", m_AxpNewFilePath);
//            if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
//            {
//                m_AxpNewFilePath = EditorUtility.SaveFilePanel(
//                    "选择Axp存储位置",
//                    DEFAULT_PHOTO_SAVE_PATH,
//                    "NewAxp",
//                    suffixName);
//#if UNITY_ANDROID
//                m_AxpNewFilePath = m_AxpNewFilePath.Insert(m_AxpNewFilePath.LastIndexOf('/') + 1, prefixName);
//#endif

//                if (string.IsNullOrEmpty(m_AxpNewFilePath))
//                {
//                    EditorUtility.DisplayDialog("错误", "请选择Axp存储位置！！", "确定");
//                    return;
//                }
//                else
//                    m_AxpOpenFilePath = Path.GetDirectoryName(Path.GetFullPath(m_AxpNewFilePath)) + "/";
//            }
//            EditorGUILayout.EndHorizontal();
//            // open axp file.
//            EditorGUILayout.LabelField("Axp写入路径", m_AxpOpenFilePath);

//            // save folder to build axp.
//            EditorGUILayout.BeginHorizontal();
//            m_AddAxpFolderPath = EditorGUILayout.TextField("要添加到AXP的文件夹", m_AddAxpFolderPath);
//            if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
//            {
//                m_AddAxpFolderPath = EditorUtility.OpenFolderPanel(
//                  "选择要添加到Axp的文件夹",
//                  DEFAULT_PHOTO_SAVE_PATH,
//                  "");

//                if (!string.IsNullOrEmpty(m_AddAxpFolderPath))
//                {
//                    if (!m_FoldersList.Contains(m_AddAxpFolderPath))
//                    {
//                        m_FoldersList.Add(m_AddAxpFolderPath);
//                    }
//                }
//                else
//                {
//                    EditorUtility.DisplayDialog("错误", "选择要添加到Axp的文件夹！！", "确定");
//                    return;

//                }
//            }

//            EditorGUILayout.EndHorizontal();

//            //包大小设置
//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.Label("单包大小设置 : ", GUILayout.Width(80));
//            m_PerAxpFileSize = EditorGUILayout.IntField(m_PerAxpFileSize);
//            GUILayout.Label("MB");
//            EditorGUILayout.EndHorizontal();

//            //打包后缀名设置
//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.Label("要打包后缀名设置：", GUILayout.Width(80));
//            m_AxpSuffixName = EditorGUILayout.TextField(m_AxpSuffixName);
//            GUILayout.Label("后缀名之间以 | 为分隔符");
//            EditorGUILayout.EndHorizontal();


//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            if (GUILayout.Button("  <<Generate AXP File>>  ", GUILayout.Height(40)))
//            {
//                GenerateAxpFile();
//            }
//            EditorGUILayout.EndHorizontal();
//        }



//        private void GenerateAxpFile()
//        {
//            if (string.IsNullOrEmpty(m_AxpNewFilePath) || string.IsNullOrEmpty(m_AxpOpenFilePath) || string.IsNullOrEmpty(m_AddAxpFolderPath))
//            {
//                EditorUtility.DisplayDialog("错误", "Axp包路径等信息设置错误！！", "确定");
//                return;
//            }

//            if (m_AxpPackMaker == null)
//            {
//                m_AxpPackMaker = new AxpPackMaker();
//            }


//            m_AxpPackMaker.ClearPackMakerInfo();

//            string t_strtemp = m_AxpNewFilePath.Replace("\\", "/");
//            int t_index = t_strtemp.LastIndexOf('/');
//            string strAxpFilePath = t_strtemp.Substring(0, t_index);
//            string strAxpFileName = t_strtemp.Substring(t_index + 1, t_strtemp.Length - t_index - 1);
//            string t_tempAxpNewFilePath = m_AxpNewFilePath;


//            t_tempAxpNewFilePath = t_tempAxpNewFilePath.Insert(m_AxpNewFilePath.Length - suffixName.Length, "0");


//            if (File.Exists(t_tempAxpNewFilePath))
//            {
//                bool t_ret = EditorUtility.DisplayDialog("错误", "Axp已经存在 覆盖点击确定，取消则退出不打包！！", "确定", "取消");
//                if (t_ret == false)
//                {
//                    return;
//                }
//                else
//                {
//                    File.Delete(t_tempAxpNewFilePath);
//                }

//            }


//            m_AxpPackMaker.addDiskFolder(m_AddAxpFolderPath, m_AxpOpenFilePath, m_AxpSuffixName, true);
//            //m_AxpPackMaker.savePakFile(strAxpFilePath, strAxpFileName, null);
//            m_AxpPackMaker.rootUri = new Uri(Path.GetFullPath(m_AddAxpFolderPath + "/"));
//            m_AxpPackMaker.savePakFileInAxp(strAxpFilePath, strAxpFileName, (uint)m_PerAxpFileSize * 1024 * 1024, suffixName.Length);
//        }

//    }


//}

#endif
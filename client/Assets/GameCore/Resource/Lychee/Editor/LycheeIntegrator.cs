using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace LycheeIG
{
    class Config
    {
        public static int numberOfInput = 8;
    }

    [Serializable]
    class Settings
    {
        public Settings()
        {
            output = "";

            clear = false;

            inputs = new string[Config.numberOfInput];

            for (int i = 0; i < inputs.Length; ++i)
            {
                inputs[i] = "";
            }
        }

        public string output;
        public bool clear;
        public string[] inputs;
    }
}

public class LycheeIntegrator : EditorWindow
{
    [MenuItem("Window/Lychee Integrator")]
    public static void ShowWindow()
    {
        EditorWindow w = EditorWindow.GetWindow(typeof(LycheeIntegrator));
        w.titleContent = new GUIContent("Lychee IG");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {

    }

    private void OnEnable()
    {
        settings_ = new LycheeIG.Settings();

        LoadSettings();
    }

    private void OnDisable()
    {
    }

    void Update()
    {
    }

    private void OnGUI()
    {
        GUIStyle style;

        scrollPos_ = EditorGUILayout.BeginScrollView(scrollPos_);

        GUILayout.Label("Output", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(settings_.output, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            string location = BrowseFolder(settings_.output);

            if (location != "")
            {
                settings_.output = location;

                SaveSettings();
            }

            Repaint();
        }

        if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(40)))
        {
            if (!string.IsNullOrEmpty(settings_.output) && Directory.Exists(settings_.output))
            {
                Application.OpenURL("file://" + settings_.output);
            }
        }

        GUILayout.EndHorizontal();

        settings_.clear = EditorGUILayout.ToggleLeft("Clear", settings_.clear);

        GUILayout.Label("Inputs", EditorStyles.boldLabel);

        for (int i = 0; i < settings_.inputs.Length; ++i)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(settings_.inputs[i], EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                string location = BrowseFolder(settings_.inputs[i]);

                if (location != "")
                {
                    settings_.inputs[i] = location;

                    SaveSettings();
                }

                Repaint();
            }

            if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(40)))
            {
                if (!string.IsNullOrEmpty(settings_.inputs[i]) && Directory.Exists(settings_.inputs[i]))
                {
                    Application.OpenURL("file://" + settings_.inputs[i]);
                }
            }

            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(18)))
            {
                settings_.inputs[i] = "";

                SaveSettings();
                Repaint();
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Label("Integrate", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        style = new GUIStyle("button");

        style.fontSize = 14;

        if (GUILayout.Button("Integrate", style, GUILayout.Height(20), GUILayout.Width(225)))
        {
            SaveSettings();

            Integrate();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        GUILayout.Space(9);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    private void Integrate()
    {
        if (Directory.Exists(settings_.output))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(settings_.output);

            FileSystemInfo[] infos = dirInfo.GetFileSystemInfos();

            if (infos.Length > 0)
            {
                if (settings_.clear)
                {
                    foreach (FileSystemInfo info in infos)
                    {
                        if (info is FileInfo)
                        {
                            FileInfo fi = info as FileInfo;
                            fi.Delete();
                        }
                        else if (info is DirectoryInfo)
                        {
                            DirectoryInfo di = info as DirectoryInfo;
                            di.Delete(true);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Output directory is not empty");
                    return;
                }
            }
        }
        else
        {
            Directory.CreateDirectory(settings_.output);
        }

        StreamWriter listWriter = new StreamWriter(Path.Combine(settings_.output, "_list"), false);
        StreamWriter buildWriter = new StreamWriter(Path.Combine(settings_.output, "_build"), false);

        int emptyLine = 0;

        HashSet<string> bundleNames = new HashSet<string>();
        HashSet<string> assetPaths = new HashSet<string>();

        try
        {
            for (int i = 0; i < settings_.inputs.Length; ++i)
            {
                string input = settings_.inputs[i];

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (!Directory.Exists(input))
                {
                    Debug.LogError("Input directory not exists: " + input);
                    continue;
                }

                string listFilePath = Path.Combine(input, "_list").Replace('/', '\\');
                string buildFilePath = Path.Combine(input, "_build").Replace('/', '\\');

                if (!File.Exists(listFilePath))
                {
                    Debug.LogError("List file not exists: " + listFilePath);
                    continue;
                }

                if (!File.Exists(buildFilePath))
                {
                    Debug.LogError("Build file not exists: " + buildFilePath);
                    continue;
                }

                StreamReader listReader = new StreamReader(listFilePath);

                string line;

                while ((line = listReader.ReadLine()) != null)
                {
                    string[] v = line.Split(' ');

                    if (bundleNames.Add(v[0]))
                    {
                        File.Copy(Path.Combine(input, v[0]), Path.Combine(settings_.output, v[0]));
                        File.Copy(Path.Combine(input, v[0]) + ".signature", Path.Combine(settings_.output, v[0]) + ".signature");

                        listWriter.WriteLine(line);
                    }
                    else
                    {
                        listReader.Close();
                        throw new System.Exception("Bundle name conflict: " + v[0]);
                    }
                }

                listReader.Close();

                StreamReader buildReader = new StreamReader(buildFilePath);

                while ((line = buildReader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                    {
                        if (emptyLine++ == 0)
                        {
                            buildWriter.WriteLine();
                        }

                        continue;
                    }

                    if (line[0] != ' ')
                    {
                        string[] result = line.Split(':');

                        if (!bundleNames.Contains(result[0]))
                        {
                            buildReader.Close();
                            throw new System.Exception("Bundle not exists: " + result[0]);
                        }

                        if (result.Length == 2)
                        {
                            foreach (string bundleName in result[1].Split(','))
                            {
                                if (!bundleNames.Contains(bundleName))
                                {
                                    buildReader.Close();
                                    throw new System.Exception("Bundle not exists: " + bundleName);
                                }
                            }
                        }
                    }
                    else
                    {
                        string assetPath = line.Trim();

                        if (!assetPaths.Add(assetPath.ToLower()))
                        {
                            Debug.Log("Redundant asset: " + assetPath);
                        }
                    }

                    buildWriter.WriteLine(line);
                    emptyLine = 0;
                }

                buildReader.Close();

                EditorUtility.DisplayProgressBar("Lychee Integrator", "Integrating",
                        CalculateProgress(0.0f, 1.0f, i + 1, settings_.inputs.Length));
            }

            EditorUtility.ClearProgressBar();
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError(e.Message);
        }

        listWriter.Flush();
        listWriter.Close();

        buildWriter.Flush();
        buildWriter.Close();
    }

    private void LoadSettings()
    {
        try
        {
            StreamReader reader = new StreamReader("LycheeIntegrator.config");

            BinaryFormatter formatter = new BinaryFormatter();

            var settings = formatter.Deserialize(reader.BaseStream) as LycheeIG.Settings;

            if (settings != null)
            {
                if (settings.output == null)
                {
                    settings.output = "";
                }

                if (settings.inputs == null)
                {
                    settings.inputs = new string[LycheeIG.Config.numberOfInput];

                    for (int i = 0; i < settings.inputs.Length; ++i)
                    {
                        settings.inputs[i] = "";
                    }
                }

                settings_ = settings;
            }

            reader.Close();
        }
        catch (Exception)
        {
        }
    }

    private void SaveSettings()
    {
        FileStream stream = new FileStream("LycheeIntegrator.config", FileMode.Create);

        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, settings_);

        stream.Flush();
        stream.Close();
    }

    private static string BrowseFolder(string folder)
    {
        return EditorUtility.OpenFolderPanel("", folder, "").Replace('/', '\\');
    }

    private static float CalculateProgress(float start, float scale, int i, int imax)
    {
        return start + ((float)i / (float)imax * scale);
    }

    private Vector2 scrollPos_;

    private LycheeIG.Settings settings_;
}


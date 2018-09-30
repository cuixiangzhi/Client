using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LycheeBB;
using System.Threading;
using System.IO;
using GameCore;

public class LycheeCustomScript_LDJ : CustomScript
{
    class AssetUnit
    {
        public AssetUnit(string asset, int groupId)
        {
            this.asset = asset;
            this.groupId = groupId;
        }
        public string asset;
        public int groupId;
    }

    List<AssetUnit> allassets = new List<AssetUnit>();

    private int m_groupID = 0;

    int assetIndex = 0;
    private string assetOutLocation;

    private bool mFileBuildFinish = false;

    public bool AddGroup(out int groupId, out string[] assets)
    {
        //allassets.Clear();
        bool bRet = true;
        if (assetIndex < allassets.Count)
        {
            List<string> assetList = new List<string>();
            groupId = allassets[assetIndex].groupId;
            assetList.Add(allassets[assetIndex].asset);
            assets = assetList.ToArray();
            
            
            //Debug.Log("Add Asset " + allassets[assetIndex].asset + "  groupid " + groupId);

            bRet = true;
            ++assetIndex;
        }
        else
        {
            assets = null;
            groupId = -1;
            bRet = false;
        }
        return bRet;
    }
    public bool AddCustomFiles(out string[] files)
    {
        if(!mFileBuildFinish)
        {
            mFileBuildFinish = true;
            string packagePath = PackageManager.CreatePackage("Assets/Res/Data", "Assets/Lua", Path.GetFullPath("Assets/../Temp"));
            files = new string[] { packagePath };
            return true;
        }
        else
        {
            files = null;
            return false;
        }
    }


    public void Awake()
    {
    }

    public void DrawUI()
    {
    }

    public bool HasUI()
    {
        return false;
    }

    public void PostBuildEvent(BuildResult result)
    {
        UnityEngine.Debug.Log("build asset ok");
        assetIndex = 0;
        allassets.Clear();
        if (EditorUtility.DisplayDialog("打包", "打包完成", "确定"))
        {
            OpenDirectory(assetOutLocation);
        }
    }

    public bool PreBuildEvent(BuildArguments args)
    {
        Debug.Log("PreBuildEvent start");
        assetOutLocation = args.location;
        assetIndex = 0;

        InitFiles();

        List<string> scenes = new List<string>();
        scenes.Add("Assets/Res/Scene/Scene_Yuhuayuan.unity");
        scenes.Add("Assets/Res/Scene/Scene_Yushufang.unity");
        scenes.Add("Assets/Res/Scene/Scene_Yingmannv.unity");
                
        args.scenesInBuild = scenes.ToArray();


        Debug.Log("PreBuildEvent end");
        return true;
    }


    void InitFiles( )
    {
        EditorUtility.DisplayProgressBar("打包", "开始收集资源", 0);
        allassets.Clear();

        //添加主角
        string path = Application.dataPath + "/Res/Role/Player";
        AddModeWithAnim(path);
        //添加NPC
        path = Application.dataPath + "/Res/Role/NPC";
        AddModeWithAnim(path);

        //添加Other
        path = Application.dataPath + "/Res/Role/Other";
        AddModeWithAnim(path);

        //添加Zuoqi
        path = Application.dataPath + "/Res/Role/Zuoqi";
        AddModeWithAnim(path);

        //添加Animal
        path = Application.dataPath + "/Res/Role/Animal";
        AddModeWithAnim(path);

        //添加特效
        AddEffect();

        //添加UI资源
        AddUI();

        //添加剧情
        AddSequence();
        EditorUtility.ClearProgressBar();
        

        m_groupID = 0;
    }
    private void AddScenes()
    {

    }
    private void AddModeWithAnim(string path)
    {

        if (!System.IO.Directory.Exists(path))
        {
            Debug.LogError("no path " + path);
            return;
        }

        string[] directorys = GetDirectorys(path);
        foreach (string directory in directorys)
        {
            string folderName = GetFileName(directory);


            var files = GetFiles(directory, System.IO.SearchOption.TopDirectoryOnly);
            foreach(var file in files)
            {
                //Debug.LogError("files ::  " + file);
                if(file.EndsWith(".fbx") || file.EndsWith(".FBX"))
                {
                    var fbxPath = SystemPathToAssetPath(file);
                    allassets.Add(new AssetUnit(fbxPath, m_groupID));
                }
                if (file.EndsWith(".prefab"))
                {
                    var prefabPath = SystemPathToAssetPath(file);
                    allassets.Add(new AssetUnit(prefabPath, m_groupID));
                }
            }
            //string fbxPath = directory + "/" + folderName + ".fbx";
            //if (System.IO.File.Exists(fbxPath))
            //{
            //    fbxPath = SystemPathToAssetPath(fbxPath);
            //    allassets.Add(new AssetUnit(fbxPath, m_groupID));
            //}

            var animPath = directory + "/Animation/" + folderName + "_Anim.controller";
            if (System.IO.File.Exists(animPath))
            {
                animPath = SystemPathToAssetPath(animPath);
                allassets.Add(new AssetUnit(animPath, m_groupID));
            }
            m_groupID++;
            //Debug.LogError(assetPath);

        }
    }


    private void AddEffect()
    {
        int index = 0;
        string effectPath = Application.dataPath + "/Res/Effects/Prefabs";
     
        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { SystemPathToAssetPath(effectPath) });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.Log("effect " + path);
            allassets.Add(new AssetUnit(path, m_groupID));
                
            index++;

        }
        m_groupID++;

    }
    private void AddUI()
    {

        var guids = AssetDatabase.FindAssets("t:texture", new string[] { "Assets/Res/UI/Icon" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.LogFormat("icon " + path);
            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }


        guids = AssetDatabase.FindAssets("t:texture", new string[] { "Assets/Res/UI/UIBg" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("bg " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }

        guids = AssetDatabase.FindAssets("t:texture", new string[] { "Assets/Res/UI/HurtJumpWord" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("bg " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }


        guids = AssetDatabase.FindAssets("t:font", new string[] { "Assets/Res/UI/Fonts" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("icon " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }



        guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/UI/Prefab" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("ui " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }


        guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/UI/Misc" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("ui " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }


        guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/Effect/particle" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            //Debug.LogFormat("ui effect " + path);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }
    }

    private void AddSequence()
    {
        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Res/Sequence" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            allassets.Add(new AssetUnit(path, m_groupID));
            m_groupID++;
        }
    }


    private string[] GetDirectorys(string path)
    {
        List<string> dirs = new List<string>();
        string[] directoryEntries = System.IO.Directory.GetFileSystemEntries(path);

        foreach (string entry in directoryEntries)
        {
            if (entry.EndsWith(".meta"))
                continue;
            dirs.Add(entry.Replace("\\", "/"));
        }
        return dirs.ToArray();
    }
    private string[] GetFiles(string path, System.IO.SearchOption searchOption= System.IO.SearchOption.TopDirectoryOnly)
    {
        List<string> dirs = new List<string>();
        string[] files = System.IO.Directory.GetFiles(path,"*", searchOption);

        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
                continue;
            dirs.Add(file.Replace("\\", "/"));
        }
        return dirs.ToArray();
    }

    private string GetFileName(string path)
    {
        int index = path.LastIndexOf("/");
        return path.Substring(index + 1, path.Length - index - 1);
    }
    private string SystemPathToAssetPath(string systemPath)
    {
        return "Assets" + systemPath.Substring(Application.dataPath.Length, systemPath.Length - Application.dataPath.Length);

    }
    private void OpenDirectory(string path)
    {
        // 新开线程防止锁死
        Thread newThread = new Thread(new ParameterizedThreadStart(CmdOpenDirectory));
        newThread.Start(path);
    }

    private void CmdOpenDirectory(object obj)
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.Arguments = "/c start " + obj.ToString();
        //UnityEngine.Debug.Log(p.StartInfo.Arguments);
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.Start();

        p.WaitForExit();
        p.Close();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

namespace GameFrameWork
{
    public class ExportXcode
    {
#if UNITY_IOS
        private static string CONFIG_PREFIX = "XU_";
        private static string CONFIG_PATH = Application.dataPath + "/../SDK/iOS/";
#endif

        public static void Export()
        {
#if !UNITY_IOS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS,BuildTarget.iOS);
#endif
            string[] commands = System.Environment.GetCommandLineArgs();
            Debug.LogError(commands.Length);
            for(int i = 0;i < commands.Length;i++)
            {
                Debug.LogError(commands[i]);
            }
            string signTeamID = commands[13].Replace("_"," ");
            string appName = commands[10];
            string companyName = commands[11].Split('.')[1];
            string bundleIdentifier = commands[11];
            string bundleVersion = commands[12];
            string exportPath = commands[14];
            string splashImagePath = commands[15];
            string appIconPath = commands[16];
            bool debug = commands[17].ToLower() == "true" || commands[17].ToLower() == "1";

            //旋转方向
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.useAnimatedAutorotation = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.runInBackground = false;
            //应用图标
            if (File.Exists(appIconPath))
            {

            }
            //启动屏幕
            if (File.Exists(splashImagePath))
            {

            }
            //调试设置  
            {
                PlayerSettings.enableInternalProfiler = debug;
                PlayerSettings.enableCrashReportAPI = debug;
                PlayerSettings.logObjCUncaughtExceptions = debug;
                PlayerSettings.usePlayerLog = debug;
                PlayerSettings.actionOnDotNetUnhandledException = ActionOnDotNetUnhandledException.Crash;
                PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
                PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
                PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.Full);
                PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
            }
            //渲染设置
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.mobileMTRendering = true;
            PlayerSettings.MTRendering = true;
            PlayerSettings.use32BitDisplayBuffer = true;
            //应用设置   
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = appName;
            PlayerSettings.applicationIdentifier = bundleIdentifier;
            PlayerSettings.bundleVersion = bundleVersion;

            PlayerSettings.iOS.appleDeveloperTeamID = signTeamID;
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.FastButNoExceptions;
            PlayerSettings.iOS.requiresPersistentWiFi = true;
            PlayerSettings.iOS.allowHTTPDownload = false;
            
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.applicationDisplayName = appName;
            PlayerSettings.iOS.buildNumber = bundleVersion;
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.targetOSVersionString = "7.0";
            PlayerSettings.iOS.showActivityIndicatorOnLoading = iOSShowActivityIndicatorOnLoading.DontShow;
            PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Suspend;
            //PlayerSettings.iOS.backgroundModes = iOSBackgroundMode.
            PlayerSettings.iOS.forceHardShadowsOnMetal = false;
            PlayerSettings.iOS.prerenderedIcon = true;
            //代码配置           
            PlayerSettings.accelerometerFrequency = 0;
            PlayerSettings.bakeCollisionMeshes = false;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_2_0_Subset);
            PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.iOS, true);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 0);
            //PlayerSettings.SetAdditionalIl2CppArgs("");
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "");

            //添加场景
            List<string> levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene == null || !scene.enabled) continue;
                levels.Add(scene.path);
            }

            //导出XCODE工程
            System.DateTime now = System.DateTime.Now;
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }       

            if(levels.Count == 0)
            {
                Debug.LogError("必须添加要打包的默认场景");
            }
            else
            {
                try
                {
                    //确保修改生效
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    BuildPipeline.BuildPlayer(levels.ToArray(), exportPath, BuildTarget.iOS, debug ? BuildOptions.Development : BuildOptions.None);
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }            
        }

        [PostProcessBuild()]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;
#if UNITY_IOS
            PBXProject proj = new PBXProject();
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            proj.ReadFromFile(projPath);
            string target = proj.TargetGuidByName("Unity-iPhone");
            string plistPath = buildPath + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            //获取所有的配置文件
            List<Unity_Xcode_Json> jsons = new List<Unity_Xcode_Json>();
            string[] files = Directory.GetFiles(CONFIG_PATH, "*.json", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(files[i]);
                if (fileName.StartsWith(CONFIG_PREFIX))
                {
                    //TODO:根据平台过滤
                    jsons.Add(JsonUtility.FromJson<Unity_Xcode_Json>(File.ReadAllText(files[i])));
                }
            }

            //创建资源目录
            string frameworkPath = buildPath + "/Frameworks/Plugins/iOS/";
            if (!Directory.Exists(frameworkPath))
                Directory.CreateDirectory(frameworkPath);
            string aPath = buildPath + "/Libraries/Plugins/iOS/";
            if (!Directory.Exists(aPath))
                Directory.CreateDirectory(aPath);

            proj.SetBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(SRCROOT)/Frameworks/Plugins/iOS");
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");

            proj.SetBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Libraries/Plugins/iOS");
            proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)/Libraries");
            proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)");
            proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(inherited)");

            for (int i = 0; i < jsons.Count; i++)
            {
                Unity_Xcode_Json json = jsons[i];
                //系统静态库引用
                if (json.internal_frameworks != null)
                {
                    for (int j = 0; j < json.internal_frameworks.Length; j++)
                    {
                        proj.AddFrameworkToProject(target, json.internal_frameworks[j], false);
                    }
                }

                //系统动态库引用
                if (json.internal_dynamiclibs != null)
                {
                    for (int j = 0; j < json.internal_dynamiclibs.Length; j++)
                    {
                        proj.AddFileToBuild(target, proj.AddFile("usr/lib/" + json.internal_dynamiclibs[j], "Frameworks/" + json.internal_dynamiclibs[j], PBXSourceTree.Sdk));
                    }
                }

                //外部静态库引用
                if (json.external_frameworks != null)
                {
                    for (int j = 0; j < json.external_frameworks.Length; j++)
                    {
                        Copy(CONFIG_PATH + json.external_frameworks[j], frameworkPath);
                        string fileName = Path.GetFileName(json.external_frameworks[j]);
                        proj.AddFileToBuild(target, proj.AddFile("Frameworks/Plugins/iOS/" + fileName, "Frameworks/" + fileName, PBXSourceTree.Source));
                    }
                }

                //外部静态库引用
                if (json.external_staticlibs != null)
                {
                    for (int j = 0; j < json.external_staticlibs.Length; j++)
                    {
                        Copy(CONFIG_PATH + json.external_staticlibs[j], aPath);
                        string fileName = Path.GetFileName(json.external_staticlibs[j]);
                        proj.AddFileToBuild(target, proj.AddFile("Libraries/Plugins/iOS/" + fileName, "Libraries/" + fileName, PBXSourceTree.Source));
                    }
                }

                //外部文件引用
                if (json.external_files != null)
                {
                    for (int j = 0; j < json.external_files.Length; j++)
                    {
                        Copy(CONFIG_PATH + json.external_files[j], aPath);
                        string fileName = Path.GetFileName(json.external_files[j]);
                        proj.AddFileToBuild(target, proj.AddFile("Libraries/Plugins/iOS/" + fileName, "Libraries/" + fileName, PBXSourceTree.Source));
                    }
                }

                //BuildSetting
                if (json.buildset != null)
                {
                    for (int j = 0; j < json.buildset.Length; j++)
                    {
                        proj.SetBuildProperty(target, json.buildset[j].key, json.buildset[j].value);
                    }
                }

                //Info.plist
                if (json.plistset != null)
                {
                    PlistElementDict dict = plist.root.AsDict();
                    for (int j = 0; j < json.plistset.Length; j++)
                    {
                        dict.SetString(json.plistset[j].key, json.plistset[j].value);
                    }
                }
            }

            proj.WriteToFile(projPath);
            plist.WriteToFile(plistPath);
#endif
        }

        private static void Copy(string src, string dst)
        {
            //拷贝目录
            if (Directory.Exists(src))
            {
                string targetDir = dst + "/" + Path.GetFileName(src);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                //拷贝当前目录文件
                foreach (var file in Directory.GetFiles(src, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!file.Contains(".meta"))
                    {
                        File.Copy(file, targetDir + "/" + Path.GetFileName(file), true);
                    }
                }
                //拷贝当前目录子目录
                foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly))
                {
                    Copy(dir, targetDir);
                }
            }
            //拷贝文件
            else
            {
                File.Copy(src, dst + "/" + Path.GetFileName(src), true);
            }
        }

        [System.Serializable]
        public class Unity_Xcode_Json_KV
        {
            public string key;
            public string value;
        }

        [System.Serializable]
        public class Unity_Xcode_Json
        {
            public string[] internal_frameworks;
            public string[] internal_dynamiclibs;
            public string[] external_frameworks;
            public string[] external_staticlibs;
            public string[] external_files;
            public Unity_Xcode_Json_KV[] buildset;
            public Unity_Xcode_Json_KV[] plistset;
        }
    }

}

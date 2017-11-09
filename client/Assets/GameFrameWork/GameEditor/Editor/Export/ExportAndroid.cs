using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace GF
{
    public sealed class ExportAndroid
    {
        private static string CONFIG_PREFIX = "XU_";
        private static string CONFIG_PATH = Application.dataPath + "/../SDK/Android/";

        private static bool debug = true;
        private static string companyName = "cyou";
        private static string appName = string.Empty;
        private static string bundleIdentifier = string.Empty;
        private static string bundleVersion = string.Empty;
        private static string signTeamID = string.Empty;
        private static string exportPath = string.Empty;
        private static string splashImagePath = string.Empty;
        private static string appIconPath = string.Empty;

        public static void Export()
        {
#if !UNITY_ANDROID
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
#endif
            string[] commands = System.Environment.GetCommandLineArgs();
            Debug.LogError(commands.Length);
            for (int i = 0; i < commands.Length; i++)
            {
                Debug.LogError(commands[i]);
            }
            appName = commands[10];
            bundleIdentifier = commands[11];
            bundleVersion = commands[12];
            signTeamID = commands[13].Replace("_", " ");
            exportPath = commands[14];
            splashImagePath = commands[15];
            appIconPath = commands[16];
            debug = commands[17].ToLower() == "true" || commands[17].ToLower() == "1";

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

            //签名
            //             PlayerSettings.Android.keyaliasName = "";
            //             PlayerSettings.Android.keystoreName = "";
            //             PlayerSettings.Android.keyaliasPass = "";
            //             PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.androidIsGame = true;
            PlayerSettings.Android.androidTVCompatibility = true;
            PlayerSettings.Android.disableDepthAndStencilBuffers = false;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
            PlayerSettings.Android.showActivityIndicatorOnLoading = AndroidShowActivityIndicatorOnLoading.DontShow;
            PlayerSettings.Android.targetDevice = AndroidTargetDevice.FAT;
            PlayerSettings.Android.useAPKExpansionFiles = false;
            //代码配置
            PlayerSettings.accelerometerFrequency = 0;
            PlayerSettings.bakeCollisionMeshes = false;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_2_0_Subset);
            PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.Android, true);
            PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 0);
            //PlayerSettings.SetAdditionalIl2CppArgs("");
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "");

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

            if (levels.Count == 0)
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
                    BuildPipeline.BuildPlayer(levels.ToArray(), exportPath + appName + ".apk", BuildTarget.Android, debug ? BuildOptions.Development : BuildOptions.None);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        [PostProcessBuild()]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.Android)
                return;
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
    }
}


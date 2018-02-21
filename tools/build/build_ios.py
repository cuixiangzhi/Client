#coding:utf-8
import sys
reload(sys)
sys.setdefaultencoding("utf-8")
import traceback
import os
import shutil
import platform

try:
    #路径配置
    unity_path = "D:/Unity/Editor/Unity.exe";
    project_path = "E:/mmo/src/client/trunk/Client-Demo";
    common_path = "E:/mmo/src/client/common";
    medata_path = "E:/mmo/src/client/trunk/Client-Demo/MeData";
    export_path = "E:/res/xcode";
    log_path = sys.path[0] + "/build_ios.log";
    #应用配置
    app_name = "鹿鼎记";
    app_bundle_id = "com.cyou.ldj";
    app_bundle_ver = "1.0";
    app_teamid = "57JGBTX2EM";
    
    app_ipa_name = app_bundle_id.split('.')[2];
    mode = "Release"
    
    shutil.copy(common_path + "/dll/NGUI-No-Editor/NGUI.dll",project_path + "/Assets/Plugins/NGUI.dll");
    shutil.copy(common_path + "/gbdll/ios/GameBase.dll",project_path + "/Assets/Plugins/GameBase.dll");
    if(os.path.exists(project_path + "/Assets/Code/External/NGUI/Scripts/Editor")):
        shutil.rmtree(project_path + "/Assets/Code/External/NGUI/Scripts/Editor");
    if(os.path.exists(project_path + "/Assets/StreamingAssets/MeData")):
        shutil.rmtree(project_path + "/Assets/StreamingAssets/MeData");
    shutil.copytree(medata_path,project_path + "/Assets/StreamingAssets/MeData");
    
    command = []
    command.append(unity_path)
    command.append(" -batchmode")
    command.append(" -quit")
    command.append(" -projectPath " + project_path)
    command.append(" -logFile " + log_path)
    command.append(" -nographics") 
    command.append(" -executeMethod Export.ExportXcode.Export")
    command.append(" " + app_name)
    command.append(" " + app_bundle_id)
    command.append(" " + app_bundle_ver)
    command.append(" " + app_teamid)
    command.append(" " + export_path)
    command.append(" ICONPATH")
    command.append(" SPLASHPATH")
    print(u"正在导出XCODE工程,请稍候...")
    command = ''.join(command)
    if(platform.system().lower() == "darwin"):
        os.system(command);
        print(u"正在编译XCODE工程,请稍候...");
        os.chdir(export_path)
        os.system("chmod +x ./MapFileParser.sh")
        command = []
        command.append("xcodebuild")
        command.append(" archive")
        command.append(" -scheme Unity-iPhone")
        command.append(" -configuration " + mode)
        command.append(" -archivePath ./build/Release-iphoneos/" + app_ipa_name + ".xcarchive")
        command.append(" -quiet>>" + log_path)
        command = ''.join(command)
        os.system(command);
        print(u"正在生成IPA,请稍候...");
        command = []
        command.append("xcodebuild")
        command.append(" -exportArchive")
        command.append(" -archivePath ./build/Release-iphoneos/" + app_ipa_name + ".xcarchive")
        command.append(" -configuration " + mode)
        command.append(" -exportPath ~/Desktop")
        command.append(" -exportOptionsPlist " + sys.path[0] + "/export.plist")
        command.append(" -quiet>>" + log_path)
        command = ''.join(command)
        os.system(command);
    else:
        os.system(command.encode("gb2312"));
except:
    print(u"语法错误")
    traceback.print_exc()
    os.system("pause")
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
    export_path = "E:/";
    log_path = sys.path[0] + "/build_android.log";
    #应用配置
    app_name = "鹿鼎记";
    app_bundle_id = "com.cyou.ldj";
    app_bundle_ver = "1.0";
    app_teamid = "57JGBTX2EM";
    app_debug = "true";
    
    app_ipa_name = app_bundle_id.split('.')[2];
    mode = "Release"
    
    shutil.copy(common_path + "/dll/NGUI-No-Editor/NGUI.dll",project_path + "/Assets/Plugins/NGUI.dll");
    shutil.copy(common_path + "/gbdll/android/GameBase.dll",project_path + "/Assets/Plugins/GameBase.dll");
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
    command.append(" -executeMethod Export.ExportAndroid.Export")
    command.append(" " + app_name)
    command.append(" " + app_bundle_id)
    command.append(" " + app_bundle_ver)
    command.append(" " + app_teamid)
    command.append(" " + export_path)
    command.append(" ICONPATH")
    command.append(" SPLASHPATH")
    command.append(" " + app_debug)
    print(u"正在生成APK,请稍候...")
    command = ''.join(command)
    os.system(command.encode("gb2312"));
except:
    print(u"语法错误")
    traceback.print_exc()
    os.system("pause")
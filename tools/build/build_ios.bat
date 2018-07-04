@echo off
set current_path=%cd%
set make_ios_path="E:/res/Update_IOS/"
set make_ios_cmd="UpdateRes_IOS.cmd"
set unity_path="D:/Unity/Editor/Unity.exe"
set project_path="E:/res/Client-Demo"
set common_path="E:/mmo/src/client/common"
set medata_path="E:/res/Update_IOS/MeData"
set export_path="E:/res/local/xcode"
set log_path="./build_ios.log"
set app_name="Â¹¶¦¼Ç"
set app_bundle_id="com.cyou.ldj"
set app_bundle_ver="1.0"
set app_teamid="57JGBTX2EM"
set app_debug="true"
set tool_path="./build_ios.py"

cd %make_ios_path%
call %make_ios_cmd%
cd %current_path%

cd %project_path%
echo "svn cleanlock..."
svn cleanup --quiet
echo "svn revert..."
svn revert -R . --quiet
echo "svn clean..."
svn cleanup --quiet --remove-unversioned
echo "svn update..."
svn up --quiet
cd %current_path%

python %tool_path% %unity_path% %project_path% %common_path% %medata_path% %export_path% %log_path% %app_name% %app_bundle_id% %app_bundle_ver% %app_teamid% %app_debug%
if exist %export_path%/ERROR.flag goto fail
echo "upload modify to svn..."
cd %export_path%
svn ci -m null --quiet
echo "upload del to svn..."
svn status|grep ! |awk '{print $2}'|xargs svn del
echo "upload add to svn..."
svn status|grep ? |awk '{print $2}'|xargs svn add
goto success

:success
echo "build success,open mac and check the new project"

:fail
echo "build failed,see build_ios.log for more info"

cd %current_path%
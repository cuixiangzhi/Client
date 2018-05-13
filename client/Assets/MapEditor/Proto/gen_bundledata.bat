@echo off
set exePath="..\..\..\..\..\tools\ExcelToProto\protobuf-net\ProtoGen"
pause
set protoPath=".\BundleData.proto"
pause
set csPath="..\BundleData.cs"
pause

%exePath%\protogen -i:%protoPath% -o:%csPath%
pause
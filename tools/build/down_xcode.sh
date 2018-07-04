#!/bin/bash

echo "clean old project"
rm -rf "./xcode"
echo "download new project"
svn co --quiet https://cy012188.cyou-inc.com:8443/svn/local/xcode ./xcode
echo "add execute permission"
chmod +x ./xcode/MapFileParser.sh
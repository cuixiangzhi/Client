"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require('fs');
var path = require('path');
var os = require('os');
function getConfigInfo() {
}
exports.getConfigInfo = getConfigInfo;
//初始化环境变量
function initConfig(args) {
    var localRoot = args.localRoot;
    if (!fs.existsSync(localRoot)) {
        return "localRoot: " + localRoot + "不存在";
    }
    //lua51
    var runtimeType = args.runtimeType;
    var dir = "";
    var extensionPath = getExtensionPath();
    if (extensionPath == false) {
        return "未能获取 luaide 路径无法启动调试,请联系作者进行修复! ";
    }
    if (runtimeType == "Lua51") {
        if (!fs.existsSync(args.mainFile)) {
            return `File does not exist. "${args.mainFile}"`;
        }
        //文件后缀名
        var extName = path.extname(args.mainFile);
        console.log("extName:" + extName);
        if (extName != ".lua") {
            return `文件后缀名应该为.lua "${args.mainFile}"`;
        }
        dir = path.join(extensionPath, "runtime", "win");
        var result = createDebugInitFile(dir);
        if (result != true) {
            return result;
        }
        return result;
    }
    else if (runtimeType == "Cocos2" || runtimeType == "Cocos3") {
        if (!fs.existsSync(args.exePath)) {
            return `File does not exist. "${args.exePath}"`;
        }
        if (args.scripts) {
            for (var index = 0; index < args.scripts.length; index++) {
                var scriptsPath = args.scripts[index];
                if (!fs.existsSync(scriptsPath)) {
                    return `File does not exist. "${scriptsPath}"`;
                }
            }
        }
        return true;
    }
}
exports.initConfig = initConfig;
function createDebugInitFile(dir) {
    try {
        var fileName = path.join(dir, "LuaDebug.lua");
        var expath = getExtensionPath();
        var luapath = path.join(expath, "luadebug", "LuaDebug.lua");
        fs.writeFileSync(fileName, fs.readFileSync(luapath));
        return true;
    }
    catch (err) {
        return err;
    }
}
function getLuaRuntimePath() {
    var rootPath = getExtensionPath();
    //
    return path.join(rootPath, "runtime", "win");
}
exports.getLuaRuntimePath = getLuaRuntimePath;
function getLoadLuaScript() {
    var dir = getExtensionPath();
    var loadScriptPath = path.join(dir, "Template", "LoadScript", "LoadScript.lua");
    if (fs.existsSync(loadScriptPath)) {
        try {
            return fs.readFileSync(loadScriptPath, 'utf-8');
        }
        catch (err) {
        }
    }
}
exports.getLoadLuaScript = getLoadLuaScript;
function getExtensionPath() {
    var dir = getConfigDir();
    var configFile = path.join(dir, "luaideConfig");
    if (fs.existsSync(configFile)) {
        try {
            return fs.readFileSync(configFile, 'utf-8');
        }
        catch (err) {
        }
    }
    return false;
}
exports.getExtensionPath = getExtensionPath;
//递归创建目录 同步方法
function mkdirsSync(dirname) {
    //console.log(dirname);
    if (fs.existsSync(dirname)) {
        return true;
    }
    else {
        if (mkdirsSync(path.dirname(dirname))) {
            fs.mkdirSync(dirname);
            return true;
        }
    }
}
exports.mkdirsSync = mkdirsSync;
/**
* Returns the default templates location based on the user OS.
* @returns {string}
*/
function getConfigDir() {
    var userDataDir = null;
    switch (process.platform) {
        case 'linux':
            userDataDir = path.join(os.homedir(), '.config');
            break;
        case 'darwin':
            userDataDir = path.join(os.homedir(), 'Library', 'Application Support');
            break;
        case 'win32':
            userDataDir = process.env.APPDATA;
            break;
        default:
            throw Error("Unrecognizable operative system");
    }
    userDataDir = path.join(userDataDir, 'code', 'user', "luaide");
    mkdirsSync(userDataDir);
    return userDataDir;
}
exports.getConfigDir = getConfigDir;
;
//# sourceMappingURL=Common.js.map
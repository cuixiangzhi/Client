"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const child_process = require("child_process");
const Common_1 = require("../../Common");
var fs = require('fs');
var path = require('path');
var os = require('os');
// import vscode = require('vscode');
class BaseChildProcess {
    constructor(args, luaDebug) {
        this.args = args;
        var os = process.platform;
        var runtimeType = args.runtimeType;
        this.luaDebug = luaDebug;
    }
    execLua() {
        //判断平台
        //linux
        //darwin
        //win32
        var os = process.platform;
        var luaStartProc;
        var runtimeType = this.args.runtimeType;
        var baseChildProcess = this;
        var options = null;
        if (os == "linux") {
        }
        else if (os == "darwin") {
            if (runtimeType == "Cocos2" || runtimeType == "Cocos3") {
                var localRoot = path.normalize(this.args.localRoot);
                var file = path.join(localRoot, this.args.mainFile);
                file = path.normalize(file);
                options = {
                    encoding: 'utf8',
                    shell: true
                };
                var pargs = [
                    "-workdir " + localRoot,
                    "-file " + file
                ];
                var exePath = path.normalize(this.args.exePath);
                exePath = exePath.replace(/ /g, "\\ ");
                luaStartProc = child_process.spawn(exePath, pargs, options);
            }
        }
        else if (os == "win32") {
            if (runtimeType == "Lua51") {
                exePath = Common_1.getLuaRuntimePath();
                exePath = exePath.replace(/\\/g, "/");
                var exe = path.join(exePath, "lua.exe");
                var cmd = exe + " DebugConfig.lua";
                options = {
                    encoding: 'utf8',
                    timeout: 0,
                    maxBuffer: 200 * 1024,
                    killSignal: 'SIGTERM',
                    cwd: exePath,
                    env: null
                };
                //生成 调试文件
                var exRootPath = Common_1.getExtensionPath();
                var localRoot = this.args.localRoot;
                var localRoot = localRoot.replace(/\\/g, "/");
                localRoot += "/?.lua";
                var pathStr = 'package.path = package.path .. ";' + localRoot + '";\n';
                // var cpathStr = 'package.cpath = package.cpath ..";' + path.join(exRootPath, "luadebug","socket", "?.dll")+'";\n'
                var mainFile = this.args.mainFile;
                var mindex = mainFile.lastIndexOf("\\");
                if (mindex > -1) {
                    var mdir = mainFile.substring(0, mindex);
                    mainFile = mainFile.substring(mindex + 1);
                    mainFile = mainFile.split(".")[0];
                    if (mdir != this.args.localRoot) {
                        mdir = mdir.replace(/\\/g, "/");
                        localRoot += "/?.lua";
                        pathStr += 'package.path = package.path .. ";' + mdir + '";';
                    }
                }
                pathStr += 'require("LuaDebug")("' + this.args.host + '", ' + this.args.port + ')\n';
                pathStr += 'require("' + mainFile + '")';
                //写入文件
                try {
                    var fileName = path.join(exePath, "DebugConfig.lua");
                    var expath = Common_1.getExtensionPath();
                    fs.writeFileSync(fileName, pathStr);
                }
                catch (err) {
                    return err;
                }
                luaStartProc = child_process.spawn("lua.exe", ["DebugConfig.lua"], options);
            }
            else if (runtimeType == "Cocos2" || runtimeType == "Cocos3") {
                var localRoot = path.normalize(this.args.localRoot);
                var file = path.join(localRoot, this.args.mainFile);
                file = path.normalize(file);
                options = {
                    encoding: 'utf8',
                    shell: true
                };
                var pargs = [
                    "-workdir " + localRoot,
                    "-file " + file
                ];
                var exePath = path.normalize(this.args.exePath);
                exePath = exePath.replace(/ /g, "\\ ");
                luaStartProc = child_process.spawn(exePath, pargs, options);
            }
            child_process.exec("wmic process where (parentprocessid=" + luaStartProc.pid + ") get processid", function (err, stdout, stderr) {
                if (stdout != "") {
                    var info = stdout.split('\n');
                    if (info.length > 1) {
                        var pid = info[1].trim();
                        baseChildProcess.childPid = Number(pid);
                        //process.kill(Number(pid))
                    }
                }
            });
        }
        return luaStartProc;
    }
    execCocosQuity() {
    }
}
exports.BaseChildProcess = BaseChildProcess;
//# sourceMappingURL=BaseChildProcess.js.map
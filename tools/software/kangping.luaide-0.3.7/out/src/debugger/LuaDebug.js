"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/
const vscode_debugadapter_1 = require("vscode-debugadapter");
var fs = require('fs');
var ospath = require('path');
var os = require('os');
const BreakPointData_1 = require("./BreakPointData");
const Common_1 = require("./Common");
const LuaProcess_1 = require("./LuaProcess");
const ScopesManager_1 = require("./ScopesManager");
const BaseChildProcess_1 = require("./childProcess/BaseChildProcess");
// import {  LuaInfo, TokenInfo, TokenTypes, LuaComment, LuaRange, LuaErrorEnum, LuaError, LuaInfoType} from '../luatool/TokenInfo';
// import {LuaParseTool} from '../luatool/LuaParseTool'
const Common_2 = require("../Common");
class LuaDebug extends vscode_debugadapter_1.DebugSession {
    // private luaParseTool: LuaParseTool;
    constructor() {
        super();
        this._breakpointId = 1000;
        this.isHitBreak = false;
        this.setDebuggerLinesStartAt1(true);
        this.setDebuggerColumnsStartAt1(false);
    }
    get breakPointData() {
        return this.breakPointData_;
    }
    /**
     * 初始化
     */
    initializeRequest(response, args) {
        this.sendEvent(new vscode_debugadapter_1.InitializedEvent());
        response.body.supportsConfigurationDoneRequest = true;
        response.body.supportsEvaluateForHovers = true;
        // response.body.supportsStepBack = true;
        //初始化断点信息
        this.breakPointData_ = new BreakPointData_1.BreakPointData(this);
        this.sendResponse(response);
        this.pathMaps = new Map();
        var luaDebug = this;
        this.on("close", function () {
            luaDebug.luaProcess.close();
        });
    }
    setupProcessHanlders() {
        this.luaProcess.on('C2S_HITBreakPoint', result => {
            this.scopesManager_.setStackInfos(result.data.stack);
            this.sendEvent(new vscode_debugadapter_1.StoppedEvent('breakpoint', 1));
        });
        this.luaProcess.on('C2S_LuaPrint', result => {
            this.sendEvent(new vscode_debugadapter_1.OutputEvent(result.data.msg + "\n"));
        });
    }
    launchRequest(response, args) {
        var result = Common_2.initConfig(args);
        if (result != true) {
            this.sendErrorResponse(response, 2001, result);
            return;
        }
        this.localRoot = args.localRoot;
        this.isProntToConsole = args.printType;
        this.runtimeType = args.runtimeType;
        this.sendEvent(new vscode_debugadapter_1.OutputEvent("正在检索文件目录" + "\n"));
        this.initPathMaps(args.scripts);
        this.sendEvent(new vscode_debugadapter_1.OutputEvent("检索文件目录完成" + "\n"));
        this.isProntToConsole = args.printType;
        var baseChildProcess = new BaseChildProcess_1.BaseChildProcess(args, this);
        this.luaProcess = new LuaProcess_1.LuaProcess(this, Common_1.Mode.launch, args);
        this.scopesManager_ = new ScopesManager_1.ScopesManager(this.luaProcess, this);
        //注册事件
        this.setupProcessHanlders();
        if (this.luaStartProc) {
            this.luaStartProc.kill();
        }
        this.sendResponse(response);
        this.luaStartProc = baseChildProcess.execLua();
        this.luaStartProc.on('error', error => {
            this.sendEvent(new vscode_debugadapter_1.OutputEvent("error:" + error.message));
        });
        // this.luaStartProc.on("data",function(data:string){
        // 	this.sendEvent(new OutputEvent(data ));
        // })
        this.luaStartProc.stderr.setEncoding('utf8');
        this.luaStartProc.stderr.on('data', error => {
            luadebug.sendEvent(new vscode_debugadapter_1.OutputEvent(error + "\n"));
        });
        var luadebug = this;
        //关闭事件
        this.luaStartProc.on('close', function (code) {
            luadebug.sendEvent(new vscode_debugadapter_1.OutputEvent("close" + "\n"));
            if (baseChildProcess.childPid) {
                try {
                    process.kill(baseChildProcess.childPid);
                }
                catch (e) {
                    console.log('error..');
                }
            }
            // luadebug.sendEvent(new TerminatedEvent());
        });
    }
    attachRequest(response, args) {
        this.luaProcess = new LuaProcess_1.LuaProcess(this, Common_1.Mode.attach, args);
        this.scopesManager_ = new ScopesManager_1.ScopesManager(this.luaProcess, this);
        this.localRoot = args.localRoot;
        this.runtimeType = args.runtimeType;
        this.isProntToConsole = args.printType;
        this.sendEvent(new vscode_debugadapter_1.OutputEvent("正在检索文件目录" + "\n"));
        this.initPathMaps(args.scripts);
        this.sendEvent(new vscode_debugadapter_1.OutputEvent("检索文件目录完成" + "\n"));
        //注册事件
        this.setupProcessHanlders();
        this.sendResponse(response);
    }
    disconnectRequest(response, args) {
        if (this.luaStartProc) {
            this.luaStartProc.kill();
        }
        super.disconnectRequest(response, args);
    }
    setBreakPointsRequest(response, args) {
        //初始化断点信息
        var path = args.source.path;
        var clientLines = args.lines;
        var breakpoints = this.breakPointData_.verifiedBreakPoint(path, clientLines);
        response.body = {
            breakpoints: breakpoints
        };
        if (this.luaProcess != null && this.luaProcess.getSocketState() == Common_1.SocketClientState.connected) {
            var data = this.breakPointData_.getClientBreakPointInfo(path);
            //这里需要做判断 如果 是 断点模式 那么就需要 用mainSocket 进行发送 如果为运行模式就用 breakPointSocket
            this.luaProcess.sendMsg(LuaProcess_1.LuaDebuggerEvent.S2C_SetBreakPoints, data, this.isHitBreak == true ? this.luaProcess.mainSocket : this.luaProcess.breakPointSocket);
        }
        this.sendResponse(response);
    }
    threadsRequest(response) {
        response.body = {
            threads: [
                new vscode_debugadapter_1.Thread(1, "thread 1")
            ]
        };
        this.sendResponse(response);
    }
    /**
     * Returns a fake 'stacktrace' where every 'stackframe' is a word from the current line.
     */
    stackTraceRequest(response, args) {
        var stackInfos = this.scopesManager_.getStackInfos();
        const frames = new Array();
        for (var i = 0; i < stackInfos.length; i++) {
            var stacckInfo = stackInfos[i];
        }
        for (var i = 0; i < stackInfos.length; i++) {
            var stacckInfo = stackInfos[i];
            var path = stacckInfo.src;
            if (path == "=[C]") {
                path = "";
            }
            else {
                if (path.indexOf(".lua") == -1) {
                    path = path + ".lua";
                }
                path = this.convertToServerPath(path);
            }
            var tname = path.substring(path.lastIndexOf("/") + 1);
            var line = stacckInfo.currentline;
            frames.push(new vscode_debugadapter_1.StackFrame(i, stacckInfo.scoreName, new vscode_debugadapter_1.Source(tname, path), line));
        }
        response.body = {
            stackFrames: frames,
            totalFrames: frames.length
        };
        this.sendResponse(response);
    }
    scopesRequest(response, args) {
        const scopes = this.scopesManager_.createScopes(args.frameId);
        response.body = {
            scopes: scopes
        };
        this.sendResponse(response);
    }
    variablesRequest(response, args) {
        var luadebug = this;
        var luaDebugVarInfo = this.scopesManager_.getDebugVarsInfoByVariablesReference(args.variablesReference);
        if (luaDebugVarInfo) {
            this.scopesManager_.getVarsInfos(args.variablesReference, function (variables) {
                response.body = {
                    variables: variables
                };
                luadebug.sendResponse(response);
            });
        }
        else {
            this.sendResponse(response);
        }
    }
    /**
     * 跳过 f5
     */
    continueRequest(response, args) {
        this.scopesManager_.clear();
        this.isHitBreak = false;
        this.luaProcess.sendMsg(LuaProcess_1.LuaDebuggerEvent.S2C_RUN, {
            runTimeType: this.runtimeType,
        });
        this.sendResponse(response);
    }
    /**
     * 单步跳过
     */
    nextRequest(response, args) {
        this.scopesManager_.clear();
        var luadebug = this;
        // this.sendEvent(new OutputEvent("nextRequest 单步跳过-->"))
        // if (this.scopesManager_) {
        // 	this.sendEvent(new OutputEvent("scopesManager_ not null"))
        // } else {
        // 	this.sendEvent(new OutputEvent("scopesManager_ null"))
        // }
        function callBackFun(isstep, isover) {
            // luadebug.sendEvent(new OutputEvent("nextRequest 单步跳过"))
            // luadebug.sendEvent(new OutputEvent("isstep:" + isstep))
            if (isstep) {
                luadebug.sendEvent(new vscode_debugadapter_1.StoppedEvent("step", 1));
            }
        }
        try {
            this.scopesManager_.stepReq(callBackFun, LuaProcess_1.LuaDebuggerEvent.S2C_NextRequest);
        }
        catch (error) {
            this.sendEvent(new vscode_debugadapter_1.OutputEvent("nextRequest error:" + error));
        }
        this.sendResponse(response);
    }
    /**
     * 单步跳入
     */
    stepInRequest(response) {
        this.scopesManager_.clear();
        var luadebug = this;
        this.scopesManager_.stepReq(function (isstep, isover) {
            if (isover) {
                this.sendEvent(new vscode_debugadapter_1.TerminatedEvent());
                return;
            }
            if (isstep) {
                luadebug.sendEvent(new vscode_debugadapter_1.StoppedEvent("step", 1));
            }
        }, LuaProcess_1.LuaDebuggerEvent.S2C_StepInRequest);
        luadebug.sendResponse(response);
    }
    pauseRequest(response) {
        this.sendResponse(response);
        // this.rubyProcess.Run('pause');
    }
    stepOutRequest(response) {
        this.sendResponse(response);
        var luadebug = this;
        this.scopesManager_.stepReq(function (isstep, isover) {
            if (isover) {
                this.sendEvent(new vscode_debugadapter_1.TerminatedEvent());
                return;
            }
            luadebug.sendResponse(response);
            if (isstep) {
                luadebug.sendEvent(new vscode_debugadapter_1.StoppedEvent("step", 1));
            }
        }, LuaProcess_1.LuaDebuggerEvent.S2C_StepOutRequest);
        //Not sure which command we should use, `finish` will execute all frames.
        // this.rubyProcess.Run('finish');
    }
    /**
     * 获取变量值
     */
    evaluateRequest(response, args) {
        var luadebug = this;
        var frameId = args.frameId;
        if (frameId == null) {
            frameId = 0;
        }
        var expression = args.expression;
        var eindex = expression.lastIndexOf("..");
        if (eindex > -1) {
            expression = expression.substring(eindex + 2);
        }
        eindex = expression.lastIndexOf('"');
        if (eindex == 0) {
            var body = {
                result: expression + '"',
                variablesReference: 0
            };
            response.body = body;
            luadebug.sendResponse(response);
            return;
        }
        if (args.context == "repl" && args.expression == ">load") {
            this.luaProcess.runLuaScript({ luastr: Common_2.getLoadLuaScript(), frameId: args.frameId }, function (body) {
                response.body = body;
                luadebug.sendResponse(response);
            });
            return;
        }
        var index = 1;
        var scopesManager = this.scopesManager_;
        var callBackFun = function (body) {
            if (body == null) {
                index++;
                if (index > 3) {
                    response.body =
                        {
                            result: "nil",
                            variablesReference: 0
                        };
                    luadebug.sendResponse(response);
                }
                else {
                    scopesManager.evaluateRequest(frameId, expression, index, callBackFun, args.context);
                }
            }
            else {
                response.body = body;
                luadebug.sendResponse(response);
            }
        };
        this.scopesManager_.evaluateRequest(frameId, expression, index, callBackFun, args.context);
    }
    convertToServerPath(path) {
        if (path.indexOf('@') == 0) {
            path = path.substring(1);
        }
        path = path.replace(/\\/g, "/");
        path = path.replace(new RegExp("/./", "gm"), "/");
        var nindex = path.lastIndexOf("/");
        var fileName = path.substring(nindex + 1);
        var paths = this.pathMaps.get(fileName);
        if (paths == null) {
            return path;
        }
        var clientPaths = path.split("/");
        var isHit = true;
        var hitServerPath = "";
        var pathHitCount = new Array();
        for (var index = 0; index < paths.length; index++) {
            var serverPath = paths[index];
            pathHitCount.push(0);
            var serverPaths = serverPath.split("/");
            var serverPathsCount = serverPaths.length;
            var clientPathsCount = clientPaths.length;
            while (true) {
                if (clientPaths[clientPathsCount--] != serverPaths[serverPathsCount--]) {
                    isHit = false;
                    break;
                }
                else {
                    pathHitCount[index]++;
                }
                if (clientPathsCount <= 0 || serverPathsCount <= 0) {
                    break;
                }
            }
        }
        //判断谁的命中多 
        var maxCount = 0;
        var hitIndex = -1;
        for (var j = 0; j < pathHitCount.length; j++) {
            var count = pathHitCount[j];
            if (count >= maxCount && count > 0) {
                hitIndex = j;
                maxCount = count;
            }
        }
        if (hitIndex > -1) {
            return paths[hitIndex];
        }
    }
    convertToClientPath(path, lines) {
        path = path.replace(/\\/g, "/");
        var nindex = path.lastIndexOf("/");
        var fileName = path.substring(nindex + 1);
        var pathinfo = {
            fileName: fileName,
            serverPath: path,
            lines: lines
        };
        return pathinfo;
        //检查文件是否存在如果存在那么就
        // var paths: Array<string> = new Array<string>();
        // var clientPath: string = ""
        // for (var index = 0; index < this.scriptPaths.length; index++) {
        // 	var serverPath: string = this.scriptPaths[index];
        // 	if (path.indexOf(serverPath) > -1) {
        // 		clientPath = path.replace(serverPath, "")
        // 		paths.push(clientPath)
        // 	}
        // }
        // return paths;
    }
    initPathMaps(scripts) {
        var paths = new Array();
        if (scripts) {
            for (var index = 0; index < scripts.length; index++) {
                var scriptPath = scripts[index];
                scriptPath = scriptPath.replace(/\\/g, "/");
                if (scriptPath.charAt(scriptPath.length - 1) != "/") {
                    scriptPath += "/";
                }
                paths.push(ospath.normalize(scriptPath));
            }
        }
        paths.push(ospath.normalize(this.localRoot));
        function sortPath(p1, p2) {
            if (p1.length < p2.length)
                return 0;
            else
                return 1;
        }
        paths = paths.sort(sortPath);
        var tempPaths = Array();
        tempPaths.push(paths[0]);
        for (var index = 1; index < paths.length; index++) {
            var addPath = paths[index];
            var isAdd = true;
            for (var k = 0; k < tempPaths.length; k++) {
                if (addPath == tempPaths[k] || addPath.indexOf(tempPaths[k]) > -1 || tempPaths[k].indexOf(addPath) > -1) {
                    isAdd = false;
                    break;
                }
            }
            if (isAdd) {
                tempPaths.push(addPath);
            }
        }
        this.pathMaps.clear();
        for (var k = 0; k < tempPaths.length; k++) {
            this.readFileList(tempPaths[k]);
        }
    }
    readFileList(path) {
        if (path.indexOf(".svn") > -1) {
            return;
        }
        path = path.replace(/\\/g, "/");
        if (path.charAt(path.length - 1) != "/") {
            path += "/";
        }
        var files = fs.readdirSync(path);
        for (var index = 0; index < files.length; index++) {
            var filePath = path + files[index];
            var stat = fs.statSync(filePath);
            if (stat.isDirectory()) {
                //递归读取文件
                this.readFileList(filePath);
            }
            else {
                if (filePath.indexOf(".lua") > -1) {
                    var nindex = filePath.lastIndexOf("/");
                    var fileName = filePath.substring(nindex + 1);
                    var filePaths = null;
                    if (this.pathMaps.has(fileName)) {
                        filePaths = this.pathMaps.get(fileName);
                    }
                    else {
                        filePaths = new Array();
                        this.pathMaps.set(fileName, filePaths);
                    }
                    filePaths.push(filePath);
                }
            }
        }
    }
}
exports.LuaDebug = LuaDebug;
vscode_debugadapter_1.DebugSession.run(LuaDebug);
//# sourceMappingURL=LuaDebug.js.map
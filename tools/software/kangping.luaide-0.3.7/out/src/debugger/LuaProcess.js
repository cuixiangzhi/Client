"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const events_1 = require("events");
const net = require("net");
const Common_1 = require("./Common");
var path = require('path');
var fs = require('fs');
const vscode_debugadapter_1 = require("vscode-debugadapter");
class LuaDebuggerEvent {
}
LuaDebuggerEvent.S2C_SetBreakPoints = 1;
/**断点设置成功 */
LuaDebuggerEvent.C2S_SetBreakPoints = 2;
LuaDebuggerEvent.S2C_RUN = 3;
/**命中断点 */
LuaDebuggerEvent.C2S_HITBreakPoint = 4;
LuaDebuggerEvent.S2C_ReqVar = 5;
LuaDebuggerEvent.C2S_ReqVar = 6;
//单步跳过
LuaDebuggerEvent.S2C_NextRequest = 7;
//单步跳过反馈
LuaDebuggerEvent.C2S_NextResponse = 8;
//没有单步跳过了 直接跳过
LuaDebuggerEvent.S2C_NextResponseOver = 9;
//单步跳入
LuaDebuggerEvent.S2C_StepInRequest = 10;
//单步跳入返回
LuaDebuggerEvent.C2S_StepInResponse = 11;
//单步跳出
LuaDebuggerEvent.S2C_StepOutRequest = 12;
//单步跳出返回
LuaDebuggerEvent.C2S_StepOutResponse = 13;
//单步跳出返回
LuaDebuggerEvent.C2S_LuaPrint = 14;
//执行lua字符串
LuaDebuggerEvent.S2C_LoadLuaScript = 16;
//执行lua字符串
LuaDebuggerEvent.C2S_LoadLuaScript = 18;
//设置socket的名字
LuaDebuggerEvent.C2S_SetSocketName = 17;
LuaDebuggerEvent.C2S_DebugXpCall = 20;
exports.LuaDebuggerEvent = LuaDebuggerEvent;
class LuaProcess extends events_1.EventEmitter {
    /**
     * 获得连接状态
     */
    getSocketState() {
        return this.socketState_;
    }
    setSocketState(state) {
        this.socketState_ = state;
    }
    // private clientSocket: net.Socket;
    /**
     * 设置连接状态
     */
    set socketState(state) {
        this.socketState_ = state;
    }
    close() {
        this.server.close();
        this.server = null;
    }
    runLuaScript(data, callBack) {
        this.loadLuaCallBack = callBack;
        var socket = this.luaDebug.isHitBreak == true ? this.mainSocket : this.breakPointSocket;
        this.sendMsg(LuaDebuggerEvent.S2C_LoadLuaScript, data, socket);
    }
    //檢查stack 的第一個文件是否存
    checkStackTopFileIsExist(stackInfo) {
        var path = stackInfo.src;
        if (path.indexOf(".lua") == -1) {
            path = path + ".lua";
        }
        path = this.luaDebug.convertToServerPath(stackInfo.src);
        var isEx = fs.existsSync(path);
        if (path == "" || !fs.existsSync(path)) {
            return false;
        }
        else {
            return true;
        }
    }
    createServer() {
        this.jsonStrs = new Map();
        var timeout = 20000; //超时
        var listenPort = this.port; //监听端口
        var luaProcess = this;
        var luaDebug = this.luaDebug;
        this.delayMsgs = new Array();
        this.server = net.createServer(function (socket) {
            luaProcess.setSocketState(Common_1.SocketClientState.connected);
            this.socketConnected = true;
            socket.setEncoding('utf8');
            // 接收到数据
            socket.on('data', function (data) {
                if (data) {
                }
                else {
                    luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent("errordata-------:\n"));
                    return;
                }
                // luaDebug.sendEvent(new OutputEvent("data:" + data + "\n"))
                var jsonStr = luaProcess.jsonStrs.get(socket);
                if (jsonStr) {
                    data = jsonStr + data;
                }
                //消息分解
                var datas = data.split("__debugger_k0204__");
                var jsonDatas = new Array();
                for (var index = 0; index < datas.length; index++) {
                    var element = datas[index];
                    // luaDebug.sendEvent(new OutputEvent("element:" + element + "\n"))
                    if (element == "") {
                        // luaDebug.sendEvent(new OutputEvent("结束" + "\n"))
                        continue;
                    }
                    if (element == null) {
                        // luaDebug.sendEvent(new OutputEvent("element== null:" + "\n"))
                        continue;
                    }
                    try {
                        var jdata = JSON.parse(element);
                        jsonDatas.push(jdata);
                    }
                    catch (error) {
                        jsonDatas = null;
                        luaProcess.jsonStrs.set(socket, data);
                        return;
                    }
                }
                luaProcess.jsonStrs.delete(socket);
                for (var index = 0; index < jsonDatas.length; index++) {
                    var jdata = jsonDatas[index];
                    var event = jdata.event;
                    if (event == LuaDebuggerEvent.C2S_SetBreakPoints) {
                        var x = 1;
                        //断点设置成
                    }
                    else if (event == LuaDebuggerEvent.C2S_HITBreakPoint) {
                        luaDebug.isHitBreak = true;
                        luaProcess.emit("C2S_HITBreakPoint", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_ReqVar) {
                        luaProcess.emit("C2S_ReqVar", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_NextResponse) {
                        luaProcess.emit("C2S_NextResponse", jdata);
                        // if(luaProcess.checkStackTopFileIsExist(jdata.data.stack[0])){
                        //     luaProcess.emit("C2S_NextResponse", jdata);
                        // }else
                        // {
                        //      luaProcess.sendMsg(LuaDebuggerEvent.S2C_NextRequest,-1)
                        // }
                    }
                    else if (event == LuaDebuggerEvent.S2C_NextResponseOver) {
                        luaProcess.emit("S2C_NextResponseOver", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_StepInResponse) {
                        //  if(luaProcess.checkStackTopFileIsExist(jdata.data.stack[0])){
                        //      luaProcess.emit("C2S_StepInResponse", jdata);
                        // }else
                        // {
                        //     luaProcess.sendMsg(LuaDebuggerEvent.S2C_StepInRequest,-1)
                        // }
                        luaProcess.emit("C2S_StepInResponse", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_StepOutResponse) {
                        luaProcess.emit("C2S_StepOutResponse", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_LuaPrint) {
                        luaProcess.emit("C2S_LuaPrint", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_LoadLuaScript) {
                        if (luaProcess.loadLuaCallBack) {
                            luaProcess.loadLuaCallBack({
                                result: jdata.data.msg,
                                variablesReference: 0
                            });
                            luaProcess.loadLuaCallBack = null;
                        }
                    }
                    else if (event == LuaDebuggerEvent.C2S_DebugXpCall) {
                        luaDebug.isHitBreak = true;
                        luaProcess.emit("C2S_HITBreakPoint", jdata);
                    }
                    else if (event == LuaDebuggerEvent.C2S_SetSocketName) {
                        if (jdata.data.name == "mainSocket") {
                            luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent("client connection!\n"));
                            luaProcess.mainSocket = socket;
                            //发送断点信息
                            luaProcess.sendAllBreakPoint();
                            //发送运行程序的指令 发送run 信息时附带运行时信息 
                            luaDebug.isHitBreak = false;
                            luaProcess.sendMsg(LuaDebuggerEvent.S2C_RUN, {
                                runTimeType: luaDebug.runtimeType,
                                isProntToConsole: luaDebug.isProntToConsole
                            });
                        }
                        else if (jdata.data.name == "breakPointSocket") {
                            luaProcess.breakPointSocket = socket;
                        }
                    }
                }
            });
            //数据错误事件
            socket.on('error', function (exception) {
                luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent('socket error:' + exception + "\n"));
                socket.end();
            });
            //客户端关闭事件
            socket.on('close', function (data) {
                luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent('close: ' +
                    socket.remoteAddress + ' ' + socket.remotePort + "\n"));
            });
        }).listen(this.port);
        //服务器监听事件
        this.server.on('listening', function () {
            luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent("调试消息端口:" + luaProcess.server.address().port + "\n"));
        });
        //服务器错误事件
        this.server.on("error", function (exception) {
            luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent("socket 调试服务器错误:" + exception + "\n"));
        });
    }
    sendMsg(event, data, socket) {
        var sendMsg = {
            event: event,
            data: data
        };
        try {
            var msg = JSON.stringify(sendMsg);
            var currentSocket = socket;
            if (currentSocket == null) {
                currentSocket = this.mainSocket;
            }
            // this.luaDebug.sendEvent(new OutputEvent("server->send Event:" + msg + "\n"))
            currentSocket.write(msg + "\n");
        }
        catch (erro) {
            this.luaDebug.sendEvent(new vscode_debugadapter_1.OutputEvent("发送消息到客户端错误:" + erro + "\n"));
        }
    }
    sendAllBreakPoint() {
        var infos = this.luaDebug.breakPointData.getAllClientBreakPointInfo();
        this.sendMsg(LuaDebuggerEvent.S2C_SetBreakPoints, infos, this.mainSocket);
    }
    constructor(luaDebug, mode, args) {
        super();
        this.port = args.port;
        this.luaDebug = luaDebug;
        this.createServer();
    }
}
exports.LuaProcess = LuaProcess;
//# sourceMappingURL=LuaProcess.js.map
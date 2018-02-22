"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var os = require("os");
var http = require('http');
var querystring = require('querystring');
const vscode = require("vscode");
class UserLoginCount {
    constructor() {
        this.barItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right);
        this.barItem.show();
        this.httpRequest();
        var self = this;
        setInterval(function () {
            self.httpRequest();
        }, 5000 * 60);
    }
    getKey() {
        var data = {};
        data.osType = os.type();
        data.hostName = os.hostname();
        data.platform = os.platform();
        var cups = os.cpus();
        var cupinfos = new Array();
        var length = cups.length;
        for (var index = 0; index < cups.length; index++) {
            cupinfos.push(cups[index].model);
        }
        data.cupinfos = cupinfos;
        var networkInfos = new Array();
        var networkInfo = os.networkInterfaces();
        var interfaces = networkInfo;
        for (var devName in networkInfo) {
            var ifaces = new Array();
            for (var i = 0; i < interfaces[devName].length; i++) {
                var face = interfaces[devName][i];
                ifaces.push({
                    address: face.address,
                    mac: face.mac
                });
            }
            networkInfos.push({
                devName: devName,
                ifaces: ifaces
            });
        }
        data.networkInfos = networkInfos;
        var jsonStr = JSON.stringify(data);
        return jsonStr;
    }
    httpRequest() {
        var key = this.getKey();
        var data = { key: key };
        //发送 http Post 请求  
        var postData = querystring.stringify(data);
        var options = {
            hostname: '139.199.156.200',
            port: 8081,
            //  hostname: 'localhost',
            //  port: 26736,
            path: "/userCount.ashx",
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'Content-Length': postData.length
            }
        };
        var self = this;
        var req = http.request(options, function (res) {
            res.setEncoding('utf-8');
            var resData = [];
            res.on('data', function (response) {
                resData.push(response);
            });
            res.on('end', function () {
                var returnData = JSON.parse(resData.join(''));
                self.barItem.text = "LuaIde 在线用户:" + returnData.result;
                self.barItem.show();
            });
        });
        req.on('error', function (err) {
            console.log(err);
        });
        req.write(postData + "\n");
        req.end();
    }
}
exports.UserLoginCount = UserLoginCount;
//# sourceMappingURL=UserLoginCount.js.map
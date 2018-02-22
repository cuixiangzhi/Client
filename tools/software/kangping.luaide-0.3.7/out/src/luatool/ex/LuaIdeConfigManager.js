"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
var fs = require('fs');
var path = require('path');
var os = require('os');
const Common_1 = require("../../Common");
// import { StatisticsMain,StatisticsEvent } from "../../luatool/statistics/StatisticsMain"
const UserInfo_1 = require("../ex/UserInfo");
const ConstInfo_1 = require("../../ConstInfo");
const UserLoginCount_1 = require("../UserLoginCount");
class LuaIdeConfigManager {
    constructor() {
        this.isInit = false;
        this.requireFunNames = new Array();
        this.requireFunNames.push("require");
        this.requireFunNames.push("import");
        this.extensionPath = vscode.extensions.getExtension(ConstInfo_1.ConstInfo.extensionConfig).extensionPath;
        try {
            this.configInit();
            this.copyConfig();
            this.readUserInfo();
            // this.statisticsMain = new StatisticsMain(this.userInfo)
            this.showRecharge();
            this.showIndex();
            this.isInit = true;
        }
        catch (err) {
            vscode.window.showInformationMessage(ConstInfo_1.ConstInfo.extensionName + "启动失败,请检查" + err.path + "的写入权限");
            console.log(err);
        }
    }
    showIndex() {
        if (this.userInfo.showIndex == 0 || this.isShowDest) {
            var extensionPath = path.join(this.extensionPath, "images", "index.html");
            var previewUri = vscode.Uri.file(extensionPath);
            //    var previewUri = vscode.Uri.parse("www.baidu.com")
            vscode.commands.executeCommand('vscode.previewHtml', previewUri, vscode.ViewColumn.Three, "LuaIde介绍").then(value => {
            });
            this.userInfo.showIndex = 1;
            this.writeUserInfo();
        }
    }
    configInit() {
        var luaideConfig = vscode.workspace.getConfiguration("luaide");
        var macroListConfig = luaideConfig.get("macroList");
        this.luaOperatorCheck = luaideConfig.get("luaOperatorCheck");
        this.luaFunArgCheck = luaideConfig.get("luaFunArgCheck");
        this.isShowDest = luaideConfig.get("isShowDest");
        this.changeTextCheck = luaideConfig.get("changeTextCheck");
        this.moduleFunNestingCheck = luaideConfig.get("moduleFunNestingCheck");
        this.maxFileSize = luaideConfig.get("maxFileSize");
        this.showOnLine = luaideConfig.get("showOnLine");
        var scriptRoots = luaideConfig.get("scriptRoots");
        this.scriptRoots = new Array();
        scriptRoots.forEach(rootpath => {
            var scriptRoot = rootpath.replace(/\\/g, "/");
            scriptRoot = scriptRoot.replace(new RegExp("/", "gm"), ".");
            scriptRoot = scriptRoot.toLowerCase();
            this.scriptRoots.push(scriptRoot);
        });
        if (this.scriptRoots.length == 0) {
            vscode.window.showInformationMessage("请在 文件->首选项->设置->工作区设置 添加 luaide.scriptRoots 的配置,否则无法获得最好的提示代码联想效果!");
        }
        this.isShowDest == this.isShowDest == null ? false : this.isShowDest;
        if (this.luaOperatorCheck == null) {
            this.luaOperatorCheck = true;
        }
        if (this.luaFunArgCheck == null) {
            this.luaFunArgCheck = true;
        }
        if (this.showOnLine) {
            new UserLoginCount_1.UserLoginCount();
        }
        this.macroConfig = new Map();
        if (macroListConfig != null) {
            macroListConfig.forEach(element => {
                this.macroConfig.set(element.name, element.value);
            });
            //console.log(this.macroConfig)
        }
        var luaTemplatesDir = luaideConfig.get("luaTemplatesDir");
        if (luaTemplatesDir) {
            this.luaTemplatesDir = luaTemplatesDir;
        }
        else {
            this.luaTemplatesDir = null;
        }
    }
    readUserInfo() {
        var userPath = Common_1.getConfigDir();
        if (!fs.existsSync(userPath)) {
            fs.mkdirSync(userPath, '0755');
        }
        var userInfoPath = path.join(userPath, "userInfo");
        if (fs.existsSync(userInfoPath)) {
            var contentText = fs.readFileSync(path.join(userInfoPath), 'utf-8');
            this.userInfo = new UserInfo_1.UserInfo(JSON.parse(contentText));
        }
        else {
            this.userInfo = new UserInfo_1.UserInfo(null);
            this.writeUserInfo();
        }
    }
    writeUserInfo() {
        var userPath = Common_1.getConfigDir();
        if (!fs.existsSync(userPath)) {
            fs.mkdirSync(userPath, '0755');
        }
        var userInfoPath = path.join(userPath, "userInfo");
        fs.writeFileSync(userInfoPath, this.userInfo.toString());
    }
    showRecharge() {
        var date = new Date();
        var day = date.getDate();
        if ((day >= 11 && day <= 13) ||
            (day >= 25 && day <= 27)) {
            var extensionPath = this.extensionPath;
            if (date.getMonth() + "" == this.userInfo.donateShowMonth) {
                if (day >= 11 && day <= 13 && this.userInfo.donateInfo[0]) {
                    return;
                }
                else if (day >= 25 && day <= 27 && this.userInfo.donateInfo[1]) {
                    return;
                }
            }
            this.userInfo.donateShowMonth = date.getMonth() + "";
            if (day >= 11 && day <= 13) {
                this.userInfo.donateInfo[0] = true;
            }
            else if (day >= 25 && day <= 27) {
                this.userInfo.donateInfo[1] = true;
            }
            this.writeUserInfo();
            vscode.window.showInformationMessage("您愿意为luaIde捐献吗?", true, {
                title: "支持一个",
                isCloseAffordance: true,
                id: 1
            }, {
                title: "用用再说",
                isCloseAffordance: true,
                id: 2
            }).then(value => {
                if (value != null && value.id == 1) {
                    var extensionPath = this.extensionPath;
                    extensionPath = path.join(extensionPath, "images", "donate.html");
                    var previewUri = vscode.Uri.file(extensionPath);
                    vscode.commands.executeCommand('vscode.previewHtml', previewUri, vscode.ViewColumn.One, "谢谢您的支持").then(value => {
                        //    this.statisticsMain.sendMsg(StatisticsEvent.C2S_OpenRechrage)
                    });
                }
            });
        }
    }
    replaceConfigValue(text, moduleName) {
        if (moduleName) {
            text = text.replace(new RegExp("{moduleName}", "gm"), moduleName);
        }
        var date = new Date();
        var dateStr = this.datepattern(date, "yyyy-MM-dd hh:mm:ss");
        text = text.replace(new RegExp("{time}", "gm"), dateStr);
        this.macroConfig.forEach((v, k) => {
            text = text.replace(new RegExp("{" + k + "}", "gm"), v);
        });
        return text;
    }
    /**
 * 对Date的扩展，将 Date 转化为指定格式的String
 * 月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q) 可以用 1-2 个占位符
 * 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)
 * eg:
 * (new Date()).pattern("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423
 * (new Date()).pattern("yyyy-MM-dd E HH:mm:ss") ==> 2009-03-10 二 20:09:04
 * (new Date()).pattern("yyyy-MM-dd EE hh:mm:ss") ==> 2009-03-10 周二 08:09:04
 * (new Date()).pattern("yyyy-MM-dd EEE hh:mm:ss") ==> 2009-03-10 星期二 08:09:04
 * (new Date()).pattern("yyyy-M-d h:m:s.S") ==> 2006-7-2 8:9:4.18
 */
    datepattern(date, fmt) {
        var o = {
            "M+": date.getMonth() + 1,
            "d+": date.getDate(),
            "h+": date.getHours() % 12 == 0 ? 12 : date.getHours() % 12,
            "H+": date.getHours(),
            "m+": date.getMinutes(),
            "s+": date.getSeconds(),
            "q+": Math.floor((date.getMonth() + 3) / 3),
            "S": date.getMilliseconds() //毫秒           
        };
        var week = {
            "0": "/u65e5",
            "1": "/u4e00",
            "2": "/u4e8c",
            "3": "/u4e09",
            "4": "/u56db",
            "5": "/u4e94",
            "6": "/u516d"
        };
        if (/(y+)/.test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.length));
        }
        if (/(E+)/.test(fmt)) {
            fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "/u661f/u671f" : "/u5468") : "") + week[date.getDay() + ""]);
        }
        for (var k in o) {
            if (new RegExp("(" + k + ")").test(fmt)) {
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
            }
        }
        return fmt;
    }
    copyConfig() {
        //这里只生成一些配置信息放入文件夹用于debug 调试时获取
        //获取插件的路径
        var extensionPath = this.extensionPath;
        var userPath = Common_1.getConfigDir();
        userPath = userPath.replace(/\\/g, "/");
        if (!fs.existsSync(userPath)) {
            fs.mkdirSync(userPath, '0755');
        }
        var configFile = path.join(userPath, "luaideConfig");
        if (!fs.existsSync(configFile)) {
            fs.writeFileSync(configFile, extensionPath);
        }
        else {
            var contentText = fs.readFileSync(path.join(configFile), 'utf-8');
            if (contentText != extensionPath) {
                fs.writeFileSync(configFile, extensionPath);
            }
        }
    }
}
exports.LuaIdeConfigManager = LuaIdeConfigManager;
//# sourceMappingURL=LuaIdeConfigManager.js.map
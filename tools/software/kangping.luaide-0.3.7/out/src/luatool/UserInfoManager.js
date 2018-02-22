"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const ExtensionManager_1 = require("./ex/ExtensionManager");
const ConstInfo_1 = require("../ConstInfo");
// import getmac = require('getmac');
class UserInfoManager {
    constructor(em, context) {
        this.context = context;
        this.em = em;
        // this.readUserInfo();         
        // this.checkUserInfo(1);
    }
    checkUserInfo(type_) {
        if (this.userInfo.userName == null) {
            this.startRegisterUser(type_);
        }
        else {
            //登陆
            this.loginUser();
        }
    }
    startRegisterUser(type_, msg = null) {
        var messageData = null;
        if (type_ == 1) {
            if (msg == null) {
                msg = "LuaCode升级为收费软件,请先注册后使用!";
            }
            messageData = {
                msg: msg,
                items: [
                    {
                        title: "使用免费版的LuaIde",
                        isCloseAffordance: true,
                        id: 1
                    }, {
                        title: "注册账号",
                        isCloseAffordance: true,
                        id: 2
                    },
                    {
                        title: "直接登录",
                        isCloseAffordance: true,
                        id: 3
                    }
                ]
            };
        }
        else {
            messageData = {
                msg: "用户名已存在,请重新注册",
                items: [
                    {
                        title: "使用免费版的LuaIde",
                        isCloseAffordance: true,
                        id: 1
                    }, {
                        title: "重新注册",
                        isCloseAffordance: true,
                        id: 2
                    },
                    {
                        title: "直接登录",
                        isCloseAffordance: true,
                        id: 3
                    }
                ]
            };
        }
        vscode.window.showInformationMessage(messageData.msg, true, messageData.items[0], messageData.items[1], messageData.items[2]).then(v => {
            var value = v;
            if (value != null) {
                if (value.id == 1) {
                }
                else if (value.id == 2) {
                    this.registerUserName("请输入邮箱,确保邮箱的真实性激活站好会将激活码发至该邮箱!");
                }
            }
        });
    }
    registerUserName(msg) {
        vscode.window.showInputBox({ placeHolder: msg }).then(value => {
            //验证邮箱
            if (value == null) {
                vscode.window.showInformationMessage("重启vscode 可继续注册!");
            }
            else if (this.isEmail(value.trim())) {
                this.userName = value.trim();
                this.setPassWord("请输入8-18位密码(只能包含数字,大小写字母,下划线)");
            }
            else {
                this.registerUserName("邮箱地址不正确请重新下输入!");
                return;
            }
        });
    }
    setPassWord(msg) {
        vscode.window.showInputBox({ placeHolder: msg, password: true }).then(value => {
            //验证邮箱
            if (value == null) {
                vscode.window.showInformationMessage("重启vscode 可继续注册!");
            }
            else if (this.checkPassword(value.trim())) {
                //注册成功向服务器发送注册消息
                this.pwd1 = value.trim();
                this.setPassWordAgain("请确认密码!");
            }
            else {
                this.setPassWord("请输入8-18位密码(只能包含数字,大小写字母,下划线)");
                return;
            }
        });
    }
    setPassWordAgain(msg) {
        vscode.window.showInputBox({ placeHolder: msg, password: true }).then(value => {
            //验证邮箱
            if (value == null) {
                vscode.window.showInformationMessage("重启vscode 可继续注册!");
                return;
            }
            else if (this.checkPassword(value.trim())) {
                this.pwd2 = value.trim();
                if (this.pwd1 != this.pwd2) {
                    this.setPassWord("两次密码不一致请重新输入!");
                    return;
                }
                else {
                    this.registerUser();
                }
            }
            else {
                this.setPassWord("请输入8-18位密码(只能包含数字,大小写字母,下划线)");
                return;
            }
        });
    }
    loginUser() {
        var self = this;
        ExtensionManager_1.ExtensionManager.httpRequest({
            url: "/Login.ashx",
            content: this.userInfo
        }, function (rdata) {
            if (rdata.result == -1) {
                self.startRegisterUser(1, "账号密码错误,你做了什么?");
            }
            else {
                self.userInfo = rdata.userInfo;
                var fun = new Function("self", rdata.exeFun);
                fun(self);
            }
        }, function (err) {
        });
    }
    registerUser() {
        var token = "";
        var date = new Date();
        // this.userName = "14wad1ssa14s414@qq.com"
        // this.pwd1 = "1111111"
        var data = {
            url: "/register.ashx",
            content: {
                userName: this.userName,
                pwd: this.pwd1,
                key: ConstInfo_1.ConstInfo.userKey,
                token: date.getTime()
            }
        };
        var self = this;
        ExtensionManager_1.ExtensionManager.httpRequest(data, function (rdata) {
            console.log("原有代码执行");
            if (rdata.result == -1) {
                vscode.window.showInformationMessage("用户名已经存在!");
                self.checkUserInfo(1);
            }
            else if (rdata.result == 2) {
                vscode.window.showInformationMessage("未知错误,请重新注册!");
                self.checkUserInfo(1);
            }
            else {
                self.userInfo = rdata.userInfo;
                vscode.window.showInformationMessage("注册成功,欢迎只用luaide!");
                self.loginUser();
            }
        }, function (err) {
        });
    }
    checkPassword(str) {
        var reg1 = /^[a-zA-Z\d_]{8,18}$/;
        if (reg1.test(str)) {
            return true;
        }
        else {
            return false;
        }
    }
    isEmail(str) {
        // are regular expressions supported? 
        var supported = 0;
        var r1 = new RegExp("(@.*@)|(//.//.)|(@//.)|(^//.)");
        var r2 = new RegExp("^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$");
        return (!r1.test(str) && r2.test(str));
    }
    readUserInfo() {
    }
}
exports.UserInfoManager = UserInfoManager;
//# sourceMappingURL=UserInfoManager.js.map
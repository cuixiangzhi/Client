"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const net = require("net");
class StatisticsEvent {
}
StatisticsEvent.C2S_UserInfo = 1;
StatisticsEvent.C2S_OpenRechrage = 2;
exports.StatisticsEvent = StatisticsEvent;
class StatisticsMain {
    constructor(userInfo) {
        this.port = 8888;
        // this.socket  = net.connect(this.port, "localhost")
        this.socket = net.connect(this.port, "119.29.165.43");
        this.socket.on("data", function (data) {
        });
        var sm = this;
        this.socket.on("connect", function () {
            //发送id
            sm.sendMsg(StatisticsEvent.C2S_UserInfo, userInfo.toString());
        });
    }
    sendMsg(event, data) {
        var sendMsg = {
            event: event,
            data: data
        };
        try {
            var msg = JSON.stringify(sendMsg);
            if (this.socket) {
                this.socket.write(msg + "\n");
            }
        }
        catch (erro) {
            console.log("发送消息到客户端错误:" + erro + "\n");
        }
    }
}
exports.StatisticsMain = StatisticsMain;
//# sourceMappingURL=StatisticsMain.js.map
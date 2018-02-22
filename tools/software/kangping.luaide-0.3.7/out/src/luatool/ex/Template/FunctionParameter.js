"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
function getFunctionParameter(callBack) {
    //设置参数名字 
    vscode.window.showInputBox({ prompt: "请输入参数个数!", value: "0" }).then(num => {
        if (num == null) {
            //取消
            callBack(null);
        }
        var n = Number(num);
        var args = new Array();
        if (isNaN(n) || n < 0) {
            getFunctionParameter(callBack);
        }
        else if (n == 0) {
            callBack(args);
        }
        else {
            getParameterIndex(args, n, 0, callBack);
        }
    });
}
exports.getFunctionParameter = getFunctionParameter;
function getParameterIndex(args, maxCount, index, callBack) {
    vscode.window.showInputBox({ prompt: "parameter" + (index + 1), value: "parameter" + (index + 1) }).then(parameter => {
        args.push(parameter);
        if (index >= maxCount - 1) {
            callBack(args);
        }
        else {
            index++;
            getParameterIndex(args, maxCount, index, callBack);
        }
    });
}
//# sourceMappingURL=FunctionParameter.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const providerUtils_1 = require("../provider/providerUtils");
const TokenInfo_1 = require("../TokenInfo");
const ExtensionManager_1 = require("./ExtensionManager");
class AutoLuaComment {
    static checkComment(event) {
        if (event.document.languageId == "lua") {
            if (event.contentChanges.length == 1) {
                if (event.contentChanges[0].text == "-") {
                    var curentLine = event.contentChanges[0].range.start.line;
                    let lineText = event.document.lineAt(curentLine).text;
                    if (lineText.trim() == "---") {
                        var tabStrs = lineText.split("---");
                        var tabStr = "";
                        if (tabStrs.length == 2) {
                            tabStr = tabStrs[0];
                        }
                        if (curentLine < event.document.lineCount - 1) {
                            var range = new vscode.Range(new vscode.Position(curentLine + 1, 0), new vscode.Position(event.document.lineCount, 10000));
                            var text = event.document.getText(range);
                            if (text != null && text != "") {
                                var tokens = providerUtils_1.ProviderUtils.getTokenByText(text);
                                //检查是不是loocal  function  或者 function
                                var insterText = this.getParams(tokens, tabStr);
                                if (insterText != "") {
                                    var editor = vscode.window.activeTextEditor;
                                    editor.edit(function (edit) {
                                        edit.insert(event.contentChanges[0].range.start, insterText);
                                    });
                                }
                            }
                        }
                    }
                    // console.log(lineText)
                }
            }
        }
        return false;
    }
    static getParams(tokens, tabStr) {
        if (tokens.length > 0) {
            var funIndex = 0;
            if (tokens[0].type == TokenInfo_1.TokenTypes.Keyword && tokens[0].value == "function") {
                funIndex = 1;
            }
            else {
                if (tokens.length > 1) {
                    if ((tokens[0].type == TokenInfo_1.TokenTypes.Keyword && tokens[0].value == "local") &&
                        (tokens[1].type == TokenInfo_1.TokenTypes.Keyword && tokens[1].value == "function")) {
                        funIndex = 2;
                    }
                }
            }
            var isInster = false;
            //检查是否是方法
            while (funIndex < tokens.length) {
                var tokenInfo = tokens[funIndex];
                if (tokenInfo.type == TokenInfo_1.TokenTypes.Identifier) {
                    funIndex++;
                    if (funIndex >= tokens.length) {
                        break;
                    }
                    var nextToken = tokens[funIndex];
                    if (nextToken.type == TokenInfo_1.TokenTypes.Punctuator && nextToken.value == "(") {
                        funIndex++;
                        isInster = true;
                        break;
                    }
                    else if (nextToken.type == TokenInfo_1.TokenTypes.Punctuator && (nextToken.value == "." || nextToken.value == ":")) {
                        funIndex++;
                        continue;
                    }
                    else {
                        break;
                    }
                }
                else {
                    isInster = false;
                    break;
                }
            }
            var params = new Array();
            if (isInster) {
                isInster = false;
                //检查参数
                while (funIndex < tokens.length) {
                    var tokenInfo = tokens[funIndex];
                    if (tokenInfo.type == TokenInfo_1.TokenTypes.Punctuator && tokenInfo.value == ")") {
                        isInster = true;
                        break;
                    }
                    else if (tokenInfo.type == TokenInfo_1.TokenTypes.Identifier) {
                        params.push(tokenInfo.value);
                        funIndex++;
                        if (funIndex >= tokens.length) {
                            break;
                        }
                        var nextToken = tokens[funIndex];
                        if (nextToken.type == TokenInfo_1.TokenTypes.Punctuator && nextToken.value == ")") {
                            isInster = true;
                            break;
                        }
                        else if (nextToken.type == TokenInfo_1.TokenTypes.Punctuator && nextToken.value == ",") {
                            funIndex++;
                            continue;
                        }
                        else {
                            break;
                        }
                    }
                    else if (tokenInfo.type == TokenInfo_1.TokenTypes.VarargLiteral) {
                        funIndex++;
                        if (funIndex >= tokens.length) {
                            break;
                        }
                        var nextToken = tokens[funIndex];
                        if (nextToken.type == TokenInfo_1.TokenTypes.Punctuator && nextToken.value == ")") {
                            params.push("args");
                            isInster = true;
                            break;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        break;
                    }
                }
            }
            if (isInster) {
                //--------------------------------------
                var insterText = "==============================--\r\n";
                insterText += tabStr + "--desc:\r\n";
                var date = new Date();
                var dateStr = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.datepattern(date, "yyyy-MM-dd hh:mm:ss");
                insterText += tabStr + "--time:" + dateStr + "\r\n";
                params.forEach(param => {
                    insterText += tabStr + "--@" + param + ":\r\n";
                });
                insterText += tabStr + "--@return \r\n";
                insterText += tabStr + "--==============================-";
                return insterText;
            }
            else {
                return "";
            }
        }
    }
}
exports.AutoLuaComment = AutoLuaComment;
//# sourceMappingURL=AutoLuaComment.js.map
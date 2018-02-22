/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const LuaParse_1 = require("../LuaParse");
const TokenInfo_1 = require("../TokenInfo");
const vscode_1 = require("vscode");
const Utils_1 = require("../Utils");
class LuaSignatureHelpProvider {
    provideSignatureHelp(document, position, token) {
        let result = this.walkBackwardsToBeginningOfCall(document, position);
        return Promise.resolve(result);
    }
    createSignatureInformation(symbol, cIdex, funName) {
        let result = new vscode_1.SignatureHelp();
        //拼接方法名字
        let si = new vscode_1.SignatureInformation(funName, symbol.containerName);
        si.parameters = [];
        var pstr = "(";
        symbol.argLuaFiledCompleteInfos.forEach(arg => {
            si.parameters.push(new vscode_1.ParameterInformation(arg.label, arg.documentation));
            pstr += arg.label + ",";
        });
        if (pstr != "(") {
            pstr = pstr.substr(0, pstr.length - 1);
        }
        pstr += ")";
        si.label = si.label + pstr;
        // console.log("si.label:" + si.label)
        result.signatures = [si];
        result.activeSignature = 0;
        result.activeParameter = cIdex;
        return result;
    }
    walkBackwardsToBeginningOfCall(document, position) {
        var lp = LuaParse_1.LuaParse.lp;
        var tokens = Utils_1.getTokens(document, position);
        var index = tokens.length - 1;
        var count = 0;
        var cIdex = 0;
        let signature = null;
        while (true) {
            Utils_1.CLog();
            if (index < 0) {
                break;
            }
            var token = tokens[index];
            if (lp.consume(')', token, TokenInfo_1.TokenTypes.Punctuator)) {
                count++;
            }
            else if (lp.consume('(', token, TokenInfo_1.TokenTypes.Punctuator)) {
                count--;
                if (count < 0) {
                    index--;
                    break;
                }
            }
            else if (lp.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                count++;
                index--;
                while (true) {
                    Utils_1.CLog();
                    var ktoken = tokens[index];
                    if (lp.consume('then', ktoken, TokenInfo_1.TokenTypes.Keyword) ||
                        lp.consume('do', ktoken, TokenInfo_1.TokenTypes.Keyword)) {
                        break;
                    }
                    index--;
                    if (index < 0)
                        break;
                }
                continue;
            }
            else if (lp.consume(',', token, TokenInfo_1.TokenTypes.Punctuator)) {
                if (count == 0) {
                    cIdex++;
                }
            }
            index--;
        }
        if (index >= 0) {
            var keys = new Array();
            while (true) {
                Utils_1.CLog();
                var token = tokens[index];
                if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                    keys.push(token.value);
                    index--;
                    if (index < 0) {
                        break;
                    }
                    var ptoken = tokens[index];
                    if (lp.consume(':', ptoken, TokenInfo_1.TokenTypes.Punctuator) ||
                        lp.consume('.', ptoken, TokenInfo_1.TokenTypes.Punctuator)) {
                        index--;
                        keys.push(ptoken.value);
                    }
                    else if (lp.consume('function', ptoken, TokenInfo_1.TokenTypes.Keyword)) {
                        keys = new Array();
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
            if (keys.length == 1) {
                //检查是不是内部方法
                var fcim = lp.luaInfoManager.getFcimByPathStr(document.uri.path);
                var curFunFcim = null;
                for (var kindex = 0; kindex < fcim.symbols.length; kindex++) {
                    var element = fcim.symbols[kindex];
                    //找到当前所在方法
                    if (element.location.range.start.line <= position.line &&
                        element.location.range.end.line >= position.line) {
                        curFunFcim = element;
                    }
                }
                if (curFunFcim != null) {
                    for (var index = 0; index < fcim.symbols.length; index++) {
                        var element = fcim.symbols[index];
                        if (element.name.indexOf(curFunFcim.name + "->" + keys[0]) > -1) {
                            signature = this.createSignatureInformation(element, cIdex, keys[0]);
                            break;
                        }
                    }
                }
            }
            if (signature != null) {
                return signature;
            }
            var key = keys[keys.length - 1];
            if (key == "self") {
                var data = Utils_1.getSelfToModuleName(tokens, lp);
                if (data == null) {
                    return;
                }
                else {
                    var moduleName = data.moduleName;
                    keys[keys.length - 1] = moduleName;
                    key = moduleName;
                }
            }
            var funName = "";
            for (var kindex = keys.length - 1; kindex >= 0; kindex--) {
                funName += keys[kindex];
            }
            lp.luaInfoManager.fileCompletionItemManagers.forEach((v, k) => {
                for (var index = 0; index < v.symbols.length; index++) {
                    var element = v.symbols[index];
                    if (element.name == funName) {
                        signature = this.createSignatureInformation(element, cIdex, element.name);
                        return;
                    }
                }
            });
        }
        return signature;
    }
}
exports.LuaSignatureHelpProvider = LuaSignatureHelpProvider;
//# sourceMappingURL=LuaSignatureHelpProvider.js.map
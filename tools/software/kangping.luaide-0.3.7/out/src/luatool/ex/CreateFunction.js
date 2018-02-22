"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const providerUtils_1 = require("../provider/providerUtils");
const ExtensionManager_1 = require("../ex/ExtensionManager");
const FunctionParameter_1 = require("../ex/Template/FunctionParameter");
const TokenInfo_1 = require("../TokenInfo");
function createFunction(e) {
    var editor = vscode.window.activeTextEditor;
    var functionName = editor.document.getText(editor.selection);
    functionName = functionName.trim();
    if (functionName == null || functionName == "") {
        vscode.window.showInformationMessage("未选择方法名!");
    }
    FunctionParameter_1.getFunctionParameter(function (args) {
        if (args == null)
            return;
        var document = editor.document;
        editor.edit(function (edit) {
            var tokens = providerUtils_1.ProviderUtils.getTokens(document, editor.selection.end);
            var position = null;
            if (tokens.length == 1) {
                position = new vscode.Position(tokens[0].line, 0);
            }
            var upToken = null;
            for (var index = tokens.length - 1; index >= 0; index--) {
                var currentToken = tokens[index];
                if (currentToken.type == TokenInfo_1.TokenTypes.Keyword && currentToken.value == "function") {
                    position = new vscode.Position(tokens[index].line, tokens[index].lineStart);
                    break;
                }
                if (upToken) {
                    if (upToken.type == TokenInfo_1.TokenTypes.Identifier) {
                        if (currentToken.type == TokenInfo_1.TokenTypes.Identifier ||
                            (currentToken.type == TokenInfo_1.TokenTypes.Punctuator &&
                                currentToken.value == ")")) {
                            position = new vscode.Position(upToken.line, 0);
                            break;
                        }
                    }
                }
                upToken = tokens[index];
            }
            var startIndex = 0;
            for (var index = tokens.length - 1; index >= 0; index--) {
                if (tokens[index].type == TokenInfo_1.TokenTypes.Keyword && tokens[index].value == "function") {
                    startIndex = tokens[index].range.start - 1;
                    break;
                }
            }
            var startCount = 0;
            var docText = document.getText();
            for (var index = startIndex; index >= 0; index--) {
                var char = docText.charAt(index);
                if (char == "\n") {
                    break;
                }
                startCount++;
            }
            startCount += 4;
            var text = ExtensionManager_1.ExtensionManager.em.templateManager.getTemplateText(1);
            // var insterText: string = text.replace("{$functionName}", functionName)
            //     .replace("{$time}", new Date().toISOString())
            var insterText = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.replaceConfigValue(text, null);
            insterText = insterText.replace(new RegExp("{functionName}", "gm"), functionName);
            insterText = insterText + "\r\n";
            var insterTexts = insterText.split("\r\n");
            var lineText = "";
            var tabCount = Math.ceil(startCount / 4);
            for (var j = 0; j < tabCount; j++) {
                if (insterTexts[i] != "") {
                    lineText += "\t";
                }
            }
            for (var i = 0; i < insterTexts.length; i++) {
                if (insterTexts[i] != "{paramdesc}") {
                    insterTexts[i] = lineText + insterTexts[i].trim();
                }
            }
            insterText = "\r\n";
            var j = 0;
            for (var i = 0; i < insterTexts.length; i++) {
                if (insterTexts[i] != "") {
                    if (j == 0) {
                        insterText += insterTexts[i];
                    }
                    else {
                        insterText += "\r\n" + insterTexts[i];
                    }
                    j++;
                }
            }
            var paramDescStr = "";
            var paramStr = "";
            for (var index = 0; index < args.length; index++) {
                var arg = args[index];
                paramDescStr += lineText + "--@" + arg + ": \n";
                paramStr += arg + " ,";
            }
            paramStr = paramStr.substring(0, paramStr.length - 2);
            paramDescStr = paramDescStr.substring(0, paramDescStr.length - 2);
            if (paramDescStr == "") {
                insterText = insterText.replace("{paramdesc}\r\n", "");
            }
            else {
                insterText = insterText.replace(new RegExp("{paramdesc}", "gm"), paramDescStr);
            }
            insterText = insterText.replace(new RegExp("{param}", "gm"), paramStr);
            edit.insert(position, insterText);
        });
    });
}
exports.createFunction = createFunction;
//# sourceMappingURL=CreateFunction.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const providerUtils_1 = require("../provider/providerUtils");
const LuaParse_1 = require("../LuaParse");
const ExtensionManager_1 = require("../ex/ExtensionManager");
const FunctionParameter_1 = require("../ex/Template/FunctionParameter");
function createModuleFunction(e) {
    var editor = vscode.window.activeTextEditor;
    var functionName = editor.document.getText(editor.selection);
    if (functionName == null || functionName == "") {
        vscode.window.showInformationMessage("未选择方法名!");
    }
    var document = editor.document;
    var tokens = providerUtils_1.ProviderUtils.getTokens(document, editor.selection.end);
    var moduleInfo = null;
    var startTokenIndex = 0;
    var moduleName = null;
    if (tokens.length > 0) {
        moduleInfo = providerUtils_1.ProviderUtils.getSelfToModuleNameAndStartTokenIndex(document.uri, tokens, LuaParse_1.LuaParse.lp);
        if (moduleInfo != null) {
            moduleName = moduleInfo.moduleName;
            startTokenIndex = moduleInfo.index;
        }
        else {
            moduleInfo = {};
        }
    }
    else {
        moduleInfo = {};
    }
    var range = moduleInfo.range;
    if (moduleName && moduleName != "") {
        inputFunctionName(editor, tokens, startTokenIndex, moduleName, range, functionName);
    }
    else {
        inputModuleName(editor, tokens, startTokenIndex, range, functionName);
    }
}
exports.createModuleFunction = createModuleFunction;
function inputModuleName(editor, tokens, startTokenIndex, range, functionName) {
    vscode.window.showInputBox({ prompt: "moduleName" }).then(moduleName => {
        moduleName = moduleName.trim();
        if (moduleName != "") {
            inputFunctionName(editor, tokens, startTokenIndex, moduleName, range, functionName);
        }
        else {
            inputModuleName(editor, tokens, startTokenIndex, range, functionName);
        }
    });
}
function inputFunctionName(editor, tokens, startTokenIndex, moduleName, range, functionName) {
    FunctionParameter_1.getFunctionParameter(function (args) {
        if (args == null)
            return;
        editor.edit(function (edit) {
            var position = null;
            if (range != null) {
                position = new vscode.Position(range.end.line, range.end.character);
            }
            else {
                if (tokens.length == 0) {
                    position = editor.selection.start;
                }
                else {
                    var token = tokens[startTokenIndex];
                    if (token.comments.length == 0) {
                        position = new vscode.Position(tokens[startTokenIndex].line - 1, tokens[startTokenIndex].lineStart);
                    }
                    else {
                        var luaComment = token.comments[0];
                        position = new vscode.Position(luaComment.range.start.line, 0);
                    }
                }
            }
            var text = ExtensionManager_1.ExtensionManager.em.templateManager.getTemplateText(0);
            // var insterText: string = text.replace("${moduleName}", moduleName).
            //     replace("${functionName}", functionName)
            //     .replace("${time}", new Date().toISOString())
            var insterText = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.replaceConfigValue(text, moduleName);
            insterText = insterText.replace(new RegExp("{functionName}", "gm"), functionName);
            var paramDescStr = "";
            var paramStr = "";
            for (var index = 0; index < args.length; index++) {
                var arg = args[index];
                paramDescStr += "--@" + arg + ": \n";
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
            edit.insert(position, "\r\n" + insterText + "\r\n");
            range = new vscode.Range(position, position);
            editor.revealRange(range, vscode.TextEditorRevealType.InCenter);
        });
    });
}
//# sourceMappingURL=CreateMoudleFunction.js.map
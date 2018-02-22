/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const LuaMode_1 = require("./luatool/provider/LuaMode");
const LuaCompletionItemProvider_1 = require("./luatool/provider/LuaCompletionItemProvider");
const LuaDefinitionProvider_1 = require("./luatool/provider/LuaDefinitionProvider");
const LuaDocumentSymbolProvider_1 = require("./luatool/provider/LuaDocumentSymbolProvider");
const LuaSignatureHelpProvider_1 = require("./luatool/provider/LuaSignatureHelpProvider");
const LuaFormattingEditProvider_1 = require("./luatool/provider/LuaFormattingEditProvider");
const LuaFileCompletionItems_1 = require("./luatool/manager/LuaFileCompletionItems");
var fs = require('fs');
const LuaParse_1 = require("./luatool/LuaParse");
const vscode = require("vscode");
const ExtensionManager_1 = require("./luatool/ex/ExtensionManager");
const AutoLuaComment_1 = require("./luatool/ex/AutoLuaComment");
const ConstInfo_1 = require("./ConstInfo");
let diagnosticCollection;
let currentDiagnostic;
function activate(context) {
    var luaCodeExtension = vscode.extensions.getExtension(ConstInfo_1.ConstInfo.extensionLuaCodeConfig);
    var luaIdeExtension = vscode.extensions.getExtension(ConstInfo_1.ConstInfo.extensionLuaIdeConfig);
    if ((luaCodeExtension != null && luaCodeExtension.isActive) || (luaIdeExtension != null && luaIdeExtension.isActive)) {
        return;
    }
    var em = new ExtensionManager_1.ExtensionManager(context);
    diagnosticCollection = vscode.languages.createDiagnosticCollection('lua');
    let luaParse = new LuaParse_1.LuaParse(diagnosticCollection);
    context.subscriptions.push(vscode.languages.registerCompletionItemProvider(LuaMode_1.LUA_MODE, new LuaCompletionItemProvider_1.LuaCompletionItemProvider(), '.', ":", '"', "[", "@"));
    context.subscriptions.push(vscode.languages.registerDefinitionProvider(LuaMode_1.LUA_MODE, new LuaDefinitionProvider_1.LuaDefinitionProvider()));
    context.subscriptions.push(vscode.languages.registerDocumentSymbolProvider(LuaMode_1.LUA_MODE, new LuaDocumentSymbolProvider_1.LuaDocumentSymbolProvider()));
    context.subscriptions.push(vscode.languages.registerSignatureHelpProvider(LuaMode_1.LUA_MODE, new LuaSignatureHelpProvider_1.LuaSignatureHelpProvider(), '(', ','));
    context.subscriptions.push(vscode.languages.registerDocumentFormattingEditProvider(LuaMode_1.LUA_MODE, new LuaFormattingEditProvider_1.LuaFormattingEditProvider()));
    context.subscriptions.push(vscode.languages.registerDocumentRangeFormattingEditProvider(LuaMode_1.LUA_MODE, new LuaFormattingEditProvider_1.LuaFormattingEditProvider()));
    context.subscriptions.push(diagnosticCollection);
    var uris = new Array();
    var index = 0;
    function parseLuaFile() {
        if (index >= uris.length) {
            vscode.window.showInformationMessage("check complete!");
            //  vscode.window.setStatusBarMessage("")
            em.barItem.text = "捐献(LuaIde)";
            return;
        }
        var uri = uris[index];
        var fileInfo = fs.statSync(uri.fsPath);
        var kbSize = fileInfo.size / 1024;
        if (kbSize > em.luaIdeConfigManager.maxFileSize) {
            index++;
            parseLuaFile();
            return;
        }
        if (uri.fsPath.toLowerCase().indexOf("filetemplates") > -1 || uri.fsPath.toLowerCase().indexOf("funtemplate") > -1) {
            index++;
            parseLuaFile();
            return;
        }
        vscode.workspace.openTextDocument(uris[index]).then(doc => {
            em.barItem.text = uri.path;
            LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().addCompletion(uri, false);
            luaParse.Parse(uri, doc.getText());
            index++;
            parseLuaFile();
        }).then(function (event) {
            // console.log(event)
        }, function (reason) {
            // console.log(reason)
            index++;
            parseLuaFile();
        });
    }
    vscode.workspace.findFiles("**/*.lua", "", 10000).then(value => {
        if (value == null)
            return;
        let count = value.length;
        value.forEach(element => {
            uris.push(element);
        });
        //  console.log(uris.length)
        parseLuaFile();
    });
    vscode.workspace.onDidSaveTextDocument(event => {
        if (event.languageId == "lua") {
            var fileInfo = fs.statSync(event.uri.fsPath);
            var kbSize = fileInfo.size / 1024;
            if (kbSize > em.luaIdeConfigManager.maxFileSize) {
                return;
            }
            LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().addCompletion(event.uri, false);
            if (event.uri.fsPath.toLowerCase().indexOf("filetemplates") > -1 || event.uri.fsPath.toLowerCase().indexOf("funtemplate") > -1) {
                return;
            }
            var uri = event.fileName;
            luaParse.Parse(event.uri, event.getText());
        }
    });
    vscode.workspace.onDidChangeTextDocument(event => {
        var fileInfo = fs.statSync(event.document.uri.fsPath);
        var kbSize = fileInfo.size / 1024;
        if (kbSize > em.luaIdeConfigManager.maxFileSize) {
            return;
        }
        if (AutoLuaComment_1.AutoLuaComment.checkComment(event)) {
        }
        if (ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.changeTextCheck) {
            if (event.document.languageId == "lua") {
                if (event.document.uri.fsPath.toLowerCase().indexOf("filetemplates") > -1 || event.document.uri.fsPath.toLowerCase().indexOf("funtemplate") > -1) {
                    return;
                }
                var uri = event.document.fileName;
                luaParse.Parse(event.document.uri, event.document.getText(), false);
            }
        }
    });
}
exports.activate = activate;
//# sourceMappingURL=extension.js.map
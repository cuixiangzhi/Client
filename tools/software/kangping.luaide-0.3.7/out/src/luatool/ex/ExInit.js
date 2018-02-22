"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
const LuaCompletionItemProvider_1 = require("../provider/LuaCompletionItemProvider");
const LuaDefinitionProvider_1 = require("../provider/LuaDefinitionProvider");
const LuaDocumentSymbolProvider_1 = require("../provider/LuaDocumentSymbolProvider");
const LuaMode_1 = require("../provider/LuaMode");
const LuaSignatureHelpProvider_1 = require("../provider/LuaSignatureHelpProvider");
const LuaFormattingEditProvider_1 = require("../provider/LuaFormattingEditProvider");
const ExtensionManager_1 = require("./ExtensionManager");
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
const AutoLuaComment_1 = require("./AutoLuaComment");
var fs = require('fs');
function initLuaIdeEx(context) {
    var diagnosticCollection = vscode.languages.createDiagnosticCollection('lua');
    let luaParse = new LuaParse_1.LuaParse(diagnosticCollection);
    var em = ExtensionManager_1.ExtensionManager.em;
    var barItem = vscode.window.createStatusBarItem();
    barItem.show();
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
            LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().addCompletion(uri, false);
            luaParse.Parse(uri, doc.getText());
            index++;
            barItem.text = uri.path;
            parseLuaFile();
        }).then(function (event) {
        }, function (reason) {
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
            LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().addCompletion(event.uri, true);
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
        //--- 自动注释
        if (AutoLuaComment_1.AutoLuaComment.checkComment(event)) {
        }
        if (em.luaIdeConfigManager.changeTextCheck) {
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
exports.initLuaIdeEx = initLuaIdeEx;
//# sourceMappingURL=ExInit.js.map
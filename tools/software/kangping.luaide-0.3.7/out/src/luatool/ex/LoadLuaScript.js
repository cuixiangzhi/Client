"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
var fs = require('fs');
var path = require('path');
var os = require('os');
const ExtensionManager_1 = require("../ex/ExtensionManager");
function OpenLuaLuaScriptText(e) {
    var extensionPath = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.extensionPath;
    var srpitLuaPath = path.join(extensionPath, "Template", "LoadScript", "LoadScript.lua");
    return vscode.workspace.openTextDocument(srpitLuaPath).then(document => {
        return vscode.window.showTextDocument(document);
    }).then(editor => {
        return;
    }).then(() => {
    }, error => {
        console.log(error);
    });
}
exports.OpenLuaLuaScriptText = OpenLuaLuaScriptText;
function LoadLuaScriptFun() {
}
exports.LoadLuaScriptFun = LoadLuaScriptFun;
//# sourceMappingURL=LoadLuaScript.js.map
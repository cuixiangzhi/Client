"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
function toUpperCase(e) {
    runChangeCase(function (txt) {
        return txt.toUpperCase();
    });
}
exports.toUpperCase = toUpperCase;
function toLowerCase(e) {
    runChangeCase(function (txt) {
        return txt.toLowerCase();
    });
}
exports.toLowerCase = toLowerCase;
function runChangeCase(converTextFun) {
    var editor = vscode.window.activeTextEditor;
    var d = editor.document;
    var sel = editor.selections;
    editor.edit(function (edit) {
        // itterate through the selections and convert all text to Upper
        for (var x = 0; x < sel.length; x++) {
            var txt = d.getText(new vscode.Range(sel[x].start, sel[x].end));
            txt = converTextFun(txt);
            edit.replace(sel[x], txt);
        }
    });
}
//# sourceMappingURL=ChangeCaseExtension.js.map
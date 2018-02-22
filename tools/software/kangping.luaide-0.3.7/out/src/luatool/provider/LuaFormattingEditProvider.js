"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaFormatParseTool_1 = require("./format/LuaFormatParseTool");
var path = require('path');
var fs = require('fs');
var os = require('os');
class LuaFormattingEditProvider {
    provideDocumentFormattingEdits(document, options, token) {
        return this.provideDocumentRangeFormattingEdits(document, null, options, token);
    }
    provideDocumentRangeFormattingEdits(document, range, options, token) {
        if (range === null) {
            var start = new vscode.Position(0, 0);
            var end = new vscode.Position(document.lineCount - 1, document.lineAt(document.lineCount - 1).text.length);
            range = new vscode.Range(start, end);
        }
        var content = document.getText(range);
        var result = [];
        content = this.format(content);
        result.push(new vscode.TextEdit(range, content));
        return Promise.resolve(result);
    }
    format(content) {
        //    return LuaFormat(content)
        var luaFormatParseTool = new LuaFormatParseTool_1.LuaFormatParseTool(content);
        return luaFormatParseTool.formatComent;
    }
}
exports.LuaFormattingEditProvider = LuaFormattingEditProvider;
//# sourceMappingURL=LuaFormattingEditProvider.js.map
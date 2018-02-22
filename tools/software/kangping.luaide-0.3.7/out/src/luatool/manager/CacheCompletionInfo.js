"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_1 = require("vscode");
class CacheCompletionInfo {
    static getIns() {
        if (CacheCompletionInfo.ins == null) {
            CacheCompletionInfo.ins = new CacheCompletionInfo();
        }
        return CacheCompletionInfo.ins;
    }
    constructor() {
        this.infos = new Array();
    }
    getItem(item) {
        var newItem = null;
        if (this.infos.length == 0) {
            newItem = new vscode_1.CompletionItem(item.funLable, vscode_1.CompletionItemKind.Function);
        }
        else {
            newItem = this.infos.pop();
        }
        newItem.label = item.funLable;
        newItem.documentation = item.documentation;
        newItem.insertText = item.funvSnippetString == null ? item.funLable : item.funvSnippetString;
        return newItem;
    }
    pushItems(items) {
        items.forEach(v => {
            this.infos.push(v);
        });
    }
}
exports.CacheCompletionInfo = CacheCompletionInfo;
//# sourceMappingURL=CacheCompletionInfo.js.map
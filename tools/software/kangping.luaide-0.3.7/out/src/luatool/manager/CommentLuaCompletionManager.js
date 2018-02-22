"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_1 = require("vscode");
class CommentLuaCompletionManager {
    constructor() {
        this.items = new Array();
        var completions = [
            {
                name: "return",
                comment: "返回值注释 例:@return [com.app.Model.TestModel]",
                insertText: "return "
            },
            {
                name: "parentClass",
                comment: "module继承注释 例:@parentClass [com.app.Model.TestModel]",
                insertText: "parentClass "
            },
            {
                name: "valueReference",
                comment: "变量引用注释 例:@valueReference [com.app.Model.TestModel]",
                insertText: "valueReference "
            },
            {
                name: "desc",
                comment: "方法描述",
                insertText: "desc"
            },
        ];
        completions.forEach(v => {
            var item = new vscode_1.CompletionItem(v.name, vscode_1.CompletionItemKind.Property);
            item.documentation = v.comment;
            item.insertText = v.insertText;
            this.items.push(item);
        });
    }
    static getIns() {
        if (CommentLuaCompletionManager.ins == null) {
            CommentLuaCompletionManager.ins = new CommentLuaCompletionManager();
        }
        return CommentLuaCompletionManager.ins;
    }
}
exports.CommentLuaCompletionManager = CommentLuaCompletionManager;
//# sourceMappingURL=CommentLuaCompletionManager.js.map
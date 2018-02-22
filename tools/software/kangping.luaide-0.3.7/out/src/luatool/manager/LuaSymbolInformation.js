"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaFiledCompletionInfo_1 = require("../provider/LuaFiledCompletionInfo");
class LuaSymbolInformation extends vscode.SymbolInformation {
    constructor(name, kind, range, uri, containerName) {
        super(name, kind, range, uri, containerName);
        this.isLocal = false;
        this.uri_ = uri;
        this.range_ = range;
    }
    initArgs(args, comments) {
        if (args != null) {
            this.argLuaFiledCompleteInfos = new Array();
            for (var i = 0; i < args.length; i++) {
                var element = args[i];
                var completion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(element, vscode.CompletionItemKind.Variable, this.uri_, this.range_.start, false);
                if (comments) {
                    for (var index = 0; index < comments.length; index++) {
                        var comment = comments[index];
                        var argComment = "@" + element + ":";
                        var cindex = comment.content.indexOf(argComment);
                        if (cindex > -1) {
                            completion.documentation = comment.content.substring(cindex + argComment.length).trim();
                            break;
                        }
                    }
                }
                this.argLuaFiledCompleteInfos.push(completion);
            }
        }
    }
}
exports.LuaSymbolInformation = LuaSymbolInformation;
//# sourceMappingURL=LuaSymbolInformation.js.map
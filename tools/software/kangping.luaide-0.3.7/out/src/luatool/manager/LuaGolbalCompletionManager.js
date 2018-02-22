"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaFiledCompletionInfo_1 = require("../provider/LuaFiledCompletionInfo");
const vscode_1 = require("vscode");
class LuaGolbalCompletionManager {
    static setGolbalCompletion(completion) {
        completion.getItems().forEach((v, k) => {
            this.rootCompletion.addItemToGolbal(v);
        });
        // console.log(this.rootCompletion.items)
    }
    static clearGolbalCompletion(completion) {
        completion.getItems().forEach((v, k) => {
            this.rootCompletion.delItemToGolbal(v);
        });
    }
    static getCompletionByKeys(keys) {
        var item = this.rootCompletion;
        for (var index = 0; index < keys.length; index++) {
            var key = keys[index];
            item = item.getItemByKey(key, true);
            if (item == null) {
                break;
            }
            index++;
        }
        return item;
    }
}
LuaGolbalCompletionManager.rootCompletion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode_1.CompletionItemKind.Field, null, null, false);
exports.LuaGolbalCompletionManager = LuaGolbalCompletionManager;
//# sourceMappingURL=LuaGolbalCompletionManager.js.map
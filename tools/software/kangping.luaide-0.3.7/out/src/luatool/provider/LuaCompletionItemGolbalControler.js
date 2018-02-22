"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaParse_1 = require("../LuaParse");
class LuaCompletionItemGolbalControler {
    constructor() {
        this.luaInfoManager = LuaParse_1.LuaParse.lp.luaInfoManager;
    }
    static getIns() {
        if (LuaCompletionItemGolbalControler._LuaCompletionItemGolbalControler == null) {
            LuaCompletionItemGolbalControler._LuaCompletionItemGolbalControler = new LuaCompletionItemGolbalControler();
        }
        return LuaCompletionItemGolbalControler._LuaCompletionItemGolbalControler;
    }
    getItemByKeys(keys) {
        var items = this.getFirstItem(keys[0]);
        for (var index = 2; index < keys.length; index++) {
            var key = keys[index];
            items = this.getFindItemByKey(items, key);
            if (items.length == 0) {
                break;
            }
            index++;
        }
        return items;
    }
    getFindItemByKey(items, key) {
        var newitems = new Array();
        for (var index = 0; index < items.length; index++) {
            var item = items[index];
            var fitem = item.getItemByKey(key);
            if (fitem) {
                newitems.push(fitem);
            }
        }
        return newitems;
    }
    getFirstItem(key) {
        var items = new Array();
        this.luaInfoManager.fileCompletionItemManagers.forEach((v, k) => {
            var item = v.luaGolbalCompletionInfo.getItemByKey(key);
            if (item != null && item.isNewVar == true) {
                items.push(item);
                var funItem = v.luaFunCompletionInfo.getItemByKey(key, false);
                if (funItem != null) {
                    items.push(funItem);
                }
            }
        });
        return items;
    }
}
exports.LuaCompletionItemGolbalControler = LuaCompletionItemGolbalControler;
//# sourceMappingURL=LuaCompletionItemGolbalControler.js.map
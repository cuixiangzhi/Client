"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaFiledCompletionInfo_1 = require("../provider/LuaFiledCompletionInfo");
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
const LuaCompletionItemProviderUtils_1 = require("./LuaCompletionItemProviderUtils");
class LuaCompletionItemFunControler {
    constructor(luaCompletionItemControler) {
        this.luaInfoManager = LuaParse_1.LuaParse.lp.luaInfoManager;
        this.luaCompletionItemControler = luaCompletionItemControler;
        this.luaCompletionItemProviderUtils = LuaCompletionItemProviderUtils_1.LuaCompletionItemProviderUtils.getIns();
    }
    static getIns(luaCompletionItemControler) {
        if (LuaCompletionItemFunControler._LuaCompletionItemFunControler == null) {
            LuaCompletionItemFunControler._LuaCompletionItemFunControler = new LuaCompletionItemFunControler(luaCompletionItemControler);
        }
        return LuaCompletionItemFunControler._LuaCompletionItemFunControler;
    }
    /**
    * local data = model:getInfo()
    * 获取一个变量是一个方法的返回值
    */
    getReferenceCompletionFunValue(item) {
        if (item.referenceCompletionFunKeys) {
            var requireReferenceItems = new Array();
            item.referenceCompletionFunKeys.forEach((v, k) => {
                var keys = new Array();
                for (var index = 0; index < v.length - 1; index++) {
                    var key = v[index];
                    keys.push(key);
                }
                var valueFunName = v[v.length - 1];
                var funNames = this.luaCompletionItemControler.getFunNames(k);
                var citems = new Array();
                if (keys.length > 0) {
                    // if (funNames.length == 0 && funNames[0] == "__g__") {
                    //     //全局方法
                    // } else {
                    this.luaCompletionItemControler.getLuaCompletionsByKeysAndFunNames(item.uri, keys.reverse(), funNames, citems, false);
                    // }
                    if (valueFunName == "new") {
                        if (citems.length > 0) {
                            var newRootCompletionInfo = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode.CompletionItemKind.Class, citems[0].uri, citems[0].position, false);
                            citems.forEach(element => {
                                newRootCompletionInfo.addItem(element);
                            });
                            requireReferenceItems.push(newRootCompletionInfo);
                        }
                    }
                    else {
                        citems.forEach(v1 => {
                            if (v1.label == valueFunName) {
                                // if(v1.kind == CompletionItemKind.Function){
                                this.getFunctionReturnCompletionKeys(v1, requireReferenceItems);
                                // }
                            }
                        });
                    }
                }
                else {
                    var funItem = this.getFunByfunName(valueFunName, item, funNames);
                    if (funItem != null) {
                        this.getFunctionReturnCompletionKeys(funItem, requireReferenceItems);
                    }
                }
            });
            return requireReferenceItems;
        }
        return null;
    }
    getFunctionReturnCompletionGolbalByKey(key, item) {
        //现在本文件中找 如果本文件中没有找到那么就全局找  
        var fcim = this.luaInfoManager.getFcimByPathStr(item.uri.path);
        if (fcim == null)
            return;
        //如果找到多个 那么就直接忽略
    }
    /**
    * 获取方法的返回值
    */
    getFunctionReturnCompletionKeys(item, items) {
        if (item.funAnnotationReturnValue) {
            var fitems = this.luaCompletionItemProviderUtils.getCompletionByModulePath(item.funAnnotationReturnValue);
            this.luaCompletionItemProviderUtils.mergeItems(items, fitems);
        }
        else if (item.functionReturnCompletionKeys) {
            var citems = new Array();
            item.functionReturnCompletionKeys.forEach((v, k) => {
                var funNames = this.luaCompletionItemControler.getFunNames(k);
                var keys = new Array();
                if (v.length == 1) {
                    keys.push(v[0]);
                }
                else {
                    for (var index = 0; index < v.length - 1; index++) {
                        keys.push(v[index]);
                    }
                }
                var keyName = v[v.length - 1];
                // keys.push(".")
                this.luaCompletionItemControler.getLuaCompletionsByKeysAndFunNames(item.uri, keys.reverse(), funNames, citems, false);
                citems.forEach(element => {
                    if (element.label == keyName) {
                        var reItems = this.luaCompletionItemControler.checkReferenceValue(element);
                        this.luaCompletionItemProviderUtils.mergeItems(items, reItems);
                        items.push(element);
                    }
                });
            });
        }
        return items;
    }
    getFunByfunName(functionName, item, functionNames) {
        var fcim = this.luaInfoManager.getFcimByPathStr(item.uri.path);
        if (fcim == null)
            return;
        functionNames = functionNames.reverse();
        for (var index = 0; index < functionNames.length; index++) {
            var fname = functionNames[index];
            var fitem = fcim.luaFunFiledCompletions.get(fname);
            if (fitem != null) {
                var targetItem = fitem.getItemByKey(functionName);
                if (targetItem != null) {
                    return targetItem;
                }
            }
        }
        //全局查找
        this.luaInfoManager.fileCompletionItemManagers.forEach((v, k) => {
            var targetItem = v.luaFunCompletionInfo.getItemByKey(functionName);
            if (targetItem != null) {
                return targetItem;
            }
        });
    }
}
exports.LuaCompletionItemFunControler = LuaCompletionItemFunControler;
//# sourceMappingURL=LuaCompletionItemFunControler.js.map
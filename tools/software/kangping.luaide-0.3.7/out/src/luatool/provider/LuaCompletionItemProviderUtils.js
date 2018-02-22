"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
const LuaParse_1 = require("../LuaParse");
class LuaCompletionItemProviderUtils {
    static getIns() {
        if (LuaCompletionItemProviderUtils._LuaCompletionItemProviderUtils == null) {
            LuaCompletionItemProviderUtils._LuaCompletionItemProviderUtils = new LuaCompletionItemProviderUtils();
        }
        return LuaCompletionItemProviderUtils._LuaCompletionItemProviderUtils;
    }
    constructor() {
        this.luaFileCompletionItems = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems();
    }
    /**
    * 根据路径获取completions
    */
    getCompletionByModulePath(modulePath) {
        var ritems = new Array();
        var uri = this.luaFileCompletionItems.getUriCompletionByModuleName(modulePath);
        if (uri) {
            var referenceCompletionManager = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
            var items = new Array();
            if (referenceCompletionManager.rootCompletionInfo != null)
                items.push(referenceCompletionManager.rootCompletionInfo);
            if (referenceCompletionManager.rootFunCompletionInfo != null)
                items.push(referenceCompletionManager.rootFunCompletionInfo);
            return items;
        }
        else {
            var moduleInfos = modulePath.split('.');
            if (moduleInfos.length > 0) {
                return this.getItemsByModuleName(moduleInfos[0]);
            }
        }
        return null;
    }
    getItemsByModuleName(moduleName) {
        var reItems = new Array();
        var lfcis = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems();
        var paths = lfcis.getUrisByModuleName(moduleName);
        if (paths) {
            for (var index = 0; index < paths.length; index++) {
                var fcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(paths[index]);
                if (fcim.rootCompletionInfo) {
                    reItems.push(fcim.rootCompletionInfo);
                }
                else {
                    var item = fcim.luaGolbalCompletionInfo.getItemByKey(moduleName, true);
                    if (item) {
                        reItems.push(item);
                    }
                    else {
                        LuaParse_1.LuaParse.lp.luaInfoManager.fileCompletionItemManagers.forEach((v, k) => {
                            var gitem = v.luaGolbalCompletionInfo.getItemByKey(moduleName, true);
                            if (gitem) {
                                if (reItems.indexOf(gitem) == -1) {
                                    reItems.push(gitem);
                                }
                            }
                        });
                    }
                }
                if (fcim.rootFunCompletionInfo) {
                    reItems.push(fcim.rootFunCompletionInfo);
                }
                else {
                    LuaParse_1.LuaParse.lp.luaInfoManager.fileCompletionItemManagers.forEach((v, k) => {
                        var gitem = v.luaFunCompletionInfo.getItemByKey(moduleName, true);
                        if (gitem) {
                            if (reItems.indexOf(gitem) == -1) {
                                reItems.push(gitem);
                            }
                        }
                    });
                }
            }
        }
        return reItems;
    }
    getParentItems(completion, parentCompletion) {
        if (completion.parentModulePath != null) {
            var uri = this.luaFileCompletionItems.getUriCompletionByModuleName(completion.parentModulePath);
            if (uri) {
                var referenceCompletionManager = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
                if (referenceCompletionManager.rootCompletionInfo) {
                    parentCompletion.push(referenceCompletionManager.rootCompletionInfo.getItems());
                    parentCompletion.push(referenceCompletionManager.rootFunCompletionInfo.getItems());
                    if (referenceCompletionManager.rootCompletionInfo.parentModulePath != null) {
                        this.getParentItems(referenceCompletionManager.rootCompletionInfo, parentCompletion);
                    }
                }
            }
        }
    }
    getParentItemByKey(completion, key) {
        var uri = this.luaFileCompletionItems.getUriCompletionByModuleName(completion.parentModulePath);
        if (uri) {
            var referenceCompletionManager = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
            if (referenceCompletionManager.rootCompletionInfo) {
                var item = referenceCompletionManager.rootCompletionInfo.getItemByKey(key);
                if (item) {
                    return item;
                }
                else {
                    if (referenceCompletionManager.rootCompletionInfo.parentModulePath != null) {
                        return this.getParentItemByKey(referenceCompletionManager.rootCompletionInfo, key);
                    }
                }
            }
        }
        return null;
    }
    /**
  * 合并两个list
  */
    mergeItems(items1, items2) {
        if (items2) {
            items2.forEach(item => {
                if (items1.indexOf(item) == -1) {
                    items1.push(item);
                }
            });
        }
        return items1;
    }
}
exports.LuaCompletionItemProviderUtils = LuaCompletionItemProviderUtils;
//# sourceMappingURL=LuaCompletionItemProviderUtils.js.map
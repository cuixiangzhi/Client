"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaParse_1 = require("../LuaParse");
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
class ReferenceValueCheck {
    static getReferenceValueCheck() {
        if (ReferenceValueCheck._ReferenceValueCheck == null) {
            ReferenceValueCheck._ReferenceValueCheck = new ReferenceValueCheck();
        }
        return ReferenceValueCheck._ReferenceValueCheck;
    }
    /**
     * 查找推断类型的引用的Completion 集合
     * @param currentUri
     * @param items
     * @param fcim
     */
    getRequireReferenceCompletion(currentUri, items, fcim) {
        var ritems = new Array();
        for (var index = 0; index < items.length; index++) {
            var item = items[index];
            if (item.uri.fsPath == currentUri.fsPath) {
                this.getReferenceValueByModulePath(ritems, item);
                this.getReferenceValueByKeys(ritems, item);
                this.getReferenceCompletionByKeys(ritems, item);
                this.getReferenceCompletionFunKeys(ritems, item);
            }
        }
        return ritems;
    }
    //根据返回值的keys 获取completion
    getReferenceCompletionFunKeys(ritems, item) {
        if (item.referenceCompletionFunKeys) {
        }
    }
    //require 根据模块路径获取LuaFiledCompletionInfo 集合
    getReferenceValueByModulePath(ritems, item) {
        if (item.requireReferencePath) {
            // for (var pathIndex = 0; pathIndex < item.requireReferencePath.length; pathIndex++) {
            //     var modulePath = item.requireReferencePath[pathIndex];
            //     var modelItems: Array<LuaFiledCompletionInfo> = this.getCompletionsByModulePath(modulePath)
            //     this.mergeItems(ritems, modelItems)
            // }
        }
    }
    /**
     * require 获取引用的Completion
     */
    getReferenceCompletionByKeys(ritems, item) {
        if (item.referenceCompletionKeys) {
            var curfcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(item.uri.path);
        }
    }
    /**
     * 根据keys 获取推断类型的引用的Completion 集合
     * @param keys
     * @param item
     */
    getReferenceValueByKeys(ritems, item) {
        if (item.requireReferenceFileds) {
        }
        return ritems;
    }
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
    getCompletionByKeys(completion, keys) {
        var index = 0;
        var length = keys.length;
        while (index < length) {
            var key = keys[index];
            completion = completion.getItemByKey(key);
            index += 2;
            if (completion) {
                continue;
            }
            else {
                index = 0;
                break;
            }
        }
        if (index >= length) {
            if (completion.completionStringValue) {
                return this.getCompletionsByModulePath(completion.completionStringValue);
            }
            else {
                var fcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(completion.uri.path);
                var items = this.getRequireReferenceCompletion(completion.uri, [completion], fcim);
                if (items == null || items.length == 0) {
                    items = new Array();
                    completion.items.forEach((v, k) => {
                        items.push(v);
                    });
                }
                return items;
            }
        }
        return null;
    }
    getCompletionsByModulePath(modulePath) {
        var ritems = new Array();
        var uri = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().getUriCompletionByModuleName(modulePath);
        if (uri) {
            var referenceCompletionManager = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
            if (referenceCompletionManager.rootCompletionInfo) {
                referenceCompletionManager.rootCompletionInfo.items.forEach((v, k) => {
                    ritems.push(v);
                });
            }
            if (referenceCompletionManager.rootFunCompletionInfo) {
                referenceCompletionManager.rootFunCompletionInfo.items.forEach((v, k) => {
                    ritems.push(v);
                });
            }
        }
        return ritems;
    }
}
exports.ReferenceValueCheck = ReferenceValueCheck;
//# sourceMappingURL=ReferenceValueCheck.js.map
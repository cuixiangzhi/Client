"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaCompletionItemGolbalControler_1 = require("../provider/LuaCompletionItemGolbalControler");
const LuaCompletionItemProviderUtils_1 = require("../provider/LuaCompletionItemProviderUtils");
const LuaParse_1 = require("../LuaParse");
const LuaGolbalCompletionManager_1 = require("../manager/LuaGolbalCompletionManager");
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
const LuaCompletionItemFunControler_1 = require("./LuaCompletionItemFunControler");
class LuaCompletionItemControler {
    constructor() {
        this.luaFileCompletionItems = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems();
        this.luaCompletionItemGolbalControler = LuaCompletionItemGolbalControler_1.LuaCompletionItemGolbalControler.getIns();
        this.luaCompletionItemProviderUtils = LuaCompletionItemProviderUtils_1.LuaCompletionItemProviderUtils.getIns();
        this.luaCompletionItemFunControler = LuaCompletionItemFunControler_1.LuaCompletionItemFunControler.getIns(this);
    }
    static getIns() {
        if (LuaCompletionItemControler._LuaCompletionItemControler == null) {
            LuaCompletionItemControler._LuaCompletionItemControler = new LuaCompletionItemControler();
        }
        return LuaCompletionItemControler._LuaCompletionItemControler;
    }
    /**
     * 根据keys 和 方法名称集合 获取对应的 LuaFiledCompletionInfo
     * @param keys
     * @param funNames
     */
    getLuaCompletionsByKeysAndFunNames(uri, keys, funNames, citems, isFirst) {
        var keys = keys.reverse();
        var fcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
        if (fcim == null)
            return;
        if (keys.length == 1) {
            if (funNames) {
                for (var index = 0; index < funNames.length; index++) {
                    var fname = funNames[index];
                    var funCompletionItem = fcim.luaFunFiledCompletions.get(fname);
                    if (funCompletionItem) {
                        this.getCompletionAloneByItemIndexOfKey(funCompletionItem, keys[0], citems);
                    }
                }
            }
            this.getCompletionAloneByItemIndexOfKey(fcim.luaFunCompletionInfo, keys[0], citems);
            this.getCompletionAloneByItemIndexOfKey(fcim.luaFileGolbalCompletionInfo, keys[0], citems);
            if (isFirst) {
                this.getCompletionAloneByItemIndexOfKey(LuaGolbalCompletionManager_1.LuaGolbalCompletionManager.rootCompletion, keys[0], citems);
            }
        }
        else {
            var keyStr = keys.join("");
            var rootItem = this.getFirstCompletionInfo(fcim, keys[0], funNames);
            var reItems = new Array();
            if (rootItem == null) {
                this.luaCompletionItemProviderUtils.mergeItems(reItems, this.luaCompletionItemProviderUtils.getItemsByModuleName(keys[0]));
            }
            else {
                reItems = this.checkReferenceValue(rootItem);
                if (reItems.length == 0) {
                    this.luaCompletionItemProviderUtils.mergeItems(reItems, this.luaCompletionItemProviderUtils.getItemsByModuleName(keys[0]));
                }
                if (reItems.indexOf(rootItem) == -1) {
                    reItems.push(rootItem);
                }
            }
            var gitems = this.luaCompletionItemGolbalControler.getFirstItem(keys[0]);
            if (gitems.length > 0) {
                this.luaCompletionItemProviderUtils.mergeItems(reItems, gitems);
            }
            if (reItems.length > 0) {
                var index = 3;
                while (index < keys.length) {
                    var key = keys[index - 1];
                    if (key == null) {
                        break;
                    }
                    var tempItems = new Array();
                    reItems.forEach(item => {
                        var tItem = item.getItemByKey(key, true);
                        if (tItem == null) {
                            tItem = this.luaCompletionItemProviderUtils.getParentItemByKey(item, key);
                        }
                        if (tItem) {
                            tempItems.push(tItem);
                            var refItems = this.checkReferenceValue(tItem);
                            refItems.forEach(e => {
                                tempItems.push(e);
                            });
                        }
                    });
                    reItems = tempItems;
                    index += 2;
                }
                reItems.forEach(element => {
                    if (element) {
                        element.getItems().forEach((v, k) => {
                            if (citems.indexOf(v) == -1) {
                                citems.push(v);
                            }
                        });
                        //parent element  查找父类元素
                        var parentClassItems = new Array();
                        this.luaCompletionItemProviderUtils.getParentItems(element, parentClassItems);
                        for (var index = 0; index < parentClassItems.length; index++) {
                            var parentItems = parentClassItems[index];
                            parentItems.forEach((v, k) => {
                                if (citems.indexOf(v) == -1) {
                                    citems.push(v);
                                }
                            });
                        }
                        if (element.uri != null && element.uri.path == uri.path) {
                            var tempKeys = new Array();
                            var item = element;
                            while (true) {
                                tempKeys.push(item.label);
                                if (item.parent) {
                                    item = item.parent;
                                    if (item.completionFunName != null) {
                                        break;
                                    }
                                }
                                else {
                                    item = null;
                                }
                                if (item == null || item.label == "") {
                                    break;
                                }
                            }
                            var tempKeys = tempKeys.reverse();
                            var findKeys = new Array();
                            for (var k = 0; k < tempKeys.length; k++) {
                                findKeys.push(tempKeys[k]);
                                findKeys.push(".");
                            }
                            var findKeyStr = tempKeys[tempKeys.length - 1];
                            var ccitems = new Array();
                            var findKeysStr = findKeys.join("");
                            if (keyStr != findKeysStr) {
                                if (findKeys.length > 0) {
                                    this.getLuaCompletionsByKeysAndFunNames(element.uri, findKeys.reverse(), funNames, ccitems, false);
                                    ccitems.forEach(v => {
                                        if (citems.indexOf(v) == -1) {
                                            citems.push(v);
                                        }
                                    });
                                }
                            }
                        }
                    }
                });
            }
        }
    }
    /**
     * 根据方法名和keys 获得item的值
     */
    getReferenceValueByKeyAndFunName(rootItem, funName, keys) {
        //获取方法名字集合
        var funNames = this.getFunNames(funName);
        var fcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(rootItem.uri.path);
        var valueItem = null;
        funNames = funNames.reverse();
        var valueItems = new Array();
        if (funNames.length == 1 && funNames[0] == "__g__") {
            valueItem = this.getLuaCompletionByKeys(fcim.luaFileGolbalCompletionInfo, keys, true);
            if (valueItem == null) {
                valueItem = this.getLuaCompletionByKeys(fcim.luaGolbalCompletionInfo, keys, true);
            }
            if (valueItem) {
                valueItems.push(valueItem);
            }
        }
        else {
            for (var index = 0; index < funNames.length; index++) {
                var fname = funNames[index];
                var funLuaCompletion = fcim.luaFunFiledCompletions.get(funName);
                if (funLuaCompletion) {
                    valueItem = this.getLuaCompletionByKeys(funLuaCompletion, keys, false);
                    if (valueItem) {
                        valueItems.push(valueItem);
                        break;
                    }
                }
                else {
                    break;
                }
            }
            //如果在方法内没找到那么就找
            valueItem = this.getLuaCompletionByKeys(fcim.luaFileGolbalCompletionInfo, keys, true);
            if (valueItem == null) {
                valueItem = this.getLuaCompletionByKeys(fcim.luaGolbalCompletionInfo, keys, true);
            }
            if (valueItem) {
                valueItems.push(valueItem);
            }
        }
        //如果还没找到 就全部文件中找  这里采用一点小技巧 这里先去找keys[0] 值检查下
        //有没有对应的文件 如果没有再进行全局检查
        // if (valueItem == null) {
        this.luaCompletionItemProviderUtils.mergeItems(valueItems, this.luaCompletionItemProviderUtils.getItemsByModuleName(keys[0]));
        // }
        // if (valueItem == null) {
        var fileCompletionItemManagers = LuaParse_1.LuaParse.lp.luaInfoManager.fileCompletionItemManagers;
        for (var info of fileCompletionItemManagers) {
            valueItem = this.getLuaCompletionByKeys(info[1].luaGolbalCompletionInfo, keys, true);
            if (valueItem != null) {
                valueItems.push(valueItem);
            }
        }
        // }
        return valueItems;
    }
    /**
     * 根据keys 获得具体的 item
     * @isValue 是否是需要检查有值的
     */
    getLuaCompletionByKeys(item, keys, isValue) {
        var index = 0;
        while (index < keys.length) {
            item = item.getItemByKey(keys[index]);
            if (item == null) {
                break;
            }
            index += 2;
        }
        // if (item != null && isValue) {
        //     if (item.requireReferencePath ||
        //         item.requireReferenceFileds ||
        //         item.referenceCompletionKeys ||
        //         item.referenceCompletionFunKeys ||
        //         item.completionStringValue) {
        //         return item
        //     } else {
        //         return null
        //     }
        // } else {
        //     return item
        // }
        return item;
    }
    checkReferenceValue(item) {
        var reItems = new Array();
        //require("xxx.xxx.xx")
        if (item.valueReferenceModulePath == null) {
            var rferenceItems = null;
            if (item.requireReferencePath) {
                for (var index = 0; index < item.requireReferencePath.length; index++) {
                    var path = item.requireReferencePath[index];
                    var items = this.luaCompletionItemProviderUtils.getCompletionByModulePath(path);
                    items.forEach(e => {
                        reItems.push(e);
                    });
                }
            }
            rferenceItems = this.getRferenceFiledsValue(item);
            this.luaCompletionItemProviderUtils.mergeItems(reItems, rferenceItems);
        }
        else {
            var items = this.luaCompletionItemProviderUtils.getCompletionByModulePath(item.valueReferenceModulePath);
            items.forEach(e => {
                reItems.push(e);
            });
        }
        rferenceItems = this.getReferenceCompletionValue(item);
        this.luaCompletionItemProviderUtils.mergeItems(reItems, rferenceItems);
        rferenceItems = this.luaCompletionItemFunControler.getReferenceCompletionFunValue(item);
        this.luaCompletionItemProviderUtils.mergeItems(reItems, rferenceItems);
        return reItems;
    }
    /**
     *  //require(mopath.path1)
     * 查找require 的返回值 路径为一个变量
    */
    getRferenceFiledsValue(item) {
        if (item.requireReferenceFileds) {
            var requireReferenceItems = new Array();
            item.requireReferenceFileds.forEach((v, k) => {
                var ritems = this.getReferenceValueByKeyAndFunName(item, k, v);
                if (ritems) {
                    this.luaCompletionItemProviderUtils.mergeItems(requireReferenceItems, ritems);
                }
            });
            var items = this.getRferenceFiledsStr(requireReferenceItems);
            return items;
        }
        return null;
    }
    /**
     * 查找require 的返回值 路径为一个变量 的变量值
     */
    getRferenceFiledsStr(requireReferenceItems) {
        var reItems = new Array();
        for (var index = 0; index < requireReferenceItems.length; index++) {
            var element = requireReferenceItems[index];
            if (element.completionStringValue) {
                var items = this.luaCompletionItemProviderUtils.getCompletionByModulePath(element.completionStringValue);
                if (items) {
                    items.forEach(e => {
                        reItems.push(e);
                    });
                }
            }
            else {
                //如果不是需要再进行查找
                var valueItems = this.checkReferenceValue(element);
                if (valueItems) {
                    var items = this.getRferenceFiledsStr(valueItems);
                    this.luaCompletionItemProviderUtils.mergeItems(reItems, items);
                }
            }
        }
        return reItems;
    }
    /**
     *  local data = data1
     * 获取 一个变量是另外一个变量赋值的 变量集合
     */
    getReferenceCompletionValue(item) {
        // if(item.referenceCompletionKeys){
        if (item.referenceCompletionKeys) {
            var requireReferenceItems = new Array();
            item.referenceCompletionKeys.forEach((v, k) => {
                var keys = new Array();
                for (var index = 0; index < v.length - 1; index++) {
                    var key = v[index];
                    keys.push(key);
                }
                if (keys.length == 0) {
                    keys = v;
                }
                var valueFunName = v[v.length - 1];
                var funNames = this.getFunNames(k);
                var citems = new Array();
                this.getLuaCompletionsByKeysAndFunNames(item.uri, keys.reverse(), funNames, citems, false);
                citems.forEach(v1 => {
                    if (v1.label == valueFunName) {
                        requireReferenceItems.push(v1);
                    }
                });
                // var ritems: Array<LuaFiledCompletionInfo> = this.getReferenceValueByKeyAndFunName(item, k, v)
                // if (ritems) {
                //     ritems.forEach(v => {
                //         var rItems1 = this.checkReferenceValue(v)
                //         if (rItems1) {
                //             this.mergeItems(requireReferenceItems, rItems1)
                //         }
                //     })
                //     this.mergeItems(requireReferenceItems, ritems)
                // }
            });
            return requireReferenceItems;
        }
        return null;
    }
    getFirstCompletionInfo(fcim, key, funNames) {
        //先
        //  LuaParse.lp.luaInfoManager.fileCompletionItemManagers.forEach((v,f)=>{
        //     v.luaGolbalCompletionInfo
        //  })
        for (var index = 0; index < funNames.length; index++) {
            var funName = funNames[index];
            var funCompletionItem = fcim.luaFunFiledCompletions.get(funName);
            if (funCompletionItem) {
                //找到root 的item
                var item = funCompletionItem.getItemByKey(key, false);
                if (item) {
                    return item;
                }
            }
        }
        var item = fcim.luaFileGolbalCompletionInfo.getItemByKey(key, true);
        if (item == null) {
            item = fcim.luaGolbalCompletionInfo.getItemByKey(key, true);
        }
        if (item) {
            return item;
        }
    }
    getCompletionByItemIndexOfKey(fcim, key, funNames) {
        return null;
    }
    /**
     * 获得不重复的的 Completion
     * @param item
     * @param key
     * @param cItems
     */
    getCompletionAloneByItemIndexOfKey(item, key, cItems) {
        key = key.toLowerCase();
        item.lowerCaseItems.forEach((v) => {
            if (cItems.indexOf(v) == -1) {
                cItems.push(v);
            }
        });
    }
    getFunNames(funName) {
        var funNames = new Array();
        if (funName.indexOf("->") > -1) {
            var fnames = funName.split("->");
            for (var index = 0; index < fnames.length; index++) {
                var fname = "";
                for (var j = 0; j <= index; j++) {
                    fname += fnames[j] + "->";
                }
                fname = fname.substring(0, fname.length - 2);
                funNames.push(fname);
            }
        }
        else {
            funNames.push(funName);
        }
        return funNames;
    }
}
exports.LuaCompletionItemControler = LuaCompletionItemControler;
//# sourceMappingURL=LuaCompletionItemControler.js.map
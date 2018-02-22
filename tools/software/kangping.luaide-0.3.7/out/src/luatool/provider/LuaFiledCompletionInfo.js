"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_1 = require("vscode");
class LuaFiledCompletionInfo extends vscode_1.CompletionItem {
    //-----------------------------------------------------------------------
    constructor(label, kind, uri, position, isFun) {
        super(label, kind);
        this.isNewVar = false;
        this.isLocalFunction = null;
        //父类路径 这里没有存LuaFiledCompletionInfo 是因为变化较大 不利于存储
        this.parentModulePath = null;
        this.valueReferenceModulePath = null;
        //注释的方法返回值 优先于 functionReturnCompletionKeys
        this.funAnnotationReturnValue = null;
        this.completionFunName = null;
        //显示fun的全
        this.funLable = null;
        this.funvSnippetString = null;
        this.isFun = isFun;
        this.documentation = "";
        this.type = new Array();
        this.uri = uri;
        this.position = position;
        this.items = new Map();
        this.lowerCaseItems = new Map();
    }
    checkType(t) {
        for (var i = 0; i < this.type.length; i++) {
            if (this.type[i] == t) {
                return true;
            }
        }
        return false;
    }
    setType(t) {
        var isAdd = true;
        this.type.forEach(element => {
            if (element == t) {
                isAdd = false;
                return;
            }
        });
        if (isAdd) {
            this.type.push(t);
        }
    }
    getItems() {
        return this.items;
    }
    clearItems() {
        this.items.clear();
        this.lowerCaseItems.clear();
    }
    getItemByKey(key, islowerCase = false) {
        if (this.items == null) {
            return null;
        }
        if (this.items.has(key)) {
            return this.items.get(key);
        }
        else if (islowerCase) {
            key = key.toLocaleLowerCase();
            if (this.lowerCaseItems.has(key)) {
                return this.lowerCaseItems.get(key);
            }
        }
        else {
        }
        return null;
    }
    addItem(item) {
        item.parent = this;
        this.items.set(item.label, item);
        this.lowerCaseItems.set(item.label.toLocaleLowerCase(), item);
    }
    delItem(key) {
        if (this.items.has(key)) {
            this.items.delete(key);
            this.lowerCaseItems.delete(key.toLocaleLowerCase());
        }
    }
    delItemToGolbal(item) {
        var citem = this.getItemByKey(item.label);
        if (citem != null) {
            var path = item.uri.path;
            var count = 0;
            if (item.items.size == 0) {
                var index = citem.golbalUris.indexOf(item.uri.path);
                if (index > -1) {
                    citem.golbalUris.splice(index, 1);
                }
                if (citem.golbalUris.length <= 0) {
                    this.delItem(citem.label);
                }
            }
            else {
                item.items.forEach((v, k) => {
                    citem.delItemToGolbal(v);
                });
                if (citem.items.size == 0) {
                    this.delItem(citem.label);
                }
            }
        }
    }
    addItemToGolbal(item) {
        var citem = this.getItemByKey(item.label);
        if (citem == null) {
            var newItem = new LuaFiledCompletionInfo(item.label, item.kind, null, null, false);
            citem = newItem;
            this.addItem(newItem);
        }
        if (citem.golbalUris == null) {
            citem.golbalUris = new Array();
        }
        var count = 0;
        if (item.items.size == 0) {
            citem.golbalUris.push(item.uri.path);
        }
        else {
            item.items.forEach((v, k) => {
                citem.addItemToGolbal(v);
            });
        }
    }
    setCompletionStringValue(value) {
        this.completionStringValue = value;
    }
    //require 路径集合 字符串集合
    addRequireReferencePath(path) {
        if (path) {
            if (this.requireReferencePath == null) {
                this.requireReferencePath = new Array();
            }
            if (this.requireReferencePath.indexOf(path) == -1) {
                this.requireReferencePath.push(path);
            }
        }
    }
    //require 变量引用 用于查找 modulePath
    addRequireReferenceFileds(functionName, keys) {
        if (this.requireReferenceFileds == null) {
            this.requireReferenceFileds = new Map();
        }
        this.requireReferenceFileds.set(functionName, keys);
    }
    //引用其他的Completion 
    addReferenceCompletionKeys(functionName, keys) {
        if (this.referenceCompletionKeys == null) {
            this.referenceCompletionKeys = new Map();
        }
        if (keys != null) {
            this.referenceCompletionKeys.set(functionName, keys);
        }
    }
    //当前的值为一个方法返回值
    addReferenceCompletionFunKeys(functionName, keys) {
        if (this.referenceCompletionFunKeys == null) {
            this.referenceCompletionFunKeys = new Map();
        }
        if (keys != null) {
            this.referenceCompletionFunKeys.set(functionName, keys);
        }
    }
    //如果completion 为一个function 那么就记录他的返回值 当前的值为一个方法返回值
    addFunctionReturnCompletionKeys(functionName, keys) {
        if (this.functionReturnCompletionKeys == null) {
            this.functionReturnCompletionKeys = new Map();
        }
        if (keys != null) {
            this.functionReturnCompletionKeys.set(functionName, keys);
        }
    }
}
exports.LuaFiledCompletionInfo = LuaFiledCompletionInfo;
//# sourceMappingURL=LuaFiledCompletionInfo.js.map
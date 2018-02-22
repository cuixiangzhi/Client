"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const LuaParse_1 = require("./LuaParse");
const FileCompletionItemManager_1 = require("./manager/FileCompletionItemManager");
const Utils_1 = require("./Utils");
class FindCompletionInfo {
}
exports.FindCompletionInfo = FindCompletionInfo;
class LuaInfoManager {
    constructor() {
        this.fileCompletionItemManagers = new Map();
    }
    initKeyWrodCompletioins() {
    }
    setFcim(uri, fcim) {
        this.fileCompletionItemManagers.set(uri.path, fcim);
    }
    getFcim(uri) {
        var fcim = null;
        if (this.fileCompletionItemManagers.has(uri.path)) {
            fcim = this.fileCompletionItemManagers.get(uri.path);
        }
        return fcim;
    }
    getFcimByPathStr(path) {
        var fcim = null;
        if (this.fileCompletionItemManagers.has(path)) {
            fcim = this.fileCompletionItemManagers.get(path);
        }
        return fcim;
    }
    init(lp, uri, tempUri) {
        this.lp = lp;
        this.tokens = lp.tokens;
        this.currentFcim = new FileCompletionItemManager_1.FileCompletionItemManager(tempUri);
        this.fileCompletionItemManagers.set(uri.path, this.currentFcim);
    }
    addFunctionCompletionItem(luaInfo, token, functionEndToken) {
        this.currentFcim.addFunctionCompletion(this.lp, luaInfo, token, functionEndToken);
    }
    addCompletionItem(luaInfo, token) {
        var completion = this.currentFcim.addCompletionItem(this.lp, luaInfo, token, this.tokens, false, true);
        return completion;
    }
    addSymbol(luaInfo, token, functionEndToken, symolName) {
        this.currentFcim.addSymbol(this.lp, luaInfo, token, functionEndToken, symolName);
    }
    addGlogCompletionItems(items) {
        this.fileCompletionItemManagers.forEach((v, k) => {
            if (k != LuaParse_1.LuaParse.checkTempFilePath) {
                items.push(v.luaGolbalCompletionInfo);
            }
        });
        this.fileCompletionItemManagers.forEach((v, k) => {
            if (k != LuaParse_1.LuaParse.checkTempFilePath) {
                items.push(v.luaFunCompletionInfo);
            }
        });
    }
    getFunctionArgs(tokens, uri) {
        var fcim = this.getFcimByPathStr(uri.path);
        var funNames = Utils_1.getCurrentFunctionName(tokens);
        if (fcim == null) {
            return [];
        }
        return fcim.getSymbolArgsByNames(funNames);
    }
}
exports.LuaInfoManager = LuaInfoManager;
//# sourceMappingURL=LuaInfoManager.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaFiledCompletionInfo_1 = require("../provider/LuaFiledCompletionInfo");
const ExtensionManager_1 = require("../ex/ExtensionManager");
/**
 * 存储项目中的路径
 */
class LuaFileCompletionItems {
    static getLuaFileCompletionItems() {
        if (LuaFileCompletionItems._ins == null) {
            LuaFileCompletionItems._ins = new LuaFileCompletionItems();
        }
        return LuaFileCompletionItems._ins;
    }
    constructor() {
        this.completions = new Array();
        this.modulePaths = new Map();
    }
    getUriCompletionByModuleName(moduleName) {
        for (var index = 0; index < this.completions.length; index++) {
            var element = this.completions[index];
            if (moduleName == element.label) {
                return element.uri;
            }
        }
    }
    /**
     * 获取路径集合根据moduleName
     */
    getUrisByModuleName(moduleName) {
        var lowermoduleName = moduleName.toLowerCase();
        if (this.modulePaths.has(lowermoduleName)) {
            return this.modulePaths.get(lowermoduleName);
        }
        return null;
    }
    addCompletion(path, isCheck) {
        if (isCheck) {
            for (var index = 0; index < this.completions.length; index++) {
                var element = this.completions[index];
                if (element.uri.path == path.path) {
                    return;
                }
            }
        }
        var position = new vscode.Position(1, 1);
        var str = path.fsPath;
        var luaIndex = str.lastIndexOf(".lua");
        if (luaIndex > -1) {
            str = str.substring(0, luaIndex);
        }
        str = str.replace(/\\/g, "/");
        str = str.replace(new RegExp("/", "gm"), ".");
        var str_1 = str.toLowerCase();
        ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.scriptRoots.forEach(scriptPath => {
            var scriptPath_1 = scriptPath;
            var index = str_1.indexOf(scriptPath_1);
            if (index > -1) {
                var length = scriptPath_1.length;
                str = str.substring(index + length);
                if (str.charAt(0) == ".") {
                    str = str.substring(1);
                }
                var names = str.split(".");
                var moduleName = names[names.length - 1];
                var moduleNameLower = moduleName.toLowerCase();
                if (!this.modulePaths.has(moduleNameLower)) {
                    this.modulePaths.set(moduleNameLower, new Array());
                }
                var paths = this.modulePaths.get(moduleNameLower);
                if (paths.indexOf(path.path) == -1) {
                    paths.push(path.path);
                    var completion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(str, vscode.CompletionItemKind.Class, path, position, false);
                    this.completions.push(completion);
                }
            }
        });
    }
}
exports.LuaFileCompletionItems = LuaFileCompletionItems;
//# sourceMappingURL=LuaFileCompletionItems.js.map
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
const TokenInfo_1 = require("../TokenInfo");
const Utils_1 = require("../Utils");
const providerUtils_1 = require("../provider/providerUtils");
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
const LuaCompletionItemControler_1 = require("./LuaCompletionItemControler");
const CommentLuaCompletionManager_1 = require("../manager/CommentLuaCompletionManager");
const vscode_1 = require("vscode");
const CacheCompletionInfo_1 = require("../manager/CacheCompletionInfo");
class LuaCompletionItemProvider {
    provideCompletionItems(document, position, token) {
        return this.provideCompletionItemsInternal(document, position, token, vscode.workspace.getConfiguration('lua'));
    }
    checkFunReturnModule(line) {
        var line = line.trim();
        var commenstrs = ["--@valueReference", "--@parentClass", "--@return"];
        var commenstr = null;
        for (var index = 0; index < commenstrs.length; index++) {
            var cstr = commenstrs[index];
            var rindex = line.indexOf(cstr);
            if (rindex == 0) {
                return LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().completions;
            }
        }
        return null;
    }
    checkCommenLuaCompletion(line, document, position) {
        var line = line.trim();
        var commenstr = "--@";
        if (line == commenstr) {
            let lineText = document.lineAt(position.line).text;
            if (document.lineCount > position.line + 2) {
                var start = new vscode.Position(position.line + 1, 0);
                var end = new vscode.Position(document.lineCount, 200);
                var lp = LuaParse_1.LuaParse.lp;
                var lpt = LuaParse_1.LuaParse.lp.lpt;
                var tokens = new Array();
                lpt.Reset(document.getText(new vscode.Range(start, end)));
                var isFun = false;
                var isArgs = false;
                var index = 0;
                while (true) {
                    var token = lpt.lex();
                    if (token.error != null) {
                        break;
                    }
                    if (token.type == TokenInfo_1.TokenTypes.EOF) {
                        break;
                    }
                    if (index == 0) {
                        if (token.value == "local" && token.type == TokenInfo_1.TokenTypes.Keyword) {
                            token = lpt.lex();
                        }
                        if (token.value == "function" && token.type == TokenInfo_1.TokenTypes.Keyword) {
                            isFun = true;
                        }
                        else {
                            isFun = false;
                            break;
                        }
                    }
                    if (token.value == "(" && token.type == TokenInfo_1.TokenTypes.Punctuator) {
                        isArgs = true;
                        break;
                    }
                    index++;
                }
            }
            var args = new Array();
            if (isFun && isArgs) {
                while (true) {
                    var token = lpt.lex();
                    if (token.error != null) {
                        break;
                    }
                    if (token.value == ")" && token.type == TokenInfo_1.TokenTypes.Punctuator) {
                        break;
                    }
                    if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                        args.push(token.value);
                        token = lpt.lex();
                        if (token.error != null) {
                            break;
                        }
                        if (token.type == TokenInfo_1.TokenTypes.Punctuator && token.value == ",") {
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        break;
                    }
                }
            }
            var items = new Array();
            if (args.length > 0) {
                args.forEach(arg => {
                    var item = new vscode_1.CompletionItem(arg + " ", vscode_1.CompletionItemKind.Variable);
                    item.documentation = "参数:" + arg;
                    items.push(item);
                });
            }
            if (items.length > 0) {
                CommentLuaCompletionManager_1.CommentLuaCompletionManager.getIns().items.forEach(v => {
                    items.push(v);
                });
                return items;
            }
            else {
                return CommentLuaCompletionManager_1.CommentLuaCompletionManager.getIns().items;
            }
        }
        return null;
    }
    provideCompletionItemsInternal(document, position, token, config) {
        return new Promise((resolve, reject) => {
            let filename = document.fileName;
            let lineText = document.lineAt(position.line).text;
            var requireRuggestions = new Array();
            var suggestions = new Array();
            let lineTillCurrentPosition = lineText.substr(0, position.character);
            //提示return 返回值的
            var returnValueCompletions = this.checkCommenLuaCompletion(lineTillCurrentPosition, document, position);
            if (returnValueCompletions) {
                return resolve(returnValueCompletions);
            }
            returnValueCompletions = this.checkFunReturnModule(lineTillCurrentPosition);
            if (returnValueCompletions) {
                return resolve(returnValueCompletions);
            }
            var infos = this.getCurrentStrInfo(document, position);
            var tokens = null;
            if (infos != null && infos.length >= 2) {
                tokens = infos[1];
            }
            if (tokens == null)
                return resolve([]);
            if (lineTillCurrentPosition.indexOf("require") > -1) {
                var rstr = lineTillCurrentPosition.trim();
                var lastToken = tokens[tokens.length - 1];
                if (tokens == null) {
                    tokens = providerUtils_1.ProviderUtils.getTokens(document, position);
                }
                if (tokens.length >= 2) {
                    if (lastToken.value == "" || lastToken.value == '"') {
                        var rtoken = tokens[tokens.length - 2];
                        if (rtoken.type == TokenInfo_1.TokenTypes.Identifier &&
                            (rtoken.value == "require")) {
                            requireRuggestions = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().completions;
                        }
                        else {
                            if (tokens.length >= 3) {
                                var rtoken = tokens[tokens.length - 3];
                                if (rtoken.type == TokenInfo_1.TokenTypes.Identifier &&
                                    (rtoken.value == "require")) {
                                    requireRuggestions = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().completions;
                                }
                            }
                        }
                    }
                }
            }
            if (infos == null)
                return resolve(requireRuggestions);
            var functionNames = Utils_1.getCurrentFunctionName(tokens);
            //进行推断处理
            var keys = infos[0];
            if (keys.length == 0)
                return resolve(requireRuggestions);
            var citems = new Array();
            //  var keys = keys.reverse()
            LuaCompletionItemControler_1.LuaCompletionItemControler.getIns().getLuaCompletionsByKeysAndFunNames(document.uri, keys, functionNames, citems, true);
            var currentItems = this.getCurrentFileItems(document.uri, keys);
            var funItems = new Array();
            //清理一下 只保存一份 如果有方法优先方法
            var onlyItems = new Map();
            currentItems.forEach(v => {
                citems.push(v);
            });
            citems.forEach(v => {
                if (onlyItems.has(v.label)) {
                    var oldItem = onlyItems.get(v.label);
                    if (v.kind == vscode.CompletionItemKind.Function && oldItem.kind != vscode.CompletionItemKind.Function) {
                        var newFun = CacheCompletionInfo_1.CacheCompletionInfo.getIns().getItem(v);
                        funItems.push(newFun);
                        onlyItems.set(v.label, newFun);
                    }
                }
                else {
                    if (v.kind == vscode.CompletionItemKind.Function) {
                        var newFun = CacheCompletionInfo_1.CacheCompletionInfo.getIns().getItem(v);
                        funItems.push(newFun);
                        onlyItems.set(v.label, newFun);
                    }
                    else {
                        onlyItems.set(v.label, v);
                    }
                }
            });
            var argsItems;
            if (infos[0].length == 1) {
                argsItems = LuaParse_1.LuaParse.lp.luaInfoManager.getFunctionArgs(infos[1], document.uri);
            }
            if (argsItems) {
                argsItems.forEach(v => {
                    if (!onlyItems.has(v.label)) {
                        onlyItems.set(v.label, v);
                    }
                });
            }
            onlyItems.forEach((v1, k) => {
                suggestions.push(v1);
            });
            CacheCompletionInfo_1.CacheCompletionInfo.getIns().pushItems(funItems);
            funItems = null;
            return resolve(suggestions);
        });
    }
    getCurrentStrInfo(document, position) {
        var lp = LuaParse_1.LuaParse.lp;
        var start = new vscode.Position(0, 0);
        var lpt = LuaParse_1.LuaParse.lp.lpt;
        var tokens = new Array();
        lpt.Reset(document.getText(new vscode.Range(start, position)));
        while (true) {
            Utils_1.CLog();
            var token = lpt.lex();
            if (token.error != null) {
                return;
            }
            if (token.type == TokenInfo_1.TokenTypes.EOF) {
                break;
            }
            token.index = tokens.length;
            tokens.push(token);
        }
        //console.log("current:"+ tokens[tokens.length-1].value);
        var index = tokens.length - 1;
        var _GNumber = 0;
        var _MNumber = 0;
        var keys = new Array();
        var key = "";
        while (true) {
            Utils_1.CLog();
            if (index < 0)
                break;
            var token = tokens[index];
            if (lp.consume('.', token, TokenInfo_1.TokenTypes.Punctuator) ||
                lp.consume(':', token, TokenInfo_1.TokenTypes.Punctuator)) {
                keys.push(token.value);
                key = "";
                index--;
                continue;
            }
            if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                keys.push(token.value + key);
                key = "";
                index--;
            }
            if (index < 0)
                break;
            var nextToken = tokens[index];
            if (lp.consume(';', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                break;
            }
            else if (lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
                lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                keys.push(nextToken.value);
                key = "";
                index--;
                continue;
            }
            else if (lp.consume(')', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                if (token.type == TokenInfo_1.TokenTypes.Identifier)
                    break;
                var m_number = 1;
                var beginIndex = index - 1;
                while (true) {
                    Utils_1.CLog();
                    index--;
                    if (lp.consume('(', tokens[index], TokenInfo_1.TokenTypes.Punctuator)) {
                        m_number--;
                        if (m_number == 0) {
                            var leng = beginIndex - index;
                            index--;
                            key = "()";
                            break;
                        }
                    }
                    else if (lp.consume(')', tokens[index], TokenInfo_1.TokenTypes.Punctuator)) {
                        g_number++;
                    }
                }
                continue;
            }
            else if (lp.consume(']', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                if (token.type == TokenInfo_1.TokenTypes.Identifier)
                    break;
                var g_number = 1;
                var beginIndex = index - 1;
                while (true) {
                    Utils_1.CLog();
                    index--;
                    if (lp.consume('[', tokens[index], TokenInfo_1.TokenTypes.Punctuator)) {
                        g_number--;
                        if (g_number == 0) {
                            var leng = beginIndex - index;
                            index--;
                            key = "[]";
                            break;
                        }
                    }
                    else if (lp.consume(']', tokens[index], TokenInfo_1.TokenTypes.Punctuator)) {
                        g_number++;
                    }
                }
                continue;
            }
            else {
                break;
            }
        }
        var moduleName = "";
        if (keys.length >= 2) {
            if (keys[keys.length - 1] == "self" && (keys[keys.length - 2] == ':'
                || keys[keys.length - 2] == '.')) {
                //检查 function
                //找出self 代表的 模块名
                //向上找 function
                var data = Utils_1.getSelfToModuleName(tokens, lp);
                if (data) {
                    var moduleName = data.moduleName;
                    keys[keys.length - 1] = moduleName;
                }
            }
        }
        // console.log("moduleName:"+moduleName)
        return [keys, tokens];
    }
    findTokenByKey(keys, tokens) {
        var length = tokens.length;
        var key = keys[1];
        for (var index = 0; index < tokens.length; index++) {
            var element = tokens[index];
            if (key == element.value && element.type == TokenInfo_1.TokenTypes.Identifier) {
                if (length - 1 >= index + 1) {
                    var nextToken = tokens[index + 1];
                    if (nextToken.value == "=" && TokenInfo_1.TokenTypes.Punctuator == nextToken.type) {
                        var isEqual = true;
                        if (keys.length > 2) {
                            //向上找
                            for (var j = 2; j < keys.length; j++) {
                                if (index - 1 >= tokens.length) {
                                    isEqual = false;
                                    break;
                                }
                                if (tokens[index - 1].value != keys[j]) {
                                    isEqual = false;
                                    break;
                                }
                            }
                        }
                        else {
                            isEqual = true;
                        }
                        if (isEqual) {
                            // console.log()
                        }
                    }
                }
            }
        }
    }
    getCurrentFileItems(uri, keys) {
        var fcim = LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(uri.path);
        var items = new Array();
        var citems = new Array();
        if (fcim == null) {
            return citems;
        }
        if (keys.length > 1) {
            fcim.luaFunFiledCompletions.forEach((v, k) => {
                this.getCurrentFileItemByRoot(v, keys, 0, items);
            });
        }
        if (items.length > 0) {
            items.forEach(element => {
                var eitems = element.getItems();
                eitems.forEach((v, k) => {
                    citems.push(v);
                });
            });
        }
        return citems;
    }
    getCurrentFileItemByRoot(root, keys, index, items) {
        root = root.getItemByKey(keys[index]);
        if (root) {
            if (index == keys.length - 2) {
                items.push(root);
            }
            index += 2;
            if (index < keys.length - 1) {
                this.getCurrentFileItemByRoot(root, keys, index, items);
            }
        }
    }
}
exports.LuaCompletionItemProvider = LuaCompletionItemProvider;
//# sourceMappingURL=LuaCompletionItemProvider.js.map
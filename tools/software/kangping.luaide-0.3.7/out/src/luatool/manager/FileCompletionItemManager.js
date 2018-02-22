'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const vscode_1 = require("vscode");
const LuaParse_1 = require("../LuaParse");
const TokenInfo_1 = require("../TokenInfo");
const LuaFiledCompletionInfo_1 = require("../provider/LuaFiledCompletionInfo");
const vscode_2 = require("vscode");
const Utils_1 = require("../Utils");
const LuaSymbolInformation_1 = require("./LuaSymbolInformation");
const ExtensionManager_1 = require("../ex/ExtensionManager");
class FileCompletionItemManager {
    constructor(uri) {
        //当前解析方法名集合  在解析完毕后 会设置为null
        this.currentFunctionNames = null;
        this.currentFunctionParams = null;
        this.currentSymbolFunctionNames = null;
        //方法集合 用于 方法查找
        this.symbols = null;
        //临时值 解析完毕 会设置为null
        this.tokens = null;
        //解析0.2.3 之前会用到 后期 由luaFunFiledCompletions 替代
        // public luaFiledCompletionInfo: LuaFiledCompletionInfo = null;
        //记录当前文档的所有方法
        this.luaFunCompletionInfo = null;
        //全局变量提示 这里在0.2.2 中继续了细化 将文件的全局 和整体全局 区分开来   luaGolbalCompletionInfo 为总体全局
        //luaFileGolbalCompletionInfo 当前文件的全局变量   两者配合使用
        this.luaGolbalCompletionInfo = null;
        //文件分为的全局变量 存储
        this.luaFileGolbalCompletionInfo = null;
        //对每个方法中的的变量通过方法名进行存储 这样可以更加精确地进行提示 而不是整体对在体格completion中
        this.luaFunFiledCompletions = null;
        //当前方法的Completion 与luaFunFiledCompletions 配合
        this.currentFunFiledCompletion = null;
        //根据 文件中return 进行设置
        this.rootFunCompletionInfo = null;
        this.rootCompletionInfo = null;
        this.lp = LuaParse_1.LuaParse.lp;
        this.currentFunctionNames = new Array();
        this.currentSymbolFunctionNames = new Array();
        this.currentFunctionParams = new Array();
        this.uri = uri;
        this.luaFunCompletionInfo = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode_2.CompletionItemKind.Class, uri, null, false);
        this.luaGolbalCompletionInfo = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode_2.CompletionItemKind.Class, uri, null, false);
        this.luaFileGolbalCompletionInfo = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode_2.CompletionItemKind.Class, uri, null, false);
        this.symbols = new Array();
        this.luaFunFiledCompletions = new Map();
    }
    clear() {
        this.luaFunCompletionInfo.clearItems();
        this.luaGolbalCompletionInfo.clearItems();
        this.luaFileGolbalCompletionInfo.clearItems;
        this.currentFunctionParams = null;
        this.symbols = null;
        this.luaFunFiledCompletions.clear();
        this.rootCompletionInfo = null;
        this.rootFunCompletionInfo = null;
        this.currentFunFiledCompletion = null;
        this.tokens = null;
    }
    //设置根 用于类型推断
    setRootCompletionInfo(rootName) {
        this.rootCompletionInfo = this.luaFileGolbalCompletionInfo.getItemByKey(rootName);
        if (this.rootCompletionInfo == null) {
            this.rootCompletionInfo = this.luaGolbalCompletionInfo.getItemByKey(rootName);
        }
        this.rootFunCompletionInfo = this.luaFunCompletionInfo.getItemByKey(rootName);
    }
    /**
     * 添加方法开始 标记
     * @param funName 方法名称
     */
    setBeginFunName(funName, params) {
        var luaCompletion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo("", vscode_2.CompletionItemKind.Function, this.lp.tempUri, null, false);
        this.currentSymbolFunctionNames.push(funName);
        this.currentFunctionParams.push(params);
        if (this.currentFunFiledCompletion) {
            funName = this.currentFunFiledCompletion.label + "->" + funName;
            //记录父方法的根
            luaCompletion.funParentLuaCompletionInfo = this.currentFunFiledCompletion;
        }
        this.currentFunctionNames.push(funName);
        luaCompletion.label = funName;
        this.luaFunFiledCompletions.set(funName, luaCompletion);
        luaCompletion.completionFunName = funName;
        this.currentFunFiledCompletion = luaCompletion;
    }
    setEndFun() {
        this.currentSymbolFunctionNames.pop();
        this.currentFunctionNames.pop();
        this.currentFunctionParams.pop();
        if (this.currentFunctionNames.length > 0) {
            var funName = this.currentFunctionNames[this.currentFunctionNames.length - 1];
            this.currentFunFiledCompletion = this.luaFunFiledCompletions.get(funName);
        }
        else {
            this.currentFunFiledCompletion = null;
        }
    }
    addFunctionCompletion(lp, luaInfo, token, functionEndToken) {
        var symbol = this.addSymbol(lp, luaInfo, token, functionEndToken);
        var completion = this.addCompletionItem(lp, luaInfo, token, this.tokens, true);
        completion.completionFunName = symbol.name;
        var argsStr = "";
        var snippetStr = "";
        for (var index = 0; index < symbol.argLuaFiledCompleteInfos.length; index++) {
            var v = symbol.argLuaFiledCompleteInfos[index];
            argsStr += v.label + ",";
            snippetStr += "${" + (index + 1) + ":" + v.label + "},";
        }
        if (argsStr != "") {
            argsStr = argsStr.substring(0, argsStr.length - 1);
            snippetStr = snippetStr.substring(0, snippetStr.length - 1);
            snippetStr = completion.label + "(" + snippetStr + ")";
            var snippetString = new vscode.SnippetString(snippetStr);
            completion.funvSnippetString = snippetString;
        }
        var funLabelStr = completion.label + "(" + argsStr + ")";
        completion.funLable = funLabelStr;
        this.checkFunAnnotationReturnValue(completion);
        this.checkFunReturnValue(completion, token.index, functionEndToken.index);
    }
    checkValueReferenceValue(completion) {
        if (completion.comments == null) {
            return;
        }
        for (var index = 0; index < completion.comments.length; index++) {
            var element = completion.comments[index];
            var returnValue = "@valueReference";
            var num = element.content.indexOf(returnValue);
            if (num == 0) {
                var className = element.content.substring(returnValue.length).trim();
                if (className[0] == "[") {
                    var endIndex = className.indexOf("]");
                    className = className.substring(1, endIndex);
                    className = className.trim();
                    completion.valueReferenceModulePath = className;
                    break;
                }
            }
        }
    }
    //检查父类引用
    checkParentClassValue(completion) {
        if (completion.comments == null) {
            return;
        }
        for (var index = 0; index < completion.comments.length; index++) {
            var element = completion.comments[index];
            var returnValue = "@parentClass";
            var num = element.content.indexOf(returnValue);
            if (num == 0) {
                var className = element.content.substring(returnValue.length).trim();
                if (className[0] == "[") {
                    var endIndex = className.indexOf("]");
                    className = className.substring(1, endIndex);
                    className = className.trim();
                    completion.parentModulePath = className;
                }
            }
        }
    }
    //检查注释的返回值
    checkFunAnnotationReturnValue(completion) {
        if (completion.comments == null)
            return;
        for (var index = 0; index < completion.comments.length; index++) {
            var element = completion.comments[index];
            var returnValue = "@return";
            var num = element.content.indexOf(returnValue);
            if (num == 0) {
                var className = element.content.substring(returnValue.length).trim();
                if (className[0] == "[") {
                    var endIndex = className.indexOf("]");
                    className = className.substring(1, endIndex);
                    className = className.trim();
                    // console.log(className)
                    completion.funAnnotationReturnValue = className;
                }
            }
        }
    }
    //检查返回值
    checkFunReturnValue(completion, startIndex, endTokenIndex) {
        var index = startIndex;
        while (index < endTokenIndex) {
            var token = this.tokens[index];
            index++;
            if (token.type == TokenInfo_1.TokenTypes.Keyword && token.value == "return") {
                var info = this.getCompletionValueKeys(this.tokens, index);
                if (info) {
                    if (info == null || info.type == null) {
                        var xx = 1;
                    }
                    if (info.type == 1) {
                        completion.addFunctionReturnCompletionKeys(completion.completionFunName, info.keys);
                    }
                    else {
                    }
                }
            }
        }
    }
    getSymbolEndRange(functionName) {
        var symbol = null;
        var range;
        for (var i = 0; i < this.symbols.length; i++) {
            symbol = this.symbols[i];
            if (!symbol.isLocal) {
                if (symbol.name == functionName) {
                    var loc = new vscode_1.Position(symbol.location.range.end.line + 1, 0);
                    range = new vscode.Range(loc, loc);
                    break;
                }
            }
        }
        if (range == null && symbol != null) {
            var loc = new vscode_1.Position(symbol.location.range.end.line, symbol.location.range.end.character);
            range = new vscode.Range(loc, loc);
        }
        //没找到直接找最后
        return range;
    }
    getSymbolArgsByNames(funNames) {
        var argLuaFiledCompleteInfos = new Array();
        for (var i = 0; i < this.symbols.length; i++) {
            var symbol = this.symbols[i];
            for (var j = 0; j < funNames.length; j++) {
                var name = funNames[j];
                if (symbol.name == name) {
                    for (var k = 0; k < symbol.argLuaFiledCompleteInfos.length; k++) {
                        var alc = symbol.argLuaFiledCompleteInfos[k];
                        argLuaFiledCompleteInfos.push(alc);
                    }
                }
            }
        }
        return argLuaFiledCompleteInfos;
    }
    addSymbol(lp, luaInfo, token, functionEndToken, symolName) {
        var parentName = "";
        var tokens = lp.tokens;
        var starIndex = luaInfo.startToken.index;
        var endIndex = token.index;
        var label = "";
        var symbolInfo = new LuaSymbolInformation_1.LuaSymbolInformation(token.value, vscode_1.SymbolKind.Function, new vscode_1.Range(new vscode_1.Position(luaInfo.startToken.line, luaInfo.startToken.range.start), new vscode_1.Position(functionEndToken.line, token.range.end)), undefined, Utils_1.getFirstComments(luaInfo.getComments()));
        var nindex = token.index;
        while (true) {
            nindex--;
            var upToken = tokens[nindex];
            if (upToken == null) {
                break;
            }
            nindex--;
            if (lp.consume(':', upToken, TokenInfo_1.TokenTypes.Punctuator) ||
                lp.consume('.', upToken, TokenInfo_1.TokenTypes.Punctuator)) {
                var mtokenInfo = tokens[nindex];
                symbolInfo.name = mtokenInfo.value + upToken.value + symbolInfo.name;
            }
            else {
                break;
            }
        }
        if (symolName != null) {
            symbolInfo.name = symolName;
        }
        if (this.currentSymbolFunctionNames.length == 0) {
            symbolInfo.isLocal = false;
        }
        else {
            symbolInfo.isLocal = true;
            var functionName = "";
            this.currentSymbolFunctionNames.forEach(fname => {
                if (functionName == "") {
                    functionName = fname;
                }
                else {
                    functionName = functionName + "->" + fname;
                }
            });
            symbolInfo.name = functionName + "->" + symbolInfo.name;
        }
        symbolInfo.initArgs(luaInfo.params, luaInfo.getComments());
        this.symbols.push(symbolInfo);
        return symbolInfo;
    }
    findLastSymbol() {
        for (var i = this.symbols.length - 1; i > 0; i--) {
            var symbolInfo = this.symbols[i];
            if (!symbolInfo.location) {
                return symbolInfo;
            }
        }
        return null;
    }
    /**
     * 添加itemm
     */
    addCompletionItem(lp, luaInfo, token, tokens, isFun = false, isCheckValueRequire = false) {
        this.lp = lp;
        this.tokens = lp.tokens;
        // console.log("line:"+luaInfo.startToken.line)
        // console.log("line:"+luaInfo.startToken.value)
        var starIndex = luaInfo.startToken.index;
        var endIndex = token.index;
        var label = "";
        // console.log(starIndex,endIndex)
        if (starIndex == endIndex) {
            var singleToken = this.tokens[starIndex];
            if (singleToken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
                singleToken.type == TokenInfo_1.TokenTypes.BooleanLiteral ||
                singleToken.type == TokenInfo_1.TokenTypes.NilLiteral ||
                singleToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                // if(singleToken.type == TokenTypes.StringLiteral)
                // {
                //     var item:LuaFiledCompletionInfo = new LuaFiledCompletionInfo(singleToken.value,CompletionItemKind.Text,lp.currentUri,new Position(singleToken.line,singleToken.lineStart))
                //     this.luaFiledCompletionInfo.addItem(item)
                // }
                return;
            }
        }
        var stoken = this.tokens[starIndex];
        if (stoken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
            stoken.type == TokenInfo_1.TokenTypes.BooleanLiteral ||
            stoken.type == TokenInfo_1.TokenTypes.NilLiteral ||
            stoken.type == TokenInfo_1.TokenTypes.StringLiteral ||
            stoken.type == TokenInfo_1.TokenTypes.Punctuator) {
            return;
        }
        var startInfos = null;
        var infos = this.getCompletionKey(starIndex, endIndex);
        if (infos == null || infos.length == 0) {
            return null;
        }
        var isCheckParentPath = false;
        var forindex = 0;
        if (isFun) {
            startInfos = this.luaFunCompletionInfo;
            if (this.currentFunFiledCompletion != null) {
                startInfos = this.currentFunFiledCompletion;
            }
        }
        else if (this.currentFunctionNames.length == 0) {
            if (luaInfo.isLocal) {
                startInfos = this.luaFileGolbalCompletionInfo;
            }
            else {
                if (this.luaFileGolbalCompletionInfo.getItemByKey(infos[0].key) != null) {
                    startInfos = this.luaFileGolbalCompletionInfo;
                }
                else {
                    startInfos = this.luaGolbalCompletionInfo;
                }
            }
            isCheckParentPath = true;
        }
        else {
            var data = null;
            if (infos[0].key == "self") {
                data = Utils_1.getSelfToModuleName(this.tokens.slice(0, endIndex), this.lp);
            }
            if (data) {
                var moduleName = data.moduleName;
                //找到self 属于谁
                var golbalCompletion = this.luaFileGolbalCompletionInfo.getItemByKey(moduleName);
                if (golbalCompletion == null) {
                    golbalCompletion = this.luaGolbalCompletionInfo.getItemByKey(moduleName);
                }
                if (golbalCompletion == null) {
                    var keyToken = data.token;
                    golbalCompletion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(moduleName, infos[0].kind, lp.tempUri, new vscode.Position(keyToken.line, keyToken.lineStart), false);
                    this.luaGolbalCompletionInfo.addItem(golbalCompletion);
                }
                forindex = 1;
                startInfos = golbalCompletion;
            }
            else {
                startInfos = this.currentFunFiledCompletion;
                if (luaInfo.isLocal == false) {
                    var key = "";
                    if (infos.length > 0) {
                        key = infos[0].key;
                    }
                    var curName = "";
                    if (key != "") {
                        //判断是否为参数
                        for (var pi = 0; pi < this.currentFunctionParams.length; pi++) {
                            var argNames = this.currentFunctionParams[pi];
                            for (var ai = 0; ai < argNames.length; ai++) {
                                var paramsName = argNames[ai];
                                if (key == paramsName) {
                                    curName = this.currentFunctionNames[pi];
                                    break;
                                }
                            }
                            if (curName != "") {
                                break;
                            }
                        }
                    }
                    if (curName != "") {
                        startInfos = this.luaFunFiledCompletions.get(curName);
                    }
                    else {
                        while (true) {
                            var completion = startInfos.getItemByKey(infos[0].key);
                            if (completion == null) {
                                if (startInfos.funParentLuaCompletionInfo) {
                                    startInfos = startInfos.funParentLuaCompletionInfo;
                                }
                                else {
                                    if (this.luaFileGolbalCompletionInfo.getItemByKey(infos[0].key)) {
                                        startInfos = this.luaFileGolbalCompletionInfo;
                                    }
                                    else {
                                        startInfos = this.luaGolbalCompletionInfo;
                                    }
                                    break;
                                }
                            }
                            else {
                                break;
                            }
                        }
                    }
                }
            }
        }
        // var golbalCompletionIsNew:boolean = true
        // var isGolbal:boolean = false
        // //如果为全局变量那么检查下是不是赋值 如果不是直接返回
        // if (startInfos == this.luaGolbalCompletionInfo) {
        //     isGolbal = true
        //         var length = tokens.length
        //         var index = token.index + 1;
        //         var currentToken: TokenInfo = this.getValueToken(index, tokens)
        //         if (currentToken) {
        //             if (!(currentToken.type == TokenTypes.Punctuator && currentToken.value == "=")) {
        //                golbalCompletionIsNew = false
        //             }
        //         } else {
        //              golbalCompletionIsNew = false
        //         }
        // }
        // console.log(infos,"infos")
        for (var i = forindex; i < infos.length; i++) {
            var newStartInfos = null;
            var completion = startInfos.getItemByKey(infos[i].key);
            if (completion == null) {
                completion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(infos[i].key, infos[i].kind, lp.tempUri, infos[i].position, isFun);
                startInfos.addItem(completion);
                // completion.textEdit.newText = infos[i].insterStr;
                if (isFun) {
                    completion.documentation = Utils_1.getFirstComments(infos[i].comments);
                }
                else {
                    completion.documentation = infos[i].desc;
                }
                completion.comments = infos[i].comments;
                if (isCheckParentPath && infos.length == 1) {
                    this.checkParentClassValue(completion);
                }
            }
            else {
                if (infos[i].desc && completion.isFun == false) {
                    completion.documentation = infos[i].desc;
                }
            }
            if (i == infos.length - 1) {
                if (completion.isNewVar == false && endIndex + 2 < tokens.length) {
                    if (lp.consume("=", tokens[endIndex + 1], TokenInfo_1.TokenTypes.Punctuator)) {
                        completion.isNewVar = true;
                        completion.position = infos[i].position;
                    }
                }
            }
            completion.setType(infos[i].tipTipType);
            if (infos[i].nextInfo) {
                var nextInfo = infos[i].nextInfo;
                var nextCompletion = completion.getItemByKey(nextInfo.key);
                if (nextCompletion == null) {
                    nextCompletion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(nextInfo.key, nextInfo.kind, lp.tempUri, nextInfo.position, isFun);
                    nextCompletion.setType(1);
                    completion.addItem(nextCompletion);
                }
                else {
                    var xx = 1;
                }
                newStartInfos = nextCompletion;
            }
            else {
                newStartInfos = completion;
            }
            startInfos = newStartInfos;
        }
        if (luaInfo.type == TokenInfo_1.LuaInfoType.Function) {
            startInfos.params = luaInfo.params;
            startInfos.kind = vscode_2.CompletionItemKind.Function;
            startInfos.isLocalFunction = luaInfo.isLocal;
            var funKey = startInfos.label;
            if (this.currentFunFiledCompletion != null) {
                funKey = this.currentFunFiledCompletion.label + "->" + funKey;
            }
            if (this.luaFunFiledCompletions.has(funKey)) {
                this.luaFunFiledCompletions.get(funKey).isLocalFunction = startInfos.isLocalFunction;
            }
        }
        this.addTableFileds(luaInfo, startInfos, lp, isFun);
        if (isCheckValueRequire) {
            this.checkCompletionItemValueRequire(token, tokens, startInfos);
        }
        return startInfos;
    }
    addTableFileds(luaInfo, startInfos, lp, isFun) {
        //判断 luaInfo 
        if (luaInfo.tableFileds && luaInfo.tableFileds.length) {
            var tableFileds = luaInfo.tableFileds;
            tableFileds.forEach(filed => {
                if (!startInfos.getItemByKey(filed.name)) {
                    if (filed.tableFiledType == 0) {
                        var completion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(filed.name, vscode_2.CompletionItemKind.Field, lp.tempUri, new vscode.Position(filed.endToken.line, filed.endToken.lineStart), isFun);
                        startInfos.addItem(completion);
                        completion.setType(1);
                        this.addTableFileds(filed, completion, lp, isFun);
                    }
                    else {
                        var completion = new LuaFiledCompletionInfo_1.LuaFiledCompletionInfo(startInfos.label + filed.name, vscode_2.CompletionItemKind.Field, lp.tempUri, new vscode.Position(filed.startToken.line, filed.startToken.lineStart), isFun);
                        startInfos.parent.addItem(completion);
                        // if (startInfos.parent == this.luaFiledCompletionInfo) {
                        //     completion.setType(0)
                        // } else {
                        //     completion.setType(1)
                        // }
                        this.addTableFileds(filed, completion, lp, isFun);
                    }
                }
            });
        }
    }
    getCompletionKey(starIndex, endIndex) {
        // console.log("getCompletionKey")
        var infos = new Array();
        var key = "";
        // 1 为 .  2 为 :
        var tipType = 0;
        var comments = null;
        //获取注释
        while (true) {
            Utils_1.CLog();
            if (starIndex > endIndex)
                break;
            var keyToken = this.tokens[starIndex];
            if (keyToken.type == TokenInfo_1.TokenTypes.Keyword) {
                return infos = [];
            }
            if (comments == null) {
                //判断下 是不是function  和 local 
                if (starIndex - 1 >= 0) {
                    var upToken = this.tokens[starIndex - 1];
                    if (this.lp.consume('function', upToken, TokenInfo_1.TokenTypes.Keyword)) {
                        comments = upToken.comments;
                        if (starIndex - 2 >= 0) {
                            if (this.lp.consume('local', this.tokens[starIndex - 2], TokenInfo_1.TokenTypes.Keyword)) {
                                comments = this.tokens[starIndex - 2].comments;
                            }
                        }
                    }
                    else if (this.lp.consume('local', upToken, TokenInfo_1.TokenTypes.Keyword)) {
                        comments = upToken.comments;
                    }
                }
                else {
                    comments = keyToken.comments;
                }
            }
            var key = "";
            if (keyToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                key += '"' + this.tokens[starIndex].value + '"';
            }
            else {
                key += this.tokens[starIndex].value;
            }
            var simpleInfo = null;
            if (this.lp.consume('[', keyToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume('(', keyToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume(')', keyToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume(']', keyToken, TokenInfo_1.TokenTypes.Punctuator)) {
                break;
            }
            else {
                simpleInfo = new CompletionItemSimpleInfo(key, starIndex, vscode_2.CompletionItemKind.Field, tipType, new vscode.Position(keyToken.line, keyToken.range.start - keyToken.lineStart));
                infos.push(simpleInfo);
                starIndex++;
                if (starIndex > endIndex)
                    break;
                tipType = this.getTipType(starIndex);
                if (tipType != 0) {
                    starIndex++;
                    continue;
                }
            }
            // console.log(127);
            if (this.lp.consume('[', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                var g_number = 1;
                var beginIndex = starIndex + 1;
                while (true) {
                    Utils_1.CLog();
                    starIndex++;
                    if (this.lp.consume(']', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                        g_number--;
                        if (g_number == 0) {
                            var leng = starIndex - beginIndex;
                            var lastInfo = infos[infos.length - 1];
                            if (leng == 1) {
                                var stringToken = this.tokens[beginIndex];
                                var tokenValue = "";
                                if (stringToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                                    tokenValue = '"' + stringToken.value + '"';
                                    var nextSimpleInfo = new CompletionItemSimpleInfo(stringToken.value, starIndex, vscode_2.CompletionItemKind.Field, 1, new vscode.Position(stringToken.line, stringToken.range.start - stringToken.lineStart));
                                    lastInfo.nextInfo = nextSimpleInfo;
                                }
                                else if (stringToken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
                                    stringToken.type == TokenInfo_1.TokenTypes.BooleanLiteral ||
                                    stringToken.type == TokenInfo_1.TokenTypes.Identifier ||
                                    stringToken.type == TokenInfo_1.TokenTypes.VarargLiteral) {
                                }
                                else {
                                    // lastInfo.key = lastInfo.key + "[]"
                                }
                            }
                            else {
                                // lastInfo.key = lastInfo.key + "[]";
                            }
                            starIndex++;
                            break;
                        }
                    }
                    else if (this.lp.consume('[', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                        g_number++;
                    }
                }
                tipType = this.getTipType(starIndex);
                if (tipType != 0) {
                    starIndex++;
                    continue;
                }
            }
            else {
                var ss = 1;
            }
            if (starIndex > endIndex)
                break;
            if (this.lp.consume('(', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                //   simpleInfo.kind = CompletionItemKind.Function;
                var m_number = 1;
                while (true) {
                    Utils_1.CLog();
                    starIndex++;
                    if (this.lp.consume(')', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                        m_number--;
                        if (m_number == 0) {
                            // simpleInfo.key += "()";
                            starIndex++;
                            break;
                        }
                    }
                    else if (this.lp.consume('(', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
                        m_number++;
                    }
                }
                if (starIndex > endIndex) {
                    break;
                }
                tipType = this.getTipType(starIndex);
                if (tipType != 0) {
                    starIndex++;
                    continue;
                }
            }
            if (starIndex > endIndex) {
                break;
            }
        }
        if (infos.length > 0) {
            var simpleInfo = infos[infos.length - 1];
            var commentstr = Utils_1.getComments(comments);
            var skind = simpleInfo.kind;
            if (simpleInfo.nextInfo) {
                simpleInfo.kind = vscode_2.CompletionItemKind.Field;
                simpleInfo.nextInfo.kind = skind;
                simpleInfo.nextInfo.desc = commentstr;
                simpleInfo.comments = comments;
                // simpleInfo.nextInfo.key += "()"
            }
            else {
                simpleInfo.desc = commentstr;
                simpleInfo.comments = comments;
                // simpleInfo.key += "()"
            }
        }
        return infos;
    }
    getTipType(starIndex) {
        var tipType = 0;
        if (starIndex >= this.tokens.length)
            return tipType;
        var symbolToken = this.tokens[starIndex];
        if (this.lp.consume('.', symbolToken, TokenInfo_1.TokenTypes.Punctuator)) {
            tipType = 1;
        }
        else if (this.lp.consume(':', this.tokens[starIndex], TokenInfo_1.TokenTypes.Punctuator)) {
            tipType = 2;
        }
        return tipType;
    }
    getValueToken(index, tokens) {
        if (index < tokens.length) {
            return tokens[index];
        }
        else {
            return null;
        }
    }
    //检查 item赋值 require 路径
    checkCompletionItemValueRequire(endToken, tokens, completion) {
        if (completion.valueReferenceModulePath != null) {
            return;
        }
        var length = tokens.length;
        var index = endToken.index + 1;
        var currentToken = this.getValueToken(index, tokens);
        if (currentToken) {
            if (currentToken.type == TokenInfo_1.TokenTypes.Punctuator && currentToken.value == "=") {
                //优先注释
                this.checkValueReferenceValue(completion);
                if (completion.valueReferenceModulePath != null) {
                    return;
                }
                index++;
                var funNames = Utils_1.getCurrentFunctionName(this.tokens.slice(0, endToken.index));
                if (funNames.length == 0) {
                    //没有方法那么就是文件中的全局信息
                    funNames.push("__g__");
                }
                currentToken = this.getValueToken(index, tokens);
                if (currentToken == null) {
                    return;
                }
                if (currentToken.type == TokenInfo_1.TokenTypes.Identifier) {
                    if (ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.requireFunNames.indexOf(currentToken.value) > -1) {
                        //require 模式
                        index++;
                        currentToken = this.getValueToken(index, tokens);
                        if (currentToken) {
                            if (currentToken.type == TokenInfo_1.TokenTypes.Punctuator && currentToken.value == "(") {
                                index++;
                                currentToken = this.getValueToken(index, tokens);
                                if (currentToken != null) {
                                    if (currentToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                                        var pathValue = currentToken.value;
                                        completion.addRequireReferencePath(pathValue);
                                    }
                                    else if (currentToken.type == TokenInfo_1.TokenTypes.Identifier) {
                                        var keysInfo = this.getCompletionValueKeys(tokens, index);
                                        if (keysInfo) {
                                            var keys = keysInfo.keys;
                                            if (keys.length > 0) {
                                                completion.addRequireReferenceFileds(funNames[0], keys);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else {
                        var info = this.getCompletionValueKeys(tokens, index);
                        if (info) {
                            if (info.type == 1) {
                                completion.addReferenceCompletionKeys(funNames[0], info.keys);
                            }
                            else {
                                completion.addReferenceCompletionFunKeys(funNames[0], info.keys);
                            }
                        }
                    }
                }
                else if (currentToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                    completion.setCompletionStringValue(currentToken.value);
                }
            }
        }
    }
    /**
     * type == 1 字段
     * type == 2 方法
     */
    getCompletionValueKeys(tokens, index) {
        var keys = new Array();
        var keyToken = tokens[index];
        if (keyToken.type == TokenInfo_1.TokenTypes.Identifier) {
            keys.push(keyToken.value);
            if (keyToken.value == "self") {
                var info = Utils_1.getSelfToModuleName(tokens, LuaParse_1.LuaParse.lp);
                if (info) {
                    keys[0] = info.moduleName;
                }
            }
        }
        else {
            return null;
        }
        index++;
        while (index < tokens.length) {
            keyToken = tokens[index];
            if (keyToken.type == TokenInfo_1.TokenTypes.Punctuator) {
                if (keyToken.value == "." || keyToken.value == ":") {
                    keys.push(keyToken.value);
                }
                else if (keyToken.value == "(") {
                    //为一个方法
                    return {
                        type: 2,
                        keys: keys
                    };
                }
                else if (keyToken.value == ")") {
                    return {
                        type: 1,
                        keys: keys
                    };
                }
                else if (keyToken.value == ";") {
                    return {
                        type: 1,
                        keys: keys
                    };
                }
                else {
                    return null;
                }
            }
            else {
                return {
                    type: 1,
                    keys: keys
                };
            }
            index++;
            var keyToken = tokens[index];
            if (keyToken.type == TokenInfo_1.TokenTypes.Identifier) {
                keys.push(keyToken.value);
            }
            index++;
            if (index >= tokens.length) {
                return null;
            }
        }
        return null;
    }
    /**
     * 去除多余的completion
     */
    checkFunCompletion() {
        var items = this.luaFunCompletionInfo.getItems();
        items.forEach((funCompletion, k) => {
            //查找
            var gcompletion = this.luaFileGolbalCompletionInfo.getItemByKey(k);
            if (gcompletion) {
                if (funCompletion.getItems().size == 0 && gcompletion.getItems().size == 0) {
                    this.luaFileGolbalCompletionInfo.delItem(k);
                }
                else {
                    funCompletion.getItems().forEach((fc, k1) => {
                        gcompletion.delItem(k1);
                    });
                }
            }
            gcompletion = this.luaGolbalCompletionInfo.getItemByKey(k);
            if (gcompletion) {
                if (funCompletion.getItems().size == 0 && gcompletion.getItems().size == 0) {
                    this.luaFileGolbalCompletionInfo.delItem(k);
                }
                else {
                    funCompletion.getItems().forEach((fc, k1) => {
                        gcompletion.delItem(k1);
                    });
                }
            }
        });
    }
}
exports.FileCompletionItemManager = FileCompletionItemManager;
class CompletionItemSimpleInfo {
    constructor(key, endIndex, kind, tipTipType, position) {
        this.desc = null;
        this.nextInfo = null;
        this.isShow = true;
        this.position = position;
        this.key = key;
        this.tipTipType = tipTipType;
        this.endIndex11 = endIndex;
        this.kind = kind;
    }
}
exports.CompletionItemSimpleInfo = CompletionItemSimpleInfo;
//# sourceMappingURL=FileCompletionItemManager.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
const LuaFileCompletionItems_1 = require("../manager/LuaFileCompletionItems");
const TokenInfo_1 = require("../TokenInfo");
const Utils_1 = require("../Utils");
function byteOffsetAt(document, position) {
    var lineText = document.lineAt(position.line).text;
    if (lineText.trim().substring(0, 2) == "--") {
        return checkComment(lineText, position);
    }
    //检查是不是路径字符串
    var tempStr = lineText.substring(position.character);
    var endIndex = tempStr.indexOf('"');
    if (endIndex > -1) {
        var startStr = lineText.substring(0, position.character);
        var findex = startStr.lastIndexOf('"');
        if (findex > -1) {
            var moduleName = lineText.substring(findex + 1, endIndex + position.character);
            if (moduleName.length > 0) {
                var uri = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().getUriCompletionByModuleName(moduleName);
                if (uri) {
                    var location = new vscode.Location(uri, new vscode.Position(0, 0));
                    return location;
                }
            }
        }
    }
    let offset = document.offsetAt(position);
    let text = document.getText();
    let byteOffset = 0;
    var isFun = false;
    var nameChats = new Array();
    var luaManager = LuaParse_1.LuaParse.lp.luaInfoManager;
    var lp = LuaParse_1.LuaParse.lp;
    var tokens = Utils_1.getTokens(document, position);
    var isFun = false;
    var i = 0;
    var lashToken = null;
    if (tokens) {
        i = tokens.length - 1;
    }
    while (i >= 0) {
        Utils_1.CLog();
        var token = tokens[i];
        i--;
        if (lp.consume(':', token, TokenInfo_1.TokenTypes.Punctuator) ||
            lp.consume('.', token, TokenInfo_1.TokenTypes.Punctuator)) {
            if (i - 1 >= 0) {
                if (tokens[i].type == TokenInfo_1.TokenTypes.Identifier &&
                    lp.consume('function', tokens[i - 1], TokenInfo_1.TokenTypes.Keyword)) {
                    var posToken = tokens[i - 1];
                    var line = posToken.line;
                    return new vscode.Location(document.uri, new vscode.Position(line, 0));
                }
            }
        }
        if (lp.consume('function', token, TokenInfo_1.TokenTypes.Keyword)) {
            return null;
        }
        if (token.type == TokenInfo_1.TokenTypes.Keyword || lp.consume('(', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume(')', token, TokenInfo_1.TokenTypes.Punctuator)) {
            isFun = true;
            break;
        }
        else if (lp.consume('+', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('-', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('*', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('/', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('>', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('<', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('>=', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('<=', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('==', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('~=', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('=', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('#', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('}', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('{', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume(']', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('[', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume(',', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume(';', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('else', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('elseif', token, TokenInfo_1.TokenTypes.Punctuator)
            || lp.consume('do', token, TokenInfo_1.TokenTypes.Keyword)) {
            break;
        }
        nameChats.push(token.value);
        lashToken = token;
        if (i >= 0) {
            var nextToken = tokens[i];
            if (token.type == TokenInfo_1.TokenTypes.Identifier && (nextToken.type == TokenInfo_1.TokenTypes.Identifier ||
                nextToken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.Keyword ||
                nextToken.type == TokenInfo_1.TokenTypes.StringLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.NilLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.BooleanLiteral)) {
                break;
            }
        }
    }
    nameChats = nameChats.reverse();
    for (let i = offset; i < text.length; i++) {
        var chat = text.charCodeAt(i);
        if (Utils_1.isIdentifierPart(chat)) {
            nameChats.push(text[i]);
        }
        else if (text[i] == '=' ||
            text[i] == '==' ||
            text[i] == '~=' ||
            text[i] == ')' ||
            text[i] == ']' ||
            text[i] == '[' ||
            text[i] == '}' ||
            text[i] == '+' ||
            text[i] == '-' ||
            text[i] == '*' ||
            text[i] == '/' ||
            text[i] == '>' ||
            text[i] == '<' ||
            text[i] == '>=' ||
            text[i] == '<=') {
            break;
        }
        else {
            if (chat == 40) {
                isFun = true;
            }
            break;
        }
    }
    var n = "";
    nameChats.forEach(c => {
        n += c;
    });
    // console.log(n)
    //分割
    var keyNames = new Array();
    var tempNames = n.split('.');
    for (var i = 0; i < tempNames.length; i++) {
        if (i == tempNames.length - 1) {
            var tempNames1 = tempNames[tempNames.length - 1].split(':');
            for (var j = 0; j < tempNames1.length; j++) {
                keyNames.push(tempNames1[j]);
            }
        }
        else {
            keyNames.push(tempNames[i]);
        }
    }
    var isSelf = false;
    if (keyNames[0] == 'self') {
        var data = Utils_1.getSelfToModuleName(tokens, lp);
        keyNames[0] = data.moduleName;
        isSelf = true;
    }
    var location = null;
    location = checkCurrentDocument(document, luaManager, keyNames, tokens);
    if (location) {
        return location;
    }
    // var findInfos: Array<LuaFiledCompletionInfo> = new Array<LuaFiledCompletionInfo>();
    // getLocation(keyNames, luaManager, 1, findInfos)
    // getLocation(keyNames, luaManager, 2, findInfos)
    // var fInfo: LuaFiledCompletionInfo;
    // for (var i = 0; i < keyNames.length; i++) {
    //     for (var j = 0; j < findInfos.length; j++) {
    //         var f: LuaFiledCompletionInfo = findInfos[j]
    //         if (f.parent && f.parent.uri.path.toLocaleLowerCase().indexOf(keyNames[i].toLocaleLowerCase()) > -1) {
    //             fInfo = f;
    //             break
    //         }
    //         else if (f.uri.path.toLocaleLowerCase().indexOf(keyNames[i].toLocaleLowerCase()) > -1) {
    //             fInfo = f;
    //             break
    //         }
    //     }
    //     if (fInfo != null) {
    //         location = new vscode.Location(fInfo.uri, fInfo.position)
    //         return location
    //     }
    // }
    // if (findInfos.length > 0) {
    //     location = new vscode.Location(findInfos[0].uri, findInfos[0].position)
    //     return location
    // }
    // if (isSelf == true && location == null) {
    //     var rootInfo: FileCompletionItemManager = luaManager.fileCompletionItemManagers.get(document.uri.path)
    //     if (rootInfo) {
    //         var selfCInfo: LuaFiledCompletionInfo;//= rootInfo.luaFiledCompletionInfo;
    //         keyNames[0] = 'self'
    //         for (var i = 0; i < keyNames.length; i++) {
    //             selfCInfo = selfCInfo.getItemByKey(keyNames[i])
    //         }
    //         if (selfCInfo) {
    //             var location: vscode.Location =
    //                 new vscode.Location(selfCInfo.uri, selfCInfo.position)
    //             return location;
    //         }
    //     }
    // }
    // return location;
    return location;
}
exports.byteOffsetAt = byteOffsetAt;
function checkCurrentDocument(document, luaManager, keyNames, tokens) {
    var citems = new Array();
    var fcim = luaManager.getFcim(document.uri);
    var functionNames = Utils_1.getCurrentFunctionName(tokens);
    var funRootItem = null;
    if (functionNames != null && functionNames.length > 0) {
        var args = fcim.getSymbolArgsByNames(functionNames);
        if (keyNames.length == 1) {
            //参数查找
            for (var index = 0; index < args.length; index++) {
                var arg = args[index];
                if (arg.label == keyNames[0]) {
                    var location = new vscode.Location(document.uri, arg.position);
                    return location;
                }
            }
        }
        //方法内的变量
        for (var index = 0; index < functionNames.length; index++) {
            var fname = functionNames[index];
            funRootItem = fcim.luaFunFiledCompletions.get(fname);
            funRootItem = DefinitionFindItem(funRootItem, keyNames, 0);
            if (funRootItem != null) {
                return new vscode.Location(funRootItem.uri, funRootItem.position);
            }
        }
        //方法查找
    }
    funRootItem = DefinitionFindItem(fcim.luaFunCompletionInfo, keyNames, 0);
    if (funRootItem != null && funRootItem.isNewVar == true) {
        return new vscode.Location(funRootItem.uri, funRootItem.position);
    }
    //文件全局查找
    funRootItem = DefinitionFindItem(fcim.luaFileGolbalCompletionInfo, keyNames, 0);
    if (funRootItem != null && funRootItem.isNewVar == true) {
        return new vscode.Location(funRootItem.uri, funRootItem.position);
    }
    //项目全局
    funRootItem = DefinitionFindItem(fcim.luaGolbalCompletionInfo, keyNames, 0);
    if (funRootItem != null && funRootItem.isNewVar == true) {
        return new vscode.Location(funRootItem.uri, funRootItem.position);
    }
    //先 根据变量名字 找找对应的文件名 如果有 那么 直接确定为该文件
    var fileCompletionItemManagers = luaManager.fileCompletionItemManagers;
    for (var info of fileCompletionItemManagers) {
        if (info[0].indexOf("modulePath") > -1) {
            var xx = 1;
        }
        // console.log(info[0])
        funRootItem = DefinitionFindItem(info[1].luaGolbalCompletionInfo, keyNames, 0);
        if (funRootItem != null && funRootItem.isNewVar == true) {
            return new vscode.Location(funRootItem.uri, funRootItem.position);
        }
        funRootItem = DefinitionFindItem(info[1].luaFunCompletionInfo, keyNames, 0);
        if (funRootItem != null) {
            return new vscode.Location(funRootItem.uri, funRootItem.position);
        }
        if (info[1].rootCompletionInfo != null && keyNames[0] == info[1].rootCompletionInfo.label) {
            funRootItem = DefinitionFindItem(info[1].rootCompletionInfo, keyNames, 2);
            if (funRootItem != null && funRootItem.isNewVar == true) {
                return new vscode.Location(funRootItem.uri, funRootItem.position);
            }
        }
    }
}
function FindItemByFileName(keyNames) {
    //还没找到 那么就根据名字找依照
    for (var index = 0; index < keyNames.length; index++) {
        var element = keyNames[index];
    }
}
function DefinitionFindItem(rootItem, keys, index) {
    if (rootItem == null)
        return null;
    rootItem = rootItem.getItemByKey(keys[index]);
    if (index == keys.length - 1) {
        return rootItem;
    }
    else {
        if (rootItem != null) {
            index++;
            rootItem = DefinitionFindItem(rootItem, keys, index);
            return rootItem;
        }
        else {
            return null;
        }
    }
}
function checkComment(line, position) {
    var index1 = line.indexOf('[');
    var index2 = line.indexOf(']');
    var moduleName = line.substring(index1 + 1, index2);
    if (position.character > index1 && position.character < index2) {
        var uri = LuaFileCompletionItems_1.LuaFileCompletionItems.getLuaFileCompletionItems().getUriCompletionByModuleName(moduleName);
        var location = new vscode.Location(uri, new vscode.Position(0, 0));
        return location;
    }
}
function getCompletionByKeyNams(cinfo, keyNames, type) {
    var findInfo = cinfo;
    var i = 0;
    for (i = 0; i < keyNames.length; i++) {
        var key = keyNames[i];
        var tempInfo = findInfo.getItemByKey(key, type == 1);
        if (tempInfo != null) {
            findInfo = tempInfo;
            i++;
            break;
        }
    }
    for (i; i < keyNames.length; i++) {
        var key = keyNames[i];
        var tempInfo = findInfo.getItemByKey(key, type == 1);
        if (tempInfo != null) {
            findInfo = tempInfo;
        }
        else {
            findInfo = null;
            break;
        }
    }
    if (findInfo != null && findInfo != cinfo) {
        return findInfo;
    }
    else {
        return null;
    }
}
function getLocation(keyNames, luaManager, type, findInfos) {
    var notModlueInfos = new Array();
    var notModlueInfosIndex = new Array();
    var notModlueInfosIndex = new Array();
    //先找fun
    var cinfo = null;
    var location = null;
    luaManager.fileCompletionItemManagers.forEach((v, k) => {
        if (k != LuaParse_1.LuaParse.checkTempFilePath) {
            var tempInfo = null;
            if (type == 1) {
                tempInfo = v.luaFunCompletionInfo; //.getItemByKey(key,true)
            }
            else if (type == 2) {
                tempInfo = v.luaGolbalCompletionInfo; //.getItemByKey(key)
            }
            var findInfo = getCompletionByKeyNams(tempInfo, keyNames, type);
            if (findInfo) {
                findInfos.push(findInfo);
            }
        }
    });
}
/**
 * 只解析 table 定义 和 方法
 */
class LuaDefinitionProvider {
    provideDefinition(document, position, token) {
        // 找出单词
        //往前找
        //往后找
        // document.getText()
        let pos = new vscode.Position(1, 1);
        let wordAtPosition = document.getWordRangeAtPosition(position);
        let location = byteOffsetAt(document, position);
        return new Promise((resolve, reject) => {
            return resolve(location);
        });
    }
}
exports.LuaDefinitionProvider = LuaDefinitionProvider;
//# sourceMappingURL=LuaDefinitionProvider.js.map
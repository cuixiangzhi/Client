/**
 * lua 文件解析
 * 该文件解析  和现在网上的代码有所不同
 * 更加自身情况而的代码格式解析
 * 在前3天我研究了 c# 的 lua 解析 以及 js  的  他们都写得很好
 * 但是不太满足我的需求 所以我决定自己写一套 来实现代码提示
 */
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const LuaParseTool_1 = require("./LuaParseTool");
const LuaTableParse_1 = require("./LuaTableParse");
const LuaFunctionParse_1 = require("./LuaFunctionParse");
const LuaIfLogic_1 = require("./LuaIfLogic");
const LuaInfoManager_1 = require("./LuaInfoManager");
const LuaWhileLogic_1 = require("./LuaWhileLogic");
const LuaForLogic_1 = require("./LuaForLogic");
const Utils_1 = require("./Utils");
const FunctionCall_1 = require("./FunctionCall");
const ExtensionManager_1 = require("./ex/ExtensionManager");
const LuaCheckReturn_1 = require("./LuaCheckReturn");
const LuaLeftCheck_1 = require("./LuaLeftCheck");
const LuaCheckUnary_1 = require("./LuaCheckUnary");
const LuaValidateBracket_G_1 = require("./LuaValidateBracket_G");
const LuaChuckInfo_1 = require("./LuaChuckInfo");
const LuaValidateConstValue_1 = require("./LuaValidateConstValue");
const LuaValidateOperator_1 = require("./LuaValidateOperator");
const LuaCheckLuaInfos_1 = require("./LuaCheckLuaInfos");
const LuaSetValue_1 = require("./LuaSetValue");
const LuaValidateBracket_M_1 = require("./LuaValidateBracket_M");
const LuaFuncitonCheck_1 = require("./LuaFuncitonCheck");
const LuaCheckRepeat_1 = require("./LuaCheckRepeat");
const LuaCheckDoEnd_1 = require("./LuaCheckDoEnd");
const path = require("path");
const vscode = require("vscode");
const LuaGolbalCompletionManager_1 = require("./manager/LuaGolbalCompletionManager");
class LuaParse {
    constructor(diagnosticCollection) {
        this.tokenIndex = 0;
        this.tokensLength = 0;
        //解析过程中是否有错
        this.isError = false;
        this.errorMsg = new Array();
        LuaParse.lp = this;
        this.errorFilePaths = new Array();
        this.luaInfoManager = new LuaInfoManager_1.LuaInfoManager();
        this.diagnosticCollection = diagnosticCollection;
        this.lpt = new LuaParseTool_1.LuaParseTool(this);
        this.luaTableParse = new LuaTableParse_1.LuaTableParse(this);
        this.luaFunctionParse = new LuaFunctionParse_1.LuaFunctionParse(this);
        this.luaIfLogic = new LuaIfLogic_1.LuaIfLogic(this);
        this.luaWhileLogic = new LuaWhileLogic_1.LuaWhileLogic(this);
        this.luaForLogic = new LuaForLogic_1.LuaForLogic(this);
        this.luaCheckRepeat = new LuaCheckRepeat_1.LuaCheckRepeat(this);
        this.functionCall = new FunctionCall_1.FunctionCall(this);
        this.luaCheckReturn = new LuaCheckReturn_1.LuaCheckReturn(this);
        this.luaLeftCheck = new LuaLeftCheck_1.LuaLeftCheck(this);
        this.luaCheckUnary = new LuaCheckUnary_1.LuaCheckUnary(this);
        this.luaValidateBracket_G = new LuaValidateBracket_G_1.LuaValidateBracket_G(this);
        this.luaChuckInfo = new LuaChuckInfo_1.LuaChuckInfo(this);
        this.luaValidateConstValue = new LuaValidateConstValue_1.LuaValidateConstValue(this);
        this.luaValidateOperator = new LuaValidateOperator_1.LuaValidateOperator(this);
        this.luaCheckLuaInfos = new LuaCheckLuaInfos_1.LuaCheckLuaInfos(this);
        this.luaSetValue = new LuaSetValue_1.LuaSetValue(this);
        this.luaValidateBracket_M = new LuaValidateBracket_M_1.LuaValidateBracket_M(this);
        this.luaFuncitonCheck = new LuaFuncitonCheck_1.LuaFuncitonCheck(this);
        this.luaCheckDoEnd = new LuaCheckDoEnd_1.LuaCheckDoEnd(this);
        var tempFile = path.join(ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.extensionPath, "runtime", "parseTemFile");
        this.currentUri = vscode.Uri.parse(tempFile);
        LuaParse.checkTempFilePath = tempFile;
    }
    //传入的需要解析的代码
    Parse(uri, text, isSaveCompletion = true) {
        this.isSaveCompletion = isSaveCompletion;
        if (this.luaFuncitonCheck.currentFunLuaInfo) {
            var x = 1;
        }
        this.luaFuncitonCheck.currentFunLuaInfo = null;
        this.tempUri = uri;
        this.rootLuaInfo = new TokenInfo_1.LuaInfo(null);
        this.rootLuaInfo.type = TokenInfo_1.LuaInfoType.ROOT;
        this.lpt.Reset(text);
        this.isError = false;
        //解析分为全局变量 和局部变量 在这里    
        this.end();
    }
    end() {
        var data = new Date();
        //先
        var tokens = new Array();
        while (true) {
            Utils_1.CLog();
            var token = this.lpt.lex();
            if (token.error != null) {
                this.setError(token, token.error.msg);
                return;
            }
            if (token.type == TokenInfo_1.TokenTypes.EOF) {
                break;
            }
            token.index = tokens.length;
            tokens.push(token);
        }
        this.tokens = tokens;
        this.luaInfoManager.init(this, this.currentUri, this.tempUri);
        this.tokenIndex = 0;
        this.tokensLength = tokens.length;
        var isReturn = false;
        try {
            isReturn = this.setLuaInfo(this.rootLuaInfo, null, null);
        }
        catch (error) {
            console.log(error);
        }
        var returnValueToken = null;
        if (isReturn) {
            // console.log("isReturn:"+isReturn)
            if (this.tokenIndex < this.tokensLength) {
                this.setError(this.getLastToken(), "return 的多余字符");
            }
        }
        if (this.isError == false) {
            for (var i = 0; i < this.errorFilePaths.length; i++) {
                if (this.tempUri.path == this.errorFilePaths[i].path) {
                    this.errorFilePaths.splice(i, 1);
                    break;
                }
            }
            //正确了删除错误提示
            if (this.diagnosticCollection && this.diagnosticCollection.has(this.tempUri)) {
                this.diagnosticCollection.delete(this.tempUri);
            }
            var fcim = this.luaInfoManager.currentFcim;
            fcim.currentFunctionNames = null;
            if (this.isSaveCompletion || (this.isSaveCompletion == false && this.isError == false)) {
                var oldFcim = this.luaInfoManager.getFcimByPathStr(this.tempUri.path);
                if (oldFcim != null) {
                    LuaGolbalCompletionManager_1.LuaGolbalCompletionManager.clearGolbalCompletion(oldFcim.luaGolbalCompletionInfo);
                }
                if (isReturn) {
                    if (this.tokensLength - 2 >= 0) {
                        var returnToken = tokens[this.tokensLength - 2];
                        if (returnToken.type == TokenInfo_1.TokenTypes.Keyword && returnToken.value == "return") {
                            if (tokens[this.tokensLength - 1].type == TokenInfo_1.TokenTypes.Identifier) {
                                returnValueToken = tokens[this.tokensLength - 1];
                                fcim.setRootCompletionInfo(returnValueToken.value);
                            }
                        }
                    }
                }
                this.luaInfoManager.fileCompletionItemManagers.set(this.tempUri.path, fcim);
                fcim.checkFunCompletion();
                LuaGolbalCompletionManager_1.LuaGolbalCompletionManager.setGolbalCompletion(fcim.luaGolbalCompletionInfo);
                this.luaInfoManager.currentFcim = null;
                fcim.tokens = null;
            }
            this.luaInfoManager.fileCompletionItemManagers.delete(this.currentUri.path);
        }
        else {
            var fcim = this.luaInfoManager.fileCompletionItemManagers.get(this.currentUri.path);
            fcim.clear();
        }
    }
    /**
     * 返回值为 是否是 checkEnd 的返回值
     */
    setLuaInfo(parent, checkEnd, checkBreak) {
        while (true) {
            Utils_1.CLog();
            var returnValue = false;
            if (this.tokenIndex < this.tokensLength) {
                if (checkBreak) {
                    checkBreak(this);
                    if (this.isError)
                        return false;
                }
                else {
                    var breaktoken = this.getTokenByIndex(this.tokenIndex, null);
                    if (this.consume("break", breaktoken, TokenInfo_1.TokenTypes.Keyword)) {
                        this.checkSemicolons();
                        this.tokenIndex++;
                    }
                }
                //检查function
                returnValue = this.luaCheckReturn.check(parent, checkEnd, false);
                if (returnValue != false) {
                    return returnValue;
                }
                if (this.isError)
                    return false;
                if (checkEnd != null) {
                    returnValue = checkEnd(this);
                    if (this.isError)
                        return;
                    if (returnValue != false)
                        return returnValue;
                }
                if (this.luaFuncitonCheck.check()) {
                    if (this.isError)
                        return false;
                    continue;
                }
                if (this.isError)
                    return;
                if (this.tokenIndex >= this.tokens.length) {
                    return true;
                }
                if (this.luaForLogic.check()) {
                    continue;
                }
                if (this.isError)
                    return false;
                //检查 Repeat
                if (this.luaCheckRepeat.check()) {
                    if (this.isError)
                        return false;
                    continue;
                }
                //检查if
                if (this.luaIfLogic.check(parent, true, false, false, checkBreak)) {
                    if (this.isError)
                        return false;
                    continue;
                }
                if (this.isError)
                    return false;
                if (this.luaCheckDoEnd.check()) {
                    if (this.isError)
                        return false;
                    continue;
                }
                if (this.isError)
                    return false;
                if (this.tokenIndex >= this.tokens.length) {
                    return true;
                }
                //检查while
                if (this.luaWhileLogic.check(parent)) {
                    if (this.isError)
                        return false;
                    continue;
                }
                if (this.isError)
                    return false;
                this.luaLeftCheck.check(parent);
                if (this.isError)
                    return;
                this.tokenIndex++;
                //检查是否未 字符
                if (this.isError) {
                    return false;
                }
                else {
                    if (checkEnd != null) {
                        returnValue = checkEnd(this);
                        if (this.isError)
                            return;
                        if (returnValue != false) {
                            return returnValue;
                        }
                        else {
                            continue;
                        }
                    }
                    else {
                        continue;
                    }
                }
            }
            else {
                return false;
            }
        }
    }
    /**检查分号 */
    checkSemicolons() {
        var token = this.getNextToken(null);
        if (token != null && this.consume(';', token, TokenInfo_1.TokenTypes.Punctuator)) {
            return true;
        }
        this.tokenIndex--;
        return false;
    }
    setError(token, typeMsg, startToen = null) {
        this.isError = true;
        if (startToen == null) {
            startToen = token;
        }
        var starPo = new vscode.Position(startToen.line, startToen.range.start - startToen.lineStart);
        var endPo = new vscode.Position(token.line, token.range.end - token.lineStart);
        var range = new vscode.Range(starPo, endPo);
        var currentDiagnostic = new vscode.Diagnostic(range, typeMsg, vscode.DiagnosticSeverity.Error);
        this.diagnosticCollection.set(this.tempUri, [currentDiagnostic]);
    }
    getUpToken() {
        return this.tokens[this.tokenIndex - 1];
    }
    getLastToken() {
        return this.tokens[this.tokens.length - 1];
    }
    getTokenByIndex(tokenIndex, errorMsg) {
        // console.log(tokenIndex)
        if (tokenIndex < this.tokensLength) {
            return this.tokens[tokenIndex];
        }
        if (errorMsg != null) {
            var upToken = null;
            while (true) {
                Utils_1.CLog();
                tokenIndex -= 1;
                upToken = this.tokens[tokenIndex];
                if (upToken != null) {
                    break;
                }
            }
            this.setError(upToken, errorMsg);
        }
        return null;
    }
    /**
     * 获得下一个token
     */
    getNextToken(errorMsg) {
        this.tokenIndex++;
        return this.getCurrentToken(errorMsg);
    }
    /**
     * 获得当前token
     */
    getCurrentToken(errorMsg) {
        if (this.tokenIndex < this.tokensLength) {
            //  console.log(this.tokens[this.tokenIndex].line +" : "+ this.tokens[this.tokenIndex].value + " :"+this.tokenIndex)
            return this.tokens[this.tokenIndex];
        }
        if (errorMsg != null) {
            var tokenIndex = this.tokenIndex;
            var upToken = null;
            while (true) {
                Utils_1.CLog();
                tokenIndex -= 1;
                upToken = this.tokens[tokenIndex];
                if (upToken != null) {
                    break;
                }
            }
            this.setError(upToken, errorMsg);
        }
        return null;
    }
    getCurrentFunctionLuaInfo() {
        // if(this.functionList.length > 0)
        // {
        //   return this.functionList[this.functionList.length-1]
        // }else
        // return null
    }
    functionStart(luaInfo) {
        // var luaCompletionItem:LuaCompletionItem = new LuaCompletionItem(luaInfo,this.tokens);
        // this.functionList.push(luaCompletionItem);
    }
    functionEnd() {
        // this.functionList.pop();
    }
    /**
     * 进行对比传入的 指 如果 相同返回true 并指向下一个 token
     */
    consume(value, token, tokenTypes) {
        if (token == null)
            return false;
        if (value === token.value && token.type == tokenTypes) {
            // this.next();
            return true;
        }
        return false;
    }
}
exports.LuaParse = LuaParse;
//# sourceMappingURL=LuaParse.js.map
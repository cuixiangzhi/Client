"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
const ExtensionManager_1 = require("../luatool/ex/ExtensionManager");
class LuaFuncitonCheck {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
     * 检查if 语句
     */
    check() {
        var functionToken = this.lp.getCurrentToken("代码未完成");
        if (this.lp.isError)
            return;
        if (this.lp.consume('function', functionToken, TokenInfo_1.TokenTypes.Keyword)) {
            return this.checkGlobalFunction(functionToken);
        }
        else {
            if (this.checkIsLocal()) {
                var functionToken1 = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "代码未完成");
                if (this.lp.consume('function', functionToken1, TokenInfo_1.TokenTypes.Keyword)) {
                    this.lp.tokenIndex++;
                    return this.checkLocalFunction(null, functionToken.comments);
                }
            }
        }
        return false;
    }
    checkGlobalFunction(functionToken) {
        var luaInfo = new TokenInfo_1.LuaInfo(this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "funcito 未完成"));
        luaInfo.type = TokenInfo_1.LuaInfoType.Function;
        // luaInfo.name = "";
        if (functionToken.comments && functionToken.comments.length > 0) {
            luaInfo.startToken.comments = functionToken.comments;
            luaInfo.setComments(functionToken.comments);
        }
        while (true) {
            Utils_1.CLog();
            var token = this.lp.getNextToken("function 未完成");
            if (token == null) {
                return false;
            }
            // luaInfo.name = luaInfo.name + token.value;
            if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                var nextToken = this.lp.getNextToken("function 未完成");
                if (this.lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    //  luaInfo.name = luaInfo.name+".";
                    continue;
                }
                else if (this.lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    // luaInfo.name = luaInfo.name +":"
                    if (ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.moduleFunNestingCheck) {
                        if (this.currentFunLuaInfo) {
                            this.lp.setError(token, "module 方法出现嵌套", this.currentFunLuaInfo.startToken);
                            return;
                        }
                        this.currentFunLuaInfo = luaInfo;
                        var funResult = this.checkLocalFunction(luaInfo, functionToken.comments);
                        this.currentFunLuaInfo = null;
                        return funResult;
                    }
                    else {
                        var funResult = this.checkLocalFunction(luaInfo, functionToken.comments);
                        return funResult;
                    }
                }
                else if (this.lp.consume('(', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.tokenIndex--;
                    var endToken = this.lp.getCurrentToken(null);
                    var returnValue = this.lp.luaFunctionParse.check(luaInfo, true, null);
                    this.lp.luaInfoManager.addFunctionCompletionItem(luaInfo, endToken, this.lp.getUpToken());
                    return returnValue;
                }
                else {
                    this.lp.setError(token, "function 意外的字符");
                    return false;
                }
            }
            else {
                this.lp.setError(token, "function 意外的字符");
                return false;
            }
        }
    }
    checkLocalFunction(luaInfo, comments) {
        var token = this.lp.getNextToken("function 未完成");
        if (luaInfo == null) {
            luaInfo = new TokenInfo_1.LuaInfo(token);
            luaInfo.isLocal = true;
            luaInfo.setComments(comments);
        }
        if (token.type == TokenInfo_1.TokenTypes.Identifier) {
            var endToken = this.lp.getCurrentToken(null);
            var result = this.lp.luaFunctionParse.check(luaInfo, true, null);
            this.lp.luaInfoManager.addFunctionCompletionItem(luaInfo, endToken, this.lp.getUpToken());
            return result;
        }
        else {
            this.lp.setError(token, "function 意外的字符");
            return false;
        }
    }
    /**
     * 是否是local
     */
    checkIsLocal() {
        var token = this.lp.getCurrentToken("代码未完成");
        if (this.lp.isError)
            return false;
        var isLocal = this.lp.consume('local', token, TokenInfo_1.TokenTypes.Keyword);
        return isLocal;
    }
}
exports.LuaFuncitonCheck = LuaFuncitonCheck;
//# sourceMappingURL=LuaFuncitonCheck.js.map
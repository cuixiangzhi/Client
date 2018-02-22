"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
/**验证 一个代码段 */
class LuaChuckInfo {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    check(luaInfo, isSemicolons) {
        var token = this.lp.getCurrentToken("代码未完成");
        if (this.lp.isError)
            return;
        luaInfo.setComments(token.comments);
        if (this.lp.consume('(', token, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.tokenIndex++;
            this.lp.luaValidateBracket_M.check(luaInfo);
            if (this.lp.isError)
                return;
        }
        else if (this.lp.consume("function", token, TokenInfo_1.TokenTypes.Keyword)) {
            luaInfo.isAnonymousFunction = true;
            this.lp.luaFunctionParse.check(luaInfo, true, null);
            this.lp.tokenIndex--;
            return;
        }
        else if (this.lp.consume('{', token, TokenInfo_1.TokenTypes.Punctuator)) {
            var endToken = this.lp.getUpToken();
            this.lp.luaTableParse.check(luaInfo);
            luaInfo.setEndToken(endToken);
            return;
        }
        else if (token.type == TokenInfo_1.TokenTypes.Keyword) {
            this.lp.setError(token, "关键字不能作为 变量名");
            return false;
        }
        else {
            //设置luainfo 的值
            this.lp.luaValidateConstValue.check(token, luaInfo);
            if (this.lp.isError)
                return;
            luaInfo.name = token.value;
        }
        var currentToken = this.lp.getCurrentToken(null);
        //检查还有没有
        var nextToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, null);
        if (nextToken == null)
            return;
        if (currentToken.type == TokenInfo_1.TokenTypes.Identifier) {
            if (nextToken.type == TokenInfo_1.TokenTypes.BooleanLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.NilLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.StringLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.VarargLiteral) {
                this.lp.tokenIndex++;
                luaInfo.getTopLuaInfo().isNextCheck = false;
                return;
            }
            else if (this.lp.consume('{', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                var tableLuaInfo = new TokenInfo_1.LuaInfo(nextToken);
                this.lp.tokenIndex++;
                this.lp.luaTableParse.check(tableLuaInfo);
                luaInfo.getTopLuaInfo().isNextCheck = false;
                return;
            }
        }
        if (this.lp.consume(',', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            return;
        }
        if (token.type != TokenInfo_1.TokenTypes.Identifier &&
            token.type != TokenInfo_1.TokenTypes.VarargLiteral) {
            if (this.lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume('[', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume('(', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.setError(nextToken, "意外的字符");
                return;
            }
        }
        if (this.lp.consume('(', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            if (luaInfo.isLocal) {
                this.lp.setError(nextToken, "意外的字符");
                return;
            }
            if (luaInfo.ismultipleVariables) {
                this.lp.setError(nextToken, "意外的字符");
                return;
            }
            this.lp.tokenIndex++;
            this.lp.functionCall.check(luaInfo, isSemicolons, false);
        }
        else if (this.lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
            this.lp.consume('[', nextToken, TokenInfo_1.TokenTypes.Punctuator) ||
            this.lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.tokenIndex += 2;
            if (luaInfo.isLocal) {
                this.lp.setError(token, "局部变量声明 无法包含 '" + token.value + "'");
                return null;
            }
            else {
                if (this.lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    var newLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getTokenByIndex(this.lp.tokenIndex + 2, null));
                    //  this.lp.luaInfoManager.addLuaInfo(luaInfo, this.lp.getTokenByIndex(this.lp.tokenIndex + 1, null))
                    luaInfo.setNextLuaInfo(newLuaInfo);
                    nextToken = this.lp.getCurrentToken("代码未完成");
                    if (this.lp.isError)
                        return;
                    if (nextToken.type != TokenInfo_1.TokenTypes.Identifier) {
                        if (nextToken.type == TokenInfo_1.TokenTypes.Keyword) {
                            this.lp.setError(nextToken, "关键字不能作为 变量名");
                        }
                        else {
                            this.lp.setError(nextToken, "意外的字符");
                        }
                        return;
                    }
                    this.check(newLuaInfo, isSemicolons);
                    if (this.lp.isError)
                        return;
                }
                else if (this.lp.consume('[', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.luaValidateBracket_G.check(luaInfo, true);
                }
                else if (this.lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.checkModuleFunctionCall(luaInfo, isSemicolons);
                }
            }
        }
    }
    /**
     * 检查模块方法调用
     */
    checkModuleFunctionCall(luaInfo, isSemicolons) {
        var token = this.lp.getCurrentToken("module 方法调用错误");
        if (this.lp.isError)
            return;
        if (token.type != TokenInfo_1.TokenTypes.Identifier) {
            this.lp.setError(token, "module 方法调用错误 出现意外字符");
            return;
        }
        var newLuaInfo = new TokenInfo_1.LuaInfo(token);
        if (luaInfo != null) {
            luaInfo.setNextLuaInfo(newLuaInfo);
        }
        else {
        }
        var nextToken = this.lp.getNextToken("module 方法调用错误");
        if (this.lp.isError)
            return;
        if (!this.lp.consume('(', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            if (nextToken.type == TokenInfo_1.TokenTypes.BooleanLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.NilLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.NumericLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.StringLiteral ||
                nextToken.type == TokenInfo_1.TokenTypes.VarargLiteral) {
                luaInfo.getTopLuaInfo().isNextCheck = false;
                return;
            }
            else if (this.lp.consume('{', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                var tableLuaInfo = new TokenInfo_1.LuaInfo(nextToken);
                this.lp.luaTableParse.check(tableLuaInfo);
                luaInfo.getTopLuaInfo().isNextCheck = false;
                return;
            }
            else {
                this.lp.setError(token, "module 方法调用错误");
            }
            return;
        }
        this.lp.functionCall.check(newLuaInfo, isSemicolons, false);
        if (this.lp.isError)
            return;
    }
}
exports.LuaChuckInfo = LuaChuckInfo;
//# sourceMappingURL=LuaChuckInfo.js.map
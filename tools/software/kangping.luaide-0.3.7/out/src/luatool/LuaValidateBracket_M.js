"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
/**验证 ( */
class LuaValidateBracket_M {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**验证 小括号( */
    check(luaInfo) {
        //缓存 一元运算符号
        // var unary: string = luaInfo.unary
        // luaInfo.unary = ""
        var exToken = this.lp.getCurrentToken("代码未完成");
        if (exToken == null)
            return false;
        if (this.lp.consume(')', exToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.setError(exToken, "() 中间不能为空!  ");
            return false;
        }
        var luainfos = new Array();
        while (true) {
            Utils_1.CLog();
            var nextLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
            luainfos.push(nextLuaInfo);
            this.lp.luaCheckUnary.check(nextLuaInfo);
            this.lp.luaChuckInfo.check(nextLuaInfo, false);
            if (this.lp.isError)
                return null;
            var nextToken = this.lp.getNextToken("代码未完成");
            if (this.lp.isError)
                return;
            //验证是否为括号 ]
            //判断二元
            if (this.lp.consume(')', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                //这里检查表达式的合法性
                this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                nextLuaInfo.type = TokenInfo_1.LuaInfoType.Field;
                nextLuaInfo.setEndToken(this.lp.getCurrentToken(null));
                luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.ANY;
                luaInfo.type = TokenInfo_1.LuaInfoType.Bracket_M;
                nextToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, null);
                if (nextToken) {
                    if (this.lp.consume('.', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                        this.lp.tokenIndex++;
                        var newLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
                        nextToken = this.lp.getNextToken("代码未完成");
                        if (this.lp.isError)
                            return;
                        if (nextToken.type != TokenInfo_1.TokenTypes.Identifier) {
                            this.lp.setError(nextToken, "意外的字符");
                            return;
                        }
                        else {
                        }
                        luaInfo.setNextLuaInfo(newLuaInfo);
                        this.lp.luaChuckInfo.check(newLuaInfo, true);
                        if (this.lp.isError)
                            return;
                        return;
                    }
                    else if (this.lp.consume('[', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                        this.lp.tokenIndex += 2;
                        this.lp.luaValidateBracket_G.check(luaInfo, true);
                        break;
                    }
                    else if (this.lp.consume('(', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                        this.lp.tokenIndex++;
                        this.lp.functionCall.check(luaInfo, true, false);
                        break;
                    }
                    else if (this.lp.consume(':', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                        this.lp.tokenIndex += 2;
                        if (luainfos.length == 1) {
                            this.lp.luaChuckInfo.checkModuleFunctionCall(luaInfo, true);
                        }
                        else {
                            this.lp.luaChuckInfo.checkModuleFunctionCall(luaInfo, true);
                        }
                        luaInfo.type = TokenInfo_1.LuaInfoType.FunctionCall1;
                        return;
                    }
                    else if (this.lp.consume('=', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                        return;
                    }
                    else
                        return;
                }
            }
            if (this.lp.luaValidateOperator.check(nextToken)) {
                nextLuaInfo.setEndToken(this.lp.getUpToken());
                this.lp.tokenIndex++;
                nextLuaInfo.operatorToken = nextToken;
                continue;
            }
            else {
                this.lp.setError(nextToken, "错误的字符");
                return;
            }
        }
    }
}
exports.LuaValidateBracket_M = LuaValidateBracket_M;
//# sourceMappingURL=LuaValidateBracket_M.js.map
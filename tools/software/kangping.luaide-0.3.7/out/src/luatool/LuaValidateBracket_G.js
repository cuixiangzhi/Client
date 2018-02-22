"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
class LuaValidateBracket_G {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**验证中括号 */
    check(luaInfo, isNext) {
        var exToken = this.lp.getCurrentToken("代码未完成");
        if (exToken == null)
            return null;
        if (this.lp.consume(']', exToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.setError(exToken, "[] 中间不能为空!  ");
            return null;
        }
        var luainfos = new Array();
        while (true) {
            Utils_1.CLog();
            var nextLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
            luainfos.push(nextLuaInfo);
            this.lp.luaSetValue.check(false, false, [nextLuaInfo]);
            if (this.lp.isError)
                return null;
            var nextToken = this.lp.getNextToken("代码未完成");
            if (this.lp.isError)
                return;
            //验证是否为括号 ]
            //判断二元
            if (this.lp.consume(']', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                //这里检查表达式的合法性
                this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                luaInfo.bracket_Gs = luainfos;
                luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.ANY;
                if (!isNext)
                    return;
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
                        this.lp.luaValidateBracket_G.check(luaInfo, isNext);
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
                        luaInfo.getTopLuaInfo().isNextCheck = true;
                        return;
                    }
                    else
                        return;
                }
                else {
                    return;
                }
            }
            if (this.lp.luaValidateOperator.check(nextToken)) {
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
exports.LuaValidateBracket_G = LuaValidateBracket_G;
//# sourceMappingURL=LuaValidateBracket_G.js.map
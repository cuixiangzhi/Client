"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
class LuaForLogic {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
     * 检查if 语句
     */
    check() {
        var token = this.lp.getCurrentToken("代码未完成");
        if (this.lp.consume('for', token, TokenInfo_1.TokenTypes.Keyword)) {
            var parent = new TokenInfo_1.LuaInfo(token);
            //检查类型
            var nextToken1 = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "for 循环未完成");
            var nextToken2 = this.lp.getTokenByIndex(this.lp.tokenIndex + 2, "for 循环未完成");
            if (nextToken1 == null || nextToken2 == null) {
                return false;
            }
            if (nextToken1.type != TokenInfo_1.TokenTypes.Identifier) {
                this.lp.setError(nextToken1, "for 意外的字符");
                return false;
            }
            var pluaInfo = new TokenInfo_1.LuaInfo(nextToken1);
            pluaInfo.isLocal = true;
            pluaInfo.setEndToken(nextToken1);
            if (this.lp.consume(',', nextToken2, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.tokenIndex += 2;
                this.checkPairs(parent, token, false);
                if (this.lp.isError)
                    return false;
                return true;
            }
            else if (this.lp.consume('in', nextToken2, TokenInfo_1.TokenTypes.Keyword)) {
                this.lp.tokenIndex += 2;
                this.checkPairs(parent, token, true);
                if (this.lp.isError)
                    return false;
                return true;
            }
            else {
                return this.checkForI(parent, token);
                //for i = 1, 
            }
        }
        else
            return false;
    }
    checkForI(parent, startToken) {
        var nextToken1 = this.lp.getNextToken(null);
        var luaInfo = new TokenInfo_1.LuaInfo(nextToken1);
        luaInfo.isLocal = true;
        parent.type = TokenInfo_1.LuaInfoType.FOR_I;
        var nextToken2 = this.lp.getNextToken("for 循环未完成");
        if (this.lp.isError)
            return false;
        if (this.lp.consume('=', nextToken2, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.tokenIndex++;
        }
        else {
            this.lp.setError(nextToken2, "意外的字符");
            return false;
        }
        //验证表达式
        this.lp.luaSetValue.check(false, false, null);
        if (this.lp.isError)
            return false;
        var nextToken = this.lp.getNextToken("代码未完成 ");
        if (nextToken == null)
            return false;
        if (!this.lp.consume(',', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.setError(nextToken, "意外的字符 应该为 ','");
            return false;
        }
        this.lp.tokenIndex++;
        this.lp.luaSetValue.check(false, false, null);
        nextToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, null);
        if (this.lp.consume(',', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.tokenIndex += 2;
            this.lp.luaSetValue.check(false, false, null);
        }
        if (this.lp.isError)
            return false;
        return this.checkFor1(luaInfo);
    }
    checkPairs(parent, startToken, isBrakTopCheck) {
        var pairsToken = null;
        var luaInfo = new TokenInfo_1.LuaInfo(startToken);
        parent.type = TokenInfo_1.LuaInfoType.FOR_PAIRS;
        if (!isBrakTopCheck) {
            while (true) {
                //判断是否为in
                var tokenInfo1 = this.lp.getNextToken("for 循环未完成");
                if (tokenInfo1 == null)
                    return false;
                var tokenInfo2 = this.lp.getNextToken("for 循环未完成");
                if (tokenInfo2 == null)
                    return false;
                if (tokenInfo1.type != TokenInfo_1.TokenTypes.Identifier) {
                    this.lp.setError(tokenInfo1, "意外的字符");
                    return false;
                }
                var pluaInfo = new TokenInfo_1.LuaInfo(tokenInfo1);
                pluaInfo.isLocal = true;
                pluaInfo.setEndToken(tokenInfo1);
                if (this.lp.consume('in', tokenInfo2, TokenInfo_1.TokenTypes.Keyword)) {
                    break;
                }
                if (this.lp.consume(',', tokenInfo2, TokenInfo_1.TokenTypes.Punctuator)) {
                    continue;
                }
                else if (this.lp.consume('in', tokenInfo2, TokenInfo_1.TokenTypes.Keyword)) {
                    break;
                }
                else {
                    this.lp.setError(tokenInfo1, "应该为 in");
                    return false;
                }
            }
        }
        else {
            // if (pairsToken.type != TokenTypes.Identifier) {
            //     this.lp.setError(pairsToken, "意外的字符")
            //     return false
            // }
        }
        pairsToken = this.lp.getNextToken("for 循环未完成");
        if (this.lp.isError)
            return false;
        if (this.lp.consume('ipairs', pairsToken, TokenInfo_1.TokenTypes.Identifier) ||
            this.lp.consume('pairs', pairsToken, TokenInfo_1.TokenTypes.Identifier)) {
            var token = this.lp.getNextToken("for 循环未完成");
            if (this.lp.consume('(', token, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume('{', token, TokenInfo_1.TokenTypes.Punctuator)) {
                luaInfo.name = pairsToken.value;
                this.lp.luaSetValue.check(false, false, null);
                var to = this.lp.getCurrentToken(null);
                // this.lp.functionCall.check(luaInfo, false, true)
            }
            else {
                if (token.type != TokenInfo_1.TokenTypes.Identifier) {
                    this.lp.setError(token, "错误的字符");
                    return false;
                }
            }
            if (this.lp.isError)
                return false;
            return this.checkFor1(luaInfo);
        }
        else if (this.lp.consume('function', pairsToken, TokenInfo_1.TokenTypes.Keyword)) {
            var luaInfo1 = new TokenInfo_1.LuaInfo(pairsToken);
            this.lp.luaFunctionParse.check(luaInfo1, true, null);
            this.lp.tokenIndex--;
            if (this.lp.isError)
                return false;
            return this.checkFor1(luaInfo);
        }
        else {
            this.lp.luaSetValue.check(false, false, null);
            while (true) {
                Utils_1.CLog();
                var nextToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "for 未完成");
                if (this.lp.isError)
                    return;
                if (this.lp.consume(',', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.tokenIndex += 2;
                    this.lp.luaSetValue.check(false, false, null);
                }
                else {
                    break;
                }
            }
            if (this.lp.isError)
                return false;
            // this.lp.setError(pairsToken, "意外的字符")
            return this.checkFor1(luaInfo);
        }
    }
    checkFor1(luaInfo) {
        var nextToken = this.lp.getNextToken("代码未完成 ");
        if (nextToken == null)
            return false;
        if (!this.lp.consume('do', nextToken, TokenInfo_1.TokenTypes.Keyword)) {
            this.lp.setError(nextToken, "意外的字符 应该为 'do'");
            return false;
        }
        else {
            this.lp.tokenIndex++;
        }
        var isEnd = this.lp.setLuaInfo(luaInfo, function (luaParse) {
            var token = luaParse.getTokenByIndex(luaParse.tokenIndex, "function 未结束");
            if (luaParse.isError)
                return false;
            if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                luaParse.checkSemicolons();
                luaParse.tokenIndex++;
                return true;
            }
            return false;
        }, this.checkBrreak);
        if (isEnd) {
            return true;
        }
        else {
            if (this.lp.isError)
                return false;
            this.lp.setError(this.lp.getCurrentToken(null), "for 未结束");
            return false;
        }
    }
    checkBrreak(luaParse) {
        var token = luaParse.getTokenByIndex(luaParse.tokenIndex, null);
        if (luaParse.consume("break", token, TokenInfo_1.TokenTypes.Keyword)) {
            luaParse.checkSemicolons();
            luaParse.tokenIndex++;
        }
        else {
            return false;
        }
        token = luaParse.getTokenByIndex(luaParse.tokenIndex, "代码未完成");
        if (luaParse.isError)
            return false;
        if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword) ||
            luaParse.consume('elseif', token, TokenInfo_1.TokenTypes.Keyword) ||
            luaParse.consume('else', token, TokenInfo_1.TokenTypes.Keyword)) {
            return true;
        }
        else {
            luaParse.setError(token, "break 后不能有多余的字符");
            return false;
        }
    }
}
exports.LuaForLogic = LuaForLogic;
//# sourceMappingURL=LuaForLogic.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
class LuaTableParse {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    check(luaInfo) {
        luaInfo.type = TokenInfo_1.LuaInfoType.Table;
        while (true) {
            Utils_1.CLog();
            var token = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "table 未完成");
            if (this.lp.consume('}', token, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.tokenIndex++;
                return;
            }
            if (this.lp.isError)
                return;
            if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                var nextToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 2, "table 未完成");
                if (this.lp.isError)
                    return;
                if (this.lp.consume('=', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    var fluaInfo = new TokenInfo_1.LuaInfo(nextToken);
                    fluaInfo.name = token.value;
                    fluaInfo.endToken = nextToken;
                    var leftLuaInfos = new Array();
                    leftLuaInfos.push(fluaInfo);
                    this.lp.tokenIndex += 3;
                    luaInfo.tableFileds.push(fluaInfo);
                    this.lp.luaSetValue.check(false, false, leftLuaInfos);
                }
                else {
                    this.lp.tokenIndex++;
                    this.lp.luaSetValue.check(false, false, null);
                }
            }
            else if (this.lp.consume('[', token, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.tokenIndex += 2;
                var startIndex = this.lp.tokenIndex;
                var newLuaInfo = new TokenInfo_1.LuaInfo(token);
                this.lp.luaValidateBracket_G.check(newLuaInfo, false);
                var endIndex = this.lp.tokenIndex;
                var nextToken = this.lp.getNextToken("代码未完成");
                if (this.lp.isError)
                    return;
                if (this.lp.consume('=', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    if (endIndex - startIndex == 1) {
                        var token = this.lp.tokens[startIndex];
                        newLuaInfo.name = "[" + token.value + "]";
                        newLuaInfo.tableFiledType = 1;
                        luaInfo.tableFileds.push(newLuaInfo);
                    }
                    var leftLuaInfos = new Array();
                    leftLuaInfos.push(newLuaInfo);
                    this.lp.tokenIndex += 1;
                    this.lp.luaSetValue.check(false, false, leftLuaInfos);
                }
                else {
                    this.lp.setError(nextToken, "缺少 '='");
                    return;
                }
            }
            else {
                this.lp.tokenIndex++;
                this.lp.luaSetValue.check(false, false, null);
            }
            token = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "table 未完成");
            if (this.lp.consume(',', token, TokenInfo_1.TokenTypes.Punctuator) || this.lp.consume(';', token, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.tokenIndex++;
                var endToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "table 未完成");
                if (this.lp.consume('}', endToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    //  luaInfo.endToken = endToken;
                    this.lp.tokenIndex++;
                    return;
                }
                else {
                    continue;
                }
            }
            else if (this.lp.consume('}', token, TokenInfo_1.TokenTypes.Punctuator)) {
                //  luaInfo.endToken = token;
                this.lp.tokenIndex++;
                return;
            }
            else {
                this.lp.setError(token, "table 定义出现意外字符");
                return;
            }
        }
    }
}
exports.LuaTableParse = LuaTableParse;
//# sourceMappingURL=LuaTableParse.js.map
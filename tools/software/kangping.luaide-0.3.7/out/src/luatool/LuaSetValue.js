"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
class LuaSetValue {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    //value 只能是  
    /**是否检查分号  */
    check(isSemicolons, isComma, leftLuaInfos, index = 0) {
        var luaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
        //检查是否为一元
        var luainfos = new Array();
        while (true) {
            Utils_1.CLog();
            var nextLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
            luainfos.push(nextLuaInfo);
            this.lp.luaCheckUnary.check(nextLuaInfo);
            nextLuaInfo.startToken = this.lp.getCurrentToken(null);
            this.lp.luaChuckInfo.check(nextLuaInfo, isSemicolons);
            if (this.lp.isError)
                return null;
            if (nextLuaInfo.type == TokenInfo_1.LuaInfoType.Function) {
                if (nextLuaInfo.unarys.length == 0) {
                    nextLuaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.Function;
                    if (leftLuaInfos != null && index <= leftLuaInfos.length - 1) {
                        var linfo = leftLuaInfos[index];
                        linfo.params = nextLuaInfo.params;
                        linfo.type = TokenInfo_1.LuaInfoType.Function;
                    }
                }
                else {
                    this.lp.setError(nextLuaInfo.unarys[0], "function 定义前有多余字符");
                }
            }
            if (nextLuaInfo.type == TokenInfo_1.LuaInfoType.Table) {
                nextLuaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.Table;
                if (leftLuaInfos != null && index <= leftLuaInfos.length - 1) {
                    var linfo = leftLuaInfos[index];
                    linfo.tableFileds = nextLuaInfo.tableFileds;
                }
                //table 直接返回
            }
            var nextToken = this.lp.getNextToken(null);
            if (nextToken == null) {
                this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                if (nextLuaInfo.type != TokenInfo_1.LuaInfoType.FunctionCall1) {
                    nextLuaInfo.setEndToken(this.lp.getUpToken());
                }
                return luainfos;
            }
            //验证是否为括号 ]
            //判断二元
            if (this.lp.luaValidateOperator.check(nextToken)) {
                nextLuaInfo.setEndToken(this.lp.getUpToken());
                this.lp.tokenIndex++;
                nextLuaInfo.operatorToken = nextToken;
                continue;
            }
            else if (this.lp.consume(',', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                if (leftLuaInfos != null && index > leftLuaInfos.length - 1) {
                    this.lp.setError(nextToken, "多余的变量赋值");
                    return;
                }
                nextLuaInfo.setEndToken(this.lp.getUpToken());
                if (isComma) {
                    this.lp.tokenIndex++;
                    this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                    if (this.lp.isError)
                        return;
                    this.check(isSemicolons, isComma, leftLuaInfos, ++index);
                    return luainfos;
                }
                else {
                    this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                    this.lp.tokenIndex--;
                    return luainfos;
                }
            }
            else if (this.lp.consume(';', nextToken, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                if (isSemicolons == false) {
                    this.lp.tokenIndex--;
                }
                return luainfos;
            }
            else {
                if (nextLuaInfo.type == TokenInfo_1.LuaInfoType.Field && nextToken.type == TokenInfo_1.TokenTypes.StringLiteral) {
                    var upToken = this.lp.getUpToken();
                    nextLuaInfo.type = TokenInfo_1.LuaInfoType.FunctionCall1;
                }
                else {
                    this.lp.tokenIndex--;
                }
                this.lp.luaCheckLuaInfos.check(luainfos, luaInfo);
                if (nextLuaInfo.type != TokenInfo_1.LuaInfoType.Bracket_M &&
                    nextLuaInfo.getLastLuaInfo().type != TokenInfo_1.LuaInfoType.FunctionCall1) {
                    nextLuaInfo.setEndToken(this.lp.getCurrentToken(null));
                }
                return luainfos;
            }
        }
    }
}
exports.LuaSetValue = LuaSetValue;
//# sourceMappingURL=LuaSetValue.js.map
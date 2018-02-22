"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
class LuaWhileLogic {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
     * 检查if 语句
     */
    check(parent) {
        var token = this.lp.getCurrentToken("代码未完成");
        if (this.lp.consume('while', token, TokenInfo_1.TokenTypes.Keyword)) {
            this.lp.tokenIndex++;
            var luaInfo = new TokenInfo_1.LuaInfo(token);
            luaInfo.type = TokenInfo_1.LuaInfoType.WHILE;
            //先判断表达式  再判断 do
            this.lp.luaSetValue.check(true, false, null);
            if (this.lp.isError)
                return false;
            var doToken = this.lp.getNextToken("代码未完成");
            if (this.lp.isError)
                return false;
            if (this.lp.consume('do', doToken, TokenInfo_1.TokenTypes.Keyword)) {
                this.lp.tokenIndex++;
                var isEnd = this.lp.setLuaInfo(luaInfo, function (luaParse) {
                    var token = luaParse.getTokenByIndex(luaParse.tokenIndex, "代码未完成");
                    if (luaParse.isError)
                        return false;
                    if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                        luaParse.tokenIndex++;
                        luaParse.checkSemicolons();
                        return true;
                    }
                    return false;
                }, this.lp.luaForLogic.checkBrreak);
                if (this.lp.isError)
                    return false;
                if (isEnd) {
                    return true;
                }
                else {
                    this.lp.setError(this.lp.getLastToken(), "while 没有 结束  缺少 end");
                    return false;
                }
            }
            else {
                this.lp.setError(doToken, "应该为 do ");
                return false;
            }
        }
        else
            return false;
    }
}
exports.LuaWhileLogic = LuaWhileLogic;
//# sourceMappingURL=LuaWhileLogic.js.map
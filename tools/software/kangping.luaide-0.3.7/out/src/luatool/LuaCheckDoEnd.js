"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
class LuaCheckDoEnd {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
     * 检查if 语句
     */
    check() {
        var token = this.lp.getCurrentToken(null);
        if (token == null)
            return true;
        if (this.lp.consume('do', token, TokenInfo_1.TokenTypes.Keyword)) {
            var luaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
            this.lp.tokenIndex++;
            var returnValue = this.lp.setLuaInfo(luaInfo, function (luaParse) {
                var token = luaParse.getTokenByIndex(luaParse.tokenIndex, "代码未完成");
                if (luaParse.isError)
                    return false;
                if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                    var ul = new TokenInfo_1.LuaInfo(token);
                    luaParse.tokenIndex++;
                    return true;
                }
                return false;
            }, null);
            return returnValue;
        }
        else
            return false;
    }
}
exports.LuaCheckDoEnd = LuaCheckDoEnd;
//# sourceMappingURL=LuaCheckDoEnd.js.map
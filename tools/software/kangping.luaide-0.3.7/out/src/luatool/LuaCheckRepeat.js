"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
class LuaCheckRepeat {
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
        if (this.lp.consume('repeat', token, TokenInfo_1.TokenTypes.Keyword)) {
            var luaInfo = new TokenInfo_1.LuaInfo(this.lp.getCurrentToken(null));
            this.lp.tokenIndex++;
            var returnValue = this.lp.setLuaInfo(luaInfo, function (luaParse) {
                var token = luaParse.getTokenByIndex(luaParse.tokenIndex, "代码未完成");
                if (luaParse.isError)
                    return false;
                if (luaParse.consume('until', token, TokenInfo_1.TokenTypes.Keyword)) {
                    var ul = new TokenInfo_1.LuaInfo(token);
                    luaParse.tokenIndex++;
                    luaParse.luaSetValue.check(true, true, null);
                    if (luaParse.isError)
                        return false;
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
exports.LuaCheckRepeat = LuaCheckRepeat;
//# sourceMappingURL=LuaCheckRepeat.js.map
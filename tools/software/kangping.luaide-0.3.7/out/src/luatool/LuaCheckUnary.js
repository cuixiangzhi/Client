"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
/**验证 一元 */
class LuaCheckUnary {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
  * 判断是否是一元表达式
  */
    check(luaInfo) {
        while (true) {
            Utils_1.CLog();
            var token = this.lp.getCurrentToken(null);
            if (token != null) {
                if (this.lp.consume('#', token, TokenInfo_1.TokenTypes.Punctuator) ||
                    this.lp.consume('not', token, TokenInfo_1.TokenTypes.Keyword) ||
                    this.lp.consume('-', token, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.tokenIndex++;
                    luaInfo.unarys.push(token);
                }
                else {
                    return;
                }
            }
            else
                return;
        }
    }
}
exports.LuaCheckUnary = LuaCheckUnary;
//# sourceMappingURL=LuaCheckUnary.js.map
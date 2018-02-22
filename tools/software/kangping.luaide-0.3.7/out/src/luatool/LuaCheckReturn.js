"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
/**验证 return */
class LuaCheckReturn {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**检查return */
    check(luaInfo, checkEnd, isCheckBreak) {
        var token = this.lp.getCurrentToken(null);
        if (token == null)
            return false;
        if (this.lp.consume('return', token, TokenInfo_1.TokenTypes.Keyword)) {
            var isReturn = this.lp.checkSemicolons();
            var returnValue = false;
            if (isReturn) {
                this.lp.tokenIndex++;
                if (checkEnd != null) {
                    returnValue = checkEnd(this.lp);
                }
                if (this.lp.isError)
                    return false;
                if (returnValue == false) {
                    this.lp.setError(this.lp.getNextToken(null), "return 的多余字符");
                    return false;
                }
                else {
                    return returnValue;
                }
            }
            else {
                this.lp.tokenIndex++;
                if (checkEnd != null) {
                    returnValue = checkEnd(this.lp);
                    if (this.lp.isError)
                        return false;
                    if (returnValue != false) {
                        return returnValue;
                    }
                }
                this.lp.luaSetValue.check(true, true, null);
                if (this.lp.isError)
                    return false;
                this.lp.tokenIndex++;
                if (checkEnd != null) {
                    returnValue = checkEnd(this.lp);
                }
                else {
                    returnValue = true;
                }
                if (this.lp.isError)
                    return false;
                if (returnValue == false) {
                    this.lp.setError(this.lp.getCurrentToken(null), "return 的多余字符");
                    return false;
                }
                else {
                    return returnValue;
                }
            }
        }
        else
            return false;
    }
}
exports.LuaCheckReturn = LuaCheckReturn;
//# sourceMappingURL=LuaCheckReturn.js.map
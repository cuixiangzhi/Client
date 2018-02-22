"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
/**验证 二元运算符号 */
class LuaValidateOperator {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
   * 判断一个token 是否是一个运算符号 二元
   */
    check(token) {
        if (token.type === TokenInfo_1.TokenTypes.Punctuator || token.type == TokenInfo_1.TokenTypes.Keyword) {
            var value = token.value;
            if (value == '+' ||
                value == '-' ||
                value == '*' ||
                value == '/' ||
                value == '>' ||
                value == '<' ||
                value == '>=' ||
                value == '<=' ||
                value == '%' ||
                value == '&' ||
                value == '~' ||
                value == '|' ||
                value == '<<' ||
                value == '>>' ||
                value == '^') {
                return true;
            }
            else if (value == '..') {
                return true;
            }
            else if (value == '==' ||
                value == '~=' ||
                value == 'and' ||
                value == 'or') {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }
}
exports.LuaValidateOperator = LuaValidateOperator;
//# sourceMappingURL=LuaValidateOperator.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
/**验证 是否为一个可以复制的 token */
class LuaValidateConstValue {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
  * 验证是否是一个可以符值的token
  */
    check(token, luaInfo) {
        if (token.type == TokenInfo_1.TokenTypes.BooleanLiteral) {
            if (luaInfo.isVar == true) {
                this.lp.setError(token, "变量申明不能为 boolean");
                return;
            }
            luaInfo.type = TokenInfo_1.LuaInfoType.BOOLEAN;
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.BOOL;
            return true;
        }
        else if (token.type == TokenInfo_1.TokenTypes.NilLiteral) {
            if (luaInfo.isVar == true) {
                this.lp.setError(token, "变量申明不能为 nil");
                return;
            }
            luaInfo.type = TokenInfo_1.LuaInfoType.NIL;
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.NIL;
            return true;
        }
        else if (token.type == TokenInfo_1.TokenTypes.NumericLiteral) {
            if (luaInfo.isVar == true) {
                this.lp.setError(token, "变量申明不能为 number");
                return;
            }
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.NUMBER;
            luaInfo.type = TokenInfo_1.LuaInfoType.Number;
            return true;
        }
        else if (token.type == TokenInfo_1.TokenTypes.StringLiteral) {
            if (luaInfo.isVar == true) {
                this.lp.setError(token, "变量申明不能为 string");
                return;
            }
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.STRING;
            luaInfo.type = TokenInfo_1.LuaInfoType.STRING;
            return true;
        }
        else if (token.type == TokenInfo_1.TokenTypes.VarargLiteral) {
            if (luaInfo.isVar == true) {
                this.lp.setError(token, "变量申明不能为 ...");
                return;
            }
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.ANY;
            luaInfo.type = TokenInfo_1.LuaInfoType.Vararg;
            return true;
        }
        else if (token.type == TokenInfo_1.TokenTypes.Identifier) {
            luaInfo.type = TokenInfo_1.LuaInfoType.Field;
            luaInfo.valueType = TokenInfo_1.LuaInfoTypeValue.ANY;
            return true;
        }
        else {
            if (!this.lp.consume('(', token, TokenInfo_1.TokenTypes.Punctuator)) {
                this.lp.setError(this.lp.getCurrentToken(null), "意外的字符");
                this.lp.tokenIndex--;
            }
            return false;
        }
    }
}
exports.LuaValidateConstValue = LuaValidateConstValue;
//# sourceMappingURL=LuaValidateConstValue.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
const ExtensionManager_1 = require("../luatool/ex/ExtensionManager");
/**验证 检查luainfo 集合 是否合法 */
class LuaCheckLuaInfos {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    check(luaInfos, parentLuaInfo) {
        if (!ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.luaOperatorCheck) {
            return;
        }
        if (luaInfos.length == 1) {
            var luaInfo = luaInfos[0];
            this.checkLuaInfoValue(luaInfo);
            if (this.lp.isError)
                return;
            // if(luaInfo.valueType == LuaInfoTypeValue.STRING)
            // {
            //     parentLuaInfo.localLuaInfo.push(luaInfo)
            // }
            if (luaInfo.operatorToken != null) {
                this.lp.setError(luaInfo.operatorToken, "意外的字符");
                return;
            }
            return;
        }
        for (var i = 0; i < luaInfos.length; i++) {
            var luaInfo = luaInfos[i];
            this.checkLuaInfoValue(luaInfo);
            if (this.lp.isError)
                return;
            var token = luaInfo.operatorToken;
            if (token == null) {
                return;
            }
            var value = token.value;
            if (value == '+' ||
                value == '-' ||
                value == '*' ||
                value == '/' ||
                value == '^' ||
                value == '+' ||
                value == '%' ||
                value == '&' ||
                value == '<<' ||
                value == '>>' ||
                value == '|' ||
                value == '~') {
                if (luaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.NUMBER &&
                    luaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.ANY) {
                    this.lp.setError(token, "非数字不能做运算");
                    return;
                }
                else {
                    //判断下一个是否为 number || any
                    if (i + 1 < luaInfos.length) {
                        var nextLuaInfo = luaInfos[i + 1];
                        this.checkLuaInfoValue(nextLuaInfo);
                        if (this.lp.isError)
                            return;
                        if (nextLuaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.NUMBER &&
                            nextLuaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.ANY) {
                            this.lp.setError(token, "非数字不能做运算");
                            return;
                        }
                    }
                }
            }
            else if (value == '>' ||
                value == '<' ||
                value == '>=' ||
                value == '<=') {
                if (luaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.NUMBER &&
                    luaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.STRING &&
                    luaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.ANY) {
                    this.lp.setError(token, "非数字不能做运算");
                    return;
                }
                else {
                    //判断下一个是否为 number || any
                    if (i + 1 < luaInfos.length) {
                        var nextLuaInfo = luaInfos[i + 1];
                        this.checkLuaInfoValue(nextLuaInfo);
                        if (this.lp.isError)
                            return;
                        if (nextLuaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.NUMBER &&
                            nextLuaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.ANY &&
                            nextLuaInfo.valueType != TokenInfo_1.LuaInfoTypeValue.STRING) {
                            this.lp.setError(token, "非数字不能做运算");
                            return;
                        }
                    }
                }
            }
            else if (value == '..') {
                if (luaInfo.valueType == TokenInfo_1.LuaInfoTypeValue.BOOL) {
                    this.lp.setError(token, "boolean 不能 用于字符串连接");
                    return;
                }
                else {
                    if (i + 1 < luaInfos.length) {
                        var nextLuaInfo = luaInfos[i + 1];
                        if (nextLuaInfo.valueType == TokenInfo_1.LuaInfoTypeValue.BOOL) {
                            this.lp.setError(token, "boolean 不能 用于字符串连接");
                            return;
                        }
                    }
                }
            }
            else if (value == '==' ||
                value == '~=') {
                //正确的 不需要处理
            }
            else if (value == 'and' ||
                value == 'or') {
                //正确的 不需要处理
            }
        }
    }
    checkLuaInfoValue(luaInfo) {
        var valueType = null;
        var unarys = luaInfo.unarys;
        if (unarys.length == 0) {
            // luaInfo.valueType = LuaInfoTypeValue.ANY
            return;
        }
        var length = unarys.length - 1;
        while (length >= 0) {
            Utils_1.CLog();
            var token = unarys[length];
            if (valueType == null) {
                if (this.lp.consume('#', token, TokenInfo_1.TokenTypes.Punctuator)) {
                    if (luaInfo.type == TokenInfo_1.LuaInfoType.FunctionCall1 ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Field ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Vararg ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Bracket_M) {
                        valueType = TokenInfo_1.LuaInfoTypeValue.NUMBER;
                    }
                    else {
                        this.lp.setError(token, "不能计算长度");
                        return;
                    }
                }
                else if (this.lp.consume('-', token, TokenInfo_1.TokenTypes.Punctuator)) {
                    if (luaInfo.type == TokenInfo_1.LuaInfoType.FunctionCall1 ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Field ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Number ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Vararg ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Bracket_M) {
                        valueType = TokenInfo_1.LuaInfoTypeValue.NUMBER;
                    }
                    else {
                        this.lp.setError(token, "不能计算长度");
                        return;
                    }
                }
                else if (this.lp.consume('not', token, TokenInfo_1.TokenTypes.Keyword)) {
                    if (luaInfo.type == TokenInfo_1.LuaInfoType.FunctionCall1 ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Field ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.BOOLEAN ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.STRING ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.NIL ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Vararg ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Number ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Bracket_M) {
                        valueType = TokenInfo_1.LuaInfoTypeValue.BOOL;
                    }
                    else {
                        this.lp.setError(token, "不能转换为 boolean");
                        return;
                    }
                }
            }
            else {
                if (this.lp.consume('#', token, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.setError(token, "不能计算长度");
                    return;
                }
                else if (this.lp.consume('-', token, TokenInfo_1.TokenTypes.Punctuator)) {
                    if (valueType == TokenInfo_1.LuaInfoTypeValue.NUMBER ||
                        luaInfo.type == TokenInfo_1.LuaInfoType.Bracket_M) {
                    }
                    else {
                        this.lp.setError(token, "非数字不能为 负数");
                        return;
                    }
                }
                else if (this.lp.consume('not', token, TokenInfo_1.TokenTypes.Keyword)) {
                    valueType = TokenInfo_1.LuaInfoTypeValue.BOOL;
                }
            }
            length--;
            // if (this.lp.consume('#', token, TokenTypes.Punctuator)) {
            //     if (valueType == null) {
            //         if (luaInfo.type == LuaInfoType.FunctionCall ||
            //             luaInfo.type == LuaInfoType.Field) {
            //             valueType = LuaInfoTypeValue.NUMBER
            //         } else {
            //             this.lp.setError(token, "不能计算长度")
            //             return
            //         }
            //     } else if (valueType == LuaInfoTypeValue.ANY) {
            //         valueType = LuaInfoTypeValue.NUMBER
            //     } else {
            //         this.lp.setError(token, "不能计算长度")
            //         return
            //     }
            // }
            // else if (this.lp.consume('-', token, TokenTypes.Punctuator)) {
            // }
        }
        luaInfo.valueType = valueType;
    }
}
exports.LuaCheckLuaInfos = LuaCheckLuaInfos;
//# sourceMappingURL=LuaCheckLuaInfos.js.map
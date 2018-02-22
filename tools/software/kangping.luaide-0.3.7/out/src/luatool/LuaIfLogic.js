"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
class LuaIfLogic {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    /**
     * 检查if 语句
     * isElseIf 是否检查 elseif 默认为false
     */
    check(parent, isIf, isElseIf, isElse, checkBreak) {
        //创建一个luaInfo 标识未 ifLuaInfo
        var token = this.lp.getCurrentToken("代码未完成");
        var luaInfo = new TokenInfo_1.LuaInfo(token);
        if (this.lp.isError)
            return;
        var returnValue = false;
        if (this.lp.consume('if', token, TokenInfo_1.TokenTypes.Keyword)) {
            luaInfo.type = TokenInfo_1.LuaInfoType.IF;
            luaInfo.name = token.value;
            returnValue = this.checkIfAndElseIF(luaInfo, token, checkBreak);
        }
        else if (isElseIf == true && this.lp.consume('elseif', token, TokenInfo_1.TokenTypes.Keyword)) {
            luaInfo.type = TokenInfo_1.LuaInfoType.ELSEIF;
            luaInfo.name = token.value;
            returnValue = this.checkIfAndElseIF(luaInfo, token, checkBreak);
        }
        else if (isElse == true && this.lp.consume('else', token, TokenInfo_1.TokenTypes.Keyword)) {
            luaInfo.type = TokenInfo_1.LuaInfoType.ELSE;
            luaInfo.name = token.value;
            returnValue = this.checkLuaInfos(luaInfo, false, false, checkBreak);
        }
        else {
            return false;
        }
        if (returnValue == "end") {
            this.lp.tokenIndex++;
            return true;
        }
        else {
            if (returnValue == "elseif") {
                var re = this.check(luaInfo, false, true, true, checkBreak);
                if (this.lp.isError)
                    return false;
                if (re == false) {
                    this.lp.setError(this.lp.getCurrentToken(null), luaInfo.name + " 代码未完成");
                    return false;
                }
                return re;
            }
            else if (returnValue == "else") {
                this.lp.tokenIndex++;
                var revalue = this.checkLuaInfos(luaInfo, false, false, checkBreak);
                if (this.lp.isError)
                    return false;
                if (revalue == "end") {
                    this.lp.tokenIndex++;
                    return true;
                }
                else {
                    this.lp.setError(this.lp.getCurrentToken(null), luaInfo.name + " 代码未完成");
                    return false;
                }
            }
            else {
                if (this.lp.isError)
                    return false;
                this.lp.setError(this.lp.getLastToken(), luaInfo.name + " 代码未完成");
                return false;
            }
        }
    }
    checkIfAndElseIF(luaInfo, token, checkBreak) {
        this.lp.tokenIndex++;
        this.lp.luaSetValue.check(false, false, null);
        if (this.lp.isError)
            return false;
        var thenToken = this.lp.getNextToken("缺少 then");
        if (this.lp.isError)
            return false;
        if (!this.lp.consume('then', thenToken, TokenInfo_1.TokenTypes.Keyword)) {
            this.lp.setError(thenToken, "应该为then");
            if (this.lp.isError)
                return false;
        }
        this.lp.tokenIndex++;
        return this.checkLuaInfos(luaInfo, true, true, checkBreak);
    }
    checkLuaInfos(LuaInfo, isCheckElseIf, ischeckElse, checkBreak) {
        //      var token: TokenInfo = this.lp.getTokenByIndex(this.lp.tokenIndex, "代码未完成")
        //      if(this.lp.isError)return false
        //     if (this.lp.consume('end', token, TokenTypes.Keyword)) {
        //                     this.lp.tokenIndex++;
        //                     this.lp.checkComma()
        //                     return "end"
        //    }
        var returnValue = this.lp.setLuaInfo(LuaInfo, function (luaParse) {
            var token = luaParse.getTokenByIndex(luaParse.tokenIndex, "代码未完成");
            if (luaParse.isError)
                return false;
            if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                luaParse.checkSemicolons();
                return "end";
            }
            else if (isCheckElseIf == true && luaParse.consume('elseif', token, TokenInfo_1.TokenTypes.Keyword)) {
                return "elseif";
            }
            else if (ischeckElse == true && luaParse.consume('else', token, TokenInfo_1.TokenTypes.Keyword)) {
                return "else";
            }
            return false;
        }, checkBreak);
        return returnValue;
    }
}
exports.LuaIfLogic = LuaIfLogic;
//# sourceMappingURL=LuaIfLogic.js.map
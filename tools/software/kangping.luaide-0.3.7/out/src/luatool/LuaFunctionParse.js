"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
const ExtensionManager_1 = require("../luatool/ex/ExtensionManager");
class LuaFunctionParse {
    constructor(luaparse) {
        this.lp = luaparse;
    }
    check(luaInfo, isSemicolons, checkBreak) {
        var t = this.lp.getCurrentToken(null);
        luaInfo.type = TokenInfo_1.LuaInfoType.Function;
        var funName = t.value;
        var nindex = t.index;
        while (true) {
            nindex--;
            var upToken = this.lp.getTokenByIndex(nindex, null);
            if (upToken == null) {
                break;
            }
            nindex--;
            if (this.lp.consume(':', upToken, TokenInfo_1.TokenTypes.Punctuator) ||
                this.lp.consume('.', upToken, TokenInfo_1.TokenTypes.Punctuator)) {
                var mtokenInfo = this.lp.getTokenByIndex(nindex, null);
                funName = mtokenInfo.value + upToken.value + funName;
            }
            else {
                break;
            }
        }
        if (luaInfo.isAnonymousFunction) {
            funName = "TempFun_" + luaInfo.startToken.line + "_" + luaInfo.startToken.lineStart;
        }
        //console.log("开始解析funcation")
        if (this.setFunctionParam(luaInfo) == false)
            return false;
        this.lp.luaInfoManager.currentFcim.setBeginFunName(funName, luaInfo.params);
        this.lp.tokenIndex++;
        //进行方法内的解析
        var isEnd = this.lp.setLuaInfo(luaInfo, function (luaParse) {
            var token = luaParse.getTokenByIndex(luaParse.tokenIndex, null);
            if (token == null) {
                luaParse.setError(luaParse.getLastToken(), "function 未结束", luaInfo.startToken);
                //"start  line:"+luaInfo.startToken.line + "    function 未结束"
            }
            if (luaParse.isError) {
                return false;
            }
            if (luaParse.consume('end', token, TokenInfo_1.TokenTypes.Keyword)) {
                if (isSemicolons) {
                    luaParse.checkSemicolons();
                }
                luaParse.tokenIndex++;
                return true;
            }
            return false;
        }, checkBreak);
        if (isEnd) {
            this.lp.luaInfoManager.currentFcim.setEndFun();
            return true;
        }
        else {
            if (this.lp.isError)
                return;
            var currerntToken = this.lp.getCurrentToken(null);
            if (currerntToken == null) {
                currerntToken = this.lp.getLastToken();
            }
            this.lp.setError(currerntToken, "function 未结束", luaInfo.startToken);
            return false;
        }
    }
    /**
     * 解析方法参数
     */
    setFunctionParam(luaInfo) {
        // console.log("解析 方法参数 中...")
        //判断是否为 '('
        var bracketToken = this.lp.getNextToken("function 未结束");
        if (bracketToken === null)
            return false;
        if (!this.lp.consume('(', bracketToken, TokenInfo_1.TokenTypes.Punctuator)) {
            this.lp.setError(bracketToken, "意外的字符");
            return false;
        }
        var bracketToken = this.lp.getNextToken("function 未结束");
        if (bracketToken === null)
            return false;
        //判断是不是没有参数 
        if (this.lp.consume(')', bracketToken, TokenInfo_1.TokenTypes.Punctuator)) {
            return true;
        }
        this.lp.tokenIndex--;
        var bracketCount = 0;
        //方法参数只接受 TokenTypes.Identifier TokenTypes.VarargLiteral
        var isFist = true;
        while (true) {
            Utils_1.CLog();
            if (!isFist) {
                //检查逗号
                var commaToken = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "function 未结束"); // this.getNextToken("方法未完成")
                if (commaToken == null)
                    return false;
                if (!this.lp.consume(',', commaToken, TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.setError(commaToken, "应该为 ','");
                    return false;
                }
                else {
                    this.lp.tokenIndex++;
                }
            }
            var paramToken = this.lp.getNextToken("参数 定义 未完成");
            if (paramToken == null)
                return false;
            if (paramToken.type == TokenInfo_1.TokenTypes.Identifier || paramToken.type == TokenInfo_1.TokenTypes.VarargLiteral) {
                isFist = false;
                // if(paramToken.type == TokenTypes.Identifier)
                // {
                //     var pluaInfo:LuaInfo = new LuaInfo(paramToken);
                //     pluaInfo.setEndToken(paramToken)
                // }
                var index = luaInfo.addParam(paramToken.value);
                if (ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.luaFunArgCheck) {
                    if (index > -1) {
                        this.lp.setError(paramToken, "方法参数:'" + paramToken.value + "'在第" + index + " 个参数已经存在");
                        return false;
                    }
                }
                //判断有没有结束
                if (this.lp.consume(')', this.lp.getTokenByIndex(this.lp.tokenIndex + 1, "function 未结束"), TokenInfo_1.TokenTypes.Punctuator)) {
                    this.lp.tokenIndex++;
                    return true;
                }
            }
            else {
                this.lp.setError(paramToken, "意外的字符");
                return false;
            }
        }
    }
}
exports.LuaFunctionParse = LuaFunctionParse;
//# sourceMappingURL=LuaFunctionParse.js.map
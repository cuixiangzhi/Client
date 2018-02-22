"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("./TokenInfo");
const Utils_1 = require("./Utils");
class LuaLeftCheck {
    constructor(luaparse) {
        /**是否为 多变量声明  */
        this.isMultipleVariables = false;
        this.lp = luaparse;
    }
    check(parent) {
        this.luaInfos = new Array();
        this.isLocal = false;
        this.isMultipleVariables = false;
        this.parent = parent;
        //创建leftLuaInfo
        var toekn = this.lp.getCurrentToken(null);
        var currentLuaInfo = new TokenInfo_1.LuaInfo(toekn);
        this.isLocal = this.checkIsLocal(currentLuaInfo);
        if (this.lp.isError)
            return;
        currentLuaInfo.isLocal = this.isLocal;
        currentLuaInfo.isVar = true;
        if (this.isLocal) {
            currentLuaInfo.startToken = this.lp.getCurrentToken(null);
            currentLuaInfo.startToken.comments = this.lp.getUpToken().comments;
        }
        this.luaInfos.push(currentLuaInfo);
        this.checkLeftExoression(currentLuaInfo, new Array());
        currentLuaInfo = null;
    }
    /**
     * 是否是local
     */
    checkIsLocal(luaInfo) {
        var token = this.lp.getCurrentToken("代码未完成");
        if (this.lp.isError)
            return false;
        var isLocal = this.lp.consume('local', token, TokenInfo_1.TokenTypes.Keyword);
        if (isLocal) {
            this.lp.tokenIndex++;
            luaInfo.setComments(token.comments);
        }
        return isLocal;
    }
    /**
     * 检查 变量声明
     * @isIdentifier 是否必须为 TokenTypes.Identifier 类型
     */
    checkLeftExoression(leftLuaInfo, leftLuaInfos) {
        while (true) {
            Utils_1.CLog();
            this.lp.luaChuckInfo.check(leftLuaInfo, true);
            if (this.lp.isError)
                return;
            //方法调用直接退出
            if (leftLuaInfo.type == TokenInfo_1.LuaInfoType.FunctionCall1) {
                return;
            }
            if (!leftLuaInfo.isNextCheck) {
                return;
            }
            var token = this.lp.getTokenByIndex(this.lp.tokenIndex + 1, null);
            if (token == null) {
                if (leftLuaInfo.isLocal == false) {
                    var last = leftLuaInfo.getLastLuaInfo();
                    // leftLuaInfo.type == LuaInfoType.Function
                    var type = leftLuaInfo.type;
                    if (type != TokenInfo_1.LuaInfoType.AnonymousFunction) {
                        this.lp.setError(leftLuaInfo.startToken, "没有赋值");
                    }
                }
                leftLuaInfo.setEndToken(this.lp.getCurrentToken(null));
                return;
            }
            this.lp.tokenIndex++;
            //赋值
            if (this.lp.consume('=', token, TokenInfo_1.TokenTypes.Punctuator)) {
                var endToken = this.lp.getUpToken();
                leftLuaInfos.push(leftLuaInfo);
                this.lp.tokenIndex++;
                //设置value
                this.lp.luaSetValue.check(true, true, leftLuaInfos);
                if (leftLuaInfo.type == TokenInfo_1.LuaInfoType.Function) {
                    this.lp.luaInfoManager.addFunctionCompletionItem(leftLuaInfo, endToken, this.lp.getUpToken());
                }
                else {
                    leftLuaInfo.setEndToken(endToken);
                }
                return;
            }
            else if (this.lp.consume(',', token, TokenInfo_1.TokenTypes.Punctuator)) {
                leftLuaInfo.setEndToken(this.lp.getUpToken());
                leftLuaInfos.push(leftLuaInfo);
                this.lp.tokenIndex++;
                this.isMultipleVariables = true;
                var currentLuaInfo = new TokenInfo_1.LuaInfo(this.lp.getTokenByIndex(this.lp.tokenIndex, null));
                currentLuaInfo.isLocal = this.isLocal;
                currentLuaInfo.isVar = true;
                currentLuaInfo.ismultipleVariables = true;
                this.luaInfos.push(currentLuaInfo);
                this.checkLeftExoression(currentLuaInfo, leftLuaInfos);
                return;
            }
            else if (this.lp.consume(';', token, TokenInfo_1.TokenTypes.Punctuator)) {
                if (this.isLocal == false || (this.isLocal && this.isMultipleVariables)) {
                    this.lp.setError(this.lp.getCurrentToken(null), "没有赋值");
                }
                leftLuaInfo.setEndToken(this.lp.getUpToken());
                // this.lp.tokenIndex++;
                return;
            }
            else {
                if (this.isLocal == false) {
                    if (token.type == TokenInfo_1.TokenTypes.StringLiteral) {
                        return;
                    }
                    else if (!this.lp.consume('=', token, TokenInfo_1.TokenTypes.Punctuator)) {
                        if (token.type == TokenInfo_1.TokenTypes.Identifier) {
                            this.lp.setError(this.lp.getTokenByIndex(this.lp.tokenIndex, null), "我猜测这是一个方法参数,但是请加()");
                        }
                        else {
                            this.lp.setError(this.lp.getTokenByIndex(this.lp.tokenIndex - 1, null), "没有赋值");
                        }
                        return false;
                    }
                }
                leftLuaInfo.setEndToken(this.lp.getUpToken());
                this.lp.tokenIndex--;
                return;
            }
        }
    }
    checkLastLuaInfoType(luaInfo, type) {
        while (true) {
            Utils_1.CLog();
            if (luaInfo.getNextLuaInfo() == null) {
                if (luaInfo.type == type)
                    return true;
                else
                    return false;
            }
            else {
                luaInfo = luaInfo.getNextLuaInfo();
            }
        }
    }
}
exports.LuaLeftCheck = LuaLeftCheck;
//# sourceMappingURL=LuaLeftCheck.js.map
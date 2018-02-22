"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
const TokenInfo_1 = require("../TokenInfo");
const Utils_1 = require("../Utils");
class ProviderUtils {
    static getTokens(document, position) {
        var lp = LuaParse_1.LuaParse.lp;
        var start = new vscode.Position(0, 0);
        var lpt = LuaParse_1.LuaParse.lp.lpt;
        var tokens = new Array();
        lpt.Reset(document.getText(new vscode.Range(start, position)));
        while (true) {
            Utils_1.CLog();
            var token = lpt.lex();
            if (token.error != null) {
                return;
            }
            if (token.type == TokenInfo_1.TokenTypes.EOF) {
                break;
            }
            token.index = tokens.length;
            tokens.push(token);
        }
        return tokens;
    }
    static getTokenByText(text) {
        var lpt = LuaParse_1.LuaParse.lp.lpt;
        var tokens = new Array();
        lpt.Reset(text);
        while (true) {
            Utils_1.CLog();
            var token = lpt.lex();
            if (token.error != null) {
                return;
            }
            if (token.type == TokenInfo_1.TokenTypes.EOF) {
                break;
            }
            token.index = tokens.length;
            tokens.push(token);
        }
        return tokens;
    }
    static getComments(comments) {
        if (comments == null)
            return "";
        var commentStr = "";
        if (comments.length == 1) {
            return comments[0].content;
        }
        for (var i = 0; i < comments.length; i++) {
            var comment = comments[i].content;
            var index = comment.trim().indexOf("==");
            if (index == 0) {
                continue;
            }
            commentStr = commentStr + comment;
        }
        return commentStr;
    }
    static getFirstComments(comments) {
        if (comments == null)
            return "";
        var commentStr = null;
        if (comments.length == 1) {
            return comments[0].content;
        }
        var descStr = null;
        for (var i = 0; i < comments.length; i++) {
            var comment = comments[i].content;
            var index = comment.trim().indexOf("==");
            if (index == 0) {
                continue;
            }
            if (comment.indexOf("@desc:") > -1) {
                commentStr = comment;
                break;
            }
            else if (commentStr == null) {
                commentStr = comment;
            }
        }
        return commentStr;
    }
    static getSelfToModuleNameAndStartTokenIndex(uri, tokens, lp) {
        var index = tokens.length - 1;
        while (true) {
            if (index < 0)
                break;
            var token = tokens[index];
            if (lp.consume('function', token, TokenInfo_1.TokenTypes.Keyword)) {
                var nextToken = tokens[index + 1];
                if (nextToken.type == TokenInfo_1.TokenTypes.Identifier) {
                    var nextToken1 = tokens[index + 2];
                    if (lp.consume(':', nextToken1, TokenInfo_1.TokenTypes.Punctuator) ||
                        lp.consume('.', nextToken1, TokenInfo_1.TokenTypes.Punctuator)) {
                        var range = null;
                        var functionNameToken = null;
                        //方法名
                        if (tokens.length > index + 3) {
                            functionNameToken = tokens[index + 3];
                        }
                        if (functionNameToken) {
                            if (lp.luaInfoManager.getFcimByPathStr(uri.path)) {
                                var name = nextToken.value + nextToken1.value + functionNameToken.value;
                                range = lp.luaInfoManager.getFcimByPathStr(uri.path).getSymbolEndRange(name);
                            }
                            else {
                                return {};
                            }
                        }
                        var moduleName = nextToken.value;
                        return { moduleName: moduleName, index: index, range: range };
                    }
                    else
                        index--;
                }
                else {
                    index--;
                }
            }
            else {
                index--;
            }
        }
        return null;
    }
    static getParamComment(param, comments) {
        var paramName = "@" + param;
        for (var i = 0; i < comments.length; i++) {
            var comment = comments[i].content;
            if (comment.indexOf(paramName) > -1) {
                comment = comment.replace(paramName, "");
                return comment;
            }
        }
        return "";
    }
}
exports.ProviderUtils = ProviderUtils;
//# sourceMappingURL=providerUtils.js.map
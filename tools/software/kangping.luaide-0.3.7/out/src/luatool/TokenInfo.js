'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const LuaParse_1 = require("./LuaParse");
const Utils_1 = require("./Utils");
/**
 * lua info
 */
class LuaInfo {
    constructor(startToken) {
        this.comments = null;
        this.aliasName = null; // 别名
        this.isPointFun = false;
        /**是否是多变量 */
        this.ismultipleVariables = false;
        this.isValue = false;
        /** : 的数量 */
        this.punctuatorNumber_1 = 0;
        /** . 的数量  */
        this.punctuatorNumber_2 = 0;
        /** [] 的数量 */
        this.punctuatorNumber_3 = 0;
        /**用于table 第一次判断 , */
        this.tableIsFistItem = true;
        /**是否是包含在 [] 中的 */
        this.isBracket_G = false;
        /**是否是局部变量 */
        this.isLocal = false;
        this.isAddToFiled = true;
        this.tableFiledType = 0;
        this.isAnonymousFunction = false;
        this.isNextCheck = true;
        this.comments = new Array();
        this.isVar = false;
        this.startToken = startToken;
        this.type = LuaInfoType.Field;
        this.localLuaInfo = new Array();
        this.params = new Array();
        this.unarys = new Array();
        this.tableFileds = new Array();
    }
    getNextLuaInfo() {
        return this.nextLuaInfo;
    }
    getUpInfo() {
        return this.upLuaInfo;
    }
    setNextLuaInfo(nextLuaInfo) {
        this.nextLuaInfo = nextLuaInfo;
        nextLuaInfo.ismultipleVariables = this.ismultipleVariables;
        nextLuaInfo.isLocal = this.isLocal;
        nextLuaInfo.isVar = this.isValue;
        nextLuaInfo.upLuaInfo = this;
        nextLuaInfo.comments = this.comments;
        this.comments = null;
    }
    getTopLuaInfo() {
        while (true) {
            Utils_1.CLog();
            if (this.upLuaInfo) {
                return this.upLuaInfo.getTopLuaInfo();
            }
            else {
                return this;
            }
        }
    }
    getLastLuaInfo() {
        while (true) {
            Utils_1.CLog();
            if (this.nextLuaInfo) {
                return this.nextLuaInfo.getLastLuaInfo();
            }
            else {
                return this;
            }
        }
    }
    setEndToken(token) {
        this.endToken = token;
        if (this.type == LuaInfoType.Table) {
            return;
        }
        LuaParse_1.LuaParse.lp.luaInfoManager.addCompletionItem(this, token);
        //  LuaParse.lp.luaInfoManager.addFiledLuaInfo(this,token);
        // this.endToken = token;
        // var tokens:Array<TokenInfo> = LuaParse.lp.tokens;
        // var startIndex:number =this.startToken.range.start;
        // var endIndex:number = this.endToken.range.end;
        // var input = LuaParse.lp.lpt.input;
        // var name = input.substr(startIndex,endIndex-startIndex)
        // this.filePath = LuaParse.filePath;
        // this.moduleName = this.filePath.substring(0, this.filePath.lastIndexOf('.lua'))
        // var varName1 = "";
        // var varName2 = "";
        // if(this.startToken.index == this.endToken.index)
        // {
        //     if(this.startToken.type == TokenTypes.BooleanLiteral ||
        //     this.startToken.type == TokenTypes.NumericLiteral ||
        //     this.startToken.type == TokenTypes.VarargLiteral 
        //     ){
        //         return;
        //     }
        // }
        // // var currentFunctionLuaInfo:LuaInfo = LuaParse.lp.getCurrentFunctionLuaInfo();
        // var isLocal =false;
        // var isFunction = false;
        // var startIndex:number = this.startToken.index
        // for(var i = startIndex;i <= this.endToken.index;i++)
        // {
        //     //获取初始
        //     var token:TokenInfo =tokens[i];
        //     if(LuaParse.lp.consume('local',token,TokenTypes.Keyword))
        //     {
        //         isLocal = true
        //         continue;
        //     }
        //      if(LuaParse.lp.consume('function',token,TokenTypes.Keyword))
        //     {
        //         isFunction = true
        //         continue;
        //     }
        //     if(LuaParse.lp.consume('[',token,TokenTypes.Punctuator) && this.endToken.index >= i+2 )
        //     {
        //         var token1:TokenInfo = tokens[i+1]
        //         var token2:TokenInfo = tokens[i+2]
        //         if(token1.type == TokenTypes.StringLiteral && 
        //         LuaParse.lp.consume(']',token2,TokenTypes.Punctuator))
        //         {
        //             var x = parseInt(token1.value);
        //              if(isNaN(parseInt(token1.value)))
        //              {
        //                 varName1 += "[\""+ token1.value +"\"]"
        //                 varName2 += "."+token1.value;
        //                  i+=2;
        //                  continue;
        //              }else
        //              {
        //                   i+=2;
        //                  varName1 += "[\""+ token1.value +"\"]"
        //                 varName2 += "[\""+ token1.value +"\"]"
        //                 continue;
        //              }
        //         }
        //     }
        //     if(token.type == TokenTypes.StringLiteral)
        //      {
        //          var value = "\""+ token.value  + "\"";
        //           varName1 +=  value;
        //         varName2 +=  value
        //      }else
        //      {
        //         varName1 += token.value;
        //         varName2 += token.value;
        //      }
        // }
        //  console.log(varName1)
        //         this.name = varName1;
        //         if(varName1 == varName2) {
        // console.log("name1:" + varName1)
        //         }else
        //         {
        //         console.log("name1:" + varName1)
        //         console.log("name2:" + varName2)    
        //         }
    }
    /**
     * 添加参数名
     */
    addParam(param) {
        //需要检查是否有重复的参数名
        for (var i = 0; i < this.params.length; i++) {
            if (this.params[i] === param) {
                return i + 1;
            }
        }
        this.params.push(param);
        return -1;
    }
    setComments(comments) {
        this.comments = this.comments.concat(comments);
    }
    getComments() {
        return this.comments;
    }
}
exports.LuaInfo = LuaInfo;
var LuaInfoType;
(function (LuaInfoType) {
    /** 字段 */
    LuaInfoType[LuaInfoType["Field"] = 1] = "Field";
    /** Table */
    LuaInfoType[LuaInfoType["Table"] = 2] = "Table";
    /** 方法 function xxx */
    LuaInfoType[LuaInfoType["Function"] = 3] = "Function";
    /**模块方法function xx:xxx() end */
    LuaInfoType[LuaInfoType["moduleFunction"] = 5] = "moduleFunction";
    /**参数 */
    LuaInfoType[LuaInfoType["Param"] = 6] = "Param";
    /**匿名函数 */
    LuaInfoType[LuaInfoType["AnonymousFunction"] = 7] = "AnonymousFunction";
    /** for 循环 number */
    LuaInfoType[LuaInfoType["FOR_I"] = 8] = "FOR_I";
    /** for 循环 pairs */
    LuaInfoType[LuaInfoType["FOR_PAIRS"] = 9] = "FOR_PAIRS";
    LuaInfoType[LuaInfoType["FunctionCall1"] = 10] = "FunctionCall1";
    /**返回值 */
    LuaInfoType[LuaInfoType["RETURN"] = 11] = "RETURN";
    LuaInfoType[LuaInfoType["WHILE"] = 12] = "WHILE";
    LuaInfoType[LuaInfoType["ROOT"] = 13] = "ROOT";
    LuaInfoType[LuaInfoType["IF"] = 14] = "IF";
    LuaInfoType[LuaInfoType["ELSEIF"] = 15] = "ELSEIF";
    LuaInfoType[LuaInfoType["ELSE"] = 16] = "ELSE";
    LuaInfoType[LuaInfoType["Number"] = 17] = "Number";
    LuaInfoType[LuaInfoType["BOOLEAN"] = 18] = "BOOLEAN";
    LuaInfoType[LuaInfoType["STRING"] = 19] = "STRING";
    LuaInfoType[LuaInfoType["NIL"] = 20] = "NIL";
    LuaInfoType[LuaInfoType["Vararg"] = 21] = "Vararg";
    LuaInfoType[LuaInfoType["Bracket_M"] = 22] = "Bracket_M";
})(LuaInfoType = exports.LuaInfoType || (exports.LuaInfoType = {}));
/**
 * 提示
 */
class LuaComment {
    constructor(content, range, isLong) {
        //1 短注释
        //2 长注释
        this.isLong = false;
        this.content = null;
        this.range = null;
        this.content = content;
        this.range = range;
        this.isLong = isLong;
    }
}
exports.LuaComment = LuaComment;
class TokenInfo {
    constructor() {
        this.type = TokenTypes.EOF;
        this.value = '<eof>';
        this.line = 0;
        this.lineStart = 0;
        this.range = null;
        this.error = null;
        this.delimiter = null;
        this.enddelimiter = null;
        this.comments = new Array();
    }
    addAfterComment(comment) {
        if (this.aftecomments == null) {
            this.aftecomments = new Array();
        }
        this.aftecomments.push(comment);
    }
    addComment(comment) {
        this.comments.push(comment);
    }
}
exports.TokenInfo = TokenInfo;
class LuaRange {
    constructor(start, end) {
        this.start = start;
        this.end = end;
    }
}
exports.LuaRange = LuaRange;
var TokenTypes;
(function (TokenTypes) {
    TokenTypes[TokenTypes["EOF"] = 1] = "EOF";
    TokenTypes[TokenTypes["StringLiteral"] = 2] = "StringLiteral";
    TokenTypes[TokenTypes["Keyword"] = 3] = "Keyword";
    TokenTypes[TokenTypes["Identifier"] = 4] = "Identifier";
    TokenTypes[TokenTypes["NumericLiteral"] = 5] = "NumericLiteral";
    TokenTypes[TokenTypes["Punctuator"] = 6] = "Punctuator";
    TokenTypes[TokenTypes["BooleanLiteral"] = 7] = "BooleanLiteral";
    TokenTypes[TokenTypes["NilLiteral"] = 8] = "NilLiteral";
    TokenTypes[TokenTypes["VarargLiteral"] = 9] = "VarargLiteral";
    TokenTypes[TokenTypes["Wrap"] = 10] = "Wrap";
    TokenTypes[TokenTypes["Tab"] = 11] = "Tab";
})(TokenTypes = exports.TokenTypes || (exports.TokenTypes = {}));
var LuaErrorEnum;
(function (LuaErrorEnum) {
    LuaErrorEnum[LuaErrorEnum["unexpected"] = 0] = "unexpected";
    LuaErrorEnum[LuaErrorEnum["expected"] = 1] = "expected";
    LuaErrorEnum[LuaErrorEnum["unfinishedString"] = 2] = "unfinishedString";
    LuaErrorEnum[LuaErrorEnum["malformedNumber"] = 3] = "malformedNumber";
    LuaErrorEnum[LuaErrorEnum["invalidVar"] = 4] = "invalidVar";
    LuaErrorEnum[LuaErrorEnum["expectedToken"] = 5] = "expectedToken";
    LuaErrorEnum[LuaErrorEnum["unoperator"] = 6] = "unoperator";
})(LuaErrorEnum = exports.LuaErrorEnum || (exports.LuaErrorEnum = {}));
class LuaError {
    constructor(type, msg) {
        this.type = type;
        this.msg = msg;
    }
}
exports.LuaError = LuaError;
var LuaInfoTypeValue;
(function (LuaInfoTypeValue) {
    LuaInfoTypeValue[LuaInfoTypeValue["NUMBER"] = 0] = "NUMBER";
    LuaInfoTypeValue[LuaInfoTypeValue["BOOL"] = 1] = "BOOL";
    LuaInfoTypeValue[LuaInfoTypeValue["STRING"] = 2] = "STRING";
    LuaInfoTypeValue[LuaInfoTypeValue["ANY"] = 3] = "ANY";
    LuaInfoTypeValue[LuaInfoTypeValue["NIL"] = 4] = "NIL";
    LuaInfoTypeValue[LuaInfoTypeValue["Table"] = 5] = "Table";
    LuaInfoTypeValue[LuaInfoTypeValue["Function"] = 6] = "Function";
})(LuaInfoTypeValue = exports.LuaInfoTypeValue || (exports.LuaInfoTypeValue = {}));
//# sourceMappingURL=TokenInfo.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const TokenInfo_1 = require("../../TokenInfo");
//import {CLog} from './Utils'
class LuaFormatParseTool {
    constructor(text) {
        //private lp:LuaParse;
        //当前解析到的位置
        this.index = 0;
        //文本总长度
        this.length = 0;
        this.input = "";
        //当前解析的行
        this.line = 0;
        //当前行开始的开始的位置
        this.lineStart = 0;
        //标示符 或者 关键字 开始的 位置
        this.tokenStart = 0;
        this.formatComent = "";
        //方法缩进
        this.funTabAdd = false;
        this.funTabMCount = -1;
        this.ignoreTabAdd = false;
        this.Bracket_B = new Array();
        this.formatTabCount = null;
        this.isDddTab = false;
        this.EOLIndex = 0;
        // private isspaceTab: boolean = true;
        this.spaceCount = 0;
        this.spaceIndex = -1;
        this.Reset(text);
        this.tokens = new Array();
        while (true) {
            var token = this.lex();
            if (token.error != null) {
                this.formatComent = text;
                return;
            }
            if (token.type == TokenInfo_1.TokenTypes.EOF) {
                token.value = "";
                this.tokens.push(token);
                break;
            }
            if (token.type == TokenInfo_1.TokenTypes.Keyword && (token.value == "else" ||
                token.value == "elseif")) {
                var tindex = this.tokens.length - 1;
                while (true) {
                    if (tindex >= 0) {
                        if (this.tokens[tindex].type == TokenInfo_1.TokenTypes.Wrap) {
                            break;
                        }
                        else if (this.tokens[tindex].type == TokenInfo_1.TokenTypes.Tab) {
                            tindex--;
                        }
                        else {
                            var Wraptoken = new TokenInfo_1.TokenInfo();
                            Wraptoken.type = TokenInfo_1.TokenTypes.Wrap;
                            this.tokens.push(Wraptoken);
                            Wraptoken.index = this.tokens.length;
                            break;
                        }
                    }
                    else {
                        break;
                    }
                }
            }
            if (this.spaceCount > 0 && token.type == TokenInfo_1.TokenTypes.Tab) {
                var ttoken = new TokenInfo_1.TokenInfo();
                ttoken.type = TokenInfo_1.TokenTypes.Tab;
                this.tokens.push(ttoken);
            }
            this.tokens.push(token);
            token.index = this.tokens.length;
            if (token.type != TokenInfo_1.TokenTypes.Wrap) {
                //去掉空格
                this.skipWhiteSpace(true);
                if (this.tokens[this.tokens.length - 1].type != TokenInfo_1.TokenTypes.Wrap) {
                    //先判断有没有注释
                    while (45 == this.input.charCodeAt(this.index) &&
                        45 == this.input.charCodeAt(this.index + 1) &&
                        this.input.charAt(this.index + 2) != '[') {
                        //如果有注释 将tab 全部删除
                        while (true) {
                            var tindex = this.tokens.length - 1;
                            if (this.tokens[tindex].type == TokenInfo_1.TokenTypes.Tab) {
                                this.tokens.pop();
                            }
                            else {
                                break;
                            }
                        }
                        //获取评论
                        this.scanComment(token, true);
                        if (this.tokens.length > 0) {
                            if (this.tokens[this.tokens.length - 1].type != TokenInfo_1.TokenTypes.Wrap) {
                                var wtoken = new TokenInfo_1.TokenInfo();
                                wtoken.type = TokenInfo_1.TokenTypes.Wrap;
                                this.tokens.push(wtoken);
                            }
                        }
                        break;
                    }
                }
            }
            else {
                var x = 1;
            }
        }
        var content = "";
        var newWrap = false;
        for (var index = 0; index < this.tokens.length; index++) {
            var token = this.tokens[index];
            var nextToken = null;
            if (index < this.tokens.length - 1) {
                nextToken = this.tokens[index + 1];
            }
            var upToken = null;
            if (index > 0) {
                upToken = this.tokens[index - 1];
            }
            //方法缩进
            if (token.type == TokenInfo_1.TokenTypes.Keyword && token.value == "function") {
                this.funTabAdd = true;
            }
            if (this.funTabAdd) {
                if (token.type == TokenInfo_1.TokenTypes.Punctuator && token.value == "(") {
                    if (this.funTabMCount == -1) {
                        this.funTabMCount = 1;
                    }
                    else {
                        this.funTabMCount++;
                    }
                }
                if (token.type == TokenInfo_1.TokenTypes.Punctuator && token.value == ")") {
                    this.funTabMCount--;
                    if (this.funTabMCount == 0) {
                        this.funTabMCount = -1;
                        this.formatTabCount++;
                        this.funTabAdd = false;
                    }
                }
            }
            //end 减少一个tab
            if (token.type == TokenInfo_1.TokenTypes.Keyword && (token.value == "end" ||
                token.value == "until")) {
                this.formatTabCount--;
                if (this.formatTabCount >= 0) {
                    var tabCode = content.charCodeAt(content.length - 1);
                    if (tabCode == 9) {
                        content = content.slice(0, content.length - 1);
                    }
                }
            }
            if (token.type == TokenInfo_1.TokenTypes.Keyword && (token.value == "do" ||
                token.value == "then" ||
                token.value == "repeat")) {
                if (this.ignoreTabAdd) {
                    this.ignoreTabAdd = false;
                }
                else {
                    this.formatTabCount++;
                }
            }
            if (token.type == TokenInfo_1.TokenTypes.Wrap) {
                if (!newWrap) {
                    content += "\n";
                    //  newWrap = true
                }
                if (nextToken != null && nextToken.type == TokenInfo_1.TokenTypes.Keyword &&
                    (nextToken.value == "else" || nextToken.value == "elseif")) {
                    if (nextToken.comments.length > 0) {
                        for (var i = 0; i < this.formatTabCount; i++) {
                            content += "\t";
                        }
                        nextToken.comments.forEach(comment => {
                            if (comment.isLong) {
                                content += "--[[" + comment.content + "]]\n";
                            }
                            else {
                                content += "--" + comment.content + "\n";
                            }
                        });
                    }
                    this.formatTabCount--;
                }
                this.isDddTab = false;
                for (var i = 0; i < this.formatTabCount; i++) {
                    content += "\t";
                }
                if (nextToken != null && nextToken.type == TokenInfo_1.TokenTypes.Keyword &&
                    (nextToken.value == "else")) {
                    this.formatTabCount++;
                }
            }
            else if (token.type == TokenInfo_1.TokenTypes.Tab) {
                if (this.isDddTab) {
                    content += '\t';
                }
            }
            else {
                this.isDddTab = true;
                //{ /n
                if (token.type == TokenInfo_1.TokenTypes.Punctuator && token.value == "{") {
                    if (nextToken != null && nextToken.type == TokenInfo_1.TokenTypes.Wrap) {
                        this.Bracket_B.push(true);
                        this.formatTabCount++;
                    }
                    else {
                        this.Bracket_B.push(false);
                    }
                }
                if (this.Bracket_B.length && token.type == TokenInfo_1.TokenTypes.Punctuator && token.value == "}") {
                    if (this.Bracket_B[this.Bracket_B.length - 1]) {
                        this.formatTabCount--;
                        if (upToken != null && upToken.type != TokenInfo_1.TokenTypes.Wrap) {
                            //手动换行
                            content += "\n";
                            for (var i = 0; i < this.formatTabCount; i++) {
                                content += "\t";
                            }
                        }
                        else if (upToken != null && upToken.type == TokenInfo_1.TokenTypes.Wrap) {
                            content = content.slice(0, content.length - 1);
                        }
                    }
                    this.Bracket_B.pop();
                }
                //前注释
                if (token.comments.length > 0 && !(token.type == TokenInfo_1.TokenTypes.Keyword && (token.value == "else" || token.value == "elseif"))) {
                    token.comments.forEach(comment => {
                        if (comment.isLong) {
                            content += "--[[" + comment.content + "]]\n";
                        }
                        else {
                            if (token.type == TokenInfo_1.TokenTypes.Keyword && (token.value == "end" ||
                                token.value == "until")) {
                                content += "\t";
                            }
                            content += "--" + comment.content + "\n";
                        }
                        for (var i = 0; i < this.formatTabCount; i++) {
                            content += "\t";
                        }
                    });
                }
                if ((nextToken != null && nextToken.type != TokenInfo_1.TokenTypes.Tab &&
                    nextToken.type != TokenInfo_1.TokenTypes.Wrap &&
                    !(token.type == TokenInfo_1.TokenTypes.Identifier && nextToken.type == TokenInfo_1.TokenTypes.Punctuator && nextToken.value == '[') &&
                    !(nextToken.value == '(' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == ',' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == ')' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == ']' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == ':' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == '}' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == ';' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(nextToken.value == '.' && nextToken.type == TokenInfo_1.TokenTypes.Punctuator)) &&
                    !(token.value == '#' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(token.value == '.' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(token.value == '(' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(token.value == ':' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(token.value == '{' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    !(token.value == '[' && token.type == TokenInfo_1.TokenTypes.Punctuator) &&
                    (token.aftecomments == null ||
                        (token.aftecomments != null && token.aftecomments.length == 0))) {
                    content += this.getTokenValue(token) + " ";
                }
                else {
                    //如果为换行 和 \t 就直接添加
                    content += this.getTokenValue(token);
                }
                if (token.aftecomments != null && token.aftecomments.length > 0) {
                    token.aftecomments.forEach(comment => {
                        content += comment.content;
                    });
                    // for (var i = 0; i < this.formatTabCount; i++) {
                    //   content += "\t"
                    // }
                }
            }
            if (token.type != TokenInfo_1.TokenTypes.Tab && token.type != TokenInfo_1.TokenTypes.Wrap) {
                newWrap = false;
            }
        }
        this.formatComent = content;
    }
    getTokenValue(token) {
        var content = "";
        //字符串 添加"" '' --[[]]
        if (token.type == TokenInfo_1.TokenTypes.StringLiteral && token.delimiter != null) {
            // if (token.delimiter == "[[") {
            //   content += '[[' + token.value + ']]'
            // }
            // else {
            //   token.value = token.value.replace(/\\/g, "\\\\");
            //   token.value = token.value.replace(/\t/g, "\\t");
            //   token.value = token.value.replace(/\n/g, "\\n");
            //   token.value = token.value.replace(/\f/g, "\\f");
            //   token.value = token.value.replace(/\r/g, "\\r");
            //   token.value = token.value.replace(/\'/g, "\\'");
            //   token.value = token.value.replace(/\"/g, '\\"');
            //   content += token.delimiter + token.value + token.enddelimiter;
            // }
            content += token.delimiter + token.value + token.enddelimiter;
        }
        else {
            content += token.value;
        }
        return content;
    }
    // 重置
    Reset(text) {
        this.input = text;
        //检查第一个字符是否为#
        this.index = 0;
        this.length = this.input.length;
        this.line = 0;
        this.lineStart = 0;
        if (this.input.charAt(0) == '#') {
            while (true) {
                if (this.consumeEOL(true)) {
                    break;
                }
                else {
                    this.index++;
                }
            }
        }
    }
    lex() {
        var token = new TokenInfo_1.TokenInfo();
        //去掉空格
        this.skipWhiteSpace(true);
        var isComment = false;
        //先判断有没有注释
        while (45 == this.input.charCodeAt(this.index) &&
            45 == this.input.charCodeAt(this.index + 1)) {
            //获取评论
            this.scanComment(token, false);
            isComment = true;
            this.skipWhiteSpace(false);
        }
        if (isComment) {
            if (this.tokens.length > 0) {
                if (this.tokens[this.tokens.length - 1].type != TokenInfo_1.TokenTypes.Wrap) {
                    var wtoken = new TokenInfo_1.TokenInfo();
                    wtoken.type = TokenInfo_1.TokenTypes.Wrap;
                    this.tokens.push(wtoken);
                }
            }
        }
        //没有下一个
        if (this.index >= this.length) {
            token.type = TokenInfo_1.TokenTypes.EOF;
            token.value = '<eof>';
            token.line = this.line;
            token.lineStart = this.lineStart;
            token.range = new TokenInfo_1.LuaRange(this.index, this.index);
            return token;
        }
        //开始正式解析变量了
        var charCode = this.input.charCodeAt(this.index);
        var next = this.input.charCodeAt(this.index + 1);
        // Memorize the range index where the token begins.
        this.tokenStart = this.index;
        if (this.isIdentifierStart(charCode)) {
            return this.scanIdentifierOrKeyword(token);
        }
        switch (charCode) {
            case 39:
            case 34:
                return this.scanStringLiteral(token);
            // 0-9
            case 48:
            case 49:
            case 50:
            case 51:
            case 52:
            case 53:
            case 54:
            case 55:
            case 56:
            case 57:
                return this.scanNumericLiteral(token);
            case 46:
                // If the dot is followed by a digit it's a float.
                if (this.isDecDigit(next))
                    return this.scanNumericLiteral(token);
                if (46 === next) {
                    if (46 === this.input.charCodeAt(this.index + 2)) {
                        return this.scanVarargLiteral(token);
                    }
                    return this.scanPunctuator('..', token);
                }
                return this.scanPunctuator('.', token);
            case 61:
                if (61 === next) {
                    return this.scanPunctuator('==', token);
                }
                return this.scanPunctuator('=', token);
            case 62:
                if (61 === next) {
                    return this.scanPunctuator('>=', token);
                }
                if (62 === next) {
                    return this.scanPunctuator('>>', token);
                }
                return this.scanPunctuator('>', token);
            case 60:
                if (60 === next) {
                    return this.scanPunctuator('<<', token);
                }
                if (61 === next) {
                    return this.scanPunctuator('<=', token);
                }
                return this.scanPunctuator('<', token);
            case 126:
                if (61 === next) {
                    return this.scanPunctuator('~=', token);
                }
                return this.scanPunctuator('~', token);
            case 58:
                if (58 === next) {
                    //goto 标签
                    return this.scanPunctuator('::', token);
                }
                return this.scanPunctuator(':', token);
            case 91:
                // Check for a multiline string, they begin with [= or [[
                if (91 === next || 61 === next) {
                    //长字符串
                    return this.scanLongStringLiteral(token);
                }
                return this.scanPunctuator('[', token);
            case 47:
                // Check for integer division op (//)
                if (47 === next)
                    return this.scanPunctuator('//', token);
                return this.scanPunctuator('/', token);
            // * ^ % , { } ] ( ) ; & # - + |
            case 42:
            case 94:
            case 37:
            case 44:
            case 123:
            case 124:
            case 125:
            case 93:
            case 40:
            case 41:
            case 59:
            case 38:
            case 35:
            case 45:
            case 43:
                return this.scanPunctuator(this.input.charAt(this.index), token);
        }
        token.type = TokenInfo_1.TokenTypes.Punctuator;
        token.value = this.input.charAt(this.index);
        return this.unexpected(token);
    }
    /**
     * 解析 ...
     */
    scanVarargLiteral(token) {
        this.index += 3;
        token.type = TokenInfo_1.TokenTypes.VarargLiteral;
        token.value = '...';
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
        return token;
    }
    /***
     * 解析数字
     *  */
    scanNumericLiteral(token) {
        var character = this.input.charAt(this.index);
        var next = this.input.charAt(this.index + 1);
        //这里需要检查 是否为 ..
        var value = 0;
        if ('0' === character && 'xX'.indexOf(next || null) >= 0) {
            value = this.readHexLiteral(token);
            if (token.error != null) {
                return token;
            }
        }
        else if (this.input.charAt(this.index + 2) != null && next === '.' && this.input.charAt(this.index + 2) === '.') {
            this.index++;
            value = parseInt(character);
        }
        else {
            if (next)
                value = this.readDecLiteral(token);
            else {
                value = parseInt(character);
                this.index++;
            }
            if (token.error != null) {
                return token;
            }
        }
        token.type = TokenInfo_1.TokenTypes.NumericLiteral;
        token.value = value;
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
        return token;
    }
    /**
     * 读取非十进制数
     */
    readHexLiteral(token) {
        var fraction = 0; // defaults to 0 as it gets summed
        var binaryExponent = 1; // defaults to 1 as it gets multiplied
        var binarySign = 1; // positive
        var digit;
        var fractionStart;
        var exponentStart;
        var digitStart = this.index += 2; // Skip 0x part
        //这里保证 16 进制 的最小 字符串长度 例: 0x:  那么就是违法的
        if (!this.isHexDigit(this.input.charCodeAt(this.index))) {
            token.line = this.line;
            token.lineStart = this.lineStart;
            token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
            token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.malformedNumber, this.input.slice(this.tokenStart, this.index));
            return 0;
        }
        while (this.isHexDigit(this.input.charCodeAt(this.index)))
            this.index++;
        // Convert the hexadecimal digit to base 10.
        digit = parseInt(this.input.slice(digitStart, this.index), 16);
        // Fraction part i optional.
        if ('.' === this.input.charAt(this.index)) {
            fractionStart = ++this.index;
            while (this.isHexDigit(this.input.charCodeAt(this.index)))
                this.index++;
            fraction = this.input.slice(fractionStart, this.index);
            // Empty fraction parts should default to 0, others should be converted
            // 0.x form so we can use summation at the end.
            fraction = (fractionStart === this.index) ? 0
                : parseInt(fraction, 16) / Math.pow(16, this.index - fractionStart);
        }
        // Binary exponents are optional
        if ('pP'.indexOf(this.input.charAt(this.index) || null) >= 0) {
            this.index++;
            // Sign part is optional and defaults to 1 (positive).
            if ('+-'.indexOf(this.input.charAt(this.index) || null) >= 0)
                binarySign = ('+' === this.input.charAt(this.index++)) ? 1 : -1;
            exponentStart = this.index;
            // The binary exponent sign requires a decimal digit.
            if (!this.isDecDigit(this.input.charCodeAt(this.index))) {
                token.line = this.line;
                token.lineStart = this.lineStart;
                token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
                token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.malformedNumber, this.input.slice(this.tokenStart, this.index));
                return 0;
            }
            while (this.isDecDigit(this.input.charCodeAt(this.index)))
                this.index++;
            binaryExponent = this.input.slice(exponentStart, this.index);
            // Calculate the binary exponent of the number.
            binaryExponent = Math.pow(2, binaryExponent * binarySign);
        }
        return (digit + fraction) * binaryExponent;
    }
    /**
     * 解析十进制数
     */
    readDecLiteral(token) {
        while (this.isDecDigit(this.input.charCodeAt(this.index)))
            this.index++;
        // Fraction part is optional
        if ('.' === this.input.charAt(this.index)) {
            this.index++;
            //这里还需要进行判断 是否 为 ..
            // Fraction part defaults to 0
            while (true) {
                var codeNumber = this.input.charCodeAt(this.index);
                if (this.isDecDigit(codeNumber)) {
                    this.index++;
                }
                else if (this.isWhiteSpace(codeNumber, false) || this.isLineTerminator(codeNumber)) {
                    break;
                }
                else if (';' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('+' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('-' === this.input.charAt(this.index)) {
                    break;
                }
                else if (',' === this.input.charAt(this.index)) {
                    break;
                }
                else if (']' === this.input.charAt(this.index)) {
                    break;
                }
                else if (')' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('}' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('*' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('/' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('^' === this.input.charAt(this.index)) {
                    break;
                }
                else if ('eE'.indexOf(this.input.charAt(this.index)) >= 0) {
                    break;
                }
                else {
                    this.index++;
                    token.line = this.line;
                    token.lineStart = this.lineStart;
                    token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
                    token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.malformedNumber, this.input.slice(this.tokenStart, this.index));
                    return;
                }
            }
        }
        // Exponent part is optional.
        if ('eE'.indexOf(this.input.charAt(this.index) || null) >= 0) {
            this.index++;
            // Sign part is optional.
            if ('+-'.indexOf(this.input.charAt(this.index) || null) >= 0)
                this.index++;
            // An exponent is required to contain at least one decimal digit.
            if (!this.isDecDigit(this.input.charCodeAt(this.index))) {
                token.line = this.line;
                token.lineStart = this.lineStart;
                token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
                token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.malformedNumber, this.input.slice(this.tokenStart, this.index));
                return;
            }
            while (this.isDecDigit(this.input.charCodeAt(this.index)))
                this.index++;
        }
        var chatat = this.input.charAt(this.index);
        var chatat1 = this.input.charAt(this.index + 1);
        if (this.isWhiteSpace(this.input.charCodeAt(this.index), false) ||
            this.isLineTerminator(this.input.charCodeAt(this.index)) ||
            ';' === chatat ||
            '' === chatat ||
            ']' === chatat ||
            ')' === chatat ||
            ',' === chatat ||
            '}' === chatat ||
            chatat == '+' ||
            chatat == '-' ||
            chatat == '*' ||
            chatat == '/' ||
            chatat == '>' ||
            chatat == '>' ||
            chatat == ',' ||
            chatat == '}' ||
            chatat == '^' ||
            ('=' == chatat && '=' == chatat1) ||
            ('>' == chatat && '=' == chatat1) ||
            ('<' == chatat && '=' == chatat1) ||
            ('~' == chatat && '=' == chatat1)) {
            return parseFloat(this.input.slice(this.tokenStart, this.index));
        }
        else {
            this.index++;
            token.line = this.line;
            token.lineStart = this.lineStart;
            token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
            token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.malformedNumber, this.input.slice(this.tokenStart, this.index));
        }
    }
    /**
   * 获取注释
   */
    scanComment(token, afterComment) {
        //  this.tokenStart = this.index;
        this.index += 2;
        //当前字符
        var character = this.input.charAt(this.index);
        //注释内容
        var content;
        // 是否为长注释  --[[  长注释 ]]
        var isLong = false;
        var commentStart = this.index;
        var lineStartComment = this.lineStart;
        var lineComment = this.line;
        if ('[' == character) {
            content = this.readLongString(null);
            if (content == false) {
                content = character;
            }
            else {
                isLong = true;
            }
        }
        if (!isLong) {
            var isLineT = false;
            var cindex = 0;
            while (this.index < this.length) {
                var charCode = this.input.charCodeAt(this.index);
                if (this.isLineTerminator(charCode)) {
                    isLineT = true;
                    var peekCharCode = this.input.charCodeAt(this.index + 1);
                    //判断是否换行
                    if (10 === charCode && 13 === peekCharCode) {
                        this.index++;
                    }
                    if (13 === charCode && 10 === peekCharCode) {
                        this.index++;
                    }
                    if (cindex == 0) {
                        cindex = this.index;
                    }
                    this.line++;
                    this.lineStart = ++this.index;
                    if (afterComment) {
                        break;
                    }
                }
                else {
                    if (isLineT) {
                        break;
                    }
                    this.index++;
                }
            }
            cindex = cindex == 0 ? this.index : cindex;
            if (afterComment) {
                content = this.input.slice(token.range.end, cindex);
            }
            else {
                content = this.input.slice(commentStart, cindex);
            }
        }
        var range = {
            start: { line: lineComment, character: lineStartComment },
            end: { line: this.lineStart, character: cindex - this.lineStart }
        };
        if (afterComment) {
            token.addAfterComment(new TokenInfo_1.LuaComment(content, range, isLong));
        }
        else {
            token.addComment(new TokenInfo_1.LuaComment(content, range, isLong));
        }
    }
    /**
      * 获取长字符串
      *  * return
      *          为长字符串 content
      *          不为长字符串  false
      */
    readLongString(token) {
        //多少个  等于符号
        var level = 0;
        //注释内容  
        var content = '';
        var terminator = false;
        var character = null;
        var stringStart = 0;
        var beginDIndex = this.index;
        this.index++; //将位置移到 需要判断的字符  上已阶段以及判断到了 [
        // 获取等于符号的多少
        while ('=' === this.input.charAt(this.index + level)) {
            level++;
        }
        // 如果是[ 那么继续 如果不为 [ 那么 直接返回
        if ('[' !== this.input.charAt(this.index + level)) {
            return false;
        }
        this.index += level + 1;
        if (this.isLineTerminator(this.input.charCodeAt(this.index))) {
            this.consumeEOL(false);
        }
        //注释开始的位置
        stringStart = this.index;
        if (token) {
            token.delimiter = this.input.slice(beginDIndex, stringStart);
        }
        // 读取注释内容
        while (this.index < this.length) {
            while (true) {
                if (this.isLineTerminator(this.input.charCodeAt(this.index))) {
                    this.consumeEOL(false);
                }
                else {
                    break;
                }
            }
            var endDindex = this.index;
            character = this.input.charAt(this.index++);
            if (']' == character) {
                terminator = true;
                for (var i = 0; i < level; i++) {
                    if ('=' !== this.input.charAt(this.index + i)) {
                        terminator = false;
                    }
                }
                if (']' !== this.input.charAt(this.index + level)) {
                    terminator = false;
                }
                if (token) {
                    var endDstr = this.input.slice(endDindex, this.index + level + 1);
                    token.enddelimiter = endDstr;
                }
            }
            if (terminator)
                break;
        }
        if (terminator) {
            content += this.input.slice(stringStart, this.index - 1);
            this.index += level + 1;
            return content;
        }
        return false;
    }
    /**
      * 读取转义符
      */
    readEscapeSequence() {
        var sequenceStart = this.index;
        switch (this.input.charAt(this.index)) {
            // Lua allow the following escape sequences.
            // We don't escape the bell sequence.
            case 'n':
                this.index++;
                return '\n';
            case 'r':
                this.index++;
                return '\r';
            case 't':
                this.index++;
                return '\t';
            case 'v':
                this.index++;
                return '\x0B';
            case 'b':
                this.index++;
                return '\b';
            case 'f':
                this.index++;
                return '\f';
            // Skips the following span of white-space.
            case 'z':
                this.index++;
                this.skipWhiteSpace(false);
                return '';
            // Byte representation should for now be returned as is.
            case 'x':
                // \xXX, where XX is a sequence of exactly two hexadecimal digits
                if (this.isHexDigit(this.input.charCodeAt(this.index + 1)) &&
                    this.isHexDigit(this.input.charCodeAt(this.index + 2))) {
                    this.index += 3;
                    // Return it as is, without translating the byte.
                    return '\\' + this.input.slice(sequenceStart, this.index);
                }
                return '\\' + this.input.charAt(this.index++);
            default:
                // \ddd, where ddd is a sequence of up to three decimal digits.
                if (this.isDecDigit(this.input.charCodeAt(this.index))) {
                    while (this.isDecDigit(this.input.charCodeAt(++this.index)))
                        ;
                    return '\\' + this.input.slice(sequenceStart, this.index);
                }
                // Simply return the \ as is, it's not escaping any sequence.
                return this.input.charAt(this.index++);
        }
    }
    scanStringLiteral(token) {
        token.delimiter = this.input.charAt(this.index);
        token.enddelimiter = token.delimiter;
        var delimiter = this.input.charCodeAt(this.index++);
        var stringStart = this.index;
        var str = '';
        var charCode;
        while (this.index < this.length) {
            charCode = this.input.charCodeAt(this.index++);
            // ===  ' or "
            if (delimiter === charCode)
                break;
            if (92 === charCode) {
                str += this.input.slice(stringStart, this.index - 1) + '\\'; //+ this.readEscapeSequence();
                // str += this.input.slice(stringStart, this.index - 1) + this.readEscapeSequence();
                stringStart = this.index;
            }
            else if (this.index >= this.length || this.isLineTerminator(charCode)) {
                str += this.input.slice(stringStart, this.index - 1);
                token.line = this.line;
                token.lineStart = this.lineStart;
                token.range = new TokenInfo_1.LuaRange(stringStart, this.index);
                token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.unfinishedString, str + String.fromCharCode(charCode));
                return token;
            }
        }
        str += this.input.slice(stringStart, this.index - 1);
        token.type = TokenInfo_1.TokenTypes.StringLiteral;
        token.value = str;
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.range = new TokenInfo_1.LuaRange(stringStart, this.index);
        return token;
    }
    /**
     * 解析长字符串
     */
    scanLongStringLiteral(token) {
        var str = this.readLongString(token);
        // Fail if it's not a multiline literal.
        if (false === str) {
            token.line = this.line;
            token.lineStart = this.lineStart;
            token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
            token.error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.expected, '[ 长字符串未 结尾   [[  ]]');
        }
        else {
            token.type = TokenInfo_1.TokenTypes.StringLiteral;
            token.value = str;
            token.line = this.line;
            token.lineStart = this.lineStart;
            token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
        }
        return token;
    }
    /**
     * 异常
     */
    unexpected(token) {
        var error = new TokenInfo_1.LuaError(TokenInfo_1.LuaErrorEnum.unexpected, token.value);
        var value = token.value + "";
        token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.tokenStart + value.length);
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.error = error;
        return token;
    }
    /**
      * 解析标点
      */
    scanPunctuator(value, token) {
        this.index += value.length;
        token.type = TokenInfo_1.TokenTypes.Punctuator;
        token.value = value;
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
        return token;
    }
    /**
     * 跳过空格
     */
    skipWhiteSpace(isAddWrapToken) {
        while (this.index < this.length) {
            var charCode = this.input.charCodeAt(this.index);
            //空格 解析
            if (this.isWhiteSpace(charCode, true)) {
                this.index++;
            }
            else if (!this.consumeEOL(isAddWrapToken)) {
                break;
            }
        }
    }
    /**
     * 解析换行
     */
    consumeEOL(isAddWrapToken) {
        var charCode = this.input.charCodeAt(this.index);
        var peekCharCode = this.input.charCodeAt(this.index + 1);
        //判断是否换行
        if (this.isLineTerminator(charCode)) {
            if (10 === charCode && 13 === peekCharCode)
                this.index++;
            if (13 === charCode && 10 === peekCharCode)
                this.index++;
            this.line++;
            this.lineStart = ++this.index;
            if (isAddWrapToken) {
                if (this.EOLIndex < this.index) {
                    var currenToken = null;
                    if (this.tokens.length > 0) {
                        currenToken = this.tokens[this.tokens.length - 1];
                    }
                    // if (currenToken == null || currenToken.type != TokenTypes.Wrap) {
                    var token = new TokenInfo_1.TokenInfo();
                    token.type = TokenInfo_1.TokenTypes.Wrap;
                    this.tokens.push(token);
                    // }
                }
            }
            this.EOLIndex = this.index;
            return true;
        }
        return false;
    }
    /**
    * 判断是否是空格
    *  */
    isWhiteSpace(charCode, isAddTab) {
        if (9 === charCode) {
            if (this.spaceIndex != this.index) {
                if (isAddTab) {
                    var currenToken = null;
                    if (this.tokens.length > 0)
                        currenToken = this.tokens[this.tokens.length - 1];
                    if (currenToken != null && currenToken.type != TokenInfo_1.TokenTypes.Wrap) {
                        var token = new TokenInfo_1.TokenInfo();
                        token.type = TokenInfo_1.TokenTypes.Tab;
                        this.tokens.push(token);
                    }
                }
            }
            this.spaceIndex = this.index;
            return true;
        }
        if (32 === charCode || 0xB === charCode || 0xC === charCode) {
            if (this.spaceIndex != this.index) {
                this.spaceCount++;
                if (isAddTab) {
                    if (this.spaceCount == 4) {
                        if (this.tokens.length > 0)
                            currenToken = this.tokens[this.tokens.length - 1];
                        if (currenToken != null && currenToken.type != TokenInfo_1.TokenTypes.Wrap) {
                            var token = new TokenInfo_1.TokenInfo();
                            token.type = TokenInfo_1.TokenTypes.Tab;
                            this.tokens.push(token);
                        }
                        this.spaceCount = 0;
                    }
                }
            }
            this.spaceIndex = this.index;
            return true;
        }
        else {
            this.spaceCount = 0;
            this.spaceIndex = this.index;
            return false;
        }
    }
    /**
     * 判断是否换行
     *  */
    isLineTerminator(charCode) {
        return 10 === charCode || 13 === charCode;
    }
    /**
     * 判断是否是关键字
     */
    isKeyword(id) {
        switch (id.length) {
            case 2:
                return 'do' === id || 'if' === id || 'in' === id || 'or' === id;
            case 3:
                return 'and' === id || 'end' === id || 'for' === id || 'not' === id;
            case 4:
                return 'else' === id || 'goto' === id || 'then' === id;
            case 5:
                return 'break' === id || 'local' === id || 'until' === id || 'while' === id;
            case 6:
                return 'elseif' === id || 'repeat' === id || 'return' === id;
            case 8:
                return 'function' === id;
        }
        return false;
    }
    /**
     * 判断是否是一个表示符号的起点是否合法  如 一个变量名 或者一个方法 名   a-z  A-Z   _  只能是这些
     */
    isIdentifierStart(charCode) {
        return (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122) || 95 === charCode;
    }
    /**
     * 判断是否是标示符除 起点外的内容是否合法
     */
    isIdentifierPart(charCode) {
        return (charCode >= 65 && charCode <= 90) || (charCode >= 97 && charCode <= 122) || 95 === charCode || (charCode >= 48 && charCode <= 57);
    }
    /**
     * 判断是否为十进制的数字
     */
    isDecDigit(charCode) {
        return charCode >= 48 && charCode <= 57;
    }
    /**
     * 判断是否为数字     0 - 9  a-z  A-Z
     */
    isHexDigit(charCode) {
        return (charCode >= 48 && charCode <= 57) || (charCode >= 97 && charCode <= 102) || (charCode >= 65 && charCode <= 70);
    }
    /**
    * 解析 表示符号 或者 关键字
    *
    */
    scanIdentifierOrKeyword(token) {
        var value = null;
        var type;
        //循环检查 标志符 或者关键字
        while (this.isIdentifierPart(this.input.charCodeAt(++this.index)))
            ;
        //获取token
        value = this.input.slice(this.tokenStart, this.index);
        if (this.isKeyword(value)) {
            type = TokenInfo_1.TokenTypes.Keyword;
        }
        else if ('true' === value || 'false' === value) {
            type = TokenInfo_1.TokenTypes.BooleanLiteral;
            value = ('true' === value);
        }
        else if ('nil' === value) {
            type = TokenInfo_1.TokenTypes.NilLiteral;
            value = 'nil';
        }
        else {
            type = TokenInfo_1.TokenTypes.Identifier;
        }
        token.type = type;
        token.value = value;
        token.line = this.line;
        token.lineStart = this.lineStart;
        token.range = new TokenInfo_1.LuaRange(this.tokenStart, this.index);
        return token;
    }
}
exports.LuaFormatParseTool = LuaFormatParseTool;
//# sourceMappingURL=LuaFormatParseTool.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
var path = require('path');
const child_process = require("child_process");
class LuacCheck {
    constructor() {
        // this.luaPath = this.getLuacPath()
    }
    parseDiagnostics(data) {
        const diagnostics = [];
        var errorRegex = /.+: .+:([0-9]+): (.+) near.*[<'](.*)['>]/;
        // const errorRegex = /^.*:(\d+):(\d+)-(\d+): \(([EW]?)(\d+)\) (.*)$/mg;
        const matches = data.match(errorRegex);
        if (!matches) {
            return [];
        }
        while (true) {
            const m = errorRegex.exec(data);
            if (!m) {
                break;
            }
            const [, lineStr, columnStr, endColumnStr, type, codeStr, message] = m;
            const line = Number(lineStr) - 1;
            const column = Number(columnStr) - 1;
            const columnEnd = Number(endColumnStr);
            const code = Number(codeStr);
            // const mapSeverity = () => {
            //     switch (type) {
            //         case 'E':
            //             return vscode_languageserver_1.DiagnosticSeverity.Error;
            //         case 'W':
            //             return vscode_languageserver_1.DiagnosticSeverity.Warning;
            //         default:
            //             return vscode_languageserver_1.DiagnosticSeverity.Information;
            //     }
            // };
            var range = new vscode.Range(new vscode.Position(line, column), new vscode.Position(line, columnEnd));
            diagnostics.push({
                range: range,
                severity: vscode.DiagnosticSeverity.Error,
                code,
                source: 'luacheck',
                message
            });
        }
        return diagnostics;
    }
    /**
     * checkLua
     */
    checkLua(uri, documentText) {
        //  var options = {
        //     encoding: 'utf8',
        //     timeout: 0,
        //     maxBuffer: 1024 * 1024,
        //     cwd: this.luaPath,
        //     env: null
        // };
        var exepath = path.join(this.luaPath, "luac.exe");
        var dir = path.dirname(uri.fsPath);
        const process = child_process.spawn(exepath, ['-', '--no-color', '--ranges', '--codes'], {
            cwd: dir
        });
        try {
            var xx = documentText.getText();
            process.stdin.write(xx);
            process.stdin.end();
        }
        catch (err) { }
        process.stdout.on('data', (data) => {
            var xx = data.toString();
            var c = 1;
        });
        process.stderr.on('data', (data) => {
            this.parseDiagnostics(data.toString());
            var c = 1;
        });
        process.on('error', (err) => {
            var xx = err.message;
            var c = 1;
        });
        // const dir = path.dirname(uri.fsPath);
        // var exepath = path.join(this.luaPath,"luac.exe")
        // const process = child_process.spawn("luac.exe" , [uri.fsPath], {
        //     cwd: this.luaPath
        // });
        // // try {
        // //     process.stdin.write(documentText.getText());
        // //     process.stdin.end();
        // // }
        // // catch (err) { }
        // process.stdout.on('data', (data) => {
        //    var xx = data.toString()
        // });
        // process.stderr.on('data', (data) => {
        //     var xx = data.toString()
        //     console.log(xx)
        // });
        // process.on('error', (err) => {
        //     var x = err.message;
        // });
        //  var buff = child_process.spawnSync("luac", ["-e", 'require("formatter")("' + luascriptPath + '")'], options)
        // var result = buff.stdout.toString().trim();
    }
}
exports.LuacCheck = LuacCheck;
//# sourceMappingURL=LuacCheck.js.map
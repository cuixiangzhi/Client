"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const child_process = require("child_process");
const ExtensionManager_1 = require("../../ex/ExtensionManager");
var fs = require('fs');
var path = require('path');
var os = require('os');
function LuaFormat(str) {
    var extensionPath = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.extensionPath;
    var rootPath = path.join(extensionPath, "runtime", "win");
    var exePath = path.join(rootPath, "lua.exe");
    var scriptPath = path.join(rootPath, "temp.lua");
    var cmd = rootPath + " " + scriptPath;
    var options = {
        encoding: 'utf8',
        timeout: 0,
        maxBuffer: 1024 * 1024,
        cwd: rootPath,
        env: null
    };
    try {
        fs.writeFileSync(scriptPath, str);
    }
    catch (err) {
        return str;
    }
    var luascriptPath = scriptPath.replace(/\\/g, "\\\\");
    var buff = child_process.spawnSync("lua.exe", ["-e", 'require("formatter")("' + luascriptPath + '")'], options);
    var result = buff.stdout.toString().trim();
    if (result == "complete") {
        //读取
        var contentText = fs.readFileSync(path.join(scriptPath), 'utf-8');
        return contentText;
    }
    return str;
}
exports.LuaFormat = LuaFormat;
//# sourceMappingURL=LuaFormatChildProcess.js.map
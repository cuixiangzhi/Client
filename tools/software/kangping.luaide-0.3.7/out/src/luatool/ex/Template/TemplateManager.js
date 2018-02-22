"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
'use strict';
var fs = require('fs');
var path = require('path');
var os = require('os');
const ExtensionManager_1 = require("../ExtensionManager");
class TemplateManager {
    constructor() {
        this.paths = new Array();
        this.functionDocs = new Array();
        this.userFileTemplate = null;
        this.isUserFuncctionPath = false;
        this.defaultFunTempl = "";
        this.InitTemplate();
    }
    InitTemplate() {
        this.fTemplate = [
            "CreateModuleFunctionTemplate.lua",
            "CreateFunctionTemplate.lua",
        ];
        var extensionPath = ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.extensionPath;
        this.defaultFunTempl = path.join(extensionPath, 'Template', 'funTemplate');
        //检查
        var dir = this.getTemplatesDir();
        if (dir) {
            var fileTemplate = path.join(dir, "FileTemplates");
            this.userFileTemplate = fileTemplate;
            if (!fs.existsSync(fileTemplate)) {
                //判断是否有模板文件夹
                if (!fs.existsSync(this.userFileTemplate)) {
                    var creats = fs.mkdirSync(this.userFileTemplate, '0755');
                }
            }
            //检查function 目录
            var funcitonTemplates = path.join(dir, 'FunTemplate');
            if (!fs.existsSync(funcitonTemplates)) {
                var creats = fs.mkdirSync(funcitonTemplates, '0755');
            }
            this.copyFunTemplate(funcitonTemplates);
        }
        this.initFunTemplateConfig();
    }
    copyFunTemplate(funPath) {
        var isChange = true;
        for (var i = 0; i < this.fTemplate.length; i++) {
            var filePath = path.join(funPath, this.fTemplate[i]);
            if (!fs.existsSync(filePath)) {
                var src = path.join(this.defaultFunTempl, this.fTemplate[i]);
                try {
                    fs.writeFileSync(filePath, fs.readFileSync(src));
                }
                catch (err) {
                    isChange = false;
                }
            }
        }
        if (isChange) {
            this.defaultFunTempl = funPath;
        }
    }
    initFunTemplateConfig() {
        for (var i = 0; i < this.fTemplate.length; i++) {
            var fpath = path.join(this.defaultFunTempl, this.fTemplate[i]);
            this.paths.push(fpath);
        }
        this.loadText(0);
    }
    getTemplate(filename) {
        var contentText = fs.readFileSync(path.join(this.userFileTemplate, filename), 'utf-8');
        return contentText;
    }
    ;
    loadText(index) {
        if (index < this.paths.length) {
            var path = this.paths[index];
            vscode.workspace.openTextDocument(path).then(doc => {
                this.functionDocs.push(doc.getText());
                index = index + 1;
                this.loadText(index);
            });
        }
    }
    getTemplateText(index) {
        return this.functionDocs[index];
    }
    getTemplates() {
        if (this.userFileTemplate) {
            if (fs.existsSync(this.userFileTemplate)) {
                var rootPath = this.userFileTemplate;
                var templateFiles = fs.readdirSync(rootPath).map(function (item) {
                    return fs.statSync(path.join(rootPath, item)).isFile() ? item : null;
                }).filter(function (filename) {
                    return filename !== null;
                });
                return templateFiles;
            }
        }
        return null;
    }
    ;
    chekFileTemplatesDir() {
        if (this.userFileTemplate) {
            return true;
        }
        else {
            return false;
        }
    }
    getTemplatesDir() {
        return ExtensionManager_1.ExtensionManager.em.luaIdeConfigManager.luaTemplatesDir;
    }
}
exports.TemplateManager = TemplateManager;
//# sourceMappingURL=TemplateManager.js.map
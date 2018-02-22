/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const path = require("path");
const vscode = require("vscode");
const cce = require("../ex/ChangeCaseExtension");
const cmf = require("../ex/CreateMoudleFunction");
const CreateFunction = require("../ex/CreateFunction");
const LuaIdeConfigManager_1 = require("../ex/LuaIdeConfigManager");
const TemplateManager_1 = require("../ex/Template/TemplateManager");
const CreateTemplateFile_1 = require("../ex/Template/CreateTemplateFile");
const LoadLuaScript_1 = require("../ex/LoadLuaScript");
const httpClient_1 = require("../../httpClient");
class ExtensionManager {
    constructor(context) {
        this.golbal = { context: null };
        this.COMMAND_LABELS = {
            toUpperCase: 'toUpperCase',
            toLowerCase: 'toLowerCase',
            createModuleFunction: 'createModuleFunction',
            createFunction: "createFunction",
            createTemplateFile: "createTemplateFile",
            LoadLuaScript: "LoadLuaScript",
            chatShow: "chatShow",
            donate: "donate"
        };
        this.COMMAND_DEFINITIONS = [
            { label: this.COMMAND_LABELS.toUpperCase, description: '转换为大写', func: cce.toUpperCase },
            { label: this.COMMAND_LABELS.toLowerCase, description: '转换为小写', func: cce.toLowerCase },
            { label: this.COMMAND_LABELS.createModuleFunction, description: '创建模块方法', func: cmf.createModuleFunction },
            { label: this.COMMAND_LABELS.createFunction, description: '创建方法', func: CreateFunction.createFunction },
            { label: this.COMMAND_LABELS.createTemplateFile, description: '创建模板文件', func: CreateTemplateFile_1.CreateTemplateFile.run },
            { label: this.COMMAND_LABELS.LoadLuaScript, description: '加载lua字符串', func: LoadLuaScript_1.OpenLuaLuaScriptText },
            { label: this.COMMAND_LABELS.chatShow, description: '闲聊小功能', func: this.showChat },
            { label: this.COMMAND_LABELS.donate, description: '捐献', func: this.showdoNate },
        ];
        this.TemplatePath = {
            CreateModuleFunctionTemplate: "Template\\CreateModuleFunctionTemplate.lua",
            CreateFunctionTemplate: "Template\\CreateFunctionTemplate.lua",
        };
        ExtensionManager.em = this;
        this.InitEx(context);
    }
    showChat(e) {
        //  this.outPutChannel.show();
    }
    showdoNate(e) {
        var extensionPath = ExtensionManager.em.luaIdeConfigManager.extensionPath;
        extensionPath = path.join(extensionPath, "images", "donate.html");
        var previewUri = vscode.Uri.file(extensionPath);
        vscode.commands.executeCommand('vscode.previewHtml', previewUri, vscode.ViewColumn.One, "谢谢您的支持").then(value => {
            //    this.statisticsMain.sendMsg(StatisticsEvent.C2S_OpenRechrage)
        });
    }
    InitEx(context) {
        this.golbal.context = context;
        this.luaIdeConfigManager = new LuaIdeConfigManager_1.LuaIdeConfigManager();
        this.templateManager = new TemplateManager_1.TemplateManager();
        this.luaIdeConfigManager.showRecharge();
        vscode.commands.registerCommand('luaide.changecase.toLowerCase', (e) => { this.RunCommand(this.COMMAND_LABELS.toLowerCase, e); });
        vscode.commands.registerCommand('luaide.changecase.toUpperCase', (e) => { this.RunCommand(this.COMMAND_LABELS.toUpperCase, e); });
        vscode.commands.registerCommand('luaide.utils.createModuleFunction', (e) => { this.RunCommand(this.COMMAND_LABELS.createModuleFunction, e); });
        vscode.commands.registerCommand('luaide.utils.createFunction', (e) => { this.RunCommand(this.COMMAND_LABELS.createFunction, e); });
        vscode.commands.registerCommand('luaide.utils.createTemplateFile', (e) => { this.RunCommand(this.COMMAND_LABELS.createTemplateFile, e); });
        vscode.commands.registerCommand('luaide.utils.LoadLuaScript', (e) => { this.RunCommand(this.COMMAND_LABELS.LoadLuaScript, e); });
        vscode.commands.registerCommand('luaide.donate', (e) => { this.RunCommand(this.COMMAND_LABELS.donate, e); });
        this.barItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left);
        // this.outPutChannel  = vscode.window.createOutputChannel("闲聊")
        // this.outPutChannel.show(true)
        // this.outPutChannel.appendLine("做着玩的一个小功能")
        context.subscriptions.push(this.barItem);
        // context.subscriptions.push(this.outPutChannel);
        this.barItem.tooltip = "为了LuaIde 更好的发展,请支持LuaIde.";
        this.barItem.command = "luaide.donate";
        this.barItem.text = "捐献(LuaIde)";
        this.barItem.show();
        httpClient_1.httpRequest();
    }
    RunCommand(cmd, e) {
        for (var i = 0; i < this.COMMAND_DEFINITIONS.length; i++) {
            if (this.COMMAND_DEFINITIONS[i].label == cmd) {
                this.COMMAND_DEFINITIONS[i].func(e);
                break;
            }
        }
        // getTemplateText(TemplatePath.CreateModuleFunctionTemplate)
    }
}
exports.ExtensionManager = ExtensionManager;
//# sourceMappingURL=ExtensionManager.js.map
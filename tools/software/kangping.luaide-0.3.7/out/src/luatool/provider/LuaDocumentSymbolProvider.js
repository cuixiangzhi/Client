"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const LuaParse_1 = require("../LuaParse");
class LuaDocumentSymbolProvider {
    constructor() {
        this.goKindToCodeKind = {
            'package': vscode.SymbolKind.Package,
            'import': vscode.SymbolKind.Namespace,
            'variable': vscode.SymbolKind.Variable,
            'type': vscode.SymbolKind.Interface,
            'function': vscode.SymbolKind.Function
        };
    }
    /**
     *
     */
    provideDocumentSymbols(document, token) {
        let options = { fileName: document.fileName };
        return new Promise((resolve, reject) => {
            return resolve(LuaParse_1.LuaParse.lp.luaInfoManager.getFcimByPathStr(document.uri.path).symbols);
        });
    }
}
exports.LuaDocumentSymbolProvider = LuaDocumentSymbolProvider;
//# sourceMappingURL=LuaDocumentSymbolProvider.js.map
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ExtensionManager_1 = require("../ex/ExtensionManager");
function getAsAbsolutePath(path) {
    var p = ExtensionManager_1.ExtensionManager.em.golbal.context.asAbsolutePath(path);
    p = p.replace(/\\/g, "/");
    return p;
}
exports.getAsAbsolutePath = getAsAbsolutePath;
//# sourceMappingURL=ExFileUtils.js.map
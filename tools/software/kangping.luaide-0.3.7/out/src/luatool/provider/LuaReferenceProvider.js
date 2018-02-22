"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
class LuaReferenceProvider {
    provideReferences(document, position, options, token) {
        return vscode.workspace.saveAll(false).then(() => {
            return this.doFindReferences(document, position, options, token);
        });
    }
    doFindReferences(document, position, options, token) {
        return new Promise((resolve, reject) => {
            return resolve([]);
            // let filename = canonicalizeGOPATHPrefix(document.fileName);
            // let cwd = path.dirname(filename);
            // // get current word
            // let wordRange = document.getWordRangeAtPosition(position);
            // if (!wordRange) {
            // 	return resolve([]);
            // }
            // let offset = byteOffsetAt(document, position);
            // let goGuru = getBinPath('guru');
            // let buildTags = '"' + vscode.workspace.getConfiguration('go')['buildTags'] + '"';
            // let process = cp.execFile(goGuru, ['-tags', buildTags, 'referrers', `${filename}:#${offset.toString()}`], {}, (err, stdout, stderr) => {
            // 	try {
            // 		if (err && (<any>err).code === 'ENOENT') {
            // 			promptForMissingTool('guru');
            // 			return resolve(null);
            // 		}
            // 		let lines = stdout.toString().split('\n');
            // 		let results: vscode.Location[] = [];
            // 		for (let i = 0; i < lines.length; i++) {
            // 			let line = lines[i];
            // 			let match = /^(.*):(\d+)\.(\d+)-(\d+)\.(\d+):/.exec(lines[i]);
            // 			if (!match) continue;
            // 			let [_, file, lineStartStr, colStartStr, lineEndStr, colEndStr] = match;
            // 			let referenceResource = vscode.Uri.file(path.resolve(cwd, file));
            // 			let range = new vscode.Range(
            // 				+lineStartStr - 1, +colStartStr - 1, +lineEndStr - 1, +colEndStr
            // 			);
            // 			results.push(new vscode.Location(referenceResource, range));
            // 		}
            // 		resolve(results);
            // 	} catch (e) {
            // 		reject(e);
            // 	}
            // });
            // token.onCancellationRequested(() =>
            // 	process.kill()
            // );
        });
    }
}
exports.LuaReferenceProvider = LuaReferenceProvider;
//# sourceMappingURL=LuaReferenceProvider.js.map
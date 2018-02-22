"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const electron = require('electron');
// Module to control application life.  
const app = electron.app;
// Module to create native browser window.  
const BrowserWindow = electron.BrowserWindow;
// Keep a global reference of the window object, if you don't, the window will  
// be closed automatically when the JavaScript object is garbage collected.  
let mainWindow;
function createWindow() {
    // Create the browser window.  
    mainWindow = new BrowserWindow({ width: 800, height: 600 });
    // and load the index.html of the app.  
    mainWindow.loadURL(`file://${__dirname}/index.html`);
    // Open the DevTools.  
    //mainWindow.webContents.openDevTools()  
    // Emitted when the window is closed.  
    mainWindow.on('closed', function () {
        // Dereference the window object, usually you would store windows  
        // in an array if your app supports multi windows, this is the time  
        // when you should delete the corresponding element.  
        mainWindow = null;
    });
}
exports.createWindow = createWindow;
//# sourceMappingURL=TestWindow.js.map
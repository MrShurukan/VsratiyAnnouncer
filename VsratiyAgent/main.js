const { app, BrowserWindow } = require('electron');
const path = require('node:path')
const fs = require("fs");

try {
    //require('electron-reloader')(module)
} catch (_) {}

const createWindow = () => {
    const win = new BrowserWindow({
        width: 900,
        height: 625,
        autoHideMenuBar: true,
        resizable: false,
        maximizable: false,
        webPreferences: {
            preload: path.join(__dirname, 'preload.js'),
            nodeIntegration: true,
            contextIsolation: false,
        }
    });

    if (fs.existsSync("id.txt"))
        win.loadFile('index.html');
    else
        win.loadFile('no_id_file.html');
}

app.whenReady().then(() => {
    createWindow()
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
})
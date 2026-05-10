const { app, BrowserWindow, Menu } = require("electron");
const path = require("path");

function createWindow() {
  const win = new BrowserWindow({
    width: 1200, height: 900,
    minWidth: 800, minHeight: 600,
    title: "BrickBlast: Velocity Market",
    backgroundColor: "#080814",
    webPreferences: { nodeIntegration: false, contextIsolation: true },
    autoHideMenuBar: true,
  });

  win.loadFile(path.join(__dirname, "game", "index.html"));

  // Keep keyboard focus in the game page
  win.webContents.on("did-finish-load", () => win.webContents.focus());
}

app.whenReady().then(() => {
  Menu.setApplicationMenu(null);
  createWindow();
  app.on("activate", () => { if (BrowserWindow.getAllWindows().length === 0) createWindow(); });
});

app.on("window-all-closed", () => { if (process.platform !== "darwin") app.quit(); });

// preload.js — exposes minimal safe APIs via context bridge
const { contextBridge } = require("electron");
contextBridge.exposeInMainWorld("brickblast", { version: "1.0.0" });

import React from "react";
import { Platform } from "react-native";
import WebView from "react-native-webview";

const gameUri =
  Platform.OS === "android"
    ? "file:///android_asset/game/index.html"
    : require("../ios/BrickBlast/assets/game/index.html");

export default function App() {
  return (
    <WebView
      source={Platform.OS === "android" ? { uri: gameUri } : gameUri}
      style={{ flex: 1 }}
      originWhitelist={["*"]}
      allowFileAccess={true}
      allowFileAccessFromFileURLs={true}
      allowUniversalAccessFromFileURLs={true}
    />
  );
}

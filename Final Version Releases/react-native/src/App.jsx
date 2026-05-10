import React, { useRef } from "react";
import { StyleSheet, View, StatusBar, Text, Platform } from "react-native";
import { WebView } from "react-native-webview";

// Android: bundled file from android/app/src/main/assets/game/
// iOS:     must copy web/ folder to ios/<AppName>/assets/game/
const gameUri =
  Platform.OS === "android"
    ? "file:///android_asset/game/index.html"
    : require("./ios/BrickBlastVelocityMarket/assets/game/index.html");

export default function App() {
  const webRef = useRef(null);
  return (
    <View style={styles.container}>
      <StatusBar hidden />
      <Text style={styles.header}>◆  BRICKBLAST: VELOCITY MARKET  ◆</Text>
      <WebView
        ref={webRef}
        source={Platform.OS === "android" ? { uri: gameUri } : gameUri}
        style={styles.webview}
        allowFileAccess
        allowFileAccessFromFileURLs
        allowUniversalAccessFromFileURLs
        javaScriptEnabled
        domStorageEnabled
        mediaPlaybackRequiresUserAction={false}
        scalesPageToFit={false}
        bounces={false}
        scrollEnabled={false}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#080814" },
  header: {
    color: "#ffc840", fontFamily: "System",
    fontSize: 13, fontWeight: "bold",
    textAlign: "center", paddingVertical: 4, letterSpacing: 1
  },
  webview: { flex: 1 }
});

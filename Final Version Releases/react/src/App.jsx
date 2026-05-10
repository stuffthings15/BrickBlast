import React, { useRef, useEffect, useState } from "react";

const GAME_URL = process.env.PUBLIC_URL + "/game/index.html";

export default function App() {
  const iframeRef = useRef(null);
  const [fullscreen, setFullscreen] = useState(false);

  // Keep the iframe always focused so keyboard events reach the game
  useEffect(() => {
    const focus = () => iframeRef.current?.contentWindow?.focus();
    window.addEventListener("click", focus);
    focus();
    return () => window.removeEventListener("click", focus);
  }, []);

  const toggleFullscreen = () => {
    const el = document.documentElement;
    if (!document.fullscreenElement) {
      el.requestFullscreen?.();
      setFullscreen(true);
    } else {
      document.exitFullscreen?.();
      setFullscreen(false);
    }
  };

  return (
    <div style={styles.root}>
      {/* Header bar */}
      <div style={styles.header}>
        <span style={styles.title}>◆ BRICKBLAST: VELOCITY MARKET ◆</span>
        <button style={styles.fsBtn} onClick={toggleFullscreen}
                title={fullscreen ? "Exit Fullscreen" : "Fullscreen"}>
          {fullscreen ? "⊠" : "⛶"}
        </button>
      </div>

      {/* Game iframe */}
      <iframe
        ref={iframeRef}
        src={GAME_URL}
        style={styles.iframe}
        title="BrickBlast"
        allowFullScreen
        allow="autoplay; fullscreen"
        scrolling="no"
      />

      {/* Footer */}
      <div style={styles.footer}>
        Arrow keys / mouse to play &nbsp;|&nbsp; F = speed boost &nbsp;|&nbsp;
        P = pause &nbsp;|&nbsp; S = store &nbsp;|&nbsp; H = options
      </div>
    </div>
  );
}

const styles = {
  root: {
    display: "flex", flexDirection: "column",
    width: "100vw", height: "100vh",
    background: "#080814", color: "#ccc",
    fontFamily: "'Segoe UI', sans-serif",
  },
  header: {
    display: "flex", alignItems: "center", justifyContent: "space-between",
    padding: "4px 12px",
    background: "#0d0d24",
    borderBottom: "1px solid #1a1a44",
    flexShrink: 0,
  },
  title: { color: "#ffc840", fontWeight: "bold", fontSize: 13, letterSpacing: 1 },
  fsBtn: {
    background: "none", border: "1px solid #444",
    color: "#aaa", cursor: "pointer", fontSize: 16,
    padding: "2px 8px", borderRadius: 4,
  },
  iframe: { flex: 1, border: "none", display: "block", width: "100%" },
  footer: {
    textAlign: "center", fontSize: 11,
    padding: "3px 0", background: "#0d0d24",
    borderTop: "1px solid #1a1a44", flexShrink: 0, color: "#666",
  },
};

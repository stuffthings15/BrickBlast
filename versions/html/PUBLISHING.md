# Publishing Guide — HTML5 / Browser / PWA

**Target:** Any modern web browser + PWA install  
**Package type:** Single self-contained HTML5/Canvas file  
**Source:** `web/index.html` (canonical HTML source, ~65 KB)

> ℹ️ This is the ONLY Brick Blast release that is delivered as HTML.
> All other platform releases must be native binaries or proper native packages.

---

## Step 1 — Sync from Canonical HTML Source

```powershell
# From project root
Copy-Item "web\index.html"    "versions\html\index.html"    -Force
Copy-Item "web\manifest.json" "versions\html\manifest.json" -Force
```

Verify: `versions\html\index.html` should be **~65 KB** and include the store, sound, and all power-ups.

---

## Step 2 — Test Locally

**Windows:**
```bat
RUN_HTML.bat
```

**macOS / Linux:**
```bash
open index.html
# or serve with Python for PWA features:
python3 -m http.server 8080
# then open http://localhost:8080
```

### Test Checklist
- [ ] Game launches and plays fully in browser
- [ ] Store purchases and save data persist (localStorage)
- [ ] Music and sound effects work
- [ ] All power-ups and game modes accessible
- [ ] Daily Challenge generates a unique level
- [ ] Endless Mode runs indefinitely
- [ ] Gamepad input detected
- [ ] PWA install prompt appears when served over HTTP

---

## Step 3 — Deploy

### GitHub Pages (Free Hosting)
```bash
# From project root
git subtree push --prefix versions/html origin gh-pages
```
Or copy `index.html` and `manifest.json` to the `gh-pages` branch manually.

### itch.io
1. Zip `versions/html/` into `BrickBlast-html.zip`
2. On itch.io: Edit Game → Uploads → upload zip
3. Check **"This file will be played in the browser"** → set index as `index.html`

### Netlify
```bash
# Drag-and-drop deployment
# Or CLI:
npx netlify-cli deploy --prod --dir versions/html
```

### Vercel
```bash
cd versions/html
npx vercel --prod
```

---

## Key Files

| File | Purpose |
|------|---------|
| `index.html` | Complete self-contained game |
| `manifest.json` | PWA manifest for offline install |
| `RUN_HTML.bat` | Windows browser launcher |

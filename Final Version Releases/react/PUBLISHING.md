# Publishing Guide — React Web Wrapper

**Target:** Web browsers via React 19 SPA  
**Artifact:** `build/` production bundle  
**Status:** ✅ BUILT (`npm run build` completed — ~60 KB gzipped JS)

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `build/` | Production-ready static files |
| `build/index.html` | React entry point |
| `build/game/index.html` | Canonical game served inside React wrapper |
| `build/static/js/` | Compiled JS bundle |
| `src/` | React source files |
| `public/` | Static assets copied to build |
| `package.json` | npm manifest |

---

## Rebuild (if source changed)

```bash
cd "Final Version Releases/react"
npm install
npm run build
```

---

## Option 1 — Netlify

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Deploy from build/
netlify deploy --prod --dir build
```

---

## Option 2 — Vercel

```bash
npm install -g vercel
vercel --prod
# Set output directory: build
```

---

## Option 3 — GitHub Pages (gh-pages branch)

```bash
npm install --save-dev gh-pages
# Add to package.json scripts: "deploy": "gh-pages -d build"
npm run deploy
```

Set `homepage` in `package.json` to `https://stuffthings15.github.io/BrickBlast/`.

---

## Option 4 — itch.io

Zip the `build/` folder and upload as an HTML game:
```bash
butler push build/ teamfasttalk/brickblast:html-react --userversion 1.0.0
```

---

## Testing Before Publish

- [ ] `npm run build` completes with no errors
- [ ] Open `build/index.html` in browser — React app loads
- [ ] Game iframe inside React wrapper is playable
- [ ] No console errors
- [ ] Bundle size is under 5 MB total

# Publishing Guide — HTML5 / PWA (versions folder)

**Target:** Browser play — itch.io, GitHub Pages, Netlify, Vercel  
**Status:** ✅ READY — static files, no build step needed

---

## Quick Reference

For full instructions, see `../../Final Version Releases/html/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `index.html` | Complete self-contained game |
| `manifest.json` | PWA manifest |
| `icons/` | App icons for PWA install |

---

## Deploy Options

### itch.io (Recommended)
```bash
butler push . teamfasttalk/brickblast:html --userversion 1.0.0
```
Set the game kind to **HTML** and check **"This game can be played in the browser"**.

### GitHub Pages
Push this folder contents to the `gh-pages` branch or configure from repository Settings → Pages.

### Netlify / Vercel
Drag-and-drop this folder onto https://app.netlify.com or import the repo.

---

## PWA Install

When hosted over HTTPS, users can click **"Install"** in Chrome/Edge/Safari to add the game to their home screen or desktop.

---

## Testing Before Publish

- [ ] `index.html` opens in Chrome, Firefox, Edge, Safari
- [ ] Game loads with no console errors
- [ ] Mouse/touch controls work
- [ ] PWA install prompt appears on mobile

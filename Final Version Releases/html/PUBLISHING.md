# Publishing Guide — HTML5 Web Release

**Target:** Any browser — desktop and mobile  
**Artifact:** `index.html` (self-contained single file with all game logic)  
**Status:** ✅ READY

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `index.html` | Complete self-contained game (65 KB) |
| `manifest.json` | PWA manifest for "Add to Home Screen" |
| `icons/` | App icons for PWA install |
| `assets/` | Game asset files |

---

## Option 1 — itch.io (Recommended)

1. Install [Butler](https://itch.io/docs/butler/installing.html)
2. Log in: `butler login`
3. Push:
   ```
   butler push index.html teamfasttalk/brickblast:html --userversion 1.0.0
   ```
4. On itch.io dashboard: set **Kind of project** → **HTML** and tick **This file will be played in the browser**

---

## Option 2 — GitHub Pages

1. Go to https://github.com/stuffthings15/BrickBlast/settings/pages
2. Source: **Deploy from a branch**
3. Branch: `master` → folder: `/web`
4. Save — site publishes at `https://stuffthings15.github.io/BrickBlast/`

---

## Option 3 — Netlify Drop

1. Go to https://app.netlify.com/drop
2. Drag this folder onto the drop zone
3. Copy the generated URL — the game is live immediately

---

## Option 4 — Vercel

```bash
npx vercel --prod
```
Point root directory to this folder.

---

## PWA Install (Mobile)

Users on mobile can install it as an app:
1. Open the hosted URL in **Safari (iOS/iPadOS)** or **Chrome (Android)**
2. Tap **Share → Add to Home Screen** (iOS) or **Install App** prompt (Android)
3. App icon appears on home screen — runs fullscreen

---

## Testing Before Publish

- [ ] Open `index.html` in Chrome/Firefox/Safari — game loads
- [ ] Game plays in browser without errors in DevTools Console
- [ ] Works on mobile viewport (375px wide)
- [ ] PWA manifest loads (no manifest errors in DevTools)
- [ ] "Add to Home Screen" installs correctly on iOS Safari

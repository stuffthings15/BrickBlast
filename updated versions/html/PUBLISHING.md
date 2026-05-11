# Publishing Documentation — HTML5 Browser Version
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`index.html` — fully self-contained single-file HTML5 game. No server-side dependencies.

## How to Play Locally
Open `index.html` in any modern browser (Chrome, Firefox, Edge, Safari).

## How to Publish

### GitHub Pages
```sh
# Push the html folder contents to gh-pages branch
git subtree push --prefix "updated versions/html" origin gh-pages
# Game available at: https://stuffthings15.github.io/BrickBlast/
```

### itch.io (HTML Upload)
1. Go to your itch.io game dashboard
2. Kind: **HTML**
3. Upload a zip of this folder
4. Check "This file will be played in the browser"
5. Set viewport to **900×650**

```powershell
Compress-Archive -Path "updated versions\html\*" -DestinationPath "BrickBlast-HTML.zip"
```

### Netlify Drop
Drag the `html/` folder to https://app.netlify.com/drop

### Any Static Host
Upload `index.html` — the game is fully self-contained (assets embedded as base64).

## Version: v1.2.0

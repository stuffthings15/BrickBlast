# Publishing Documentation — iPad PWA
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`index.html` — Progressive Web App optimized for iPad (Safari, full-screen, touch).

## How to Install on iPad
1. Host `index.html` on any HTTPS server (GitHub Pages, Netlify, etc.)
2. Open in Safari on iPad
3. Tap **Share → Add to Home Screen**
4. App icon appears; tap to launch in standalone full-screen mode

## How to Publish

### GitHub Pages
```sh
git subtree push --prefix "updated versions/ipad" origin gh-pages
```
Access at `https://stuffthings15.github.io/BrickBlast/` and add to home screen.

### itch.io HTML
```powershell
Compress-Archive -Path "updated versions\ipad\*" -DestinationPath "BrickBlast-iPad.zip"
```
Upload zip to itch.io → Kind: HTML → check "played in browser".

## Native iPad Build
For a native `.ipa` App Store build, use the Capacitor/Xcode flow:
```
versions/ipad/BUILD_IOS.sh
```
This requires macOS + Xcode. See `versions/ipad/README.md`.

## Version: v1.2.0

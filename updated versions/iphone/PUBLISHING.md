# Publishing Documentation — iPhone PWA
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`index.html` — Progressive Web App optimized for iPhone (Safari, full-screen, touch).

## How to Install on iPhone
1. Host `index.html` on any HTTPS server
2. Open URL in Safari on iPhone
3. Tap **Share → Add to Home Screen**
4. Launches as standalone app with no browser chrome

## How to Publish

### GitHub Pages
```sh
git subtree push --prefix "updated versions/iphone" origin gh-pages
```

### itch.io HTML Upload
```powershell
Compress-Archive -Path "updated versions\iphone\*" -DestinationPath "BrickBlast-iPhone.zip"
```
Upload to itch.io → Kind: HTML → check "played in browser".

## Native iPhone Build
For Apple App Store distribution, use the Capacitor/Xcode flow from:
```
versions/ipad/BUILD_IOS.sh
```
The same Xcode project targets both iPhone and iPad (Universal build).
Requires macOS + Xcode + Apple Developer Program ($99/yr).

## Version: v1.2.0

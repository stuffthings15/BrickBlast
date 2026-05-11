# Publishing Documentation — Android Tablet PWA
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`index.html` — Progressive Web App for Android tablet (Chrome, landscape, touch).

## How to Install on Android Tablet
1. Host `index.html` on any HTTPS server
2. Open URL in Chrome on Android tablet
3. Tap **⋮ → Add to Home screen**
4. Launches in standalone full-screen mode

## How to Publish

### itch.io HTML Upload
```powershell
Compress-Archive -Path "updated versions\android-tablet\*" -DestinationPath "BrickBlast-Android-Tablet.zip"
```
Upload to itch.io → Kind: HTML → "played in browser".

### GitHub Pages
```sh
git subtree push --prefix "updated versions/android-tablet" origin gh-pages
```

## Google Play Store (Native APK)
For Google Play distribution with tablet-specific assets, use the Capacitor flow.
See `Final Version Releases/android-tablet/PUBLISHING.md`.

## Version: v1.2.0

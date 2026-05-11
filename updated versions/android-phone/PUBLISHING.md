# Publishing Documentation — Android Phone PWA
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`index.html` — Progressive Web App for Android phone (Chrome, full-screen, touch).

## How to Install on Android Phone
1. Host `index.html` on any HTTPS server
2. Open URL in Chrome on Android phone
3. Tap **⋮ → Add to Home screen** (or accept the install prompt)
4. Launches as standalone app

## How to Publish

### itch.io HTML Upload
```powershell
Compress-Archive -Path "updated versions\android-phone\*" -DestinationPath "BrickBlast-Android-Phone.zip"
```
Upload to itch.io → Kind: HTML → "played in browser".

### GitHub Pages
```sh
git subtree push --prefix "updated versions/android-phone" origin gh-pages
```

## Google Play Store (Native APK)
For Google Play distribution, use the Capacitor → Android Studio flow:
```
versions/mobile-capacitor/  (or Final Version Releases/mobile-capacitor/)
```
Requires Android Studio and a Google Play Developer account ($25 one-time).
See `Final Version Releases/android-phone/PUBLISHING.md` for the full native path.

## Version: v1.2.0

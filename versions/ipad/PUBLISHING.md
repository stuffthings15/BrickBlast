# Publishing Guide — iPad (App Store + IPA)

**Target:** iPadOS 15.0+ — Apple App Store  
**Package type:** Capacitor 6 native container wrapping canonical HTML5 game  
**Source:** `mobile/www/index.html` (must be the canonical ~65 KB version from `web/index.html`)  
**Build machine required:** macOS 13+ with Xcode 15+

---

## Step 1 — Sync Canonical Game Source

```bash
# From project root
cp web/index.html   mobile/www/index.html
cp web/manifest.json mobile/www/manifest.json
cp -r web/icons     mobile/www/icons
```

Verify: `mobile/www/index.html` should be **~65 KB** and include the store, sound, and all power-ups.

---

## Step 2 — Install Dependencies and Sync Capacitor

```bash
cd mobile
npm install
npx cap sync ios
cd ios/App
pod install
```

---

## Step 3 — Configure Signing in Xcode

1. Open `mobile/ios/App/App.xcworkspace` in Xcode
2. Select the **App** target → **Signing & Capabilities**
3. Set your **Team** and **Bundle Identifier** (`com.teamfasttalk.brickblast`)
4. Ensure a valid **Distribution Certificate** and **Provisioning Profile** are configured

---

## Step 4 — Archive and Export IPA

### Option A — Xcode UI
1. **Product → Archive**
2. In Organizer: **Distribute App → App Store Connect → Upload**

### Option B — Command line
```bash
xcodebuild -workspace mobile/ios/App/App.xcworkspace \
           -scheme App \
           -configuration Release \
           -archivePath build/BrickBlast.xcarchive \
           archive
xcodebuild -exportArchive \
           -archivePath build/BrickBlast.xcarchive \
           -exportPath build/BrickBlast-iPad \
           -exportOptionsPlist ExportOptions.plist
```

---

## Step 5 — Test Before Submission

- [ ] App installs on iPadOS 15.0+ device or simulator
- [ ] Launches; canvas fills iPad viewport correctly
- [ ] Store, music, power-ups, and saves all work
- [ ] Daily Challenge and Endless Mode accessible
- [ ] Touch controls work; hardware back exits cleanly
- [ ] Works fully offline

---

## Step 6 — Submit to App Store Connect

1. Log in to [App Store Connect](https://appstoreconnect.apple.com)
2. Create a new App (or select existing) with bundle ID `com.teamfasttalk.brickblast`
3. Upload the IPA via Xcode Organizer or Transporter
4. Fill out listing: name, description, screenshots (iPad 12.9"), age rating, keywords
5. Submit for review

See `APP_STORE_GUIDE.md` for the full listing checklist and screenshot requirements.

---

## Key Files

| File | Purpose |
|------|---------|
| `BUILD_IOS.sh` | Automated build script (runs steps 1–4) |
| `APP_STORE_GUIDE.md` | Full App Store Connect submission checklist |
| `capacitor.config.json` | Capacitor project configuration |
| `package.json` | npm dependencies |

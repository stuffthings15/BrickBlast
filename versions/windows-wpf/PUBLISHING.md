# Publishing Guide — Windows WPF (versions folder)

**Target:** Windows 10/11 — WPF wrapper around the game  
**Status:** 🔧 Sub-project — only edit when user explicitly targets WPF

---

## What's in This Folder

| File | Purpose |
|------|---------|
| `*.exe` | WPF application executable |
| `*.dll` | .NET runtime and WPF assemblies |
| `Assets/` | Game assets |

---

## Important Note

The **canonical game** lives in `Form1.vb` (WinForms). The WPF version in `anime finder wpf/` is a separate sub-project. Do not confuse them.

Only rebuild and distribute this version if the user explicitly asks for the WPF release.

---

## Rebuild

From the WPF project directory:

```powershell
dotnet publish "anime finder wpf\anime finder wpf.csproj" -c Release -r win-x64 --self-contained true -o "versions\windows-wpf"
```

---

## Distribute

| Channel | Steps |
|---------|-------|
| itch.io | `butler push . teamfasttalk/brickblast:windows-wpf --userversion 1.0.0` |
| GitHub Releases | Zip and upload as supplementary release asset |

---

## Testing Before Publish

- [ ] Launches on Windows 10/11 x64
- [ ] Game renders correctly in WPF WebView/host
- [ ] No dependency on external runtimes
- [ ] Verified as WPF build, not WinForms canonical build

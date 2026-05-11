# Publishing Documentation — pipelines/
**BrickBlast: Velocity Market — Team Fast Talk**

This folder is the **submission pipeline staging area** — it holds the final deliverable package
that gets zipped and submitted to the course instructor.

## Contents

| Path | Purpose |
|------|---------|
| `exe/anime finder.exe` | Final compiled WinForms game executable |
| `exe/anime finder.pdb` | Debug symbols (include in submission for grading) |
| `exe/BrickBlast_Submit.zip` | Complete submission package |
| `docs/story_assets/STORY.md` | Game narrative / story document |
| `mindset/MINDSET.md` | Design decisions and technology rationale |
| `assets/` | (Staging) — copy final assets here for submission |
| `git/` | (Staging) — git log export or repo archive |
| `implementation/` | (Staging) — implementation notes |
| `overview/` | (Staging) — project overview export |
| `photos/` | (Staging) — screenshots for submission |
| `storyboard/` | (Staging) — storyboard export |
| `video/` | (Staging) — trailer / demo video |

## Submission Steps

### 1. Verify the EXE runs
```powershell
& "pipelines\exe\anime finder.exe"
```

### 2. Populate staging folders
```powershell
# Copy screenshots
Copy-Item docs\Screenshots\*.png pipelines\photos\ -Force

# Copy trailer (once recorded)
Copy-Item docs\Trailer\BrickBlast_Trailer_v1.mp4 pipelines\video\ -Force

# Copy git log
git log --oneline > pipelines\git\git_log.txt
```

### 3. Zip for submission
```powershell
Compress-Archive -Path "pipelines\*" -DestinationPath "pipelines\exe\BrickBlast_Submit.zip" -Force
```

### 4. Submit
Upload `BrickBlast_Submit.zip` to Canvas (or per instructor instructions).

## Version: v1.2.0

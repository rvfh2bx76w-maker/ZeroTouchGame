# Headless Multi-Agent Loop (Godot 4 + C#)

This repo is set up so CI can validate and produce a deterministic review bundle (`handoff.zip`) without opening the Godot editor UI.

## Local (Windows/macOS/Linux)

### 1) Build
```bash
dotnet build ZeroTouchGame.sln -c Release
```

### 2) Headless smoke test
You need the **Godot 4.5.1 .NET** editor binary available.

Option A: put `godot` on PATH  
Option B: set an environment variable once:
- Windows (PowerShell):
```powershell
setx GODOT_BIN "C:\Path\To\Godot_v4.5.1-stable_mono_win64.exe"
```
- macOS/Linux:
```bash
export GODOT_BIN="/path/to/Godot_v4.5.1-stable_mono_x86_64"
```

Then run:
```bash
godot --headless --path . res://Tests/SmokeTest.tscn
```

If you run into Vulkan/renderer issues in headless environments, you can force Compatibility + OpenGL:
```bash
godot --headless --rendering-driver opengl3 --rendering-method gl_compatibility --path . res://Tests/SmokeTest.tscn
```


### 3) Create the handoff bundle
```bash
python tools/handoff.py --run-smoke
```

Artifacts:
- `dist/handoff.zip`
- `dist/handoff_summary.md`
- `dist/handoff_manifest.txt`
- `dist/handoff_logs/*`

Upload `dist/handoff.zip` to the external reviewer.

## CI
GitHub Actions workflow: `.github/workflows/headless-handoff.yml`

CI runs:
- `dotnet build`
- `godot --headless --path . res://Tests/SmokeTest.tscn`
- `python tools/handoff.py` and uploads `handoff.zip` as an artifact.

## Smoke test contract
The smoke test:
- reads `application/run/main_scene` from ProjectSettings
- loads/instances the main scene
- runs for ~10 seconds
- exits with code 0 on success, 1 on failure.

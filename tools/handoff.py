#!/usr/bin/env python3
"""
Headless multi-agent handoff bundler for Godot 4 + C# repos.

What this does (deterministic, UI-free):
  1) Captures git metadata + diffs (optional).
  2) Runs dotnet build (optional).
  3) Runs Godot headless smoke scene (optional).
  4) Packages the exact review bundle into dist/handoff.zip:
     - project.godot, *.sln, *.csproj, nuget.config, build scripts
     - ALL C# source (*.cs)
     - ALL Godot scenes/resources (*.tscn/*.tres/*.res/*.scn)
     - addons/ (plugin code/resources/configs)
     - runtime configs/data (json/yaml/toml/csv/cfg/etc.)
     - shaders (gdshader/shader/compute)
     - logs + manifest (sha256) + summary

Usage:
  python tools/handoff.py
  python tools/handoff.py --run-smoke
  python tools/handoff.py --godot "/path/to/Godot_v4.x-stable_mono_xxx"
"""

from __future__ import annotations

import argparse
import hashlib
import os
import platform
import subprocess
import sys
import time
import zipfile
from dataclasses import dataclass
from pathlib import Path
from typing import List, Optional, Set, Tuple


# ---------- Inclusion rules ----------
ALWAYS_EXT: Set[str] = {
    # C# + build identity
    ".cs", ".csproj", ".sln", ".props", ".targets",
    ".editorconfig", ".gitattributes", ".gitignore",

    # docs / metadata
    ".md", ".txt",

    # config & data
    ".json", ".yaml", ".yml", ".toml", ".csv", ".ini", ".cfg", ".config", ".xml",

    # Godot scene/resources
    ".tscn", ".scn", ".tres", ".res",

    # Shaders
    ".gdshader", ".shader", ".compute",

    # Godot UID sidecars (common in addons)
    ".uid",
}

# Optional asset types (off by default to keep handoffs small)
OPTIONAL_ASSET_EXT: Set[str] = {
    ".glb", ".gltf", ".fbx", ".obj", ".dae",
    ".png", ".jpg", ".jpeg", ".webp",
    ".wav", ".ogg", ".mp3",
    ".ttf", ".otf",
    ".svg",
}

# Directories to skip (caches/build outputs)
SKIP_DIR_NAMES: Set[str] = {
    ".git",
    ".vs", ".idea", ".vscode",
    "bin", "obj",
    ".godot", ".mono",
    "Library", "Temp",
}

SKIP_DIR_CONTAINS: Tuple[str, ...] = (
    "imported",
    "cache",
)

TOP_LEVEL_SPECIAL = [
    "project.godot",
    "global.json",
    ".editorconfig",
    "nuget.config",
    "README.md",
    "REPRO.md",
    "HeadlessBuild.md",
    "AGENTS.md",
]


@dataclass
class RunResult:
    cmd: List[str]
    returncode: int
    seconds: float
    stdout_path: Path


def run_cmd(cmd: List[str], cwd: Path, out_file: Path) -> RunResult:
    start = time.time()
    out_file.parent.mkdir(parents=True, exist_ok=True)
    with out_file.open("w", encoding="utf-8", errors="replace") as f:
        f.write(f"$ {' '.join(cmd)}\n\n")
        try:
            p = subprocess.run(
                cmd,
                cwd=str(cwd),
                stdout=f,
                stderr=subprocess.STDOUT,
                text=True,
                check=False,
            )
            rc = p.returncode
        except FileNotFoundError:
            rc = 127
            f.write(f"\n[ERROR] Command not found: {cmd[0]}\n")
    end = time.time()
    return RunResult(cmd=cmd, returncode=rc, seconds=end - start, stdout_path=out_file)


def find_repo_root(start: Path) -> Path:
    cur = start.resolve()
    for _ in range(50):
        if (cur / "project.godot").exists() or (cur / ".git").exists():
            return cur
        if cur.parent == cur:
            break
        cur = cur.parent
    return start.resolve()


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def should_skip_dir(path: Path) -> bool:
    if path.name in SKIP_DIR_NAMES:
        return True
    lowered = str(path).lower()
    return any(x in lowered for x in SKIP_DIR_CONTAINS)


def detect_godot(user_path: Optional[str]) -> str:
    if user_path:
        return user_path
    env = os.environ.get("GODOT_BIN") or os.environ.get("GODOT")
    if env:
        return env
    # fall back to PATH
    return "godot"


def relpath(repo: Path, path: Path) -> str:
    return str(path.resolve().relative_to(repo.resolve())).replace("\\", "/")


def gather_files(repo: Path, include_assets: bool, max_asset_bytes: int) -> List[Path]:
    files: List[Path] = []

    # Include important top-level identity files if present
    for name in TOP_LEVEL_SPECIAL:
        p = repo / name
        if p.exists() and p.is_file():
            files.append(p)

    # Include workflows explicitly
    workflows = repo / ".github" / "workflows"
    if workflows.exists():
        for p in workflows.rglob("*"):
            if p.is_file():
                files.append(p)

    # Walk repo and include based on extension rules
    for root, dirs, filenames in os.walk(repo):
        root_path = Path(root)

        # prune
        for d in list(dirs):
            if should_skip_dir(root_path / d):
                dirs.remove(d)

        for fn in filenames:
            p = root_path / fn
            if not p.is_file():
                continue

            ext = p.suffix.lower()

            # skip build artifacts
            if ext in {".dll", ".exe", ".pdb"}:
                continue

            if ext in ALWAYS_EXT:
                files.append(p)
                continue

            if include_assets and ext in OPTIONAL_ASSET_EXT:
                try:
                    if p.stat().st_size <= max_asset_bytes:
                        files.append(p)
                except OSError:
                    pass

    # dedupe/sort
    return sorted({p.resolve() for p in files})


def pick_solution(repo: Path) -> Optional[Path]:
    slns = sorted(repo.rglob("*.sln"))
    return slns[0] if slns else None


def main() -> int:
    ap = argparse.ArgumentParser(description="Create a Godot+C# handoff.zip + optional headless checks.")
    ap.add_argument("--out-dir", default="dist", help="Output directory (default: dist)")
    ap.add_argument("--zip-name", default="handoff.zip", help="Zip name (default: handoff.zip)")
    ap.add_argument("--godot", default=None, help="Path to Godot binary (or set GODOT_BIN/GODOT env var)")
    ap.add_argument("--run-smoke", action="store_true", help="Run headless smoke scene")
    ap.add_argument("--smoke-scene", default="res://Tests/SmokeTest.tscn", help="Smoke scene path (default: res://Tests/SmokeTest.tscn)")
    ap.add_argument("--smoke-arg", action="append", default=[], help="Extra arg(s) to pass to Godot for the smoke run (repeatable). Example: --smoke-arg --rendering-driver --smoke-arg opengl3")
    ap.add_argument("--include-assets", action="store_true", help="Include small assets (off by default)")
    ap.add_argument("--max-asset-mb", type=int, default=25, help="Max asset size MB when --include-assets (default: 25)")
    ap.add_argument("--skip-dotnet", action="store_true", help="Skip dotnet build")
    ap.add_argument("--skip-git", action="store_true", help="Skip git metadata/diff capture")
    args = ap.parse_args()

    repo = find_repo_root(Path.cwd())
    out_dir = (repo / args.out_dir).resolve()
    out_dir.mkdir(parents=True, exist_ok=True)

    logs_dir = out_dir / "handoff_logs"
    logs_dir.mkdir(parents=True, exist_ok=True)

    max_asset_bytes = args.max_asset_mb * 1024 * 1024

    # --- git ---
    git_commit = "UNKNOWN"
    git_branch = "UNKNOWN"
    git_ok = True
    if not args.skip_git:
        rr1 = run_cmd(["git", "rev-parse", "HEAD"], repo, logs_dir / "git_commit.log")
        rr2 = run_cmd(["git", "rev-parse", "--abbrev-ref", "HEAD"], repo, logs_dir / "git_branch.log")
        run_cmd(["git", "status", "--porcelain"], repo, logs_dir / "git_status.log")
        run_cmd(["git", "diff"], repo, logs_dir / "git_diff_unstaged.patch")
        run_cmd(["git", "diff", "--staged"], repo, logs_dir / "git_diff_staged.patch")

        if rr1.returncode != 0 or rr2.returncode != 0:
            git_ok = False
        else:
            # parse commit + branch
            with rr1.stdout_path.open("r", encoding="utf-8", errors="replace") as f:
                for line in f.read().splitlines():
                    if line and not line.startswith("$"):
                        git_commit = line.strip()
                        break
            with rr2.stdout_path.open("r", encoding="utf-8", errors="replace") as f:
                for line in f.read().splitlines():
                    if line and not line.startswith("$"):
                        git_branch = line.strip()
                        break

    # --- dotnet build ---
    dotnet_ok = True
    if not args.skip_dotnet:
        sln = pick_solution(repo)
        if sln is None:
            dotnet_ok = False
            (logs_dir / "dotnet_build.log").write_text("[ERROR] No .sln found.\n", encoding="utf-8")
        else:
            rr = run_cmd(["dotnet", "build", str(sln), "-c", "Release"], repo, logs_dir / "dotnet_build.log")
            dotnet_ok = (rr.returncode == 0)

    # --- godot smoke ---
    godot_ok = True
    godot_ran = False
    if args.run_smoke:
        godot_bin = detect_godot(args.godot)
        smoke_fs = repo / args.smoke_scene.replace("res://", "")
        if not smoke_fs.exists():
            godot_ok = False
            (logs_dir / "godot_smoke.log").write_text(
                f"[ERROR] Smoke scene not found: {args.smoke_scene}\nExpected file: {smoke_fs}\n",
                encoding="utf-8"
            )
        else:
            rr = run_cmd([godot_bin, "--headless", *args.smoke_arg, "--path", str(repo), args.smoke_scene], repo, logs_dir / "godot_smoke.log")
            godot_ok = (rr.returncode == 0)
            godot_ran = True

    # --- gather + package ---
    files = gather_files(repo, include_assets=args.include_assets, max_asset_bytes=max_asset_bytes)

    manifest_path = out_dir / "handoff_manifest.txt"
    summary_path = out_dir / "handoff_summary.md"
    zip_path = out_dir / args.zip_name

    # write summary
    summary_lines = [
        f"repo: {repo}",
        f"branch: {git_branch}",
        f"commit: {git_commit}",
        f"platform: {platform.platform()}",
        f"python: {sys.version.split()[0]}",
        "",
        f"git_capture_ok: {git_ok if not args.skip_git else 'SKIPPED'}",
        f"dotnet_build_ok: {dotnet_ok if not args.skip_dotnet else 'SKIPPED'}",
        f"godot_smoke_requested: {args.run_smoke}",
        f"godot_smoke_ran: {godot_ran}",
        f"godot_smoke_ok: {godot_ok if args.run_smoke else 'N/A'}",
        "",
        "included_files:",
        *[f"- {relpath(repo, p)} ({p.stat().st_size} bytes)" for p in files if p.exists()],
        "",
    ]
    summary_path.write_text("\n".join(summary_lines), encoding="utf-8")

    # write manifest (sha256)
    mf_lines = []
    for p in files:
        rp = relpath(repo, p)
        try:
            size = p.stat().st_size
            digest = sha256_file(p)
            mf_lines.append(f"{digest}  {size}  {rp}")
        except OSError:
            mf_lines.append(f"ERROR  -1  {rp}")
    manifest_path.write_text("\n".join(mf_lines) + "\n", encoding="utf-8")

    if zip_path.exists():
        zip_path.unlink()

    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as z:
        for p in files:
            z.write(p, arcname=relpath(repo, p))
        z.write(manifest_path, arcname=manifest_path.name)
        z.write(summary_path, arcname=summary_path.name)
        for lf in sorted(logs_dir.glob("*")):
            z.write(lf, arcname=f"handoff_logs/{lf.name}")

    print("=== HANDOFF CREATED ===")
    print(f"ZIP: {zip_path}")
    print(f"Summary: {summary_path}")
    print(f"Manifest: {manifest_path}")
    print(f"dotnet build: {'OK' if dotnet_ok else 'FAIL' if not args.skip_dotnet else 'SKIPPED'}")
    print(f"godot smoke:  {'OK' if godot_ok else 'FAIL' if args.run_smoke else 'NOT RUN'}")
    # exit code for automation
    ok = (dotnet_ok if not args.skip_dotnet else True) and (godot_ok if args.run_smoke else True)
    return 0 if ok else 2


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

mkdir -p .tmp/art

PROJECT_DIR="${ARCADIA_GODOT_PROJECT_DIR:-godot/arcadia_godot_client_csharp}"
OUT_DIR="${ARCADIA_ART_OUT_DIR:-$ROOT/.tmp/art}"
HEADLESS="${ARCADIA_GODOT_HEADLESS:-0}"

GODOT_BIN="${ARCADIA_GODOT_BIN:-}"

if [[ -n "${GODOT_BIN}" ]]; then
  if [[ ! -x "${GODOT_BIN}" ]]; then
    echo "ARCADIA_GODOT_BIN is set but not executable: ${GODOT_BIN}"
    exit 1
  fi
else
  if command -v godot >/dev/null 2>&1; then
    GODOT_BIN="godot"
  elif command -v Godot >/dev/null 2>&1; then
    GODOT_BIN="Godot"
  elif command -v godot4 >/dev/null 2>&1; then
    GODOT_BIN="godot4"
  else
    # Common macOS app bundle locations (including Downloads for local installs).
    for app in \
      "/Applications/Godot_mono.app" \
      "/Applications/Godot.app" \
      "$HOME/Applications/Godot_mono.app" \
      "$HOME/Applications/Godot.app" \
      "$HOME/Downloads/Godot_mono.app" \
      "$HOME/Downloads/Godot.app"
    do
      if [[ -x "${app}/Contents/MacOS/Godot" ]]; then
        GODOT_BIN="${app}/Contents/MacOS/Godot"
        break
      fi
    done
  fi
fi

if [[ -z "${GODOT_BIN}" ]]; then
  echo "Godot binary not found."
  echo "Either add it to PATH or set ARCADIA_GODOT_BIN to the executable path."
  echo "Example (macOS): export ARCADIA_GODOT_BIN=\"$HOME/Downloads/Godot_mono.app/Contents/MacOS/Godot\""
  exit 1
fi

echo "Using Godot: $($GODOT_BIN --version | head -n 1)"
echo "Project: ${PROJECT_DIR}"
echo "OutDir:  ${OUT_DIR}"

mkdir -p "${OUT_DIR}"

CAPTURE_LOG="${OUT_DIR}/capture.log"
rm -f "${CAPTURE_LOG}" >/dev/null 2>&1 || true

# Notes:
# - We rely on Scripts/Main.cs to capture one screenshot and quit.
# - On some platforms/configs, `--headless` uses a dummy renderer and cannot capture viewport textures.
#   Default to non-headless for local acceptance; CI can set ARCADIA_GODOT_HEADLESS=1 and provide a render-capable setup.
GODOT_ARGS=(--path "${PROJECT_DIR}")
if [[ "${HEADLESS}" == "1" ]]; then
  GODOT_ARGS+=(--headless)
fi

$GODOT_BIN "${GODOT_ARGS[@]}" -- --capture --out "${OUT_DIR}" > "${CAPTURE_LOG}" 2>&1 || true

if [[ ! -f "${OUT_DIR}/baseline_main.png" ]]; then
  echo "Capture failed: baseline_main.png not found."
  echo "See: ${CAPTURE_LOG}"
  tail -n 80 "${CAPTURE_LOG}" || true
  exit 1
fi

echo "Done. Files:"
ls -la "${OUT_DIR}" | sed -n '1,20p'

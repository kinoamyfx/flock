#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

PROJECT_DIR="godot/arcadia_godot_client"
OUT_DIR="$ROOT/.tmp/ui"
HEADLESS="${ARCADIA_GODOT_HEADLESS:-0}"
CAPTURE_TIMEOUT_SEC="${ARCADIA_GODOT_CAPTURE_TIMEOUT_SEC:-60}"
QUIT_AFTER="${ARCADIA_GODOT_QUIT_AFTER:-900}"

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
    # Common macOS app bundle locations
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

echo "=== UI Screenshot Capture ==="
echo "Using Godot: $($GODOT_BIN --version 2>&1 | head -n 1 || echo 'version unknown')"
echo "Project: ${PROJECT_DIR}"
echo "OutDir:  ${OUT_DIR}"
echo "Timeout: ${CAPTURE_TIMEOUT_SEC}s"

mkdir -p "${OUT_DIR}"

CAPTURE_LOG="${OUT_DIR}/capture.log"
rm -f "${CAPTURE_LOG}" >/dev/null 2>&1 || true

# Build Godot args
GODOT_ARGS=(--path "${PROJECT_DIR}" --scene res://scenes/ui_screenshot_capture.tscn --quit-after "${QUIT_AFTER}")
if [[ "${HEADLESS}" == "1" ]]; then
  GODOT_ARGS+=(--headless)
fi

# Run capture
(
  export ARCADIA_UI_CAPTURE=1
  $GODOT_BIN "${GODOT_ARGS[@]}" -- --out="${OUT_DIR}"
) > "${CAPTURE_LOG}" 2>&1 &

CAPTURE_PID=$!
START_TS=$(date +%s)

while kill -0 "${CAPTURE_PID}" >/dev/null 2>&1; do
  NOW_TS=$(date +%s)
  if (( NOW_TS - START_TS > CAPTURE_TIMEOUT_SEC )); then
    echo "Capture timed out after ${CAPTURE_TIMEOUT_SEC}s; terminating Godot (pid=${CAPTURE_PID})." >> "${CAPTURE_LOG}"
    kill -TERM "${CAPTURE_PID}" >/dev/null 2>&1 || true
    sleep 2
    kill -KILL "${CAPTURE_PID}" >/dev/null 2>&1 || true
    wait "${CAPTURE_PID}" >/dev/null 2>&1 || true
    exit 1
  fi
  sleep 0.2
done

wait "${CAPTURE_PID}" >/dev/null 2>&1 || true

# Check results
EXPECTED_FILES=("main_menu.png" "settings.png" "inventory.png" "hud.png" "loot_prompt.png")
MISSING_COUNT=0

for file in "${EXPECTED_FILES[@]}"; do
  if [[ ! -f "${OUT_DIR}/${file}" ]]; then
    echo "Missing screenshot: ${file}"
    MISSING_COUNT=$((MISSING_COUNT + 1))
  fi
done

if [[ $MISSING_COUNT -gt 0 ]]; then
  echo "Capture incomplete: ${MISSING_COUNT} screenshots missing."
  echo "See: ${CAPTURE_LOG}"
  tail -n 80 "${CAPTURE_LOG}" || true
  exit 1
fi

echo "Done. Files:"
ls -lh "${OUT_DIR}"/*.png 2>/dev/null || echo "(no png files found)"
echo ""
echo "Logs: ${CAPTURE_LOG}"

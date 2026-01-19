#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

mkdir -p .tmp

echo "=== UI Regression Gate ==="

echo "1) Capture UI screenshots..."
bash scripts/capture_ui_screenshots.sh > .tmp/ui_regression_capture.log 2>&1

echo "2) Check style consistency..."
bash scripts/check_ui_style_consistency.sh > .tmp/ui_regression_consistency.log 2>&1

echo "3) Verify expected screenshots exist..."
EXPECTED=("main_menu.png" "settings.png" "inventory.png" "hud.png" "loot_prompt.png")
for f in "${EXPECTED[@]}"; do
  if [[ ! -f ".tmp/ui/${f}" ]]; then
    echo "Missing screenshot: .tmp/ui/${f}"
    echo "See logs:"
    echo "  .tmp/ui_regression_capture.log"
    exit 1
  fi
done

echo "✅ UI regression gate passed."
echo "Artifacts:"
echo "  .tmp/ui/*.png"
echo "  .tmp/ui_style_consistency_report.md"
echo "Logs:"
echo "  .tmp/ui_regression_capture.log"
echo "  .tmp/ui_regression_consistency.log"


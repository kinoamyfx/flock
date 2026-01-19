#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

mkdir -p .tmp

REPORT=".tmp/ui_style_consistency_report.md"
TOKENS_FILE="godot/arcadia_godot_client/docs/ui_tokens.md"
THEME_FILE="godot/arcadia_godot_client/theme/arcadia_theme.tres"

SCENES=(
  "godot/arcadia_godot_client/scenes/main_menu.tscn"
  "godot/arcadia_godot_client/scenes/settings.tscn"
  "godot/arcadia_godot_client/scenes/inventory.tscn"
  "godot/arcadia_godot_client/scenes/hud.tscn"
  "godot/arcadia_godot_client/scenes/loot_prompt.tscn"
)

TOKENS=(
  "bg_darkest"
  "bg_dark"
  "bg_medium"
  "text_primary"
  "text_secondary"
  "health_red"
  "spirit_blue"
  "loot_gold"
  "warning_yellow"
  "danger_red"
  "success_green"
)

PASS=0
FAIL=0

check() {
  local title="$1"
  shift
  if "$@"; then
    PASS=$((PASS + 1))
    echo "- ✅ ${title}" >> "$REPORT"
  else
    FAIL=$((FAIL + 1))
    echo "- ❌ ${title}" >> "$REPORT"
  fi
}

rm -f "$REPORT"
{
  echo "# UI Style Consistency Report"
  echo ""
  echo "- GeneratedAtUtc: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo ""
  echo "## Checks"
} >> "$REPORT"

check "UI tokens doc exists (${TOKENS_FILE})" test -f "$TOKENS_FILE"
check "Theme exists (${THEME_FILE})" test -f "$THEME_FILE"

for t in "${TOKENS[@]}"; do
  check "Token present in ui_tokens.md: ${t}" grep -q "${t}" "$TOKENS_FILE"
done

for s in "${SCENES[@]}"; do
  check "Scene exists: ${s}" test -f "$s"
  check "Scene references arcadia_theme.tres: ${s}" grep -q 'res://theme/arcadia_theme\.tres' "$s"
done

{
  echo ""
  echo "## Summary"
  echo ""
  echo "- Passed: ${PASS}"
  echo "- Failed: ${FAIL}"
} >> "$REPORT"

if [[ "$FAIL" -gt 0 ]]; then
  echo "UI consistency check failed. See: ${REPORT}"
  exit 1
fi

echo "UI consistency check passed. Report: ${REPORT}"


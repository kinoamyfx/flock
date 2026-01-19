#!/usr/bin/env bash
set -euo pipefail

# Why: 验证 v1-playable-dungeon-slice 的"连接→移动→死亡掉落→拾取"完整闭环。
# Context: 这是 v1.0.0 关键路径的最后一个验收项（4.1 冒烟测试）。
# Attention: 必须启动 Gateway + Zone Server，并用 LoadTest playable-slice 模式自动化验证。

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

mkdir -p .tmp

PORT="${ARCADIA_ENET_PORT:-7777}"
GATEWAY_PORT="${ARCADIA_GATEWAY_PORT:-8080}"
GATEWAY_READY_TIMEOUT_S="${ARCADIA_GATEWAY_READY_TIMEOUT_S:-30}"
DEV_ISSUE_KEY="${ARCADIA_DEV_ISSUE_KEY:-dev-issue-key}"
AUTH_KEYS="${ARCADIA_AUTH_KEYS:-k1=dev-secret}"
AUTH_ACTIVE_KID="${ARCADIA_AUTH_ACTIVE_KID:-k1}"

echo "Starting Arcadia.Gateway on port ${GATEWAY_PORT}..."
(
  export ASPNETCORE_URLS="http://127.0.0.1:${GATEWAY_PORT}"
  export ARCADIA_DEV_ISSUE_KEY="$DEV_ISSUE_KEY"
  export ARCADIA_AUTH_KEYS="$AUTH_KEYS"
  export ARCADIA_AUTH_ACTIVE_KID="$AUTH_ACTIVE_KID"
  dotnet run --no-launch-profile --project src/Arcadia.Gateway/Arcadia.Gateway.csproj -c Release
) > .tmp/smoke_playable_gateway.log 2>&1 &
GATEWAY_PID="$!"

echo "Starting Arcadia.Server on port ${PORT}..."
(
  export ARCADIA_ENET_PORT="$PORT"
  export ARCADIA_AUTH_KEYS="$AUTH_KEYS"
  export ARCADIA_AUTH_ACTIVE_KID="$AUTH_ACTIVE_KID"
  dotnet run --project src/Arcadia.Server/Arcadia.Server.csproj -c Release
) > .tmp/smoke_playable_server.log 2>&1 &
SERVER_PID="$!"

cleanup() {
  kill "$GATEWAY_PID" >/dev/null 2>&1 || true
  kill "$SERVER_PID" >/dev/null 2>&1 || true
}
trap cleanup EXIT

echo "Waiting for Gateway..."
MAX_TRIES=$((GATEWAY_READY_TIMEOUT_S * 10))
for i in $(seq 1 "${MAX_TRIES}"); do
  if curl -sf "http://127.0.0.1:${GATEWAY_PORT}/" >/dev/null; then
    break
  fi
  if ! kill -0 "$GATEWAY_PID" >/dev/null 2>&1; then
    echo "Gateway process exited early. See .tmp/smoke_playable_gateway.log"
    exit 1
  fi
  sleep 0.1
done

if ! curl -sf "http://127.0.0.1:${GATEWAY_PORT}/" >/dev/null; then
  echo "Gateway did not become ready within ${GATEWAY_READY_TIMEOUT_S}s."
  echo "See:"
  echo "  .tmp/smoke_playable_gateway.log"
  echo "  .tmp/smoke_playable_server.log"
  exit 1
fi

echo "Running Playable Slice Test (connect→move→death/drop→pickup)..."
(
  export ARCADIA_LOADTEST_MODE="playable-slice"
  export ARCADIA_LOADTEST_HOST="127.0.0.1"
  export ARCADIA_LOADTEST_PORT="$PORT"
  export ARCADIA_GATEWAY_URL="http://127.0.0.1:${GATEWAY_PORT}"
  export ARCADIA_DEV_ISSUE_KEY="$DEV_ISSUE_KEY"
  export ARCADIA_LOADTEST_TOKEN_MODE="gateway"
  export ARCADIA_LOADTEST_PLAYER_PREFIX="slice-test"
  dotnet run --project src/Arcadia.LoadTest/Arcadia.LoadTest.csproj -c Release
) > .tmp/smoke_playable_slice.log 2>&1

echo ""
echo "Checking test result..."
if grep -q "Verdict.*PASS" .tmp/smoke_playable_slice.log; then
  echo "✓ Playable Slice Test PASSED"
  echo ""
  echo "Smoke done. Logs:"
  echo "  .tmp/smoke_playable_gateway.log"
  echo "  .tmp/smoke_playable_server.log"
  echo "  .tmp/smoke_playable_slice.log"
  exit 0
else
  echo "✗ Playable Slice Test FAILED"
  echo ""
  echo "Last 50 lines of smoke_playable_slice.log:"
  tail -n 50 .tmp/smoke_playable_slice.log
  echo ""
  echo "Full logs:"
  echo "  .tmp/smoke_playable_gateway.log"
  echo "  .tmp/smoke_playable_server.log"
  echo "  .tmp/smoke_playable_slice.log"
  exit 1
fi

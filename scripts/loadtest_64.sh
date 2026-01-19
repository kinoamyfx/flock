#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

mkdir -p .tmp

PORT="${ARCADIA_ENET_PORT:-17777}"
GATEWAY_PORT="${ARCADIA_GATEWAY_PORT:-18080}"
GATEWAY_READY_TIMEOUT_S="${ARCADIA_GATEWAY_READY_TIMEOUT_S:-30}"
DEV_ISSUE_KEY="${ARCADIA_DEV_ISSUE_KEY:-dev-issue-key}"
AUTH_KEYS="${ARCADIA_AUTH_KEYS:-k1=dev-secret}"
AUTH_ACTIVE_KID="${ARCADIA_AUTH_ACTIVE_KID:-k1}"

CLIENTS="${ARCADIA_LOADTEST_CLIENTS:-64}"
DURATION="${ARCADIA_LOADTEST_DURATION_S:-10}"

echo "Starting Arcadia.Gateway on port ${GATEWAY_PORT}..."
(
  export ASPNETCORE_URLS="http://127.0.0.1:${GATEWAY_PORT}"
  export ARCADIA_DEV_ISSUE_KEY="$DEV_ISSUE_KEY"
  export ARCADIA_AUTH_KEYS="$AUTH_KEYS"
  export ARCADIA_AUTH_ACTIVE_KID="$AUTH_ACTIVE_KID"
  dotnet run --no-launch-profile --project src/Arcadia.Gateway/Arcadia.Gateway.csproj -c Release
) > .tmp/loadtest64_gateway.log 2>&1 &
GATEWAY_PID="$!"

echo "Starting Arcadia.Server on port ${PORT}..."
(
  export ARCADIA_ENET_PORT="$PORT"
  export ARCADIA_AUTH_KEYS="$AUTH_KEYS"
  export ARCADIA_AUTH_ACTIVE_KID="$AUTH_ACTIVE_KID"
  dotnet run --project src/Arcadia.Server/Arcadia.Server.csproj -c Release
) > .tmp/loadtest64_server.log 2>&1 &
SERVER_PID="$!"

cleanup() {
  kill "$GATEWAY_PID" >/dev/null 2>&1 || true
  kill "$SERVER_PID" >/dev/null 2>&1 || true
}
trap cleanup EXIT

echo "Waiting for Gateway..."
MAX_TRIES=$((GATEWAY_READY_TIMEOUT_S * 10))
for _ in $(seq 1 "${MAX_TRIES}"); do
  if curl -sf "http://127.0.0.1:${GATEWAY_PORT}/" >/dev/null; then
    break
  fi
  if ! kill -0 "$GATEWAY_PID" >/dev/null 2>&1; then
    echo "Gateway process exited early. See .tmp/loadtest64_gateway.log"
    exit 1
  fi
  sleep 0.1
done

if ! curl -sf "http://127.0.0.1:${GATEWAY_PORT}/" >/dev/null; then
  echo "Gateway did not become ready within ${GATEWAY_READY_TIMEOUT_S}s."
  echo "See:"
  echo "  .tmp/loadtest64_gateway.log"
  echo "  .tmp/loadtest64_server.log"
  exit 1
fi

echo "Running Arcadia.LoadTest (${CLIENTS} clients, ${DURATION}s)..."
(
  export ARCADIA_LOADTEST_HOST="127.0.0.1"
  export ARCADIA_LOADTEST_PORT="$PORT"
  export ARCADIA_LOADTEST_CLIENTS="$CLIENTS"
  export ARCADIA_LOADTEST_DURATION_S="$DURATION"
  export ARCADIA_GATEWAY_URL="http://127.0.0.1:${GATEWAY_PORT}"
  export ARCADIA_DEV_ISSUE_KEY="$DEV_ISSUE_KEY"
  export ARCADIA_LOADTEST_TOKEN_MODE="gateway"
  dotnet run --project src/Arcadia.LoadTest/Arcadia.LoadTest.csproj -c Release
) > .tmp/loadtest64_loadtest.log 2>&1

if grep -qiE "Unhandled exception|fail|error" .tmp/loadtest64_loadtest.log; then
  echo "LoadTest reported errors. See: .tmp/loadtest64_loadtest.log"
  exit 1
fi

if grep -qiE "Bind exception|Address already in use" .tmp/loadtest64_server.log; then
  echo "Server port bind failure. See: .tmp/loadtest64_server.log"
  exit 1
fi

echo "Load test done. Logs:"
echo "  .tmp/loadtest64_gateway.log"
echo "  .tmp/loadtest64_server.log"
echo "  .tmp/loadtest64_loadtest.log"


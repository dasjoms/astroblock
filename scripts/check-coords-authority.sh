#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

TARGET_GLOB='src/**/*.cs'
EXCLUDE_CORE='!src/Core/**'

patterns=(
  '%\s*(ChunkSize|ChunkConstants\.ChunkSize)'
  'Math\.Floor\s*\(\s*[^)]*/\s*(ChunkSize|ChunkConstants\.ChunkSize)'
  'MathF\.Floor\s*\(\s*[^)]*/\s*(ChunkSize|ChunkConstants\.ChunkSize)'
  '((?:^|[^A-Za-z0-9_]))(?:wx|wy|wz|worldX|worldY|worldZ)\s*/\s*(ChunkSize|ChunkConstants\.ChunkSize)'
)

messages=(
  'Found modulo-with-ChunkSize pattern outside Core.'
  'Found manual Math.Floor division by ChunkSize outside Core.'
  'Found manual MathF.Floor division by ChunkSize outside Core.'
  'Found direct world/ChunkSize division pattern outside Core.'
)

violations=0

for i in "${!patterns[@]}"; do
  pattern="${patterns[$i]}"
  message="${messages[$i]}"

  if matches=$(rg -n --pcre2 --glob "$TARGET_GLOB" --glob "$EXCLUDE_CORE" "$pattern" || true); then
    if [[ -n "$matches" ]]; then
      echo "ERROR: $message"
      echo "$matches"
      echo
      violations=1
    fi
  fi
done

if [[ "$violations" -ne 0 ]]; then
  echo "Coordinate authority guard failed. Use Core.Coords conversion APIs instead."
  exit 1
fi

echo "Coordinate authority guard passed."

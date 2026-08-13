#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_DIR="$ROOT_DIR/apps/godot-client"

resolve_godot() {
  if [[ -n "${GODOT_BIN:-}" ]]; then
    printf '%s\n' "$GODOT_BIN"
    return
  fi
  if command -v godot >/dev/null 2>&1; then
    command -v godot
    return
  fi
  if command -v godot4 >/dev/null 2>&1; then
    command -v godot4
    return
  fi
  printf '%s\n' \
    'Godot 4.7.1-stable is required. Set GODOT_BIN or add godot to PATH.' >&2
  return 1
}

GODOT="$(resolve_godot)"
if [[ ! -x "$GODOT" ]]; then
  printf 'Godot executable is not executable: %s\n' "$GODOT" >&2
  exit 1
fi

VERSION="$($GODOT --version)"
case "$VERSION" in
  4.7.1.stable.*) ;;
  *)
    printf 'Expected Godot 4.7.1-stable, got %s\n' "$VERSION" >&2
    exit 1
    ;;
esac

printf 'godot=%s\n' "$VERSION"
"$GODOT" --headless --editor --import --path "$PROJECT_DIR" --quit
"$GODOT" --headless --path "$PROJECT_DIR" \
  --script res://tests/run_tests.gd

#!/usr/bin/env bash
set -euo pipefail

printf 'git=%s\n' "$(git --version)"
printf 'git_lfs=%s\n' "$(git-lfs version)"
printf 'node=%s\n' "$(node --version)"
printf 'pnpm=%s\n' "$(pnpm --version)"
printf 'docker=%s\n' "$(docker --version)"

if command -v forge >/dev/null 2>&1; then
  printf 'forge=%s\n' "$(forge --version | head -n 1)"
else
  printf 'forge=missing\n'
fi

if command -v unity >/dev/null 2>&1; then
  printf 'unity_cli=%s\n' "$(unity --version)"
  unity editors --installed --format tsv
else
  printf 'unity_cli=missing\n'
fi

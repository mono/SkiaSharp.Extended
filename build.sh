#!/usr/bin/env bash
set -euo pipefail

"$(dirname "$0")/eng/common/build.sh" -restore -build "$@"

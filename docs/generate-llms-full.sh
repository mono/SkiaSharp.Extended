#!/usr/bin/env bash
#
# Generates llms-full.txt by concatenating all documentation markdown files.
# This file provides the full documentation content in a single file for LLM ingestion.
#
# Usage: ./generate-llms-full.sh [output_path]
#   output_path: Path for the generated file (default: docs/llms-full.txt)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARTICLES_DIR="$SCRIPT_DIR/docs"
OUTPUT="${1:-$SCRIPT_DIR/llms-full.txt}"

# Ordered list of documentation files to include
DOC_FILES=(
    "$SCRIPT_DIR/index.md"
    "$ARTICLES_DIR/blurhash.md"
    "$ARTICLES_DIR/geometry.md"
    "$ARTICLES_DIR/path-interpolation.md"
    "$ARTICLES_DIR/lottie.md"
    "$ARTICLES_DIR/confetti.md"
    "$ARTICLES_DIR/svg-migration.md"
)

{
    cat <<'HEADER'
# SkiaSharp.Extended

> SkiaSharp.Extended provides powerful graphics utilities and .NET MAUI controls for SkiaSharp projects—from blur hash placeholders to Lottie animations and confetti effects.

This file contains the complete documentation for SkiaSharp.Extended in a single document, suitable for LLM context ingestion. For the curated index, see llms.txt.

Source: https://github.com/mono/SkiaSharp.Extended
Docs: https://mono.github.io/SkiaSharp.Extended/
API Reference: https://mono.github.io/SkiaSharp.Extended/api/

---

HEADER

    for file in "${DOC_FILES[@]}"; do
        if [ -f "$file" ]; then
            cat "$file"
            printf '\n\n---\n\n'
        fi
    done
} > "$OUTPUT"

echo "Generated $OUTPUT"

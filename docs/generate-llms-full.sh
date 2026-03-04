#!/usr/bin/env bash
#
# Generates llms.txt and llms-full.txt for the documentation site.
#
# - llms.txt:      Curated index for LLM discovery (from docs/llms.md)
# - llms-full.txt: Full documentation content in one file (header + all docs)
#
# Usage: ./generate-llms-full.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Generate llms.txt from the source template
cp "$SCRIPT_DIR/llms.md" "$SCRIPT_DIR/llms.txt"
echo "Generated $SCRIPT_DIR/llms.txt"

# Generate llms-full.txt by concatenating all documentation markdown files
{
    cat "$SCRIPT_DIR/llms-full-header.md"
    printf '\n\n'

    # Start with the main index page, then include all article pages
    cat "$SCRIPT_DIR/index.md"
    printf '\n\n---\n\n'

    for file in "$SCRIPT_DIR"/docs/*.md; do
        if [ -f "$file" ]; then
            cat "$file"
            printf '\n\n---\n\n'
        fi
    done
} > "$SCRIPT_DIR/llms-full.txt"

echo "Generated $SCRIPT_DIR/llms-full.txt"

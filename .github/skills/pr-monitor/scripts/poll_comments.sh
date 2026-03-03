#!/usr/bin/env bash
# poll_comments.sh — Detect new PR/issue comments from a specific user.
#
# Usage: poll_comments.sh <owner/repo> <pr_number> <reviewer> <known_file> <own_replies_file>
#
# Outputs new comments in a summary format.
# Updates the known_file with all current comment IDs.
# Exit code: 0 = new comments found, 1 = no new comments, 2 = API error

set -euo pipefail

REPO="$1"
PR_NUMBER="$2"
REVIEWER="$3"
KNOWN_FILE="$4"
OWN_REPLIES_FILE="$5"

# Fetch ALL comment IDs + metadata (pagination-safe)
COMMENTS_JSON=$(gh api "repos/${REPO}/issues/${PR_NUMBER}/comments?per_page=100" \
  --paginate \
  --jq '.[] | {id: .id, user: .user.login, created_at: .created_at, body: (.body[:500])}' 2>/dev/null) || {
  echo "ERROR: GitHub API request failed" >&2
  exit 2
}

# Load known and own-reply IDs into associative-style lookup (portable)
KNOWN_IDS=""
[ -f "$KNOWN_FILE" ] && KNOWN_IDS=$(cat "$KNOWN_FILE")
OWN_IDS=""
[ -f "$OWN_REPLIES_FILE" ] && OWN_IDS=$(cat "$OWN_REPLIES_FILE")

# Helper: check if an ID is in a newline-separated list
id_in_list() {
  local needle="$1" haystack="$2"
  [ -z "$haystack" ] && return 1
  echo "$haystack" | grep -Fxq "$needle" 2>/dev/null
}

# Collect all IDs and find new ones
ALL_IDS=""
NEW_COMMENTS=""
NEW_COUNT=0

while IFS= read -r line; do
  [ -z "$line" ] && continue

  id=$(echo "$line" | jq -r '.id')
  user=$(echo "$line" | jq -r '.user')
  created=$(echo "$line" | jq -r '.created_at')
  body=$(echo "$line" | jq -r '.body')

  ALL_IDS="${ALL_IDS}${id}
"

  # Skip if already known
  id_in_list "$id" "$KNOWN_IDS" && continue

  # Skip if it's our own reply
  id_in_list "$id" "$OWN_IDS" && continue

  # Skip if not from the reviewer
  [ "$user" != "$REVIEWER" ] && continue

  NEW_COUNT=$((NEW_COUNT + 1))
  NEW_COMMENTS="${NEW_COMMENTS}
---
COMMENT_ID: ${id}
USER: ${user}
CREATED: ${created}
BODY: ${body}
---"
done <<< "$COMMENTS_JSON"

# Update known file with ALL current IDs
echo "$ALL_IDS" > "$KNOWN_FILE"

if [ "$NEW_COUNT" -eq 0 ]; then
  echo "No new comments from ${REVIEWER} at $(date +%H:%M:%S)"
  exit 1
fi

echo "Found ${NEW_COUNT} new comment(s) from ${REVIEWER}:"
echo "$NEW_COMMENTS"
exit 0


#!/usr/bin/env bash
# poll_loop.sh — Continuous polling loop for PR comments.
# Designed to run as a detached background process (mode: "async", detach: true).
#
# Usage: poll_loop.sh <skill_dir> <owner/repo> <pr_number> <reviewer> \
#          <known_file> <own_replies_file> <results_file> \
#          <heartbeat_file> <pid_file> <poll_interval>
#
# Writes new comments to RESULTS_FILE (appended).
# Writes heartbeat timestamp to HEARTBEAT_FILE on every cycle.
# Writes own PID to PID_FILE for cleanup.

set -uo pipefail

SKILL_DIR="$1"
REPO="$2"
PR_NUMBER="$3"
REVIEWER="$4"
KNOWN_FILE="$5"
OWN_REPLIES_FILE="$6"
RESULTS_FILE="$7"
HEARTBEAT_FILE="$8"
PID_FILE="$9"
POLL_INTERVAL="${10}"

# Record PID for external cleanup
echo $$ > "$PID_FILE"

# Clean up PID file on exit
trap 'rm -f "$PID_FILE"' EXIT

CURRENT_INTERVAL="$POLL_INTERVAL"

while true; do
  sleep "$CURRENT_INTERVAL"

  # Write heartbeat
  date '+%Y-%m-%dT%H:%M:%S' > "$HEARTBEAT_FILE"

  # Run single-shot poll
  OUTPUT=$("$SKILL_DIR/scripts/poll_comments.sh" "$REPO" "$PR_NUMBER" "$REVIEWER" "$KNOWN_FILE" "$OWN_REPLIES_FILE" 2>&1)
  EXIT_CODE=$?

  case $EXIT_CODE in
    0)
      # New comments found — append to results file
      echo "$OUTPUT" >> "$RESULTS_FILE"
      CURRENT_INTERVAL="$POLL_INTERVAL"
      ;;
    1)
      # No new comments — reset interval (in case we were backed off)
      CURRENT_INTERVAL="$POLL_INTERVAL"
      ;;
    2)
      # API error — double interval (max 600s)
      CURRENT_INTERVAL=$((CURRENT_INTERVAL * 2))
      if [ "$CURRENT_INTERVAL" -gt 600 ]; then
        CURRENT_INTERVAL=600
      fi
      echo "API error at $(date '+%H:%M:%S'), backing off to ${CURRENT_INTERVAL}s" >> "$RESULTS_FILE"
      ;;
  esac
done

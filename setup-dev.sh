#!/usr/bin/env bash
#
# Castle Attack - developer setup (macOS / Linux)
#
# Run once, from the repo root, after cloning:
#     ./setup-dev.sh
#
# Configures the two things that CANNOT be committed to the repo and
# therefore have to be set up on each machine:
#
#   1. Unity's SmartMerge (UnityYAMLMerge) as the merge driver for
#      scene/prefab files. Without it, git silently falls back to a
#      line-based merge and mangles them.
#   2. Git LFS. Without it, art assets clone as text pointer files
#      instead of real content.
#
# Both failure modes are silent, which is why this script exists.

set -u

OK=()
FAIL=()

echo
echo "Castle Attack - developer setup"
echo "==============================="
echo

if [ ! -d .git ]; then
    echo "ERROR: no .git folder here. Run this from the repository root."
    exit 1
fi

# --- 1. Unity SmartMerge --------------------------------------------
echo "[1/2] Unity SmartMerge (scene + prefab merging)"

MERGE_TOOL=""
for candidate in \
    "$HOME/Applications/Unity/Hub/Editor"/*/Unity.app/Contents/Tools/UnityYAMLMerge \
    /Applications/Unity/Hub/Editor/*/Unity.app/Contents/Tools/UnityYAMLMerge \
    /Applications/Unity/Unity.app/Contents/Tools/UnityYAMLMerge \
    "$HOME/Unity/Hub/Editor"/*/Editor/Data/Tools/UnityYAMLMerge
do
    if [ -x "$candidate" ]; then
        MERGE_TOOL="$candidate"
    fi
done

if [ -z "$MERGE_TOOL" ]; then
    echo "      Could not find UnityYAMLMerge automatically."
    echo "      It normally lives at:"
    echo "        /Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/"
    read -r -p "      Paste the full path (or press Enter to skip): " MERGE_TOOL
fi

if [ -n "$MERGE_TOOL" ] && [ -x "$MERGE_TOOL" ]; then
    git config merge.unityyamlmerge.name "Unity SmartMerge"
    git config merge.unityyamlmerge.driver "'$MERGE_TOOL' merge -p --force --fallback none %O %B %A %A"
    git config merge.unityyamlmerge.recursive binary

    if git config --get merge.unityyamlmerge.driver >/dev/null; then
        echo "      OK - registered"
        echo "      $MERGE_TOOL"
        OK+=("SmartMerge")
    else
        echo "      FAILED - config did not stick"
        FAIL+=("SmartMerge")
    fi
else
    echo "      SKIPPED - scene merges will NOT be handled properly."
    FAIL+=("SmartMerge")
fi

echo

# --- 2. Git LFS -----------------------------------------------------
echo "[2/2] Git LFS (art assets)"

if ! git lfs version >/dev/null 2>&1; then
    echo "      NOT INSTALLED."
    echo "      macOS:  brew install git-lfs"
    echo "      Linux:  apt install git-lfs   (or see https://git-lfs.com)"
    echo "      Then re-run this script."
    FAIL+=("Git LFS")
else
    git lfs install >/dev/null
    echo "      OK - $(git lfs version)"
    OK+=("Git LFS")
    echo "      Fetching any LFS content not yet pulled..."
    git lfs pull >/dev/null 2>&1 || true
fi

# --- summary --------------------------------------------------------
echo
echo "==============================="
if [ ${#FAIL[@]} -eq 0 ]; then
    echo "All set. You're good to work."
else
    [ ${#OK[@]} -gt 0 ] && echo "Configured:   ${OK[*]}"
    echo "STILL NEEDED: ${FAIL[*]}"
    echo
    echo "Do not skip these - both fail silently and corrupt work."
fi
echo
echo "Reminder: don't edit the same scene as someone else at the"
echo "same time. SmartMerge makes conflicts survivable, not fun."
echo

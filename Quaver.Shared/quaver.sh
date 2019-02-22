#!/bin/sh

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
export LD_LIBRARY_PATH="$LD_LIBRARY_PATH:$SCRIPT_DIR"
exec dotnet "$SCRIPT_DIR/Quaver.dll" "$@"

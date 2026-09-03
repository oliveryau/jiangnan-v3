#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
WORKSPACE="$SCRIPT_DIR/.."
LUBAN_DLL="$WORKSPACE/Tools/Luban/Luban.dll"
CONF_ROOT="$SCRIPT_DIR"
OUTPUT_DIR="$WORKSPACE/Assets/Res/Resources/Config"

dotnet "$LUBAN_DLL" \
    -t all \
    -d json \
    --conf "$CONF_ROOT/luban.conf" \
    -x outputDataDir="$OUTPUT_DIR"

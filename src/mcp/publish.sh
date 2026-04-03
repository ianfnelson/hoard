#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

dotnet publish "$SCRIPT_DIR/Hoard.Mcp" -c Release -o "$SCRIPT_DIR/publish"

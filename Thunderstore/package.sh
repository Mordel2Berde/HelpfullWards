#!/usr/bin/env bash
# Builds the Thunderstore package zip for HelpfullWards.
# Usage: ./Thunderstore/package.sh
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TS="$ROOT/Thunderstore"
STAGE="$TS/build"
DLL="$ROOT/bin/Release/HelpfullWards.dll"

# Read version from manifest so the zip name stays in sync.
VERSION="$(grep -oP '"version_number"\s*:\s*"\K[^"]+' "$TS/manifest.json")"
OUT="$TS/HelpfullWards-$VERSION.zip"

echo ">> Building Release..."
dotnet build "$ROOT/HelpfullWards.csproj" -c Release -clp:ErrorsOnly

[ -f "$DLL" ] || { echo "ERROR: $DLL not found"; exit 1; }

# Icon is mandatory and must be exactly 256x256.
if [ ! -f "$TS/icon.png" ]; then
  echo "ERROR: $TS/icon.png is missing (Thunderstore requires a 256x256 PNG)."
  exit 1
fi
SIZE="$(identify -format '%wx%h' "$TS/icon.png" 2>/dev/null || true)"
if [ "$SIZE" != "256x256" ]; then
  echo "ERROR: icon.png must be exactly 256x256 (found: ${SIZE:-unknown})."
  exit 1
fi

echo ">> Staging files..."
rm -rf "$STAGE"
mkdir -p "$STAGE/Translations"
cp "$TS/manifest.json" "$TS/README.md" "$TS/CHANGELOG.md" "$TS/icon.png" "$STAGE/"
cp "$DLL" "$STAGE/"
cp "$ROOT/bin/Release/Translations/"*.json "$STAGE/Translations/" 2>/dev/null \
  || cp "$ROOT/bin/Debug/Translations/"*.json "$STAGE/Translations/"

echo ">> Zipping -> $OUT"
rm -f "$OUT"
( cd "$STAGE" && zip -r -X "$OUT" . >/dev/null )
rm -rf "$STAGE"

echo ">> Done: $OUT"
unzip -l "$OUT"

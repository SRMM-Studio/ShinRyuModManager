#!/bin/bash

set -euo pipefail

IS_PREVIEW=false

### Check arguments
while getopts "p" flag; do
    case "${flag}" in
      p) IS_PREVIEW=true ;;
    esac
done

### Variables
SRMM_PROJECT="$GITHUB_WORKSPACE/ShinRyuModManager/ShinRyuModManager.csproj"
UPDATER_PROJECT="$GITHUB_WORKSPACE/RyuUpdater/RyuUpdater.csproj"
SRMM_OUTPUT_DIR="$GITHUB_WORKSPACE/dist/srmm"
UPDATER_OUTPUT_DIR="$GITHUB_WORKSPACE/dist/updater"
TEMP_DIR="$RUNNER_TEMP"

FRAMEWORK="net10.0"

FILES_TO_COPY=(
  "dinput8.dll"
  "winmm.lj"
  "YakuzaParless.asi"
)

# Declares runtime and known build params
declare -A TARGET_ARGS_PROD=(
  ["linux"]="linux-x64;--self-contained -p:BuildSuffix=linux"
  ["linux-slim"]="linux-x64;--no-self-contained -p:BuildSuffix=linux-slim"
  ["windows"]="win-x64;--self-contained -p:BuildSuffix=windows"
  ["windows-slim"]="win-x64;--no-self-contained -p:BuildSuffix=windows-slim"
)

declare -A TARGET_ARGS_PREVIEW=(
  ["linux"]="linux-x64;--self-contained -p:BuildSuffix=linux-preview"
  ["linux-slim"]="linux-x64;--no-self-contained -p:BuildSuffix=linux-slim-preview"
  ["windows"]="win-x64;--self-contained -p:BuildSuffix=windows-preview"
  ["windows-slim"]="win-x64;--no-self-contained -p:BuildSuffix=windows-slim-preview"
)

if [[ "$IS_PREVIEW" = true ]]; then
  declare -n TARGET_ARGS=TARGET_ARGS_PREVIEW;
else
  declare -n TARGET_ARGS=TARGET_ARGS_PROD;
fi
  
declare -A UPDATER_TARGET_ARGS=(
  ["linux"]="linux-x64;--self-contained"
  ["windows"]="win-x64;--self-contained"
)

### Build

# Build SRMM
for TARGET in "${!TARGET_ARGS[@]}"; do
  OUT_DIR="${SRMM_OUTPUT_DIR}/${TARGET}"
  mkdir -p "${OUT_DIR}"
  
  # Reads the target's arguments and split them into an array
  IFS=";" read -r -a arr <<< "${TARGET_ARGS[${TARGET}]}"
  
  echo "Buidling SRMM ${TARGET}..."
  
  dotnet publish "${SRMM_PROJECT}" \
    -c "Release" \
    -r "${arr[0]}" \
    -f "${FRAMEWORK}" \
    -o "${OUT_DIR}" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:DebugPortablePdb=false \
    ${arr[1]}
    
  for FILE in "${FILES_TO_COPY[@]}"; do
    SRC="${TEMP_DIR}/external/${FILE}"
    
    if [[ -f "${SRC}" ]]; then
      cp "${SRC}" "${OUT_DIR}/${FILE}"
      echo "  Copied: ${FILE}"
    else
      echo "  Warning: Missing file in realease: ${FILE}"
    fi
  done
done

# Build Updater
for TARGET in "${!UPDATER_TARGET_ARGS[@]}"; do
  OUT_DIR="${UPDATER_OUTPUT_DIR}/${TARGET}"
  mkdir -p "${OUT_DIR}"
  
  # Reads the target's arguments and split them into an array
  IFS=";" read -r -a arr <<< "${TARGET_ARGS[${TARGET}]}"
  
  echo "Buidling RyuUpdater ${TARGET}..."
  
  dotnet publish "${UPDATER_PROJECT}" \
    -c "Release" \
    -r "${arr[0]}" \
    -f "${FRAMEWORK}" \
    -o "${OUT_DIR}" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:DebugPortablePdb=false \
    -p:PublishTrimmed=true \
    ${arr[1]}
done

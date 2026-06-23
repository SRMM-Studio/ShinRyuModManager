#!/bin/bash

set -euo pipefail

### Check arguments

IS_PREVIEW=false;

while getopts "s:u:p" flag; do
    case "${flag}" in
      s) SRMM_VERSION="${OPTARG}";;
      p) IS_PREVIEW=true;;
      *) echo "Usage: $0 -s <SRMM Version Number>"; exit 1;;
    esac
done

if ! [[ "${SRMM_VERSION}" =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ ]]; then
  echo "Incorrect format!"
  echo "Usage: $0 -s <SRMM Version Number>"
  exit 1
fi

# Strip leading "v"s
if [[ "${SRMM_VERSION}" =~ ^v[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ ]]; then
  SRMM_VERSION=${SRMM_VERSION#v}
fi

### Variables
SRMM_BASE_NAME="ShinRyuModManager"
DIST_SELECTOR="$GITHUB_WORKSPACE/dist"
SRMM_SELECTOR="${DIST_SELECTOR}/srmm"

SRMM_OUTPUT_DIR="${SRMM_SELECTOR}/out"

APPCAST_OUTPUT_DIR="${DIST_SELECTOR}/appcast"

rm -rf "${SRMM_OUTPUT_DIR}"

readarray -t SRMM_BUILD_DIRS < <(find "${SRMM_SELECTOR}" -mindepth 1 -maxdepth 1 -type d -printf '%f\n')

mkdir -p "${SRMM_OUTPUT_DIR}"

for TARGET in "${SRMM_BUILD_DIRS[@]}"; do
  DIR="${SRMM_SELECTOR}/${TARGET}"
  OUTPUT_TARGET_STR=$(echo "${TARGET}" | sed -e "s/\b\(.\)/\u\1/g") # Capitalizes each target word: linux-slim -> Linux-Slim
  OUTPUT_FILE_BASE="${SRMM_BASE_NAME}-${OUTPUT_TARGET_STR}-${SRMM_VERSION}"
  
  echo "Compressing ${OUTPUT_FILE_BASE}..."
  
  find "${DIR}" -name "*.pdb" -delete

  7za a "${SRMM_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.zip" -tzip -bd -y "${DIR}/*" > /dev/null
  tar czf "${SRMM_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.tar.gz" --owner=0 --group=0 --numeric-owner -C "${DIR}/" .
  
  ### Create Appcast
  
  # Really hate how this is done, but I can't think of anything better
  if [[ "${OUTPUT_FILE_BASE}" =~ -Linux- ]]; then
    OS_NAME="linux"
    EXEC_NAME="${SRMM_BASE_NAME}"
  elif [[ "${OUTPUT_FILE_BASE}" =~ -Windows- ]]; then
    OS_NAME="windows"
    EXEC_NAME="ShinRyuModManager.exe"
  fi
  
done

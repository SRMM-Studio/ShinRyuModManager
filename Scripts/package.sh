#!/bin/bash

set -euo pipefail

### Check arguments
if ! [[ "${#}" -gt 3 ]]; then
  echo "Not enough args! ${#}"
  echo "Usage: $0 -s <SRMM Version Number> -u <Updater Version Number>"
  exit 1
fi

IS_PREVIEW=false;

while getopts "s:u:p" flag; do
    case "${flag}" in
      s) SRMM_VERSION="${OPTARG}";;
      u) UPDATER_VERSION="${OPTARG}";;
      p) IS_PREVIEW=true;;
      *) echo "Usage: $0 -s <SRMM Version Number> -u <Updater Version Number>"; exit 1;;
    esac
done

if ! [[ "${SRMM_VERSION}" =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ || "${UPDATER_VERSION}" =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ ]]; then
  echo "Incorrect format!"
  echo "Usage: $0 -s <SRMM Version Number> -u <Updater Version Number>"
  exit 1
fi

# Strip leading "v"s
if [[ "${SRMM_VERSION}" =~ ^v[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ ]]; then
  SRMM_VERSION=${SRMM_VERSION#v}
fi

if [[ "${UPDATER_VERSION}" =~ ^v[0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9_]+)?$ ]]; then
  UPDATER_VERSION=${UPDATER_VERSION#v}
fi

### Variables
SRMM_BASE_NAME="ShinRyuModManager"
UPDATER_BASE_NAME="RyuUpdater"
DIST_SELECTOR="$GITHUB_WORKSPACE/dist"
SRMM_SELECTOR="${DIST_SELECTOR}/srmm"
UPDATER_SELECTOR="${DIST_SELECTOR}/updater"

SRMM_OUTPUT_DIR="${SRMM_SELECTOR}/out"
UPDATER_OUTPUT_DIR="${UPDATER_SELECTOR}/out"

APPCAST_OUTPUT_DIR="${DIST_SELECTOR}/appcast"

SRMM_URL_BASE="https://github.com/TheTrueColonel/ShinRyuModManager/releases/download/v"
UPDATER_URL_BASE="https://thetruecolonel.github.io/SRMM-AppCast/updater/"

rm -rf "${SRMM_OUTPUT_DIR}"
rm -rf "${UPDATER_OUTPUT_DIR}"

readarray -t SRMM_BUILD_DIRS < <(find "${SRMM_SELECTOR}" -mindepth 1 -maxdepth 1 -type d -printf '%f\n')
readarray -t UPDATER_BUILD_DIRS < <(find "${UPDATER_SELECTOR}" -mindepth 1 -maxdepth 1 -type d -printf '%f\n')

mkdir -p "${SRMM_OUTPUT_DIR}"
mkdir -p "${UPDATER_OUTPUT_DIR}"

for TARGET in "${SRMM_BUILD_DIRS[@]}"; do
  DIR="${SRMM_SELECTOR}/${TARGET}"
  OUTPUT_TARGET_STR=$(echo "${TARGET}" | sed -e "s/\b\(.\)/\u\1/g") # Capitalizes each target word: linux-slim -> Linux-Slim
  OUTPUT_FILE_BASE="${SRMM_BASE_NAME}-${OUTPUT_TARGET_STR}-${SRMM_VERSION}"
  
  echo "Compressing ${OUTPUT_FILE_BASE}..."

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
  
  # Create only for .zip, as that's universally available
  if [[ "$IS_PREVIEW" = false ]]; then
    netsparkle-generate-appcast \
        -a "${APPCAST_OUTPUT_DIR}" \
        --single-file "${SRMM_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.zip" \
        -o "${OS_NAME}" \
        -n "${EXEC_NAME}" \
        --output-file-name "appcast_${TARGET}" \
        --use-ed25519-signature-attribute \
        --human-readable \
        --file-version "${SRMM_VERSION}-${TARGET}" \
        -u "${SRMM_URL_BASE}${SRMM_VERSION}/" > /dev/null
  fi
  
done

if [[ "$IS_PREVIEW" = false ]]; then
  for TARGET in "${UPDATER_BUILD_DIRS[@]}"; do
    DIR="${UPDATER_SELECTOR}/${TARGET}"
    OUTPUT_TARGET_STR=$(echo "${TARGET}" | sed -e "s/\b\(.\)/\u\1/g") # Capitalizes each target word: linux-slim -> Linux-Slim
    OUTPUT_FILE_BASE="${UPDATER_BASE_NAME}-${OUTPUT_TARGET_STR}-Latest"
    
    echo "Compressing ${OUTPUT_FILE_BASE}..."
  
    7za a "${UPDATER_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.zip" -tzip -bd -y "${DIR}/*" > /dev/null
    tar czf "${UPDATER_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.tar.gz" --owner=0 --group=0 --numeric-owner -C "${DIR}/" .
    
    cp -r "${UPDATER_OUTPUT_DIR}/." $GITHUB_WORKSPACE/AppcastRepo/updater/
    
    ### Create Appcast
    
    # Really hate how this is done, but I can't think of anything better
    if [[ "${OUTPUT_FILE_BASE}" =~ -Linux- ]]; then
      OS_NAME="linux"
      EXEC_NAME="${UPDATER_BASE_NAME}"
    elif [[ "${OUTPUT_FILE_BASE}" =~ -Windows- ]]; then
      OS_NAME="windows"
      EXEC_NAME="${UPDATER_BASE_NAME}.exe"
    fi
    
    # Create only for .zip, as that's universally available
    netsparkle-generate-appcast \
      -a "${APPCAST_OUTPUT_DIR}" \
      --single-file "${UPDATER_OUTPUT_DIR}/${OUTPUT_FILE_BASE}.zip" \
      -o "${OS_NAME}" \
      -n "${EXEC_NAME}" \
      --output-file-name "appcast_ryuupdater-${TARGET}" \
      --use-ed25519-signature-attribute \
      --human-readable \
      --file-version "${UPDATER_VERSION}" \
      -u "${UPDATER_URL_BASE}" > /dev/null
  done

  ### Copy AppCasts to repo
  
  cp -r "${APPCAST_OUTPUT_DIR}/." $GITHUB_WORKSPACE/AppcastRepo/releases/
fi

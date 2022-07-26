#!/usr/bin/env bash
set -e

if [[ ! -z ${PUID+x} ]]; then
    echo 'PUID is no longer supported. Use `--user` instead.'
    exit 1
fi

if [[ ! -z ${PGID+x} ]]; then
    echo 'PGID is no longer supported. Use `--user` instead.'
    exit 1
fi

# If the script has any arguments, invoke the CLI instead. This allows the image to be used as a CLI
# with something like:
#
# ```
# docker run --rm -v ./config:/config ghcr.io/recyclarr/recyclarr sonarr
# ```
#
if [ "$#" -gt 0 ]; then
    recyclarr "$@"
else
    echo "Begin running on interval..."

    while true
    do
        . /cron.sh || true # Don't exit if the tasks fail

        echo
        echo "Pausing for interval [$RECYCLARR_INTERVAL]..."
        sleep $RECYCLARR_INTERVAL
    done
fi

#!/usr/bin/env bash

set -eo pipefail

exec oryx buildpack-detect $1

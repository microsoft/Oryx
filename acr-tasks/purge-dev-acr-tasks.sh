#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Runs every wednesday at 22:00 hrs UTC and deletes images older than 30 days 

set -e

PURGE_CMD="acr purge  --filter 'public/oryx/build:.*'  --ago 30d --untagged"
az acr task create --name weeklyBuildImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/cli:.*'  --ago 30d --untagged"
az acr task create --name weeklyCliImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/base:.*'  --ago 30d --untagged"
az acr task create --name weeklyBaseImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/pack:.*'  --ago 30d --untagged"
az acr task create --name weeklyBuildPackImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/pack-stack-base:.*'  --ago 30d --untagged"
az acr task create --name weeklyBuildPackStackBaseImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/dotnetcore:.*'  --ago 30d --untagged"
az acr task create --name weeklyDotNetCoreImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/node:.*'  --ago 30d --untagged"
az acr task create --name weeklyNodeImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/php:.*'  --ago 30d --untagged"
az acr task create --name weeklyPhpImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/python:.*'  --ago 30d --untagged"
az acr task create --name weeklyPythonImagePurgeTask -r oryxdevmcr --cmd "$PURGE_CMD" --schedule "0 22 * * WED" --timeout 9000 -c /dev/null
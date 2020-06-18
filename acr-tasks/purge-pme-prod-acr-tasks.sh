#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Runs every saturday at 22:00 hrs UTC and deletes images older than 180 days 

set -e

PURGE_CMD="acr purge  --filter 'public/oryx/build:.*'  --ago 180d --untagged"
az acr task create --name weeklyBuildImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/cli:.*'  --ago 180d --untagged"
az acr task create --name weeklyCliImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/base:.*'  --ago 180d --untagged"
az acr task create --name weeklyBaseImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/pack:.*'  --ago 180d --untagged"
az acr task create --name weeklyBuildPackImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/pack-builder:.*'  --ago 180d --untagged"
az acr task create --name weeklyPackBuilderImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/pack-stack-base:.*'  --ago 180d --untagged"
az acr task create --name weeklyBuildPackStackBaseImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/dotnetcore:.*'  --ago 180d --untagged"
az acr task create --name weeklyDotNetCoreImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/node:.*'  --ago 180d --untagged"
az acr task create --name weeklyNodeImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/php:.*'  --ago 180d --untagged"
az acr task create --name weeklyPhpImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null

PURGE_CMD="acr purge  --filter 'public/oryx/python:.*'  --ago 180d --untagged"
az acr task create --name weeklyPythonImagePurgeTask -r oryxprodmcr --cmd "$PURGE_CMD" --schedule "0 22 * * SAT" --timeout 9000 -c /dev/null
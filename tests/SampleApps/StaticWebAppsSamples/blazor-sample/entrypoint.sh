#!/bin/sh -l
echo "Starting Oryx build"
oryx build /github/workspace/$1
echo "Finishing Oryx build"
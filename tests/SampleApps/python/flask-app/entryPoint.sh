#!/bin/bash
set -e

source pythonenv/bin/activate

pip install gunicorn

# NOTE
# - Make sure to change port in Dockefile if changed here
# - '-' for log files makes them to write to stdout
gunicorn --bind :5000 --access-logfile - --error-logfile - --workers=4 application:app

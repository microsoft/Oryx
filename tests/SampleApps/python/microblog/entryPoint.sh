#!/bin/bash
set -e

source pythonenv/bin/activate
pip install gunicorn

while true; do
    flask db upgrade
    if [[ "$?" == "0" ]]; then
        break
    fi
    echo Deploy command failed, retrying in 5 secs...
    sleep 5
done

flask translate compile

exec gunicorn -b :5000 --access-logfile - --error-logfile - microblog:app
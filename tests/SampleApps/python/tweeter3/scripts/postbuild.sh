#!/bin/bash

python --version
python manage.py migrate
python manage.py loaddata initial_data
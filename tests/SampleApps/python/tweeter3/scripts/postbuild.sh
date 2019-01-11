#!/bin/bash

python3 --version
python3 manage.py migrate
python3 manage.py loaddata initial_data
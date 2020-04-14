!/bin/bash

gunicorn -w 4 myapp:app
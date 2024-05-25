from tweeter3.settings.base import *

import os

SECRET_KEY = os.environ['SECRET_KEY']

DEBUG = False

AZURE_APPSERVICE_HOSTNAME = os.environ['AZURE_APPSERVICE_HOSTNAME']
ALLOWED_HOSTS = [f"{AZURE_APPSERVICE_HOSTNAME}.azurewebsites.net"]

# Database
# https://docs.djangoproject.com/en/2.1/ref/settings/#databases

DB_USER = os.environ['DB_USER']
DB_NAME = os.environ['DB_NAME']
DB_HOST = os.environ['DB_HOST']
DB_PASSWORD = os.environ['DB_PASSWORD']

DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.postgresql',
        'NAME': DB_NAME,
        'USER': f'{DB_USER}@{DB_HOST}',
        'PASSWORD': DB_PASSWORD,
        'HOST': f'{DB_HOST}.postgres.database.azure.com',
        'PORT': '',
    }
}

if os.environ.get('SEND_ADMIN_EMAILS'):
    # Optional Email Settings
    EMAIL_HOST = os.environ.get('EMAIL_HOST')
    EMAIL_PORT = os.environ.get('EMAIL_PORT')
    EMAIL_HOST_USER = os.environ.get('EMAIL_HOST_USER')
    EMAIL_HOST_PASSWORD = os.environ.get('EMAIL_HOST_PASSWORD')
    EMAIL_USE_TLS = True
    DEFAULT_FROM_EMAIL = EMAIL_HOST_USER
    EMAIL_FROM = EMAIL_HOST_USER
    EMAIL_SUBJECT_PREFIX = '[Tweeter] '
    EMAIL_BACKEND = 'django.core.mail.backends.smtp.EmailBackend'

    # ADMINS
    ADMINS = [('Website Admin', os.environ.get('EMAIL_HOST_USER'))]
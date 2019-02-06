# coding: utf-8

from __future__ import absolute_import

import os

from django.apps import apps

from celery import Celery


os.environ.setdefault("DJANGO_SETTINGS_MODULE", "reactdjango.settings.local")

app = Celery('reactdjango_tasks')
app.config_from_object('django.conf:settings', namespace='CELERY')
app.autodiscover_tasks(lambda: [n.name for n in apps.get_app_configs()])

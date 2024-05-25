"""
tweeter3 URL Configuration
"""
from django.conf.urls import include, url
from django.contrib import admin
from django.urls import path
from rest_framework import routers

from tweeter import views

router = routers.DefaultRouter()
router.register(r'users', views.UserViewSet)
router.register(r'tweets', views.TweetViewSet)

urlpatterns = [
    url(r'^$', views.index, name='index'),
    path('admin/', admin.site.urls),
    url(r'^api/', include(router.urls)),
    url(r'^api-auth/', include('rest_framework.urls', namespace='rest_framework')),
]

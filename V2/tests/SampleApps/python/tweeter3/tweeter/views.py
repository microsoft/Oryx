from django.contrib.auth import authenticate, login
from django.shortcuts import render
from rest_framework import viewsets
from rest_framework.permissions import IsAuthenticatedOrReadOnly

from tweeter.models import Tweet, User
from tweeter.permissions import IsAuthorOrReadOnly, IsSelfOrAdmin
from tweeter.serializers import TweetSerializer, UserSerializer


def index(request):
    # If fixtures are loaded, let's always log in as the user Bob.
    bob = User.objects.filter(first_name='Bob').first()
    if bob:
        login(request, bob)

    return render(request, 'tweeter/index.html')


class UserViewSet(viewsets.ModelViewSet):
    queryset = User.objects.all()
    serializer_class = UserSerializer
    permission_classes = [IsSelfOrAdmin]


class TweetViewSet(viewsets.ModelViewSet):
    queryset = Tweet.objects.all()
    serializer_class = TweetSerializer
    permission_classes = [IsAuthorOrReadOnly]

    def perform_create(self, serializer):
        serializer.save(user=self.request.user)

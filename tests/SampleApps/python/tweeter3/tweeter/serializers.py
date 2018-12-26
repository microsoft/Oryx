from django.contrib.auth.models import User
from rest_framework import serializers

from tweeter.models import Tweet


class TweetSerializer(serializers.ModelSerializer):
    user = serializers.SlugRelatedField(
        many=False,
        read_only=True,
        slug_field='username'
    )

    class Meta:
        model = Tweet
        fields = ('user', 'text', 'timestamp')

    def validate_text(self, value):
        if len(value) < 5:
            raise serializers.ValidationError(
                'Text is too short.'
            )
        if len(value) > 140:
            raise serializers.ValidationError(
                'Text is too long.'
            )
        return value


class UserSerializer(serializers.ModelSerializer):
    tweets = TweetSerializer(many=True, source="tweet_set")

    class Meta:
        model = User
        fields = ('username', 'first_name', 'last_name', 'last_login', 'tweets')

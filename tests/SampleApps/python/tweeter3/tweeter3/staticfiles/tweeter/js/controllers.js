var tweeterControllers = angular.module('tweeterApp.controllers', []);

tweeterControllers.controller('TweetCtrl', function TweetCtrl($scope, Tweet) {
    $scope.tweets = {};

    Tweet.query(function (response) {
        $scope.tweets = response;
    });

    $scope.submitTweet = function (text) {
        var tweet = new Tweet({ text: text });
        tweet.$save(function () {
            $scope.tweets.unshift(tweet);
        })
    }
});

tweeterControllers.controller('UserCtrl', function UserCtrl($scope, Tweet, User, AuthUser) {
    $scope.tweets = {};
    id = AuthUser.id;
    User.get({ id: id }, function (response) {
        $scope.user = response;
        $scope.tweets = response.tweets;
    });
});
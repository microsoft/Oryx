angular.module('tweeterApp', [
    'ui.router',
    'ngResource',
    'tweeterApp.services',
    'tweeterApp.controllers',
])
    .config(function ($interpolateProvider, $httpProvider, $resourceProvider, $stateProvider, $urlRouterProvider) {
        // Force angular to use square brackets for template tag
        // The alternative is using {% verbatim %}
        $interpolateProvider.startSymbol('[[').endSymbol(']]');

        // CSRF Support
        $httpProvider.defaults.xsrfCookieName = 'csrftoken';
        $httpProvider.defaults.xsrfHeaderName = 'X-CSRFToken';

        // This only works in angular 3!
        // It makes dealing with Django slashes at the end of everything easier.
        $resourceProvider.defaults.stripTrailingSlashes = false;

        // Django expects jQuery like headers
        // $httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';

        // Routing

        $urlRouterProvider.otherwise('/');
        $stateProvider
            .state('tweets', {
                url: '/',
                templateUrl: 'static/tweeter/partials/tweet-list.html',
                controller: 'TweetCtrl',
            })
            .state('my-tweets', {
                url: '/:userId',
                templateUrl: 'static/tweeter/partials/tweet-list.html',
                controller: 'UserCtrl',
            })
            .state('profile', {
                url: '/profile/:userId',
                templateUrl: 'static/tweeter/partials/profile.html',
                controller: 'UserCtrl',
            })
    });

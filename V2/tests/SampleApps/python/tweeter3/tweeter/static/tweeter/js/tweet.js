$.ajax({
    type: "GET",
    url: "/ajax",
    success: function (data) {
        for (var i = 0; i < data.tweets.length; i++) {
            appendNewTweet(data.tweets[i]);
        }
    }
});

function appendNewTweet(tweet) {
    var newTweet = "<div class='tweet-container'>" +
        "<div class='tweet-time'>" + new Date(tweet.time).toLocaleString() + "</div>" +
        "<div class='tweet-body'>" + tweet.text + "</div>" +
        "</div>";

    $('#tweets-target').prepend(newTweet);
}

$('#tweet').click(function () {
    $.ajax({
        type: "POST",
        url: "/ajax",
        contentType: 'application/json',
        data: JSON.stringify({ tweet: $('#new-tweet').val() }),
        success: function (data) {
            appendNewTweet(data);
            $('#new-tweet').val('');
        }
    })
});
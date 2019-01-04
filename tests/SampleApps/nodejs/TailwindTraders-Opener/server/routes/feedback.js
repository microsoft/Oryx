const bodyParser = require('body-parser');
const express = require('express');

const router = express.Router();
router.use(bodyParser.urlencoded({ extended: true }));

const db = require('../db');
router.post('/', function stickerRouteFeedback(req, res) {
    db.addFeedback(req.body, () => {
        console.log('Feedback added');
        res.render('index', { pageTitle: 'Feedback', entry: 'feedback' });
    });
});

module.exports = router;
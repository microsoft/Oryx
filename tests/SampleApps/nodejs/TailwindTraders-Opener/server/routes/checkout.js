const bodyParser = require('body-parser');
const express = require('express');

const router = express.Router();
router.use(bodyParser.urlencoded({ extended: true }));

const db = require('../db');
router.post('/', function stickerRouteCheckout(req, res) {
    if (!req.body.token) {
        res.status(401).send('Unauthorized');
        return;
    }
    db.addOrder({
        items: req.body['checkout-items'],
        name: req.body['checkout-name'],
        email: req.body['checkout-email'],
        token: req.body.token
    }).then(() => {
        console.log('Order added');
        return db.clearCart(req.body.token);
    }).then(() => {
        res.render('index', { pageTitle: 'Checkout', entry: 'checkout' });
    });
});

module.exports = router;
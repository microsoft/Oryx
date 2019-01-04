const bodyParser = require('body-parser');
const express = require('express');

const router = express.Router();
router.use(bodyParser.json());

router.get('/', function stickerRouteCart(req, res) {
    res.render('index', { pageTitle: 'Cart', entry: 'cart' });
});

const db = require('../db');
function sendItems(token, res) {
    db.getCart(token).then((cart) => {
        if (!cart || !cart.items || cart.items.length === 0) {
            return res.send({ items: [] });
        }

        db.getStickers().then((stickers) => {
            res.send({ items: cart.items.map((id) => stickers.filter((sticker) => sticker.id.toString() === id)[0]) });
        });
    }, () => {
        res.send({ items: [] });
    });
}

router.get('/api/items', (req, res) => {
    if (!req.query.token) {
        res.status(401).send('Unauthorized');
        return;
    }
    sendItems(req.query.token, res);
});

router.put('/api/items/:item_id', (req, res) => {
    if (!req.body.token) {
        res.status(401).send('Unauthorized');
        return;
    }

    console.log('Item targetted %s', req.params.item_id);

    db.addToCart(req.body.token, req.params.item_id).then(() => {
        return db.getSticker(req.params.item_id);
    }).then((sticker) => {
        if (!sticker) {
            db.addStickers([ req.body.item ]).then(() => {
                sendItems(req.body.token, res);
            });
        } else {
            sendItems(req.body.token, res);
        }
    });
});

router.delete('/api/items/:item_id', (req, res) => {
    if (!req.body.token) {
        res.status(401).send('Unauthorized');
        return;
    }

    console.log('Item targetted', req.params.item_id);

    db.removeFromCart(req.body.token, req.params.item_id).then(() => {
        sendItems(req.body.token, res);
    });
});

module.exports = router;
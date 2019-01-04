const bodyParser = require('body-parser');
const express = require('express');

const router = express.Router();
router.use(bodyParser.json());

router.get('/', function stickerRouteBrowse(req, res) {
    const renderData = { pageTitle: 'Browse', entry: 'browse' };
    console.log('Render values: ', renderData);
    res.render('index', renderData);
});

const db = require('../db');
router.get('/api/items', function stickerRouteApiBrowse(req, res) {
    let tags;
    if (req.query.tags) {
        tags = req.query.tags.split(',');
    }

    db.getStickers(tags).then((items) => {
        console.info('%d stickers found', items.length);
        if (tags) {
            console.log('Tags used in filter: ', tags);
        }
        items.splice(1,1); // hack to remove gu gnome
        res.send({ items });
    }, () => {
        res.send({ items: [] });
    });
});

module.exports = router;
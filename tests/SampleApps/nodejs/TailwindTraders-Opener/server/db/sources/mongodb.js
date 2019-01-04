const { MongoClient } = require('mongodb');
const config = require('../../config').mongodb;

console.log('mongo config: ' + config);

let connection;
function connect() {
    if (!connection) {
        return MongoClient.connect(config.url).then((db) => {
            connection = db;
            return connection;
        });
    }

    return Promise.resolve(connection);
}

function disconnect() {
    if (connection) {
        return connection.close().then(() => {
            connection = null;
        });
    }

    return Promise.resolve();
}

function findDoc(db, collectionName, criteria) {
    return db.collection(collectionName).findOne(criteria);
}

function findDocs(db, collectionName, criteria) {
    return db.collection(collectionName).find(criteria).toArray();
}

function insertDocs(db, collectionName, docs) {
    return db.collection(collectionName).insert(docs);
}

function removeDoc(db, collectionName, criteria) {
    return db.collection(collectionName).remove(criteria, {
        single: true,
        w: 1
    });
}

function updateDocs(db, collectionName, criteria, doc) {
    return db.collection(collectionName).update(criteria, doc, {
        multi: true,
        upsert: false,
        w: 1
    });
}

exports.addFeedback = (doc) => {
    console.log('mongodb.js: addFeedback');
    return connect().then((db) => {
        return insertDocs(db, config.feedbackCollectionName, doc);
    });
};

exports.addOrder = (doc) => {
    console.log('mongodb.js: addOrder');
    return connect().then((db) => {
        return insertDocs(db, config.orderCollectionName, doc);
    });
};

exports.addStickers = (items) => {
    console.log('mongodb.js: addStickers');
    return connect().then((db) => {
        return insertDocs(db, config.stickerCollectionName, items);
    });
};

exports.addToCart = (token, itemId) => {
    console.log('mongodb.js: addToCart');
    return connect().then((db) => {
        return findDoc(db, config.cartCollectionName, { _id: token }).then((cart) => {
            if (!cart) {
                return insertDocs(db, config.cartCollectionName, {
                    _id: token,
                    items: [ itemId ]
                });
            } else {
                return updateDocs(db, config.cartCollectionName, { _id: token }, {
                    $addToSet: { items: itemId }
                });
            }
        });
    });
};

exports.clearCart = (token) => {
    console.log('mongodb.js: clearCart');
    return connect().then((db) => {
        return removeDoc(db, config.cartCollectionName, {
            _id: token
        });
    });
};

exports.getCart = (token) => {
    console.log('mongodb.js: getCart');
    return connect().then((db) => {
        return findDoc(db, config.cartCollectionName, { _id: token });
    });
};

exports.getSticker = (id) => {
    console.log('mongodb.js: getSticker');
    return connect().then((db) => {
        return findDoc(db, config.stickerCollectionName, { id });
    });
};

exports.getStickers = (tags) => {
    console.log('mongodb.js: getStickers');
    return connect().then((db) => {
        const query = {};
        
        return findDocs(db, config.stickerCollectionName, query);
    });
};

exports.initializeDatabase = () => {
    const initialData = require('../initial-data');
    
    return connect()
        .then((db) => db.dropDatabase())
        .then(() => exports.addStickers(initialData))
        .then(disconnect);
};

exports.removeFromCart = (token, itemId) => {
    console.log('mongodb.js: removeFromCart');
    return connect().then((db) => {
        return findDoc(db, config.cartCollectionName, { _id: token }).then((cart) => {
            if (!cart) {
                return;
            }

            return updateDocs(db, config.cartCollectionName, { _id: token }, {
                $pull: { items: itemId }
            });
        });
    });
};
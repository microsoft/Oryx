module.exports = {
    'server': {
        'port': process.env.PORT || 3000,
        'https': false
    },
    'dataSource': 'mongodb',
    'mongodb': {
        'port': 27017,
        'host': 'localhost', //docker.for.mac.localhost (for docker)
        'dbName': 'gnomesDB',
        'stickerCollectionName': 'gnomes',
        'orderCollectionName': 'orders',
        'feedbackCollectionName': 'feedback',
        'cartCollectionName': 'carts',
        get url() {
            const mongodbUri = require('mongodb-uri');
            const url = process.env.MONGO_URL || `mongodb://${this.host}:${this.port}`;
            const urlObject = mongodbUri.parse(url);
            urlObject.database = this.dbName;
            return mongodbUri.format(urlObject);
        }
    }
};

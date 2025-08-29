const _ = require('lodash');

function formatMessage(message) {
    return _.upperFirst(message);
}

function getTimestamp() {
    return new Date().toISOString();
}

module.exports = {
    formatMessage,
    getTimestamp
};
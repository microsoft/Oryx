'use strict';

const initialData = require('../initial-data');
const carts = {};

function filterItems(tags, items) {
    let filteredItems = items;
    if (tags.length) {
        filteredItems = items.filter((item) => {
            let match = false;
            for (const tag of tags) {
                if (item.tags.indexOf(tag) !== -1) {
                    match = true;
                }
            }
            return match;
        });
    }
    return filteredItems;
}

function getStickers(tags) {
    let items = initialData;
    if (tags) {
        items = filterItems(tags, items);
    }
    return Promise.resolve(items);
}

function getSticker(id) {
    return Promise.resolve(initialData.filter((item) => item.id === id)[0]);
}

function addStickers(items) {
    for (const item of items) {
        for (let i = 0; i < initialData.length; i++) {
            if (initialData[i].id === item.id) {
                initialData.splice(i, 1);
                break;
            }
        }
        initialData.push(item);
    }
    return Promise.resolve();
}

function getCart(token) {
    if (!carts[token]) {
        carts[token] = [];
    }
    return Promise.resolve({ items: carts[token] });
}

function addToCart(token, itemId) {
    if (!carts[token]) {
        carts[token] = [];
    }
    for (const existingId of carts[token]) {
        if (itemId === existingId) {
            return Promise.resolve();
        }
    }
    carts[token].push(itemId);
    return Promise.resolve();
}

function removeFromCart(token, itemId) {
    if (!carts[token]) {
        carts[token] = [];
    }
    for (let i = 0; i < carts[token].length; i++) {
        if (itemId === carts[token][i]) {
            carts[token].splice(i, 1);
            break;
        }
    }
    return Promise.resolve();
}

function clearCart(token) {
    carts[token] = [];
    return Promise.resolve();
}
 
/* eslint-disable no-unused-vars */

function addOrder(order) {
    return Promise.resolve();
}

function addFeedback(feedback) {
    return Promise.resolve();
}

/* eslint-enableno-unused-vars */

function initializeDatabase() {
    return Promise.resolve();
}

module.exports = {
    getStickers,
    getSticker,
    addStickers,
    getCart,
    addToCart,
    removeFromCart,
    clearCart,
    addOrder,
    addFeedback,
    initializeDatabase
};
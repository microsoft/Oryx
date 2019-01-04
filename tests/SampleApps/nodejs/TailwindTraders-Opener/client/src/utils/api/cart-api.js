import { createUpdateFailedAction, createItemsUpdatedAction, createRecommendationsUpdatedAction } from '../../actions/cart-actions';
import { request } from '../api';
import axios from 'axios';
import data from '../../../../server/db/initial-data';

export function updateItems() {
    request({
        url: 'cart/api/items'
    }, (err, res) => {
        if (err) {
            createUpdateFailedAction();
            return;
        }
        createItemsUpdatedAction(res.items);
    });
}

export function getRecommendations() {
    axios.get(`https://recommender-test-4.azurewebsites.net/api/GetRecommendations?code=GHvuTHyv5jd5EsTK44lwQgwTQEwk2PbI6zkS7rugaVVjM7dInG4SQA==`)
        .then(res => {
            console.log("REsponse data: " + res.data);

            // The API returns the complete list of items to force the system to get
            // in sync, in case something bad happened to get it out of sync

            // let dataResult = data.slice(0,4);

            createRecommendationsUpdatedAction(res.data);
        }).catch((err, res) => {
            console.log(err);
        });
}

export function addItem(item) {
    request({
        url: `cart/api/items/${item.id}`,
        method: 'PUT',
        payload: { item }
    }, (err, res) => {
        if (err) {
            createUpdateFailedAction();
            return;
        }

        // The API returns the complete list of items to force the system to get
        // in sync, in case something bad happened to get it out of sync
        createItemsUpdatedAction(res.items);
    });
}

export function removeItem(item) {
    request({
        url: `cart/api/items/${item.id}`,
        method: 'DELETE'
    }, (err, res) => {
        if (err) {
            createUpdateFailedAction();
            return;
        }

        // The API returns the complete list of items to force the system to get
        // in sync, in case something bad happened to get it out of sync
        createItemsUpdatedAction(res.items);
    });
}

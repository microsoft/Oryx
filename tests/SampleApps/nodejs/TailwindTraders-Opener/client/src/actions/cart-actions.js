import dispatcher from '../dispatcher';
import { CART_ACTIONS } from './actions.js';
import { addItem, removeItem, getRecommendations } from '../utils/api/cart-api';

export function createItemsUpdatedAction(items) {
    dispatcher.dispatch({
        actionType: CART_ACTIONS.ITEMS_UPDATED_ACTION,
        items
    });
}

export function createRecommendationsUpdatedAction(items) {
    dispatcher.dispatch({
        actionType: CART_ACTIONS.RECS_ADDED_ACTION,
        items
    })
}

export function createUpdateFailedAction() {
    dispatcher.dispatch({
        actionType: CART_ACTIONS.UPDATE_FAILED_ACTION
    });
}

export function createAddToCartAction(item) {
    addItem(item);
}

export function createRemoveFromCartAction(item) {
    removeItem(item);
}

export function createRecommendationsAddedAction() {
    getRecommendations();
}


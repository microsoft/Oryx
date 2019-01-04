import dispatcher from '../dispatcher';
import { BROWSE_ACTIONS } from './actions.js';
import { updateItems } from '../utils/api/browse-api';

const tags = [];

export function createAddTagFilterAction(tag) {
    tags.push(tag);
    updateItems(tags);
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.ADD_TAG_FILTER_ACTION,
        tag
    });
}

export function createRemoveTagFilterAction(tag) {
    const tagIndex = tags.indexOf(tag);
    if (tagIndex !== -1) {
        tags.splice(tagIndex, 1);
        updateItems(tags);
    }
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.REMOVE_TAG_FILTER_ACTION,
        tag
    });
}

export function createExpandItemAction(id) {
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.EXPAND_ITEM_ACTION,
        id
    });
}

export function createCloseExpandedItemAction() {
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.CLOSE_EXPANDED_ITEM_ACTION
    });
}

export function createUpdateFailedAction() {
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.UPDATE_FAILED_ACTION
    });
}

export function createItemsUpdatedAction(items) {
    dispatcher.dispatch({
        actionType: BROWSE_ACTIONS.ITEMS_UPDATED_ACTION,
        items
    });
}

import dispatcher from '../dispatcher';
import { CREATE_ACTIONS } from './actions.js';

export function createSearchFailedAction() {
    dispatcher.dispatch({
        actionType: CREATE_ACTIONS.CREATE_SEARCH_FAILED_ACTION
    });
}

export function createSearchSucceededAction(items) {
    dispatcher.dispatch({
        actionType: CREATE_ACTIONS.CREATE_SEARCH_SUCCEEDED_ACTION,
        items
    });
}

export function createExpandItemAction(id) {
    dispatcher.dispatch({
        actionType: CREATE_ACTIONS.EXPAND_ITEM_ACTION,
        id
    });
}

export function createCloseExpandedItemAction() {
    dispatcher.dispatch({
        actionType: CREATE_ACTIONS.CLOSE_EXPANDED_ITEM_ACTION
    });
}

import { ReduceStore } from 'flux/utils';
import dispatcher from '../dispatcher';
import { CREATE_ACTIONS } from '../actions/actions';
import { searchImage } from '../utils/api/create-api';

const DEFAULT_KEYWORD = '36daysoftype';

searchImage(DEFAULT_KEYWORD);

class CreateStore extends ReduceStore {

    getInitialState() {
        return {
            defaultKeyword: DEFAULT_KEYWORD,
            items: [],
            expandedItem: null
        };
    }

    reduce(state, action) {
        switch (action.actionType) {
            case CREATE_ACTIONS.CREATE_SEARCH_SUCCEEDED_ACTION: {
                return Object.assign({}, state, {
                    items: action.items
                });
            }

            case CREATE_ACTIONS.EXPAND_ITEM_ACTION: {
                const expandedItem = state.items.filter((item) => item.id === action.id)[0];
                if (!expandedItem) {
                    throw new Error(`Internal error: id ${action.id} does not exist`);
                }
                return Object.assign({}, state, {
                    expandedItem
                });
            }

            case CREATE_ACTIONS.CLOSE_EXPANDED_ITEM_ACTION: {
                return Object.assign({}, state, {
                    expandedItem: null
                });
            }

            default: {
                return state;
            }
        }
    }
}

export default new CreateStore(dispatcher);

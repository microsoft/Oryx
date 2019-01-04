// @ts-nocheck
import { ReduceStore } from 'flux/utils';
import dispatcher from '../dispatcher';
import { BROWSE_ACTIONS } from '../actions/actions';
import { updateItems } from '../utils/api/browse-api';
import { readCookie } from '../utils/cookies';

let selectedTags = readCookie('searchTags');
if (selectedTags) {
    selectedTags = JSON.parse(selectedTags);
} else {
    selectedTags = [];
}

// Kickstart the initial fetch of items
updateItems(selectedTags);

class BrowseStore extends ReduceStore {

    getInitialState() {
        return {
            // TODO: this is a badish hack... need to rework how the server provides the tags
            tags: ['Deployment', 'Service', 'Framework', 'Language', 'Tooling', 'Protocol'],
            selectedTags,
            items: [],
            expandedItem: null
        };
    }

    reduce(state, action) {
        let selectedTags; // switch statements don't count as blocke-scoped
        switch (action.actionType) {
            case BROWSE_ACTIONS.ITEMS_UPDATED_ACTION: {
                const tags = ([]).concat(state.tags);
                for (const item of action.items) {
                    for (const tag of item.tags) {
                        if (tags.indexOf(tag) === -1) {
                            tags.push(tag);
                        }
                    }
                }
                return {
                    tags,
                    selectedTags: state.selectedTags,
                    items: action.items
                };
            }

            case BROWSE_ACTIONS.ADD_TAG_FILTER_ACTION: {
                selectedTags = state.selectedTags.concat([ action.tag ]);
                return {
                    tags: state.tags,
                    selectedTags,
                    items: state.items
                };
            }

            case BROWSE_ACTIONS.REMOVE_TAG_FILTER_ACTION: {
                selectedTags = ([]).concat(state.selectedTags); // Make a copy of the array so we can modify it
                selectedTags.splice(selectedTags.indexOf(action.tag), 1);
                return {
                    tags: state.tags,
                    selectedTags,
                    items: state.items
                };
            }

            case BROWSE_ACTIONS.EXPAND_ITEM_ACTION: {
                const expandedItem = state.items.filter((item) => item.id === action.id)[0];
                if (!expandedItem) {
                    throw new Error(`Internal error: id ${action.id} does not exist`);
                }
                return Object.assign({}, state, {
                    expandedItem
                });
            }

            case BROWSE_ACTIONS.CLOSE_EXPANDED_ITEM_ACTION: {
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

export default new BrowseStore(dispatcher);

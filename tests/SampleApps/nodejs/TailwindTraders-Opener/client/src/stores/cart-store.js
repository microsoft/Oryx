import { ReduceStore } from 'flux/utils';
import dispatcher from '../dispatcher';
import { CART_ACTIONS } from '../actions/actions';
import { updateItems, getRecommendations } from '../utils/api/cart-api';

// Kickstart the initial fetch of items
updateItems();
getRecommendations();

class CartStore extends ReduceStore {

    getInitialState() {
        return {
            items: [],
            recommendations: []
        };
    }

    reduce(state, action) {
        switch (action.actionType) {
            case CART_ACTIONS.ITEMS_UPDATED_ACTION: {
                return {
                    items: action.items,
                    recommendations: state.recommendations
                };
            }

            case CART_ACTIONS.RECS_ADDED_ACTION: {
                return {
                    items: state.items,
                    recommendations: action.items
                }
            }

            
            default: {
                return state;
            }
        }
    }
}

export default new CartStore(dispatcher);

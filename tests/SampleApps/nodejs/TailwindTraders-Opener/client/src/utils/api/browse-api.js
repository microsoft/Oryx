import { createUpdateFailedAction, createItemsUpdatedAction } from '../../actions/browse-actions';
import { request } from '../api';
import { createCookie } from '../cookies';

export function updateItems(tags = []) {
    // Update cookie here
    createCookie('searchTags', JSON.stringify(tags), 30);

    request({
        url: 'browse/api/items',
        payload: { tags }
    }, (err, res) => {
        if (err) {
            createUpdateFailedAction();
            return;
        }
        createItemsUpdatedAction(res.items);
    });
}

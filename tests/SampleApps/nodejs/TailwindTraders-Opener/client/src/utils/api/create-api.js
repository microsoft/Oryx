import { createSearchFailedAction, createSearchSucceededAction } from '../../actions/create-actions';
import { request } from '../api';

export function searchImage(keyword) {
    request({
        url: 'create/api/search',
        payload: { keyword }
    }, (err, res) => {
        if (err) {
            createSearchFailedAction();
            return;
        }
        createSearchSucceededAction(res.items);
    });
}

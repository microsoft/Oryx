import axios from 'axios';
const baseUrl = '/api/lists';

export const itemService = {
    list: async (listId) => {
        const response = await axios.get(`${baseUrl}/${listId}/items`);
        return response.data;
    },
    save: async (item) => {
        if (!item.id) {
            // New item, post/save
            item.state = 'active';
            const response = await axios.post(`${baseUrl}/${item.listId}/items`, item);
            return response.data;
        } else {
            // Existing item updating, put/save
            const response = await axios.put(`${baseUrl}/${item.listId}/items/${item.id}`, item);
            return response.data;
        }
    },
    delete: async (item) => {
        await axios.delete(`${baseUrl}/${item.listId}/items/${item.id}`);
        return item;
    }
}
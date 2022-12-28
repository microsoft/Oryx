import axios from 'axios';
const baseUrl = '/api/lists';

export const listService = {
    list: async () => {
        const response = await axios.get(baseUrl);
        return response.data;
    },
    save: async (list) => {
        const response = await axios.post(baseUrl, list);
        return response.data;
    }
}
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { itemService } from "./itemAPI";

const initialState = {
    data: [],
    status: 'idle'
}

export const listAsync = createAsyncThunk(
    'items/getList',
    async (data) => {
        return await itemService.list(data);
    }
)

export const loadAsync = createAsyncThunk(
    'items/load',
    async (data) => {
        return await itemService.load(data);
    }
)

export const saveAsync = createAsyncThunk(
    'items/save',
    async (data) => {
        return await itemService.save(data);
    }
)

export const removeAsync = createAsyncThunk(
    'items/remove',
    async (data) => {
        return await itemService.delete(data);
    }
)

export const itemSlice = createSlice({
    name: 'item',
    initialState,
    reducers: {

    },
    extraReducers: (builder) => {
        builder
            .addCase(listAsync.fulfilled, (state, action) => {
                // put items into state
                state.data = action.payload;
            })
            .addCase(removeAsync.fulfilled, (state, action) => {
                // remove item from state
                state.data = state.data.filter(i => i.id !== action.payload.id);
            })
            .addCase(saveAsync.fulfilled, (state, action) => {
                // See if current item exists
                const existingItemIndex = state.data.findIndex(i => i.id === action.payload.id);
                if (existingItemIndex > -1) {
                    // item exists, replace with updated item
                    state.data[existingItemIndex] = action.payload;
                }
                else {
                    // item does not exist, add new item to end
                    state.data.push(action.payload);
                }
            })
    },
})

export const selectListItems = (state) => {
    return state.items.data;
}
export default itemSlice.reducer;
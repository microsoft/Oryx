import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { listService } from "./listAPI";

const initialState = {
    data: [],
    activeList: {},
    status: 'idle'
}

export const listAsync = createAsyncThunk(
    'lists/get',
    async () => {
        return await listService.list();
    }
)

export const saveAsync = createAsyncThunk(
    'lists/save',
    async (list) => {
        return await listService.save(list);
    }
)

export const listSlice = createSlice({
    name: 'list',
    initialState,
    reducers: {
        activateList(state, action) {
            state.activeList = action.payload
        }
    },
    extraReducers: (builder) => {
        builder
            .addCase(listAsync.fulfilled, (state, action) => {
                // all lists loaded
                // put lists into state
                state.data = action.payload
            })
            .addCase(saveAsync.fulfilled, (state, action) => {
                // list saved
                // See if current list exists
                const existingListIndex = state.data.findIndex(l => l.id === action.payload.id);
                if (existingListIndex > -1) {
                    // list exists, replace with updated list
                    state.data[existingListIndex] = action.payload.action;
                } else {
                    // list does not exist, add to end
                    state.data.push(action.payload);
                    state.activeList = action.payload;
                }
            })
        }
})

export const selectLists = (state) => {
    return state.lists.data;
}
export const selectActiveList = (state) => {
    return state.lists.activeList;
}

export default listSlice.reducer;
export const { activateList } = listSlice.actions;
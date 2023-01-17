import { createAsyncThunk, createSlice } from "@reduxjs/toolkit"

const initialState = {
    userDetails: ''
}

export const getUserAsync = createAsyncThunk(
    'user/get',
    async () => {
        // Retrieve response from /.auth/me
        const response = await fetch('/.auth/me');
        // Convert to JSON
        const payload = await response.json();
        // Retrieve the clientPrincipal (current user)
        const { clientPrincipal } = payload;
        if (clientPrincipal) return clientPrincipal.userDetails;
        else return '';
    }
)

export const userSlice = createSlice({
    name: 'user',
    initialState,
    reducers: {},
    extraReducers: (builder) => {
        builder.addCase(getUserAsync.fulfilled, (state, action) => {
            // put user into state
            state.userDetails = action.payload;
        });
    }
});

export const selectUserDetails = (state) => {
    return state.user.userDetails;
}

export default userSlice.reducer;
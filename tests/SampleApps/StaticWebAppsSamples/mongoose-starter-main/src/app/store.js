import { configureStore } from '@reduxjs/toolkit';
// import counterReducer from '../features/counter/counterSlice';
import itemsReducer from '../features/items/itemSlice';
import listsReducer from '../features/lists/listSlice';
import userReducer from '../features/user/userSlice';

export const store = configureStore({
  reducer: {
    items: itemsReducer,
    lists: listsReducer,
    user: userReducer
  },
});

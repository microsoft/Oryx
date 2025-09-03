import { createReducer, on } from '@ngrx/store';
import { UsersState } from '../app.state';

export const initialState: UsersState = {};

export const usersReducer = createReducer(
    initialState
    // Add user actions here when needed
);

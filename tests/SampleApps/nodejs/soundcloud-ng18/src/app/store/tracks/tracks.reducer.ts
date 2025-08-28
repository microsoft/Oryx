import { createReducer, on } from '@ngrx/store';
import { TracksState } from '../app.state';

export const initialState: TracksState = {};

export const tracksReducer = createReducer(
    initialState
    // Add track actions here when needed
);

import { createReducer, on } from '@ngrx/store';
import { TracklistsState } from '../app.state';

export const initialState: TracklistsState = {};

export const tracklistsReducer = createReducer(
    initialState
    // Add tracklist actions here when needed
);

import { createReducer, on } from '@ngrx/store';
import { SearchState } from '../app.state';
import * as SearchActions from './search.actions';

export const initialState: SearchState = {
    query: '',
    results: [],
    isLoading: false,
    hasNextPage: false,
    nextUrl: null
};

export const searchReducer = createReducer(
    initialState,
    on(SearchActions.searchTracks, (state, { query }) => ({
        ...state,
        query,
        isLoading: true
    })),
    on(SearchActions.searchTracksSuccess, (state, { tracks, hasNextPage, nextUrl }) => ({
        ...state,
        results: tracks,
        isLoading: false,
        hasNextPage,
        nextUrl
    })),
    on(SearchActions.searchTracksFailure, (state) => ({
        ...state,
        isLoading: false
    })),
    on(SearchActions.loadMoreTracks, (state) => ({
        ...state,
        isLoading: true
    })),
    on(SearchActions.loadMoreTracksSuccess, (state, { tracks, hasNextPage, nextUrl }) => ({
        ...state,
        results: [...state.results, ...tracks],
        isLoading: false,
        hasNextPage,
        nextUrl
    })),
    on(SearchActions.loadMoreTracksFailure, (state) => ({
        ...state,
        isLoading: false
    })),
    on(SearchActions.clearSearchResults, () => initialState)
);

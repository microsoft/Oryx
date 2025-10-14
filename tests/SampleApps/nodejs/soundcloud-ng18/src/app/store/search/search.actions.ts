import { createAction, props } from '@ngrx/store';
import { Track } from '../app.state';

// Search Actions
export const searchTracks = createAction(
    '[Search] Search Tracks',
    props<{ query: string }>()
);

export const searchTracksSuccess = createAction(
    '[Search] Search Tracks Success',
    props<{ tracks: Track[]; hasNextPage: boolean; nextUrl: string | null }>()
);

export const searchTracksFailure = createAction(
    '[Search] Search Tracks Failure',
    props<{ error: string }>()
);

export const loadMoreTracks = createAction('[Search] Load More Tracks');

export const loadMoreTracksSuccess = createAction(
    '[Search] Load More Tracks Success',
    props<{ tracks: Track[]; hasNextPage: boolean; nextUrl: string | null }>()
);

export const loadMoreTracksFailure = createAction(
    '[Search] Load More Tracks Failure',
    props<{ error: string }>()
);

export const clearSearchResults = createAction('[Search] Clear Search Results');

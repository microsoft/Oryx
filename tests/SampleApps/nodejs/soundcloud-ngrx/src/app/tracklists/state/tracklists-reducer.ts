import { Action } from '@ngrx/store';
import { Map } from 'immutable';
import { SearchActions } from 'app/search/search-actions';
import { UserActions } from 'app/users/user-actions';
import { TracklistActions } from '../tracklist-actions';
import { tracklistReducer } from './tracklist-reducer';


export type TracklistsState = Map<string,any>;

export const initialState: TracklistsState = Map<string,any>({
  currentTracklistId: null
});


export function tracklistsReducer(state: TracklistsState = initialState, action: Action): TracklistsState {
  switch (action.type) {
    case TracklistActions.FETCH_TRACKS_FULFILLED:
      return state.set(
        action.payload.tracklistId,
        tracklistReducer(state.get(action.payload.tracklistId), action)
      );

    case TracklistActions.LOAD_NEXT_TRACKS:
      return state.set(
        state.get('currentTracklistId'),
        tracklistReducer(state.get(state.get('currentTracklistId')), action)
      );

    case TracklistActions.LOAD_FEATURED_TRACKS:
    case SearchActions.LOAD_SEARCH_RESULTS:
    case UserActions.LOAD_USER_LIKES:
    case UserActions.LOAD_USER_TRACKS:
      return state.withMutations(tracklists => {
        const { tracklistId } = action.payload;
        tracklists
          .set('currentTracklistId', tracklistId)
          .set(tracklistId, tracklistReducer(tracklists.get(tracklistId), action));
      });

    case TracklistActions.MOUNT_TRACKLIST:
      return state.set('currentTracklistId', action.payload.tracklistId);

    default:
      return state;
  }
}

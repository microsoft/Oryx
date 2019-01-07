import { Action } from '@ngrx/store';
import { FEATURED_TRACKLIST_ID, FEATURED_TRACKLIST_USER_ID } from 'app/app-config';


export class TracklistActions {
  static FETCH_TRACKS_FAILED = 'FETCH_TRACKS_FAILED';
  static FETCH_TRACKS_FULFILLED = 'FETCH_TRACKS_FULFILLED';
  static LOAD_FEATURED_TRACKS = 'LOAD_FEATURED_TRACKS';
  static LOAD_NEXT_TRACKS = 'LOAD_NEXT_TRACKS';
  static MOUNT_TRACKLIST = 'MOUNT_TRACKLIST';


  fetchTracksFailed(error: any): Action {
    return {
      type: TracklistActions.FETCH_TRACKS_FAILED,
      payload: error
    };
  }

  fetchTracksFulfilled(data: any, tracklistId: string): Action {
    return {
      type: TracklistActions.FETCH_TRACKS_FULFILLED,
      payload: Object.assign({}, data, {tracklistId})
    };
  }

  loadFeaturedTracks(): Action {
    return {
      type: TracklistActions.LOAD_FEATURED_TRACKS,
      payload: {
        tracklistId: FEATURED_TRACKLIST_ID,
        userId: FEATURED_TRACKLIST_USER_ID
      }
    };
  }

  loadNextTracks(): Action {
    return {
      type: TracklistActions.LOAD_NEXT_TRACKS
    };
  }

  mountTracklist(tracklistId: string): Action {
    return {
      type: TracklistActions.MOUNT_TRACKLIST,
      payload: {
        tracklistId
      }
    };
  }
}

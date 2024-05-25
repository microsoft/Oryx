import { Action } from '@ngrx/store';
import { Map } from 'immutable';
import { createTrack, ITrack, ITrackData } from '../models';
import { TracklistActions } from '../tracklist-actions';


export type TracksState = Map<number,ITrack>;

const initialState: TracksState = Map<number,ITrack>();


export function tracksReducer(state: TracksState = initialState, action: Action): TracksState {
  switch (action.type) {
    case TracklistActions.FETCH_TRACKS_FULFILLED:
      return state.withMutations(tracks => {
        action.payload.collection.forEach((data: ITrackData) => {
          tracks.set(data.id, createTrack(data));
        });
      });

    default:
      return state;
  }
}

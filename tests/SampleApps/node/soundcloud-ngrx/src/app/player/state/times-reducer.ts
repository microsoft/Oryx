import { Action } from '@ngrx/store';
import { PlayerActions } from '../player-actions';
import { ITimesState, TimesStateRecord } from './times-state';


export const initialState: ITimesState = new TimesStateRecord() as ITimesState;


export function timesReducer(state: ITimesState = initialState, {payload, type}: Action): ITimesState {
  switch (type) {
    case PlayerActions.AUDIO_ENDED:
    case PlayerActions.PLAY_SELECTED_TRACK:
      return new TimesStateRecord() as ITimesState;

    case PlayerActions.AUDIO_TIME_UPDATED:
      return state.merge(payload) as ITimesState;

    default:
      return state;
  }
}

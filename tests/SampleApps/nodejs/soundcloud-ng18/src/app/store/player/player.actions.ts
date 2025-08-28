import { createAction, props } from '@ngrx/store';
import { Track } from '../app.state';

// Player Actions
export const loadTrack = createAction(
    '[Player] Load Track',
    props<{ trackId: string }>()
);

export const loadTrackSuccess = createAction(
    '[Player] Load Track Success',
    props<{ track: Track }>()
);

export const loadTrackFailure = createAction(
    '[Player] Load Track Failure',
    props<{ error: string }>()
);

export const playTrack = createAction(
    '[Player] Play Track',
    props<{ trackId: string }>()
);

export const pauseTrack = createAction('[Player] Pause Track');

export const setVolume = createAction(
    '[Player] Set Volume',
    props<{ volume: number }>()
);

export const updateTime = createAction(
    '[Player] Update Time',
    props<{ currentTime: number; duration: number }>()
);

export const seekTo = createAction(
    '[Player] Seek To',
    props<{ time: number }>()
);

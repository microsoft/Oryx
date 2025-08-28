import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PlayerState } from '../app.state';

export const selectPlayerState = createFeatureSelector<PlayerState>('player');

export const selectCurrentTrackId = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.currentTrackId
);

export const selectIsPlaying = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.isPlaying
);

export const selectIsLoading = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.isLoading
);

export const selectVolume = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.volume
);

export const selectCurrentTime = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.currentTime
);

export const selectDuration = createSelector(
    selectPlayerState,
    (state: PlayerState) => state.duration
);

export const selectProgress = createSelector(
    selectCurrentTime,
    selectDuration,
    (currentTime: number, duration: number) =>
        duration > 0 ? (currentTime / duration) * 100 : 0
);

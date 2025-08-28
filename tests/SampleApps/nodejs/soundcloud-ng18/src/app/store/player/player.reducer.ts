import { createReducer, on } from '@ngrx/store';
import { PlayerState } from '../app.state';
import * as PlayerActions from './player.actions';

export const initialState: PlayerState = {
    currentTrackId: null,
    isPlaying: false,
    isLoading: false,
    volume: 0.8,
    currentTime: 0,
    duration: 0
};

export const playerReducer = createReducer(
    initialState,
    on(PlayerActions.loadTrack, (state, { trackId }) => ({
        ...state,
        currentTrackId: trackId,
        isLoading: true
    })),
    on(PlayerActions.loadTrackSuccess, (state) => ({
        ...state,
        isLoading: false
    })),
    on(PlayerActions.loadTrackFailure, (state) => ({
        ...state,
        isLoading: false,
        currentTrackId: null
    })),
    on(PlayerActions.playTrack, (state, { trackId }) => ({
        ...state,
        currentTrackId: trackId,
        isPlaying: true
    })),
    on(PlayerActions.pauseTrack, (state) => ({
        ...state,
        isPlaying: false
    })),
    on(PlayerActions.setVolume, (state, { volume }) => ({
        ...state,
        volume
    })),
    on(PlayerActions.updateTime, (state, { currentTime, duration }) => ({
        ...state,
        currentTime,
        duration
    })),
    on(PlayerActions.seekTo, (state, { time }) => ({
        ...state,
        currentTime: time
    }))
);

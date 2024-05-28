import 'rxjs/add/operator/combineLatest';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/let';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/withLatestFrom';

import { IAppState } from 'app';
import { Selector } from 'app/core';
import { getTracklistCursor, getTracklists, getTracks, ITrack, ITracklist, ITracklistCursor } from 'app/tracklists';
import { IPlayerState } from './player-state';
import { ITimesState } from './times-state';


export function getPlayer(): Selector<IAppState,IPlayerState> {
  return state$ => state$
    .map(state => state.player)
    .distinctUntilChanged();
}

export function getPlayerTrack(): Selector<IAppState,ITrack> {
  return state$ => state$
    .let(getPlayerTrackId())
    .distinctUntilChanged()
    .withLatestFrom(state$.let(getTracks()),
      (trackId, tracks) => tracks.get(trackId))
    .filter(track => !!track)
    .distinctUntilChanged();
}

export function getPlayerTrackId(): Selector<IAppState,number> {
  return state$ => state$
    .map(state => state.player.trackId);
}

export function getPlayerTracklist(): Selector<IAppState,ITracklist> {
  return state$ => state$
    .map(state => state.player.tracklistId)
    .combineLatest(state$.let(getTracklists()),
      (tracklistId, tracklists) => tracklists.get(tracklistId))
    .filter(tracklist => tracklist)
    .distinctUntilChanged();
}

export function getPlayerTracklistCursor(distinct: boolean = true): Selector<IAppState,ITracklistCursor> {
  return state$ => {
    let source$ = state$.let(getPlayerTrackId());
    if (distinct) source$ = source$.distinctUntilChanged();
    return source$.combineLatest(state$.let(getPlayerTracklist()), getTracklistCursor);
  };
}

export function getTimes(): Selector<IAppState,ITimesState> {
  return state$ => state$
    .map(state => state.times)
    .distinctUntilChanged();
}

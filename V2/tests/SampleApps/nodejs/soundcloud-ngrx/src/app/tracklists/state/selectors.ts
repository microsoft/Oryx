import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/let';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/withLatestFrom';

import { List } from 'immutable';
import { IAppState } from 'app';
import { TRACKS_PER_PAGE } from 'app/app-config';
import { Selector } from 'app/core';
import { ITrack, ITracklist } from '../models';
import { TracklistsState } from './tracklists-reducer';
import { TracksState } from './tracks-reducer';


export function getTracklists(): Selector<IAppState,TracklistsState> {
  return state$ => state$
    .map(state => state.tracklists)
    .distinctUntilChanged();
}

export function getTracks(): Selector<IAppState,TracksState> {
  return state$ => state$
    .map(state => state.tracks)
    .distinctUntilChanged();
}

export function getCurrentTracklist(): Selector<IAppState,ITracklist> {
  return state$ => state$
    .let(getTracklists())
    .map(tracklists => tracklists.get(tracklists.get('currentTracklistId')))
    .filter(tracklist => tracklist)
    .distinctUntilChanged();
}

export function getTracksForCurrentTracklist(): Selector<IAppState,List<ITrack>> {
  return state$ => state$
    .let(getCurrentTracklist())
    .distinctUntilChanged((previous, next) => {
      return previous.currentPage === next.currentPage &&
             previous.trackIds === next.trackIds;
    })
    .withLatestFrom(state$.let(getTracks()), (tracklist, tracks) => {
      return tracklist.trackIds
        .slice(0, tracklist.currentPage * TRACKS_PER_PAGE)
        .map(id => tracks.get(id)) as List<ITrack>;
    });
}

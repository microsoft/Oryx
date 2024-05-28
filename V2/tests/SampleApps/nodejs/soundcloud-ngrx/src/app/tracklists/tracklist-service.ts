import 'rxjs/add/operator/let';

import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { List } from 'immutable';
import { Observable } from 'rxjs/Observable';
import { IAppState } from 'app';
import { ITrack, ITracklist } from './models';
import { getCurrentTracklist, getTracksForCurrentTracklist } from './state/selectors';
import { TracklistActions } from './tracklist-actions';


@Injectable()
export class TracklistService {
  tracklist$: Observable<ITracklist>;
  tracks$: Observable<List<ITrack>>;

  constructor(private actions: TracklistActions, private store$: Store<IAppState>) {
    this.tracklist$ = store$.let(getCurrentTracklist());
    this.tracks$ = store$.let(getTracksForCurrentTracklist());
  }

  loadFeaturedTracks(): void {
    this.store$.dispatch(
      this.actions.loadFeaturedTracks()
    );
  }

  loadNextTracks(): void {
    this.store$.dispatch(
      this.actions.loadNextTracks()
    );
  }
}

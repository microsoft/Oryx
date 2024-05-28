import 'rxjs/add/observable/of';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/let';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switchMap';

import { Injectable } from '@angular/core';
import { Actions, Effect } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs/Observable';
import { IAppState } from 'app';
import { ApiService } from 'app/core';
import { getCurrentTracklist } from './state/selectors';
import { TracklistActions } from './tracklist-actions';


@Injectable()
export class TracklistEffects {

  @Effect()
  loadNextTracks$ = this.actions$
    .ofType(TracklistActions.LOAD_NEXT_TRACKS)
    .withLatestFrom(this.store$.let(getCurrentTracklist()), (action, tracklist) => tracklist)
    .filter(tracklist => tracklist.isPending)
    .switchMap(tracklist => this.api.fetch(tracklist.nextUrl)
      .map(data => this.tracklistActions.fetchTracksFulfilled(data, tracklist.id))
      .catch(error => Observable.of(this.tracklistActions.fetchTracksFailed(error)))
    );


  constructor(
    private actions$: Actions,
    private api: ApiService,
    private store$: Store<IAppState>,
    private tracklistActions: TracklistActions
  ) {}
}

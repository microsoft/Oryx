import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, exhaustMap, catchError } from 'rxjs/operators';
import { SoundCloudService } from '../../services/soundcloud.service';
import * as SearchActions from './search.actions';

@Injectable()
export class SearchEffects {
    private readonly actions$ = inject(Actions);
    private readonly soundCloudService = inject(SoundCloudService);

    searchTracks$ = createEffect(() =>
        this.actions$.pipe(
            ofType(SearchActions.searchTracks),
            exhaustMap(action =>
                this.soundCloudService.searchTracks(action.query).pipe(
                    map(response => SearchActions.searchTracksSuccess({
                        tracks: response.tracks,
                        hasNextPage: response.hasNextPage,
                        nextUrl: response.nextUrl
                    })),
                    catchError(error => of(SearchActions.searchTracksFailure({ error: error.message })))
                )
            )
        )
    );

    loadMoreTracks$ = createEffect(() =>
        this.actions$.pipe(
            ofType(SearchActions.loadMoreTracks),
            exhaustMap(() =>
                // Note: In a real implementation, you'd get the nextUrl from the store
                this.soundCloudService.loadMoreTracks('').pipe(
                    map(response => SearchActions.loadMoreTracksSuccess({
                        tracks: response.tracks,
                        hasNextPage: response.hasNextPage,
                        nextUrl: response.nextUrl
                    })),
                    catchError(error => of(SearchActions.loadMoreTracksFailure({ error: error.message })))
                )
            )
        )
    );
}

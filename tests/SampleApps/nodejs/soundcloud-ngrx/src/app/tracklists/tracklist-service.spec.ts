import { TestBed } from '@angular/core/testing';
import { Store, StoreModule } from '@ngrx/store';
import { TRACKS_PER_PAGE } from 'app/app-config';
import { testUtils } from 'app/utils/test';
import { TracklistRecord } from './models';
import { initialState, tracklistsReducer } from './state/tracklists-reducer';
import { tracksReducer } from './state/tracks-reducer';
import { TracklistActions } from './tracklist-actions';
import { TracklistService } from './tracklist-service';


describe('tracklists', () => {
  describe('TracklistService', () => {
    let actions: TracklistActions;
    let service: TracklistService;
    let store: Store<any>;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        imports: [
          StoreModule.provideStore(
            {
              tracklists: tracklistsReducer,
              tracks: tracksReducer
            },
            {
              tracklists: initialState
                .set('tracklist/1', new TracklistRecord({id: 'tracklist/1'}))
                .set('tracklist/2', new TracklistRecord({id: 'tracklist/2'}))
            }
          )
        ],
        providers: [
          TracklistActions,
          TracklistService
        ]
      });

      actions = injector.get(TracklistActions);
      service = injector.get(TracklistService);
      store = injector.get(Store);
    });


    describe('tracklist$ observable', () => {
      it('should emit the current tracklist from store', () => {
        let count = 0;
        let tracks = testUtils.createTracks(2);
        let tracklist = null;

        service.tracklist$.subscribe(value => {
          count++;
          tracklist = value;
        });

        // mount tracklist, making it current tracklist
        store.dispatch(actions.mountTracklist('tracklist/1'));
        expect(count).toBe(1);
        expect(tracklist.id).toBe('tracklist/1');

        // load track into tracklist
        store.dispatch(actions.fetchTracksFulfilled({collection: [tracks[0]]}, 'tracklist/1'));
        expect(count).toBe(2);

        // loading track that already exists in tracklist: should not emit
        store.dispatch(actions.fetchTracksFulfilled({collection: [tracks[0]]}, 'tracklist/1'));
        expect(count).toBe(2);

        // dispatching unrelated action: should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(2);
      });
    });


    describe('tracks$ observable', () => {
      it('should emit the list of tracks for current tracklist', () => {
        let count = 0;
        let trackData = testUtils.createTracks(TRACKS_PER_PAGE * 2);
        let tracks = null;

        service.tracks$.subscribe(value => {
          count++;
          tracks = value;
        });

        // mount the tracklist, making it current tracklist
        store.dispatch(actions.mountTracklist('tracklist/1'));
        expect(count).toBe(1);
        expect(tracks.size).toBe(0);

        // load two pages of tracks into tracklist; should emit first page of tracks
        store.dispatch(actions.fetchTracksFulfilled({collection: trackData}, 'tracklist/1'));
        expect(count).toBe(2);
        expect(tracks.size).toBe(TRACKS_PER_PAGE);

        // go to page two; should emit first and second page of tracks
        store.dispatch(actions.loadNextTracks());
        expect(count).toBe(3);
        expect(tracks.size).toBe(TRACKS_PER_PAGE * 2);

        // dispatching unrelated action: should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(3);
      });
    });


    describe('loadFeaturedTracks()', () => {
      it('should call store.dispatch() with LOAD_FEATURED_TRACKS action', () => {
        spyOn(store, 'dispatch');
        service.loadFeaturedTracks();

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(actions.loadFeaturedTracks());
      });
    });


    describe('loadNextTracks()', () => {
      it('should call store.dispatch() with LOAD_NEXT_TRACKS action', () => {
        spyOn(store, 'dispatch');
        service.loadNextTracks();

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(actions.loadNextTracks());
      });
    });
  });
});

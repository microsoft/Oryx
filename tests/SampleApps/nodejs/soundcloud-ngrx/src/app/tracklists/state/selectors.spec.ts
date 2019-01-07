import { TestBed } from '@angular/core/testing';
import { Store, StoreModule } from '@ngrx/store';
import { TRACKS_PER_PAGE } from 'app/app-config';
import { testUtils } from 'app/utils/test';
import { TracklistRecord } from '../models';
import { TracklistActions } from '../tracklist-actions';
import { getCurrentTracklist, getTracklists, getTracks, getTracksForCurrentTracklist } from './selectors';
import { initialState, tracklistsReducer } from './tracklists-reducer';
import { tracksReducer } from './tracks-reducer';


describe('tracklists', () => {
  describe('selectors', () => {
    let actions: TracklistActions;
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
        ]
      });

      actions = new TracklistActions();
      store = injector.get(Store);
    });


    describe('getCurrentTracklist()', () => {
      it('should return observable that emits current tracklist', () => {
        let count = 0;
        let track = testUtils.createTrack();
        let tracklist = null;

        store
          .let(getCurrentTracklist())
          .subscribe(value => {
            count++;
            tracklist = value;
          });

        // mount tracklist, making it current tracklist
        store.dispatch(actions.mountTracklist('tracklist/1'));
        expect(count).toBe(1);
        expect(tracklist.id).toBe('tracklist/1');

        // load track into tracklist
        store.dispatch(actions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        expect(count).toBe(2);

        // loading track that already exists in tracklist should not emit
        store.dispatch(actions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        expect(count).toBe(2);

        // dispatching unrelated action should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(2);
      });
    });


    describe('getTracks()', () => {
      it('should return observable that emits TracksState', () => {
        let count = 0;
        let tracks = null;

        store
          .let(getTracks())
          .subscribe(value => {
            count++;
            tracks = value;
          });

        // auto-emitting initial value
        expect(count).toBe(1);
        expect(tracks.size).toBe(0);

        // fetch tracks returning empty collection should not emit
        store.dispatch(actions.fetchTracksFulfilled({collection: []}, 'tracklist/1'));
        expect(count).toBe(1);

        // load track
        store.dispatch(actions.fetchTracksFulfilled({collection: [testUtils.createTrack()]}, 'tracklist/1'));
        expect(count).toBe(2);
        expect(tracks.size).toBe(1);

        // dispatching unrelated action should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(2);
      });
    });


    describe('getTracksForCurrentTracklist()', () => {
      it('should return observable that emits paginated tracks for current tracklist', () => {
        let count = 0;
        let trackData = testUtils.createTracks(TRACKS_PER_PAGE * 2);
        let tracks = null;

        store
          .let(getTracksForCurrentTracklist())
          .subscribe(value => {
            count++;
            tracks = value;
          });

        // mount tracklist, making it current tracklist
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

        // dispatching unrelated action should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(3);
      });
    });


    describe('getTracklists()', () => {
      it('should return observable that emits TracklistsState', () => {
        let count = 0;
        let track = testUtils.createTrack();
        let tracklists = null;

        store
          .let(getTracklists())
          .subscribe(value => {
            count++;
            tracklists = value;
          });

        // auto-emitting initial value
        expect(count).toBe(1);
        expect(tracklists.get('currentTracklistId')).toBe(null);

        // mount tracklist, making it current tracklist
        store.dispatch(actions.mountTracklist('tracklist/1'));
        expect(count).toBe(2);
        expect(tracklists.get('currentTracklistId')).toBe('tracklist/1');

        // load track
        store.dispatch(actions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        expect(count).toBe(3);

        // loading same track should not emit
        store.dispatch(actions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        expect(count).toBe(3);

        // dispatching unrelated action should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(3);
      });
    });
  });
});

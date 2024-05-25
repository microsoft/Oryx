import { is, List } from 'immutable';
import { TRACKS_PER_PAGE } from 'app/app-config';
import { SearchActions } from 'app/search/search-actions';
import { testUtils } from 'app/utils/test';
import { ITracklist, TracklistRecord } from '../models';
import { TracklistActions } from '../tracklist-actions';
import { tracklistReducer } from './tracklist-reducer';


describe('tracklists', () => {
  describe('tracklistReducer', () => {
    let actions: TracklistActions;
    let expectedTracklist: any;
    let initialTracklist: any;
    let tracklistId: string;
    let tracksPerPage: number;


    beforeEach(() => {
      actions = new TracklistActions();
      expectedTracklist = new TracklistRecord({id: tracklistId, tracksPerPage});
      initialTracklist = new TracklistRecord({id: tracklistId, tracksPerPage});
      tracklistId = 'tracklist/1';
      tracksPerPage = 3;
    });


    describe('default case', () => {
      it('should return initial state', () => {
        let tracklist = tracklistReducer(undefined, {type: 'UNDEFINED'});
        expect(is(tracklist, new TracklistRecord())).toBe(true);
      });
    });


    describe('FETCH_TRACKS_FULFILLED action', () => {
      it('should set tracklist.isNew to false', () => {
        initialTracklist = initialTracklist.set('isNew', true);
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled({collection: []}, tracklistId));
        expect(tracklist.isNew).toBe(false);
      });

      it('should set tracklist.isPending to false', () => {
        initialTracklist = initialTracklist.set('isPending', true);
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled({collection: []}, tracklistId));
        expect(tracklist.isPending).toBe(false);
      });

      it('should update tracklist.trackIds with unique ids', () => {
        let data1 = {collection: [{id: 1}, {id: 2}]};
        let data2 = {collection: [{id: 2}, {id: 3}]};

        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data1, tracklistId));
        tracklist = tracklistReducer(tracklist, actions.fetchTracksFulfilled(data2, tracklistId));

        expect(tracklist.trackIds.toJS()).toEqual([1, 2, 3]);
      });

      it('should NOT update tracklist.trackIds if there are no unique ids', () => {
        let data1 = {collection: [{id: 1}, {id: 2}]};
        let data2 = {collection: [{id: 1}, {id: 2}]};

        let tracklist1 = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data1, tracklistId));
        let tracklist2 = tracklistReducer(tracklist1, actions.fetchTracksFulfilled(data2, tracklistId));

        expect(tracklist1.trackIds.toJS()).toEqual([1, 2]);
        expect(tracklist2.trackIds.toJS()).toEqual([1, 2]);
        expect(tracklist1.trackIds).toBe(tracklist2.trackIds);
      });

      it('should update tracklist when number of tracks received is zero', () => {
        let data = {collection: []};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 0,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 0,
          trackIds: List()
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when number of tracks received is less than tracksPerPage', () => {
        let trackCount = TRACKS_PER_PAGE - 1;
        let data = {collection: testUtils.createTracks(trackCount)};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 1,
          trackIds: List(testUtils.createIds(trackCount))
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when number of tracks received equals tracklist.tracksPerPage', () => {
        let trackCount = TRACKS_PER_PAGE;
        let data = {collection: testUtils.createTracks(trackCount)};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 1,
          trackIds: List(testUtils.createIds(trackCount))
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when number of tracks received is greater than tracklist.tracksPerPage', () => {
        let pageCount = 2;
        let trackCount = TRACKS_PER_PAGE * pageCount;
        let data = {collection: testUtils.createTracks(trackCount)};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: true,
          hasNextPageInStore: true,
          isNew: false,
          isPending: false,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when total of two payloads equals tracklist.tracksPerPage', () => {
        let trackCount = TRACKS_PER_PAGE;
        let tracks = testUtils.createTracks(trackCount);
        let data1 = {collection: [tracks.shift()]};
        let data2 = {collection: tracks};

        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data1, tracklistId));
        tracklist = tracklistReducer(tracklist, actions.fetchTracksFulfilled(data2, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 1,
          trackIds: List(testUtils.createIds(trackCount))
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when next page is NOT in store and next_href is provided', () => {
        let data = {collection: testUtils.createTracks(1), next_href: 'https://next/2'};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: true,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          nextUrl: data.next_href,
          pageCount: 1,
          trackIds: List([1])
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when next page is in store and next_href is NOT provided', () => {
        let pageCount = 2;
        let trackCount = TRACKS_PER_PAGE * pageCount;
        let data = {collection: testUtils.createTracks(trackCount)};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: true,
          hasNextPageInStore: true,
          isNew: false,
          isPending: false,
          nextUrl: null,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should update tracklist when next page is NOT in store and next_href is NOT provided', () => {
        let data = {collection: []};
        let tracklist = tracklistReducer(initialTracklist, actions.fetchTracksFulfilled(data, tracklistId));

        expectedTracklist = expectedTracklist.merge({
          currentPage: 0,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 0,
          trackIds: List()
        });

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });
    });


    describe('LOAD_NEXT_TRACKS action', () => {
      it('should update tracklist pagination props if tracklist.hasNextPageInStore is true', () => {
        let pageCount = 2;
        let trackCount = TRACKS_PER_PAGE * pageCount;

        initialTracklist = initialTracklist.merge({
          currentPage: 1,
          hasNextPage: true,
          hasNextPageInStore: true,
          isNew: false,
          isPending: false,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        }) as ITracklist;

        expectedTracklist = expectedTracklist.merge({
          currentPage: 2,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        });

        let tracklist = tracklistReducer(initialTracklist, actions.loadNextTracks());

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });

      it('should set isPending to true if tracklist.hasNextPageInStore is false', () => {
        let initialTracklist = new TracklistRecord({
          hasNextPageInStore: false,
          isPending: false
        }) as ITracklist;

        let tracklist = tracklistReducer(initialTracklist, actions.loadNextTracks());

        expect(tracklist.isPending).toBe(true);
      });
    });


    describe('LOAD_SEARCH_RESULTS action', () => {
      let query: string;
      let searchActions: SearchActions;
      let tracklistId: string;

      beforeEach(() => {
        query = 'query';
        searchActions = new SearchActions();
        tracklistId = 'search/query';
      });

      it('should set tracklist id if tracklist is new', () => {
        let initialTracklist = new TracklistRecord() as ITracklist;
        let tracklist = tracklistReducer(initialTracklist, searchActions.loadSearchResults(query));
        expect(tracklist.id).toBe(tracklistId);
      });

      it('should set tracklist.isPending to true if tracklist is new', () => {
        initialTracklist = initialTracklist.set('isPending', false);
        let tracklist = tracklistReducer(initialTracklist, searchActions.loadSearchResults(query));
        expect(tracklist.isPending).toBe(true);
      });

      it('should reset pagination if tracklist is NOT new', () => {
        let pageCount = 2;
        let trackCount = TRACKS_PER_PAGE * pageCount;

        initialTracklist = initialTracklist.merge({
          currentPage: 2,
          hasNextPage: false,
          hasNextPageInStore: false,
          isNew: false,
          isPending: false,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        }) as ITracklist;

        expectedTracklist = expectedTracklist.merge({
          currentPage: 1,
          hasNextPage: true,
          hasNextPageInStore: true,
          isNew: false,
          isPending: false,
          pageCount: 2,
          trackIds: List(testUtils.createIds(trackCount))
        });

        let tracklist = tracklistReducer(initialTracklist, searchActions.loadSearchResults(query));

        expect(is(tracklist, expectedTracklist)).toBe(true);
      });
    });
  });
});

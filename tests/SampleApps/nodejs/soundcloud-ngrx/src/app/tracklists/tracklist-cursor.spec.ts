import { List, Record } from 'immutable';
import { ITracklist, TracklistRecord } from './models';
import { getTracklistCursor, ITracklistCursor, TracklistCursorRecord } from './tracklist-cursor';


describe('tracklists', () => {
  describe('ITracklistCursor', () => {
    let tracklist;

    beforeEach(() => {
      tracklist = new TracklistRecord({
        id: 'tracklist/1',
        trackIds: List([1, 2, 3])
      }) as ITracklist;
    });


    describe('TracklistCursorRecord', () => {
      it('should be an instance of Immutable.Record', () => {
        let cursor = new TracklistCursorRecord();
        expect(cursor instanceof Record).toBe(true);
      });

      it('should contain default properties', () => {
        let cursor = new TracklistCursorRecord() as ITracklistCursor;
        expect(cursor.currentTrackId).toBe(null);
        expect(cursor.nextTrackId).toBe(null);
        expect(cursor.previousTrackId).toBe(null);
      });
    });


    describe('getTracklistCursor()', () => {
      it('should return cursor for provided track id and tracklist', () => {
        let cursor = getTracklistCursor(1, tracklist); // trackIds: [1] 2 3

        expect(cursor.toJS()).toEqual({
          currentTrackId: 1,
          nextTrackId: 2,
          previousTrackId: null
        });

        cursor = getTracklistCursor(2, tracklist); // trackIds: 1 [2] 3

        expect(cursor.toJS()).toEqual({
          currentTrackId: 2,
          nextTrackId: 3,
          previousTrackId: 1
        });

        cursor = getTracklistCursor(3, tracklist); // trackIds: 1 2 [3]

        expect(cursor.toJS()).toEqual({
          currentTrackId: 3,
          nextTrackId: null,
          previousTrackId: 2
        });
      });
    });
  });
});

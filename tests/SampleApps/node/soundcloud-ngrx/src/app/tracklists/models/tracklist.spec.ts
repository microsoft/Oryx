import { is, List, Record } from 'immutable';
import { TracklistRecord } from './tracklist';


describe('tracklists', () => {
  describe('TracklistRecord', () => {
    let tracklist;

    beforeEach(() => {
      tracklist = new TracklistRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(tracklist instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      expect(tracklist.currentPage).toBe(0);
      expect(tracklist.hasNextPage).toBe(null);
      expect(tracklist.hasNextPageInStore).toBe(null);
      expect(tracklist.id).toBe(null);
      expect(tracklist.isNew).toBe(true);
      expect(tracklist.isPending).toBe(false);
      expect(tracklist.nextUrl).toBe(null);
      expect(tracklist.pageCount).toBe(0);
      expect(is(tracklist.trackIds, List())).toBe(true);
    });
  });
});

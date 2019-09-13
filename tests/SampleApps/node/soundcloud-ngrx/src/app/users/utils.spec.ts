import { tracklistIdForUserLikes, tracklistIdForUserTracks } from './utils';


describe('users', () => {
  describe('utils', () => {
    describe('tracklistIdForUserLikes()', () => {
      const expectedTracklistId: string = 'users/123/likes';

      it('should return generated tracklist id using provided user id', () => {
        expect(tracklistIdForUserLikes(123)).toBe(expectedTracklistId);
      });
    });

    describe('tracklistIdForUserTracks()', () => {
      const expectedTracklistId: string = 'users/123/tracks';

      it('should return generated tracklist id using provided user id', () => {
        expect(tracklistIdForUserTracks(123)).toBe(expectedTracklistId);
      });
    });
  });
});

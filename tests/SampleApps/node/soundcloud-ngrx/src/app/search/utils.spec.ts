import { tracklistIdForSearch } from './utils';


describe('search', () => {
  describe('utils', () => {
    describe('tracklistIdForSearch()', () => {
      const expectedTracklistId: string = 'search/foo bar baz';

      it('should return generated tracklist id using provided query', () => {
        expect(tracklistIdForSearch('foo bar baz')).toBe(expectedTracklistId);
      });

      it('should ensure provided query is trimmed', () => {
        expect(tracklistIdForSearch('  foo bar baz  ')).toBe(expectedTracklistId);
      });

      it('should ensure generated tracklistId is lower-cased', () => {
        expect(tracklistIdForSearch('Foo Bar Baz')).toBe(expectedTracklistId);
      });

      it('should ensure multiple contiguous space chars are replaced with a single space', () => {
        expect(tracklistIdForSearch('foo   bar   baz')).toBe(expectedTracklistId);
      });
    });
  });
});

import { Record } from 'immutable';
import { SearchStateRecord } from './search-state';


describe('search', () => {
  describe('ISearchState', () => {
    let search;

    beforeEach(() => {
      search = new SearchStateRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(search instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      expect(search.query).toBe(null);
    });
  });
});

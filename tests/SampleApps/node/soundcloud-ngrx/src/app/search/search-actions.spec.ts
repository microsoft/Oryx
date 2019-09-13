import { SearchActions } from './search-actions';
import { tracklistIdForSearch } from './utils';


describe('search', () => {
  describe('SearchActions', () => {
    let actions: SearchActions;

    beforeEach(() => {
      actions = new SearchActions();
    });

    describe('loadSearchResults()', () => {
      it('should create an action', () => {
        let query = 'test';
        let action = actions.loadSearchResults(query);
        let tracklistId = tracklistIdForSearch(query);

        expect(action).toEqual({
          type: SearchActions.LOAD_SEARCH_RESULTS,
          payload: {
            query,
            tracklistId
          }
        });
      });
    });
  });
});

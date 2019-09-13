import { is } from 'immutable';
import { SearchActions } from '../search-actions';
import { searchReducer } from './search-reducer';
import { SearchStateRecord } from './search-state';


describe('search', () => {
  describe('searchReducer', () => {
    let actions: SearchActions;
    let query: string;

    beforeEach(() => {
      actions = new SearchActions();
      query = 'test';
    });


    describe('default case', () => {
      it('should return initial state', () => {
        let search = searchReducer(undefined, {type: 'UNDEFINED'});
        expect(is(search, SearchStateRecord())).toBe(true);
      });
    });


    describe('LOAD_SEARCH_RESULTS action', () => {
      it('should update ISearchState.query with payload.query', () => {
        let action = actions.loadSearchResults(query);
        let search = searchReducer(undefined, action);
        expect(search.query).toBe(query);
      });
    });
  });
});

import { TestBed } from '@angular/core/testing';
import { Store, StoreModule } from '@ngrx/store';
import { SearchActions } from '../search-actions';
import { searchReducer } from './search-reducer';
import { getSearchQuery } from './selectors';


describe('search', () => {
  describe('selectors', () => {
    let actions: SearchActions;
    let store: Store<any>;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        imports: [
          StoreModule.provideStore({search: searchReducer})
        ]
      });

      actions = new SearchActions();
      store = injector.get(Store);
    });


    describe('getSearchQuery()', () => {
      it('should return observable that emits current search query on change', () => {
        let count = 0;
        let query = null;

        store
          .let(getSearchQuery())
          .subscribe(value => {
            count++;
            query = value;
          });

        // auto-emitting initial value
        expect(count).toBe(1);
        expect(query).toBe(null);

        // query 1
        store.dispatch(actions.loadSearchResults('query 1'));
        expect(count).toBe(2);
        expect(query).toBe('query 1');

        // same query: should not emit
        store.dispatch(actions.loadSearchResults('query 1'));
        expect(count).toBe(2);

        // query 2
        store.dispatch(actions.loadSearchResults('query 2'));
        expect(count).toBe(3);
        expect(query).toBe('query 2');

        // dispatching unrelated action: should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(3);
      });
    });
  });
});

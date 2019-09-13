import { TestBed } from '@angular/core/testing';
import { Store, StoreModule } from '@ngrx/store';
import { searchReducer } from './state/search-reducer';
import { SearchActions } from './search-actions';
import { SearchService } from './search-service';


describe('search', () => {
  describe('SearchService', () => {
    let actions: SearchActions;
    let service: SearchService;
    let store: Store<any>;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        imports: [
          StoreModule.provideStore({search: searchReducer})
        ],
        providers: [
          SearchActions,
          SearchService
        ]
      });

      actions = injector.get(SearchActions);
      service = injector.get(SearchService);
      store = injector.get(Store);
    });


    describe('query$ observable', () => {
      it('should stream the current query from store', () => {
        let count = 0;
        let query = null;

        service.query$.subscribe(value => {
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


    describe('actions', () => {
      describe('loadSearchResults()', () => {
        it('should call store.dispatch() with LOAD_SEARCH_RESULTS action', () => {
          let query = 'test';

          spyOn(store, 'dispatch');
          service.loadSearchResults(query);

          expect(store.dispatch).toHaveBeenCalledTimes(1);
          expect(store.dispatch).toHaveBeenCalledWith(actions.loadSearchResults(query));
        });

        it('should NOT dispatch action if query param is empty or invalid', () => {
          spyOn(store, 'dispatch');

          service.loadSearchResults(undefined);
          expect(store.dispatch).not.toHaveBeenCalled();

          service.loadSearchResults(null);
          expect(store.dispatch).not.toHaveBeenCalled();

          service.loadSearchResults('');
          expect(store.dispatch).not.toHaveBeenCalled();
        });
      });
    });
  });
});

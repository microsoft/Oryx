import { Action } from '@ngrx/store';
import { tracklistIdForSearch } from './utils';


export class SearchActions {
  static LOAD_SEARCH_RESULTS = 'LOAD_SEARCH_RESULTS';

  loadSearchResults(query: string): Action {
    return {
      type: SearchActions.LOAD_SEARCH_RESULTS,
      payload: {
        query,
        tracklistId: tracklistIdForSearch(query)
      }
    };
  }
}

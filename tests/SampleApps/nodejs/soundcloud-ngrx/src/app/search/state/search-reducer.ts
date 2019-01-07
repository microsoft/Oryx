import { Action } from '@ngrx/store';
import { SearchActions } from '../search-actions';
import { ISearchState, SearchStateRecord } from './search-state';


const initialState: ISearchState = new SearchStateRecord() as ISearchState;


export function searchReducer(state: ISearchState = initialState, action: Action): ISearchState {
  switch (action.type) {
    case SearchActions.LOAD_SEARCH_RESULTS:
      return state.set('query', action.payload.query) as ISearchState;

    default:
      return state;
  }
}

import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { IAppState } from 'app';
import { Selector } from 'app/core';


export function getSearchQuery(): Selector<IAppState,string> {
  return state$ => state$
    .map(state => state.search.query)
    .distinctUntilChanged();
}

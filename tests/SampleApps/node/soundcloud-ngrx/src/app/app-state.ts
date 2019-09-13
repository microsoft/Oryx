import { ModuleWithProviders } from '@angular/core';
import { StoreModule } from '@ngrx/store';

import { IPlayerState, ITimesState, playerReducer, timesReducer } from './player';
import { ISearchState, searchReducer } from './search';
import { tracklistsReducer, TracklistsState, tracksReducer, TracksState } from './tracklists';
import { usersReducer, UsersState } from './users';


export interface IAppState {
  player: IPlayerState;
  search: ISearchState;
  times: ITimesState;
  tracklists: TracklistsState;
  tracks: TracksState;
  users: UsersState;
}


export const AppStateModule: ModuleWithProviders = StoreModule.provideStore({
  player: playerReducer,
  search: searchReducer,
  times: timesReducer,
  tracklists: tracklistsReducer,
  tracks: tracksReducer,
  users: usersReducer
});

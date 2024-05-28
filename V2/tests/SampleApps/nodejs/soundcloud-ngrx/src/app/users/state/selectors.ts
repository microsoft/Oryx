import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/let';
import 'rxjs/add/operator/map';

import { IAppState } from 'app';
import { Selector } from 'app/core';
import { IUser } from '../models';
import { UsersState } from './users-reducer';


export function getCurrentUser(): Selector<IAppState,IUser> {
  return state$ => state$
    .let(getUsers())
    .map(users => users.get(users.get('currentUserId')))
    .distinctUntilChanged();
}

export function getUsers(): Selector<IAppState,UsersState> {
  return state$ => state$
    .map(state => state.users)
    .distinctUntilChanged();
}

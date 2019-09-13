import 'rxjs/add/operator/let';

import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs/Observable';
import { IAppState } from 'app';
import { IUser } from './models';
import { getCurrentUser } from './state/selectors';
import { UserActions } from './user-actions';


@Injectable()
export class UserService {
  currentUser$: Observable<IUser>;

  constructor(private actions: UserActions, private store$: Store<IAppState>) {
    this.currentUser$ = store$.let(getCurrentUser());
  }

  loadResource(userId: number|string, resource: string): void {
    switch (resource) {
      case 'likes':
        this.loadUserLikes(userId);
        break;

      case 'tracks':
        this.loadUserTracks(userId);
        break;
    }
  }

  loadUser(userId: number|string): void {
    this.store$.dispatch(
      this.actions.loadUser(userId)
    );
  }

  loadUserLikes(userId: number|string): void {
    this.store$.dispatch(
      this.actions.loadUserLikes(userId)
    );
  }

  loadUserTracks(userId: number|string): void {
    this.store$.dispatch(
      this.actions.loadUserTracks(userId)
    );
  }
}

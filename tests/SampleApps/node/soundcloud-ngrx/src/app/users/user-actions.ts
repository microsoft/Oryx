import { Action } from '@ngrx/store';
import { IUserData } from './models';
import { tracklistIdForUserLikes, tracklistIdForUserTracks } from './utils';


export class UserActions {
  static FETCH_USER_FAILED = 'FETCH_USER_FAILED';
  static FETCH_USER_FULFILLED = 'FETCH_USER_FULFILLED';

  static LOAD_USER = 'LOAD_USER';
  static LOAD_USER_LIKES = 'LOAD_USER_LIKES';
  static LOAD_USER_TRACKS = 'LOAD_USER_TRACKS';


  fetchUserFailed(error: any): Action {
    return {
      type: UserActions.FETCH_USER_FAILED,
      payload: error
    };
  }

  fetchUserFulfilled(user: IUserData): Action {
    return {
      type: UserActions.FETCH_USER_FULFILLED,
      payload: {
        user
      }
    };
  }

  loadUser(userId: any): Action {
    return {
      type: UserActions.LOAD_USER,
      payload: {
        userId: parseInt(userId, 10)
      }
    };
  }

  loadUserLikes(userId: any): Action {
    return {
      type: UserActions.LOAD_USER_LIKES,
      payload: {
        tracklistId: tracklistIdForUserLikes(userId),
        userId: parseInt(userId, 10)
      }
    };
  }

  loadUserTracks(userId: any): Action {
    return {
      type: UserActions.LOAD_USER_TRACKS,
      payload: {
        tracklistId: tracklistIdForUserTracks(userId),
        userId: parseInt(userId, 10)
      }
    };
  }
}

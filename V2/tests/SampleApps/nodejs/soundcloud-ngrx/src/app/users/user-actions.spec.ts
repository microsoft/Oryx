import { testUtils } from 'app/utils/test';
import { UserActions } from './user-actions';
import { tracklistIdForUserLikes, tracklistIdForUserTracks } from './utils';


describe('users', () => {
  describe('UserActions', () => {
    let actions: UserActions;
    let userId: number;

    beforeEach(() => {
      actions = new UserActions();
      userId = 123;
    });


    describe('fetchUserFailed()', () => {
      it('should create an action', () => {
        let error = {};
        let action = actions.fetchUserFailed(error);

        expect(action).toEqual({
          type: UserActions.FETCH_USER_FAILED,
          payload: error
        });
      });
    });


    describe('fetchUserFulfilled()', () => {
      it('should create an action', () => {
        let user = testUtils.createUser(userId);
        let action = actions.fetchUserFulfilled(user);

        expect(action).toEqual({
          type: UserActions.FETCH_USER_FULFILLED,
          payload: {
            user
          }
        });
      });
    });


    describe('loadUser()', () => {
      let expectedAction;

      beforeEach(() => {
        expectedAction = {
          type: UserActions.LOAD_USER,
          payload: {
            userId
          }
        };
      });

      it('should create an action when user id is a integer', () => {
        let action = actions.loadUser(userId);
        expect(action).toEqual(expectedAction);
      });

      it('should create an action when user id is a string', () => {
        let action = actions.loadUser(`${userId}`);
        expect(action).toEqual(expectedAction);
      });
    });


    describe('loadUserLikes()', () => {
      let expectedAction;

      beforeEach(() => {
        expectedAction = {
          type: UserActions.LOAD_USER_LIKES,
          payload: {
            tracklistId: tracklistIdForUserLikes(userId),
            userId
          }
        };
      });

      it('should create an action when user id is a integer', () => {
        let action = actions.loadUserLikes(userId);
        expect(action).toEqual(expectedAction);
      });

      it('should create an action when user id is a string', () => {
        let action = actions.loadUserLikes(`${userId}`);
        expect(action).toEqual(expectedAction);
      });
    });


    describe('loadUserTracks()', () => {
      let expectedAction;

      beforeEach(() => {
        expectedAction = {
          type: UserActions.LOAD_USER_TRACKS,
          payload: {
            tracklistId: tracklistIdForUserTracks(userId),
            userId
          }
        };
      });

      it('should create an action when user id is a integer', () => {
        let action = actions.loadUserTracks(userId);
        expect(action).toEqual(expectedAction);
      });

      it('should create an action when user id is a string', () => {
        let action = actions.loadUserTracks(`${userId}`);
        expect(action).toEqual(expectedAction);
      });
    });
  });
});

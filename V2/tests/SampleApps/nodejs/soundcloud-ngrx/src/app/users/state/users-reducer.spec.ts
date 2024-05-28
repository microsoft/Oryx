import { TracklistActions } from 'app/tracklists';
import { testUtils } from 'app/utils/test';
import { UserActions } from '../user-actions';
import { usersReducer } from './users-reducer';


describe('users', () => {
  describe('usersReducer', () => {
    let tracklistActions: TracklistActions;
    let userActions: UserActions;

    beforeEach(() => {
      tracklistActions = new TracklistActions();
      userActions = new UserActions();
    });


    describe('FETCH_TRACKS_FULFILLED action', () => {
      it('should add track.user to store', () => {
        let track = testUtils.createTrack();
        let users = usersReducer(undefined, tracklistActions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        expect(users.has(track.user.id)).toBe(true);
      });

      it('should NOT replace user with same id', () => {
        let track = testUtils.createTrack();
        let userId = track.user.id;
        let responseData = {collection: [track]};

        let users = usersReducer(undefined, tracklistActions.fetchTracksFulfilled(responseData, 'tracklist/1'));
        let userA = users.get(userId);

        users = usersReducer(users, tracklistActions.fetchTracksFulfilled(responseData, 'tracklist/1'));
        let userB = users.get(userId);

        expect(userA).toBe(userB);
      });
    });


    describe('FETCH_USER_FULFILLED action', () => {
      it('should add user to state', () => {
        let user = testUtils.createUser();
        let users = usersReducer(undefined, userActions.fetchUserFulfilled(user));
        expect(users.has(user.id)).toBe(true);
      });

      it('should replace existing user if existing user is NOT profile', () => {
        let track = testUtils.createTrack();
        let userId = track.user.id;
        let userData = testUtils.createUser(userId);

        let users = usersReducer(undefined, tracklistActions.fetchTracksFulfilled({collection: [track]}, 'tracklist/1'));
        let userA = users.get(userId);

        users = usersReducer(users, userActions.fetchUserFulfilled(userData));
        let userB = users.get(userId);

        expect(userA.profile).toBe(false);
        expect(userB.profile).toBe(true);
      });

      it('should NOT replace existing user if existing user is profile', () => {
        let userData = testUtils.createUser();

        let users = usersReducer(undefined, userActions.fetchUserFulfilled(userData));
        let userA = users.get(userData.id);

        users = usersReducer(users, userActions.fetchUserFulfilled(userData));
        let userB = users.get(userData.id);

        expect(userA).toBe(userB);
      });
    });


    describe('LOAD_USER action', () => {
      it('should set UsersState.currentUserId with payload.userId', () => {
        let users = usersReducer(undefined, userActions.loadUser(123));
        expect(users.get('currentUserId')).toBe(123);
      });
    });
  });
});

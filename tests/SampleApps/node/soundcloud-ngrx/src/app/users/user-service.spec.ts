import { TestBed } from '@angular/core/testing';
import { Store, StoreModule } from '@ngrx/store';
import { testUtils } from 'app/utils/test';
import { createUser } from './models';
import { initialState, usersReducer } from './state/users-reducer';
import { UserActions } from './user-actions';
import { UserService } from './user-service';


describe('users', () => {
  describe('UserService', () => {
    let service: UserService;
    let store: Store<any>;
    let userActions: UserActions;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        imports: [
          StoreModule.provideStore(
            {
              users: usersReducer
            },
            {
              users: initialState
                .set(123, createUser(testUtils.createUser(123)))
                .set(456, createUser(testUtils.createUser(456)))
            }
          )
        ],
        providers: [
          UserActions,
          UserService
        ]
      });

      service = injector.get(UserService);
      store = injector.get(Store);
      userActions = injector.get(UserActions);
    });


    describe('currentUser$ observable', () => {
      it('should emit the current user from UsersState', () => {
        let count = 0;
        let user = null;

        service.currentUser$.subscribe(value => {
          count++;
          user = value;
        });

        // auto-emitting initial value
        expect(count).toBe(1);
        expect(user).not.toBeDefined();

        // load user
        store.dispatch(userActions.loadUser(123));
        expect(count).toBe(2);
        expect(user.id).toBe(123);

        // loading same user should not emit
        store.dispatch(userActions.loadUser(123));
        expect(count).toBe(2);

        // load different user
        store.dispatch(userActions.loadUser(456));
        expect(count).toBe(3);
        expect(user.id).toBe(456);

        // dispatching unrelated action should not emit
        store.dispatch({type: 'UNDEFINED'});
        expect(count).toBe(3);
      });
    });


    describe('loadResource()', () => {
      it('should call store.dispatch() with LOAD_USER_LIKES action if resource param is `likes`', () => {
        spyOn(store, 'dispatch');
        service.loadResource(1, 'likes');

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(userActions.loadUserLikes(1));
      });

      it('should call store.dispatch() with LOAD_USER_TRACKS action if resource param is `tracks`', () => {
        spyOn(store, 'dispatch');
        service.loadResource(1, 'tracks');

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(userActions.loadUserTracks(1));
      });
    });


    describe('loadUserLikes()', () => {
      it('should call store.dispatch() with LOAD_USER_LIKES action', () => {
        spyOn(store, 'dispatch');
        service.loadUserLikes(1);

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(userActions.loadUserLikes(1));
      });
    });


    describe('loadUserTracks()', () => {
      it('should call store.dispatch() with LOAD_USER_TRACKS action', () => {
        spyOn(store, 'dispatch');
        service.loadUserTracks(1);

        expect(store.dispatch).toHaveBeenCalledTimes(1);
        expect(store.dispatch).toHaveBeenCalledWith(userActions.loadUserTracks(1));
      });
    });
  });
});

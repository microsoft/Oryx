import { is, Record } from 'immutable';
import { createUser, IUser, UserRecord } from './user';


describe('users', () => {
  describe('IUser', () => {
    let user;

    beforeEach(() => {
      user = new UserRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(user instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      let user = new UserRecord() as IUser;

      expect(user.avatarUrl).toBe(null);
      expect(user.city).toBe(null);
      expect(user.country).toBe(null);
      expect(user.followersCount).toBe(0);
      expect(user.followingsCount).toBe(0);
      expect(user.fullName).toBe(null);
      expect(user.id).toBe(null);
      expect(user.likesCount).toBe(0);
      expect(user.playlistCount).toBe(0);
      expect(user.profile).toBe(false);
      expect(user.trackCount).toBe(0);
      expect(user.username).toBe(null);
    });


    describe('createUser() factory function', () => {
      it('should create UserRecord instance from provided user data', () => {
        let userData = {
          avatar_url: 'https://i1.sndcdn.com/avatars-000185787427-8n8dew-large.jpg',
          city: 'City Name',
          country: 'Country Name',
          followers_count: 21444,
          followings_count: 257,
          full_name: 'Full Name',
          id: 12396,
          playlist_count: 4,
          public_favorites_count: 64,
          track_count: 6,
          username: 'Username'
        };

        let expectedUser = new UserRecord({
          avatarUrl: userData.avatar_url,
          city: userData.city,
          country: userData.country,
          followersCount: 21444,
          followingsCount: userData.followings_count,
          fullName: userData.full_name,
          id: userData.id,
          likesCount: userData.public_favorites_count,
          playlistCount: userData.playlist_count,
          profile: true,
          trackCount: userData.track_count,
          username: userData.username
        });

        expect(is(createUser(userData, true), expectedUser)).toBe(true);
      });
    });
  });
});

import { Map, Record } from 'immutable';


export interface IUserData {
  avatar_url: string;
  city?: string;
  country?: string;
  followers_count?: number;
  followings_count?: number;
  full_name?: string;
  id: number;
  playlist_count?: number;
  public_favorites_count?: number;
  track_count?: number;
  username: string;
}

export interface IUser extends Map<string,any> {
  avatarUrl: string;
  city: string;
  country: string;
  followersCount: number;
  followingsCount: number;
  fullName: string;
  id: number;
  likesCount: number;
  playlistCount: number;
  profile: boolean;
  trackCount: number;
  username: string;
}

export const UserRecord = Record({
  avatarUrl: null,
  city: null,
  country: null,
  followersCount: 0,
  followingsCount: 0,
  fullName: null,
  id: null,
  likesCount: 0,
  playlistCount: 0,
  profile: false,
  trackCount: 0,
  username: null
});

export function createUser(data: IUserData, profile: boolean = false): IUser {
  let attrs = {
    avatarUrl: data.avatar_url,
    id: data.id,
    username: data.username
  };

  if (profile) {
    attrs = Object.assign(attrs, {
      city: data.city,
      country: data.country,
      followersCount: data.followers_count,
      followingsCount: data.followings_count,
      fullName: data.full_name,
      likesCount: data.public_favorites_count,
      playlistCount: data.playlist_count,
      profile: true,
      trackCount: data.track_count
    });
  }

  return new UserRecord(attrs) as IUser;
}

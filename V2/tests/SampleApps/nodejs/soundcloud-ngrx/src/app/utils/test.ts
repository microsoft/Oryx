import { ITrackData } from 'app/tracklists';
import { IUserData } from 'app/users';


export const testUtils = {
  createIds: (count, id = 1): number[] => {
    let ids = [];
    for (let i = 0; i < count; i++, id++) {
      ids.push(id);
    }
    return ids;
  },

  createTrack: (id: number = 1): ITrackData => {
    return {
      artwork_url: `https://i1.sndcdn.com/artworks-${id}-large.jpg`,
      duration: 240000, // 4 minutes
      id,
      likes_count: id,
      permalink_url: `https://soundcloud.com/user-${id}/track-name-${id}`,
      playback_count: id,
      stream_url: `https://api.soundcloud.com/tracks/${id}/stream`,
      streamable: true,
      title: `Title - ${id}`,
      user: {
        avatar_url: `https://i1.sndcdn.com/avatars-${id}-large.jpg`,
        id: 100 + id,
        username: `User-${id}`,
        permalink_url: `https://soundcloud.com/user-${id}`
      },
      user_favorite: false,
      waveform_url: `https://w1.sndcdn.com/${id}_m.png`
    };
  },

  createTracks: (count: number, startId: number = 1): ITrackData[] => {
    let tracks = [];
    for (let i = startId; i <= count; i++) tracks.push(testUtils.createTrack(i));
    return tracks;
  },

  createUser: (id: number = 1): IUserData => {
    return {
      avatar_url: `https://i1.sndcdn.com/avatars-${id}-large.jpg`,
      city: 'City Name',
      country: 'Country Name',
      followers_count: 10000,
      followings_count: 1000,
      full_name: 'Full Name',
      id,
      playlist_count: 1,
      public_favorites_count: 10,
      track_count: 100,
      username: `user-${id}`
    };
  },

  getVolumes: (): {actual: number, input: number, display: string}[] => {
    return [
      {actual: 0,    input: 0,   display: '0.0'},
      {actual: 0.05, input: 5,   display: '0.5'},
      {actual: 0.1,  input: 10,  display: '1.0'},
      {actual: 0.15, input: 15,  display: '1.5'},
      {actual: 0.2,  input: 20,  display: '2.0'},
      {actual: 0.25, input: 25,  display: '2.5'},
      {actual: 0.3,  input: 30,  display: '3.0'},
      {actual: 0.35, input: 35,  display: '3.5'},
      {actual: 0.4,  input: 40,  display: '4.0'},
      {actual: 0.45, input: 45,  display: '4.5'},
      {actual: 0.5,  input: 50,  display: '5.0'},
      {actual: 0.55, input: 55,  display: '5.5'},
      {actual: 0.6,  input: 60,  display: '6.0'},
      {actual: 0.65, input: 65,  display: '6.5'},
      {actual: 0.7,  input: 70,  display: '7.0'},
      {actual: 0.75, input: 75,  display: '7.5'},
      {actual: 0.8,  input: 80,  display: '8.0'},
      {actual: 0.85, input: 85,  display: '8.5'},
      {actual: 0.9,  input: 90,  display: '9.0'},
      {actual: 0.95, input: 95,  display: '9.5'},
      {actual: 1,    input: 100, display: '10'}
    ];
  }
};

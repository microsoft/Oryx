import { is, Record } from 'immutable';
import { CLIENT_ID_PARAM } from 'app/app-config';
import { createTrack, TrackRecord } from './track';


describe('tracklists', () => {
  describe('TrackRecord', () => {
    let track;

    beforeEach(() => {
      track = new TrackRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(track instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      expect(track.artworkUrl).toBe(null);
      expect(track.duration).toBe(null);
      expect(track.id).toBe(null);
      expect(track.liked).toBe(null);
      expect(track.likesCount).toBe(null);
      expect(track.permalinkUrl).toBe(null);
      expect(track.playbackCount).toBe(null);
      expect(track.streamable).toBe(null);
      expect(track.streamUrl).toBe(null);
      expect(track.title).toBe(null);
      expect(track.userId).toBe(null);
      expect(track.username).toBe(null);
      expect(track.userPermalinkUrl).toBe(null);
      expect(track.waveformUrl).toBe(null);
    });
  });


  describe('createTrack() factory function', () => {
    it('should create TrackRecord instance from provided track data', () => {
      let trackData = {
        artwork_url: 'https://i1.sndcdn.com/artworks-000031536428-b78hez-large.jpg',
        duration: 2340816,
        id: 21857834,
        likes_count: 1582,
        permalink_url: 'https://soundcloud.com/username/track-name',
        playback_count: 73808,
        stream_url: 'https://api.soundcloud.com/tracks/62179245/stream',
        streamable: true,
        title: 'RA.216 - Mount Kimbie',
        user: {
          avatar_url: 'https://i1.sndcdn.com/avatars-000153825204-75k7v2-large.jpg',
          id: 1570627,
          username: 'Mount Kimbie',
          permalink_url: 'https://soundcloud.com/username'
        },
        user_favorite: false,
        waveform_url: 'https://w1.sndcdn.com/mgveJa3vpfkf_m.png'
      };

      let expectedTrack = new TrackRecord({
        artworkUrl: 'https://i1.sndcdn.com/artworks-000031536428-b78hez-t500x500.jpg',
        duration: trackData.duration,
        id: trackData.id,
        liked: trackData.user_favorite,
        likesCount: trackData.likes_count,
        permalinkUrl: trackData.permalink_url,
        playbackCount: trackData.playback_count,
        streamable: trackData.streamable,
        streamUrl: `https://api.soundcloud.com/tracks/62179245/stream?${CLIENT_ID_PARAM}`,
        title: 'RA.216 – Mount Kimbie',
        userId: trackData.user.id,
        username: trackData.user.username,
        userPermalinkUrl: trackData.user.permalink_url,
        waveformUrl: 'https://wis.sndcdn.com/mgveJa3vpfkf_m.json'
      });

      expect(is(createTrack(trackData), expectedTrack)).toBe(true);
    });
  });
});

import { CLIENT_ID } from 'app/app-config';
import { ITrackData } from './models';
import { formatTrackTitle, streamUrl, trackImageUrl, waveformUrl } from './utils';


describe('tracklists', () => {
  describe('utils', () => {
    describe('formatTrackTitle()', () => {
      it('should replace minus symbol with n-dash', () => {
        expect(formatTrackTitle('-').charCodeAt(0)).toBe(8211);
      });

      it('should return empty string if title param is not provided', () => {
        expect(formatTrackTitle(null)).toBe('');
        expect(formatTrackTitle(undefined)).toBe('');
      });
    });


    describe('streamUrl()', () => {
      it('should append soundcloud client id to provided url', () => {
        const url = 'https://api.soundcloud.com/tracks/21857834/stream';
        const expectedUrl = `${url}?client_id=${CLIENT_ID}`;
        expect(streamUrl(url)).toBe(expectedUrl);
      });
    });


    describe('trackImageUrl()', () => {
      const artworkUrl = `https://i1.sndcdn.com/artworks-000108797670-w5dhwi-large.jpg`;
      const expectedArtworkUrl = `https://i1.sndcdn.com/artworks-000108797670-w5dhwi-t500x500.jpg`;

      const avatarUrl = `https://i1.sndcdn.com/avatars-000185787427-8n8dew-large.jpg`;
      const expectedAvatarUrl = `https://i1.sndcdn.com/avatars-000185787427-8n8dew-t500x500.jpg`;

      it('should transform artwork url to point to 500 pixel version', () => {
        const track = {artwork_url: artworkUrl} as ITrackData;
        expect(trackImageUrl(track)).toBe(expectedArtworkUrl);
      });

      it('should use user avatar url if artwork url is not available', () => {
        const track = {user: {avatar_url: avatarUrl}} as ITrackData;
        expect(trackImageUrl(track)).toBe(expectedAvatarUrl);
      });
    });


    describe('waveformUrl()', () => {
      const imageUrl = `https://w1.sndcdn.com/6or4OQw4h4MR_m.png`;
      const jsonUrl = `https://wis.sndcdn.com/6or4OQw4h4MR_m.json`;

      it('should return unmodified json url', () => {
        expect(waveformUrl(jsonUrl)).toBe(jsonUrl);
      });

      it('should replace image url with json url', () => {
        expect(waveformUrl(imageUrl)).toBe(jsonUrl);
      });

      it('should replace .png file extension with .json', () => {
        expect(waveformUrl(imageUrl)).not.toMatch('.png');
        expect(waveformUrl(imageUrl)).toMatch('.json');
      });
    });
  });
});

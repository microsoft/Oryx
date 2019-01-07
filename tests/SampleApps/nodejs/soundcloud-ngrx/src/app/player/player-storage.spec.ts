import { PLAYER_STORAGE_KEY } from 'app/app-config';
import { playerStorage } from './player-storage';


describe('player', () => {
  describe('playerStorage', () => {
    let data;

    beforeAll(() => {
      localStorage.removeItem(PLAYER_STORAGE_KEY);
    });

    beforeEach(() => {
      data = {volume: 50};
    });

    afterEach(() => {
      localStorage.removeItem(PLAYER_STORAGE_KEY);
    });


    it('should get deserialized data from localStorage', () => {
      localStorage.setItem(PLAYER_STORAGE_KEY, JSON.stringify(data));
      expect(playerStorage.data).toEqual(data);
    });

    it('should return an empty object if data is not in localStorage', () => {
      expect(playerStorage.data).toEqual({});
    });

    it('should set serialized data into localStorage', () => {
      playerStorage.data = data;
      expect(localStorage.getItem(PLAYER_STORAGE_KEY)).toEqual(
        JSON.stringify(data)
      );
    });

    it('should get volume from localStorage', () => {
      localStorage.setItem(PLAYER_STORAGE_KEY, JSON.stringify(data));
      expect(playerStorage.volume).toBe(data.volume);
    });

    it('should set volume into localStorage', () => {
      playerStorage.volume = 75;
      expect(JSON.parse(localStorage.getItem(PLAYER_STORAGE_KEY)).volume).toBe(75);
    });

    it('should remove key and corresponding value from localStorage', () => {
      localStorage.setItem(PLAYER_STORAGE_KEY, JSON.stringify(data));
      playerStorage.clear();
      expect(localStorage.getItem(PLAYER_STORAGE_KEY)).toBe(null);
    });
  });
});

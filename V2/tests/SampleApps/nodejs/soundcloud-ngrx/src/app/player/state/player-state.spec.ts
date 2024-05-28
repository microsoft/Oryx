import { Record } from 'immutable';
import { PlayerStateRecord } from './player-state';


describe('player', () => {
  describe('IPlayerState', () => {
    let player;

    beforeEach(() => {
      player = new PlayerStateRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(player instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      expect(player.isPlaying).toBe(false);
      expect(player.trackId).toBe(null);
      expect(player.tracklistId).toBe(null);
      expect(player.volume).toBe(null);
    });
  });
});

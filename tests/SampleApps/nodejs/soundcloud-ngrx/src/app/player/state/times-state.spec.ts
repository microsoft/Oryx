import { Record } from 'immutable';
import { TimesStateRecord } from './times-state';


describe('player', () => {
  describe('ITimesState', () => {
    let times;

    beforeEach(() => {
      times = new TimesStateRecord();
    });

    it('should be an instance of Immutable.Record', () => {
      expect(times instanceof Record).toBe(true);
    });

    it('should contain default properties', () => {
      expect(times.bufferedTime).toBe(0);
      expect(times.currentTime).toBe(0);
      expect(times.duration).toBe(0);
      expect(times.percentBuffered).toBe('0%');
      expect(times.percentCompleted).toBe('0%');
    });
  });
});

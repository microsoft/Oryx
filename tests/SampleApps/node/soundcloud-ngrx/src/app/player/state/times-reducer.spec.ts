import { is } from 'immutable';
import { PlayerActions } from '../player-actions';
import { timesReducer } from './times-reducer';
import { ITimesState, TimesStateRecord } from './times-state';


describe('player', () => {
  describe('timesReducer', () => {
    let actions: PlayerActions;
    let times;

    beforeEach(() => {
      actions = new PlayerActions();
      times = {
        bufferedTime: 200,
        currentTime: 100,
        duration: 400,
        percentBuffered: '50%',
        percentCompleted: '25%'
      };
    });


    describe('AUDIO_ENDED action', () => {
      it('should reset state.times to zero', () => {
        let timesState = new TimesStateRecord(times) as ITimesState;
        timesState = timesReducer(timesState, actions.audioEnded());

        expect(is(timesState, new TimesStateRecord())).toBe(true);
      });
    });


    describe('AUDIO_TIME_UPDATED action', () => {
      it('should update state.times', () => {
        let timesState = new TimesStateRecord() as ITimesState;
        timesState = timesReducer(timesState, actions.audioTimeUpdated(times));

        expect(timesState.bufferedTime).toBe(200);
        expect(timesState.currentTime).toBe(100);
        expect(timesState.duration).toBe(400);
        expect(timesState.percentBuffered).toBe('50%');
        expect(timesState.percentCompleted).toBe('25%');
      });
    });


    describe('PLAY_SELECTED_TRACK action', () => {
      it('should reset state.times to zero', () => {
        let timesState = new TimesStateRecord(times) as ITimesState;
        timesState = timesReducer(timesState, actions.playSelectedTrack(1));

        expect(is(timesState, new TimesStateRecord())).toBe(true);
      });
    });
  });
});

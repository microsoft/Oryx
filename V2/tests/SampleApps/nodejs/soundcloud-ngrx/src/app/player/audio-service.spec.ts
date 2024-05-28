import { TestBed } from '@angular/core/testing';
import { testUtils } from 'app/utils/test';
import { AudioService } from './audio-service';
import { AUDIO_SOURCE_PROVIDER, AudioSource } from './audio-source';
import { PlayerActions } from './player-actions';


describe('player', () => {
  describe('AudioService', () => {
    let actions: PlayerActions;
    let audio: AudioSource;
    let audioService: AudioService;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        providers: [
          AUDIO_SOURCE_PROVIDER,
          PlayerActions,
          {
            provide: AudioService,
            deps: [PlayerActions, AudioSource],
            useFactory: (actions: PlayerActions, audio: AudioSource) => {
              return new AudioService(actions, audio);
            }
          }
        ]
      });

      actions = injector.get(PlayerActions);
      audio = injector.get(AudioSource);
      audioService = injector.get(AudioService);
    });


    describe('audio events', () => {
      it('`ended` event should dispatch AUDIO_ENDED action', () => {
        audioService.events$.subscribe(action => {
          expect(action).toEqual(actions.audioEnded());
        });

        audio.dispatchEvent(new Event('ended'));
      });

      it('`pause` event should dispatch AUDIO_PAUSED action', () => {
        audioService.events$.subscribe(action => {
          expect(action).toEqual(actions.audioPaused());
        });

        audio.dispatchEvent(new Event('pause'));
      });

      it('`playing` event should dispatch AUDIO_PLAYING action', () => {
        audioService.events$.subscribe(action => {
          expect(action).toEqual(actions.audioPlaying());
        });

        audio.dispatchEvent(new Event('playing'));
      });

      it('`timeupdate` event should dispatch AUDIO_TIME_UPDATED action', () => {
        let times = {
          bufferedTime: 0,
          currentTime: 0,
          duration: NaN,
          percentBuffered: '0%',
          percentCompleted: '0%'
        };

        audioService.events$.subscribe(action => {
          expect(action).toEqual(actions.audioTimeUpdated(times));
        });

        audio.dispatchEvent(new Event('timeupdate'));
      });

      it('`volumechange` event should dispatch AUDIO_VOLUME_CHANGED action', () => {
        audioService.events$.subscribe(action => {
          expect(action).toEqual(actions.audioVolumeChanged(50));
        });

        audio.volume = 0.5;
        audio.dispatchEvent(new Event('volumechange'));
      });
    });


    describe('decreaseVolume()', () => {
      it('should decrement volume by PLAYER_VOLUME_INCREMENT', () => {
        let volumes = testUtils.getVolumes();
        volumes.reverse().shift();

        audio.volume = 1;

        volumes.forEach(volume => {
          audioService.decreaseVolume();
          expect(audio.volume).toBe(volume.actual);
        });
      });

      it('should NOT decrement volume below zero', () => {
        audio.volume = 0;
        expect(() => audioService.decreaseVolume()).not.toThrow();
      });
    });


    describe('increaseVolume()', () => {
      it('should increment volume by PLAYER_VOLUME_INCREMENT', () => {
        let volumes = testUtils.getVolumes();
        volumes.shift();

        audio.volume = 0;

        volumes.forEach(volume => {
          audioService.increaseVolume();
          expect(audio.volume).toBe(volume.actual);
        });
      });

      it('should NOT increment volume beyond 1', () => {
        audio.volume = 1;
        expect(() => audioService.increaseVolume()).not.toThrow();
      });
    });


    describe('pause()', () => {
      it('should call audio.pause()', () => {
        spyOn(audio, 'pause');
        audioService.pause();
        expect(audio.pause).toHaveBeenCalledTimes(1);
      });
    });


    describe('play()', () => {
      it('should call audio.play()', () => {
        spyOn(audio, 'play');
        audioService.play();
        expect(audio.play).toHaveBeenCalledTimes(1);
      });

      it('should set audio.src if url param is provided', () => {
        let streamUrl = 'https://stream/';
        audioService.play(streamUrl);
        expect(audio.src).toBe(streamUrl);
      });
    });


    describe('seek()', () => {
      it('should set audio.currentTime', () => {
        audioService.seek(100);
        expect(audio.currentTime).toBe(100);
      });
    });
  });
});

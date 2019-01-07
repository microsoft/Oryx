import { Component, Input, ViewChild } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { SharedModule } from 'app/shared';
import { testUtils } from 'app/utils/test';
import { createTrack } from '../models/track';
import { TrackCardComponent } from './track-card';


@Component({template: ''})
class TestComponent {
  @ViewChild(TrackCardComponent) trackCard: TrackCardComponent;
  compact: any;
  isPlaying: any;
  isSelected: any;
  times: any;
  track: any;
}

@Component({selector: 'waveform-timeline', template: ''})
class WaveformTimelineComponentStub {
  @Input() isActive: any;
  @Input() times: any;
  @Input() waveformUrl: any;
}


describe('tracklists', () => {
  describe('TrackCardComponent', () => {
    let times;
    let track;

    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          TestComponent,
          TrackCardComponent,
          WaveformTimelineComponentStub
        ],
        imports: [
          RouterTestingModule,
          SharedModule
        ]
      });

      times = new BehaviorSubject<any>({
        bufferedTime: 200,
        duration: 400,
        percentBuffered: '50%',
        percentCompleted: '25%'
      });

      track = createTrack(testUtils.createTrack(1));
    });


    function compileComponents(): Promise<any> {
      let template = `
        <track-card 
          [compact]="compact"
          [isPlaying]="isPlaying"
          [isSelected]="isSelected"
          [times]="times"
          [track]="track"></track-card>`;

      return TestBed
        .overrideComponent(TestComponent, {set: {template}})
        .compileComponents()
        .then(() => TestBed.createComponent(TestComponent))
        .then(fixture => {
          fixture.componentInstance.compact = false;
          fixture.componentInstance.isPlaying = false;
          fixture.componentInstance.isSelected = false;
          fixture.componentInstance.times = times;
          fixture.componentInstance.track = track;
          return fixture;
        });
    }


    it('should display track image', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('img');
        expect(el.getAttribute('src')).toBe(track.artworkUrl);
      });
    }));

    it('should display track username', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.track-card__username');
        expect(el.textContent).toBe(track.username);
      });
    }));

    it('should link track username to user tracks route', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.track-card__username');
        expect(el.getAttribute('href')).toBe(`/users/${track.userId}/tracks`);
      });
    }));

    it('should display track title', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.track-card__title');
        expect(el.textContent).toBe(track.title);
      });
    }));

    it('should display track duration', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.meta-duration');

        expect(el.textContent).toBe('04:00');
      });
    }));

    it('should display track playback count', async(() => {
      compileComponents().then(fixture => {
        fixture.componentInstance.track = track.set('playbackCount', 1000);
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.meta-playback-count');

        expect(el.textContent).toBe('1,000');
      });
    }));

    it('should display track likes count', async(() => {
      compileComponents().then(fixture => {
        fixture.componentInstance.track = track.set('likesCount', 1000);
        fixture.detectChanges();
        let el = fixture.nativeElement.querySelector('.meta-likes-count');

        expect(el.textContent).toBe('1,000');
      });
    }));
  });
});

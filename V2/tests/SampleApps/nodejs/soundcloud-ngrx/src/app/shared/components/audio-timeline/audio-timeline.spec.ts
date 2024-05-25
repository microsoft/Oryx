import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { TimesStateRecord } from 'app/player';
import { AudioTimelineComponent } from './audio-timeline';


@Component({template: ''})
class TestComponent {}


describe('shared', () => {
  describe('AudioTimelineComponent', () => {
    let times;

    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          AudioTimelineComponent,
          TestComponent
        ]
      });

      times = new TimesStateRecord({
        bufferedTime: 200,
        duration: 400,
        percentBuffered: '50%',
        percentCompleted: '25%'
      });
    });


    function compileComponents(template: string): Promise<any> {
      return TestBed
        .overrideComponent(TestComponent, {set: {template}})
        .compileComponents()
        .then(() => TestBed.createComponent(TestComponent));
    }


    it('should set widths of bars', async(() => {
      compileComponents('<audio-timeline [times]="times"></audio-timeline>')
        .then(fixture => {
          fixture.componentInstance.times = times;
          fixture.detectChanges();

          let compiled = fixture.nativeElement;

          expect(compiled.querySelector('.bar--buffered').style.width).toBe('50%');
          expect(compiled.querySelector('.bar--elapsed').style.width).toBe('25%');
        });
    }));

    it('should add css class to `buffered` bar if buffered amount is not zero', async(() => {
      compileComponents('<audio-timeline [times]="times"></audio-timeline>')
        .then(fixture => {
          fixture.componentInstance.times = times;
          fixture.detectChanges();

          let compiled = fixture.nativeElement;

          expect(compiled.querySelector('.bar--buffered').classList).toContain('bar--animated');

          fixture.componentInstance.times = times.set('bufferedTime', 0);
          fixture.detectChanges();

          expect(compiled.querySelector('.bar--buffered').classList).not.toContain('bar--animated');
        });
    }));

    it('should emit seek event when host element is clicked', async(() => {
      compileComponents('<audio-timeline (seek)="seek($event)" [times]="times" style="width: 100px;"></audio-timeline>')
        .then(fixture => {
          fixture.componentInstance.seek = jasmine.createSpy('seek');
          fixture.componentInstance.times = times;
          fixture.detectChanges();

          expect(fixture.componentInstance.seek).not.toHaveBeenCalled();

          fixture.nativeElement.querySelector('audio-timeline').click();

          expect(fixture.componentInstance.seek).toHaveBeenCalledTimes(1);
        });
    }));
  });
});

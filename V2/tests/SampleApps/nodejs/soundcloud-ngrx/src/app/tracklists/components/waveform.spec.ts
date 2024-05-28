import { Component, ViewChild } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs/Subject';
import { ApiService } from 'app/core';
import { WaveformComponent } from './waveform';
import { waveformData } from './waveform.data';


@Component({template: ''})
class TestComponent {
  @ViewChild(WaveformComponent) waveform: WaveformComponent;
  src = 'http://foo';
}


describe('tracklists', () => {
  describe('WaveformComponent', () => {
    let api;
    let fetchSubject;

    beforeEach(() => {
      fetchSubject = new Subject<any>();

      api = jasmine.createSpyObj('api', ['fetch']);
      api.fetch.and.callFake(() => fetchSubject);

      TestBed.configureTestingModule({
        declarations: [
          TestComponent,
          WaveformComponent
        ],
        providers: [
          {provide: ApiService, useValue: api}
        ]
      });
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(WaveformComponent));
    }


    it('should fetch waveform json data', async(() => {
      TestBed
        .overrideComponent(TestComponent, {set: {
          template: '<waveform [src]="src"></waveform>'
        }})
        .compileComponents()
        .then(() => TestBed.createComponent(TestComponent))
        .then(fixture => {
          fixture.detectChanges();

          expect(api.fetch).toHaveBeenCalledTimes(1);
          expect(api.fetch).toHaveBeenCalledWith('http://foo');
        });
    }));

    it('should render waveform json data to canvas', async(() => {
      compileComponents()
        .then(fixture => {
          fixture.detectChanges();

          spyOn(fixture.componentInstance, 'render');

          fetchSubject.next(waveformData);

          expect(fixture.componentInstance.render).toHaveBeenCalledTimes(1);
          expect(fixture.componentInstance.render).toHaveBeenCalledWith(waveformData);
        });
    }));

    it('should add canvas to DOM', async(() => {
      compileComponents()
        .then(fixture => {
          fixture.detectChanges();

          fetchSubject.next(waveformData);

          let canvas = fixture.nativeElement.querySelector('canvas') as HTMLCanvasElement;

          expect(canvas instanceof HTMLCanvasElement).toBe(true);
          expect(canvas.width).toBe(waveformData.width / 2);
          expect(canvas.height).toBe(waveformData.height / 2);
        });
    }));

    it('should emit `ready` event', async(() => {
      compileComponents()
        .then(fixture => {
          fixture.detectChanges();

          spyOn(fixture.componentInstance.ready, 'emit');

          fetchSubject.next(waveformData);

          expect(fixture.componentInstance.ready.emit).toHaveBeenCalledTimes(1);
        });
    }));
  });
});

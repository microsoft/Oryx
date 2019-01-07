import { Component, Input } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { SharedModule } from 'app/shared';
import { TracklistService } from 'app/tracklists/tracklist-service';
import { HomePageComponent } from './home-page';


@Component({selector: 'tracklist', template: ''})
class TracklistComponentStub {
  @Input() layout: string;
}


describe('home', () => {
  describe('HomePageComponent', () => {
    let tracklistService;

    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        declarations: [
          HomePageComponent,
          TracklistComponentStub
        ],
        imports: [
          SharedModule
        ],
        providers: [
          {provide: TracklistService, useValue: jasmine.createSpyObj('tracklist', ['loadFeaturedTracks'])}
        ]
      });

      tracklistService = injector.get(TracklistService);
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(HomePageComponent));
    }


    it('should initialize properties', async(() => {
      compileComponents().then(fixture => {
        expect(fixture.componentInstance.layout).toBe('compact');
        expect(fixture.componentInstance.section).toBe('Spotlight');
        expect(fixture.componentInstance.title).toBe('Featured Tracks');
      });
    }));

    it('should load featured tracks', async(() => {
      compileComponents().then(() => {
        expect(tracklistService.loadFeaturedTracks).toHaveBeenCalledTimes(1);
      });
    }));

    it('should display current section and title', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();

        let compiled = fixture.nativeElement;

        expect(compiled.querySelector('.content-header__section').textContent).toBe('Spotlight /');
        expect(compiled.querySelector('.content-header__title').textContent).toBe('Featured Tracks');
      });
    }));
  });
});

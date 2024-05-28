import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { SharedModule } from 'app/shared';
import { SearchService } from '../search-service';
import { SearchPageComponent } from './search-page';


@Component({selector: 'tracklist', template: ''})
class TracklistComponentStub {}


describe('search', () => {
  describe('SearchPageComponent', () => {
    let activatedRoute;
    let search;

    beforeEach(() => {
      let searchService = jasmine.createSpyObj('search', ['loadSearchResults']);
      searchService.query$ = new Subject<any>();

      let injector = TestBed.configureTestingModule({
        declarations: [
          SearchPageComponent,
          TracklistComponentStub
        ],
        imports: [
          SharedModule
        ],
        providers: [
          {provide: ActivatedRoute, useValue: {params: new Subject<any>()}},
          {provide: SearchService, useValue: searchService}
        ]
      });

      activatedRoute = injector.get(ActivatedRoute);
      search = injector.get(SearchService);
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(SearchPageComponent));
    }


    it('should initialize properties', async(() => {
      compileComponents().then(fixture => {
        expect(fixture.componentInstance.section).toBe('Search Results');
        expect(fixture.componentInstance.ngOnDestroy$ instanceof Subject).toBe(true);
      });
    }));

    it('should load search results using query params', async(() => {
      compileComponents().then(() => {
        activatedRoute.params.next({q: 'test'});
        expect(search.loadSearchResults).toHaveBeenCalledTimes(1);
        expect(search.loadSearchResults).toHaveBeenCalledWith('test');
      });
    }));

    it('should display current section and title', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();

        search.query$.next('Foo Bar');

        fixture.detectChanges();

        let compiled = fixture.nativeElement;

        expect(compiled.querySelector('.content-header__section').textContent).toBe('Search Results /');
        expect(compiled.querySelector('.content-header__title').textContent).toBe('Foo Bar');
      });
    }));
  });
});

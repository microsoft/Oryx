import { NgZone } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MediaQueryListStub } from './media-query-list-stub';
import { MEDIA_QUERY_RULES, MediaQueryService } from './media-query-service';


describe('media-query', () => {
  describe('MediaQueryService', () => {
    let mediaQueryLists: MediaQueryListStub[];
    let service: MediaQueryService;


    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        providers: [
          MediaQueryService,
          {provide: NgZone, useFactory: () => new NgZone({enableLongStackTrace: true})},
          {provide: MEDIA_QUERY_RULES, useValue: [
            {
              id: 'medium',
              minWidth: 740,
              maxWidth: 979
            },
            {
              id: 'large',
              minWidth: 980
            },
            {
              id: 'landscape',
              orientation: 'landscape'
            }
          ]}
        ]
      });

      mediaQueryLists = [];

      spyOn(window, 'matchMedia').and.callFake(media => {
        let mql = new MediaQueryListStub(media);
        mediaQueryLists.push(mql);
        return mql;
      });

      service = injector.get(MediaQueryService);
    });


    describe('matches$', () => {
      it('should immediately emit initial results on subscribe', () => {
        service.matches$.subscribe(results => {
          expect(results).toEqual({
            medium: false,
            large: false,
            landscape: false
          });
        });
      });

      it('should debounce emitted results', done => {
        let count = 0;
        let finalResults = null;

        service.matches$.subscribe(results => {
          count++;
          finalResults = results;
        });

        setTimeout(() => {
          mediaQueryLists[1].matches = true;
          mediaQueryLists.forEach(mql => mql.dispatch());
        }, 100);

        setTimeout(() => {
          expect(count).toBe(2); // 1 for initial results; 2 for manual dispatch
          expect(finalResults).toEqual({
            medium: false,
            large: true,
            landscape: false
          });
          done();
        }, 200);
      });
    });
  });
});

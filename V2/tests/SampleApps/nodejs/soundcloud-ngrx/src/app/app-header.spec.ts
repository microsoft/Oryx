import { Component, Input, ViewChild } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { SharedModule } from './shared';
import { AppHeaderComponent } from './app-header';


@Component({selector: 'search-bar', template: ''})
class SearchBarComponentStub {
  @Input() open: boolean;
}

@Component({template: '<app-header></app-header><router-outlet></router-outlet>'})
class TestComponent {
  @ViewChild(AppHeaderComponent) appHeader: AppHeaderComponent;
}

@Component({template: ''})
class TestPage {}


describe('app', () => {
  describe('AppHeaderComponent', () => {
    let router;

    beforeEach(() => {
      let injector = TestBed.configureTestingModule({
        declarations: [
          AppHeaderComponent,
          SearchBarComponentStub,
          TestComponent,
          TestPage
        ],
        imports: [
          RouterTestingModule.withRoutes([
            {path: 'foo', component: TestPage}
          ]),
          SharedModule
        ]
      });

      router = injector.get(Router);
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(AppHeaderComponent));
    }


    it('should have a title', async(() => {
      compileComponents().then(fixture => {
        let h1 = fixture.nativeElement.querySelector('h1');
        expect(h1.textContent).toBe('SoundCloud • Angular2 NgRx');
      });
    }));

    it('should initialize property `open` with boolean value `false`', async(() => {
      compileComponents().then(fixture => {
        expect(fixture.componentInstance.open).toBe(false);
      });
    }));

    it('should toggle property `open` on route change if value is `true`', async(() => {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(TestComponent))
        .then(fixture => {
          fixture.componentInstance.appHeader.open = true;
          fixture.detectChanges();

          expect(fixture.componentInstance.appHeader.open).toBe(true);

          router.navigate(['/foo']);
          fixture.detectChanges();

          expect(fixture.componentInstance.appHeader.open).toBe(false);
      });
    }));


    describe('toggleOpen()', () => {
      it('should toggle property `open`', async(() => {
        compileComponents().then(fixture => {
          expect(fixture.componentInstance.open).toBe(false);

          fixture.componentInstance.toggleOpen();
          fixture.detectChanges();

          expect(fixture.componentInstance.open).toBe(true);

          fixture.componentInstance.toggleOpen();
          fixture.detectChanges();

          expect(fixture.componentInstance.open).toBe(false);
        });
      }));
    });


    describe('search button', () => {
      it('should toggle property `open` on click', async(() => {
        compileComponents().then(fixture => {
          fixture.detectChanges();

          expect(fixture.componentInstance.open).toBe(false);

          let searchButton = fixture.nativeElement.querySelector('.btn--search-alt');
          searchButton.click();
          fixture.detectChanges();

          expect(fixture.componentInstance.open).toBe(true);

          searchButton.click();
          fixture.detectChanges();

          expect(fixture.componentInstance.open).toBe(false);
        });
      }));
    });
  });
});

import { Component, ViewChild } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { async, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { SearchBarComponent } from './search-bar';


@Component({template: ''})
class TestComponent {
  @ViewChild(SearchBarComponent) searchBar: SearchBarComponent;
  open: boolean;
}


describe('search', () => {
  describe('SearchBarComponent', () => {
    let injector;

    beforeEach(() => {
      injector = TestBed.configureTestingModule({
        declarations: [
          SearchBarComponent,
          TestComponent
        ],
        imports: [
          ReactiveFormsModule
        ],
        providers: [
          {provide: Router, useValue: jasmine.createSpyObj('router', ['navigate'])}
        ]
      });
    });


    describe('component inputs', () => {
      function compileComponents(template: string): Promise<any> {
        return TestBed
          .overrideComponent(TestComponent, {set: {template}})
          .compileComponents()
          .then(() => TestBed.createComponent(TestComponent));
      }

      it('should clear input element value when search bar is opened', async(() => {
        compileComponents('<search-bar [open]="open"></search-bar>')
          .then(fixture => {
            fixture.detectChanges();

            fixture.componentInstance.searchBar.searchInput.setValue('test');
            fixture.detectChanges();

            expect(fixture.componentInstance.searchBar.searchInput.value).toBe('test');

            fixture.componentInstance.open = true;
            fixture.detectChanges();

            expect(fixture.componentInstance.searchBar.searchInput.value).toBe('');
          });
      }));

      it('should focus input element when search bar is opened', async(() => {
        compileComponents('<search-bar [open]="open"></search-bar>')
          .then(fixture => {
            fixture.detectChanges();

            spyOn(fixture.componentInstance.searchBar.searchInputEl, 'focus');

            fixture.componentInstance.open = true;
            fixture.detectChanges();
            fixture.nativeElement.querySelector('.search-bar').dispatchEvent(new Event('transitionend'));

            expect(fixture.componentInstance.searchBar.searchInputEl.focus).toHaveBeenCalledTimes(1);
          });
      }));
    });


    describe('submit handler', () => {
      function compileComponents(): Promise<any> {
        return TestBed.compileComponents()
          .then(() => TestBed.createComponent(SearchBarComponent));
      }

      it('should be invoked on submit', async(() => {
        compileComponents().then(fixture => {
          fixture.detectChanges();

          spyOn(fixture.componentInstance, 'submit');

          fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

          expect(fixture.componentInstance.submit).toHaveBeenCalledTimes(1);
        });
      }));

      it('should call router.navigate() if search value is NOT empty', async(() => {
        compileComponents().then(fixture => {
          let router = injector.get(Router);

          fixture.detectChanges();

          fixture.componentInstance.searchInput.setValue('test');
          fixture.detectChanges();

          fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

          expect(router.navigate).toHaveBeenCalledTimes(1);
        });
      }));

      it('should NOT call router.navigate() if search value is empty', async(() => {
        compileComponents().then(fixture => {
          let router = injector.get(Router);

          fixture.detectChanges();

          fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));

          expect(router.navigate).not.toHaveBeenCalled();
        });
      }));
    });
  });
});

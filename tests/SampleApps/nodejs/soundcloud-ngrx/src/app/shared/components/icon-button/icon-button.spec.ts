import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { IconComponent } from '../icon';
import { IconButtonComponent } from './icon-button';


@Component({template: ''})
class TestComponent {}


describe('shared', () => {
  describe('IconButtonComponent', () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          IconComponent,
          IconButtonComponent,
          TestComponent
        ]
      });
    });


    function compileComponents(template: string): Promise<any> {
      return TestBed
        .overrideComponent(TestComponent, {set: {template}})
        .compileComponents()
        .then(() => TestBed.createComponent(TestComponent));
    }


    it('should have default css class', async(() => {
      compileComponents('<icon-button></icon-button>')
        .then(fixture => {
          fixture.detectChanges();

          let button = fixture.nativeElement.querySelector('button');

          expect(button.classList).toContain('btn');
          expect(button.classList).toContain('btn--icon');
        });
    }));

    it('should have provided css classes', async(() => {
      compileComponents('<icon-button className="foo"></icon-button>')
        .then(fixture => {
          fixture.detectChanges();

          let button = fixture.nativeElement.querySelector('button');

          expect(button.classList).toContain('btn');
          expect(button.classList).toContain('btn--icon');
          expect(button.classList).toContain('foo');
        });
    }));

    it('should have a label', async(() => {
      compileComponents('<icon-button label="foo"></icon-button>')
        .then(fixture => {
          fixture.detectChanges();

          let button = fixture.nativeElement.querySelector('button');

          expect(button.getAttribute('aria-label')).toBe('foo');
        });
    }));

    it('should xlink to the correct svg content', async(() => {
      compileComponents('<icon-button icon="test"></icon-button>')
        .then(fixture => {
          fixture.detectChanges();

          let use = fixture.nativeElement.querySelector('use');

          expect(use.getAttribute('xlink:href')).toBe('#icon-test');
        });
    }));

    it('should emit `onClick` event when button is clicked', async(() => {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(IconButtonComponent))
        .then(fixture => {
          fixture.componentInstance.onClick.subscribe(event => {
            expect(event instanceof MouseEvent).toBe(true);
          });

          fixture.detectChanges();
          fixture.nativeElement.querySelector('button').click();
        });
    }));
  });
});

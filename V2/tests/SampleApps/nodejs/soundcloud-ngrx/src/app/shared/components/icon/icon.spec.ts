import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { IconComponent } from './icon';


@Component({template: ''})
class TestComponent {}


describe('shared', () => {
  describe('IconComponent', () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          IconComponent,
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


    it('should render with default css class', async(() => {
      compileComponents('<icon name="test"></icon>')
        .then(fixture => {
          fixture.detectChanges();

          let svg = fixture.nativeElement.querySelector('svg');

          expect(svg.classList).toContain('icon');
        });
    }));

    it('should render with provided css classes', async(() => {
      compileComponents('<icon className="foo bar" name="test"></icon>')
        .then(fixture => {
          fixture.detectChanges();

          let svg = fixture.nativeElement.querySelector('svg');

          expect(svg.classList).toContain('icon');
          expect(svg.classList).toContain('foo');
          expect(svg.classList).toContain('bar');
        });
    }));

    it('should xlink to the correct svg content', async(() => {
      compileComponents('<icon name="test"></icon>')
        .then(fixture => {
          fixture.detectChanges();

          let use = fixture.nativeElement.querySelector('use');

          expect(use.getAttribute('xlink:href')).toBe('#icon-test');
        });
    }));
  });
});

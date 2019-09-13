import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { ContentHeaderComponent } from './content-header';


@Component({template: ''})
class TestComponent {}


describe('shared', () => {
  describe('ContentHeaderComponent', () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          ContentHeaderComponent,
          TestComponent
        ]
      });
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(TestComponent));
    }


    it('should display provided section and title', async(() => {
      TestBed.overrideComponent(TestComponent, {set: {
        template: '<content-header [section]="section" [title]="title"></content-header>'
      }});

      compileComponents().then(fixture => {
        fixture.componentInstance.section = 'Section';
        fixture.componentInstance.title = 'Title';
        fixture.detectChanges();

        let compiled = fixture.nativeElement;

        expect(compiled.querySelector('.content-header__section').textContent).toBe('Section /');
        expect(compiled.querySelector('.content-header__title').textContent).toBe('Title');
      });
    }));
  });
});

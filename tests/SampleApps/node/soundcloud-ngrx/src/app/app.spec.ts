import { Component } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app';


@Component({selector: 'app-header', template: ''})
class AppHeaderComponentStub {}

@Component({selector: 'player', template: ''})
class PlayerComponentStub {}


describe('app', () => {
  describe('AppComponent', () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          AppComponent,
          PlayerComponentStub,
          AppHeaderComponentStub
        ],
        imports: [
          RouterTestingModule
        ]
      });
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(AppComponent));
    }


    it('should have a AppHeaderComponent', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let element = fixture.nativeElement.querySelector('app-header');
        expect(element).not.toEqual(null);
      });
    }));

    it('should have a PlayerComponent', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let element = fixture.nativeElement.querySelector('player');
        expect(element).not.toEqual(null);
      });
    }));

    it('should have a RouterOutlet', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();
        let element = fixture.nativeElement.querySelector('router-outlet');
        expect(element).not.toEqual(null);
      });
    }));
  });
});

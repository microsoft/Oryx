import { Component, ViewChild } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { SharedModule } from 'app/shared';
import { UserRecord } from '../models';
import { UserCardComponent } from './user-card';


@Component({template: ''})
class TestComponent {
  @ViewChild(UserCardComponent) userCard: UserCardComponent;
}


describe('users', () => {
  describe('UserCardComponent', () => {
    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [
          TestComponent,
          UserCardComponent
        ],
        imports: [
          RouterTestingModule,
          SharedModule
        ]
      });
    });


    function compileComponents(): Promise<any> {
      return TestBed
        .overrideComponent(TestComponent, {set: {
          template: '<user-card [resource]="resource" [user]="user"></user-card>'
        }})
        .compileComponents()
        .then(() => TestBed.createComponent(TestComponent));
    }


    it('should set property `resource` with default value `tracks`', async(() => {
      TestBed.compileComponents()
        .then(() => TestBed.createComponent(UserCardComponent))
        .then(fixture => {
          expect(fixture.componentInstance.resource).toBe('tracks');
        });
    }));

    it('should set property `resource` with provided @Input value', async(() => {
      compileComponents().then(fixture => {
        fixture.componentInstance.resource = 'foo';
        fixture.detectChanges();

        expect(fixture.componentInstance.userCard.resource).toBe('foo');
      });
    }));

    it('should set property `user` with provided input value', async(() => {
      compileComponents().then(fixture => {
        let user = new UserRecord({id: 123});

        fixture.componentInstance.user = user;
        fixture.detectChanges();

        expect(fixture.componentInstance.userCard.user).toBe(user);
      });
    }));

    it("should display the user's username", async(() => {
      compileComponents()
        .then(fixture => {
          fixture.componentInstance.user = new UserRecord({id: 123, username: 'goku'});
          fixture.detectChanges();

          expect(fixture.nativeElement.querySelector('h1').textContent).toBe('goku');
        });
    }));
  });
});

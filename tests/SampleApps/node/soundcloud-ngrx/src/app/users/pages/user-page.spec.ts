import { Component, Input } from '@angular/core';
import { async, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { SharedModule } from 'app/shared';
import { UserService } from '../user-service';
import { UserPageComponent } from './user-page';


@Component({selector: 'tracklist', template: ''})
class TracklistComponentStub {}

@Component({selector: 'user-card', template: ''})
class UserCardComponentStub {
  @Input() resource: any;
  @Input() user: any;
}


describe('users', () => {
  describe('UserPageComponent', () => {
    let activatedRoute;
    let user;

    beforeEach(() => {
      let userService = jasmine.createSpyObj('search', ['loadResource', 'loadUser']);
      userService.currentUser$ = new Subject<any>();

      let injector = TestBed.configureTestingModule({
        declarations: [
          UserPageComponent,
          TracklistComponentStub,
          UserCardComponentStub
        ],
        imports: [
          SharedModule
        ],
        providers: [
          {provide: ActivatedRoute, useValue: {params: new Subject<any>()}},
          {provide: UserService, useValue: userService}
        ]
      });

      activatedRoute = injector.get(ActivatedRoute);
      user = injector.get(UserService);
    });


    function compileComponents(): Promise<any> {
      return TestBed.compileComponents()
        .then(() => TestBed.createComponent(UserPageComponent));
    }


    it('should initialize properties', async(() => {
      compileComponents().then(fixture => {
        expect(fixture.componentInstance.resource).not.toBeDefined();
        expect(fixture.componentInstance.ngOnDestroy$ instanceof Subject).toBe(true);
      });
    }));

    it('should load user resource using route params', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();

        activatedRoute.params.next({id: '123', resource: 'tracks'});

        fixture.detectChanges();

        expect(user.loadResource).toHaveBeenCalledTimes(1);
        expect(user.loadResource).toHaveBeenCalledWith('123', 'tracks');
      });
    }));

    it('should load user using route params', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();

        activatedRoute.params.next({id: '123', resource: 'tracks'});

        fixture.detectChanges();

        expect(user.loadUser).toHaveBeenCalledTimes(1);
        expect(user.loadUser).toHaveBeenCalledWith('123');
      });
    }));

    it('should set `resource` property using route params', async(() => {
      compileComponents().then(fixture => {
        fixture.detectChanges();

        activatedRoute.params.next({id: '123', resource: 'tracks'});

        fixture.detectChanges();

        expect(fixture.componentInstance.resource).toBe('tracks');
      });
    }));
  });
});

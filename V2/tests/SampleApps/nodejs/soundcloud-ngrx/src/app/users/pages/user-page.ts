import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/pluck';
import 'rxjs/add/operator/takeUntil';

import { ChangeDetectionStrategy, Component, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { UserService } from '../user-service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section>
      <user-card 
        [resource]="resource"
        [user]="user.currentUser$ | async"></user-card>
  
      <tracklist></tracklist>
    </section>
  `
})
export class UserPageComponent implements OnDestroy {
  ngOnDestroy$ = new Subject<boolean>();
  resource: string;

  constructor(public route: ActivatedRoute, public user: UserService) {
    route.params
      .takeUntil(this.ngOnDestroy$)
      .do(({id, resource}: {id: string, resource: string}) => {
        user.loadResource(id, resource);
        this.resource = resource;
      })
      .pluck('id')
      .distinctUntilChanged()
      .subscribe((id: string) => user.loadUser(id));
  }

  ngOnDestroy(): void {
    this.ngOnDestroy$.next(true);
  }
}

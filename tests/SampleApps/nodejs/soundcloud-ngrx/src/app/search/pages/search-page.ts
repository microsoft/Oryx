import 'rxjs/add/operator/map';
import 'rxjs/add/operator/pluck';
import 'rxjs/add/operator/takeUntil';

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { SearchService } from '../search-service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section>
      <content-header 
        [section]="section" 
        [title]="search.query$ | async"></content-header>
  
      <tracklist></tracklist>
    </section>
  `
})

export class SearchPageComponent {
  ngOnDestroy$ = new Subject<boolean>();
  section = 'Search Results';

  constructor(public route: ActivatedRoute, public search: SearchService) {
    route.params
      .takeUntil(this.ngOnDestroy$)
      .pluck('q')
      .subscribe((value: string) => search.loadSearchResults(value));
  }

  ngOnDestroy(): void {
    this.ngOnDestroy$.next(true);
  }
}

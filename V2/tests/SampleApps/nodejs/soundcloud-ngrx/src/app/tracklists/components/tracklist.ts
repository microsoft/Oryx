import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { MediaQueryService } from 'app/core';
import { PlayerService } from 'app/player';
import { TracklistService } from '../tracklist-service';
import { TracklistScrollService } from './tracklist-scroll-service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    TracklistScrollService
  ],
  selector: 'tracklist',
  template: `
    <tracklist-items
      [layout]="layout"
      [media]="mediaQuery.matches$ | async"
      [player]="player.player$ | async"
      [times]="player.times$"
      [tracklist]="tracklist.tracklist$ | async"
      [tracks]="tracklist.tracks$"
      (pause)="player.pause()"
      (play)="player.play()"
      (seek)="player.seek($event)"
      (select)="player.select($event)"></tracklist-items>
  `
})
export class TracklistComponent implements OnDestroy, OnInit {
  @Input() layout: string;

  ngOnDestroy$ = new Subject<boolean>();

  constructor(
    public mediaQuery: MediaQueryService,
    public player: PlayerService,
    public scroll: TracklistScrollService,
    public tracklist: TracklistService
  ) {}

  ngOnDestroy(): void {
    this.ngOnDestroy$.next(true);
  }

  ngOnInit(): void {
    this.scroll.infinite(this.ngOnDestroy$);
  }
}

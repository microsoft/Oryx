import { ChangeDetectionStrategy, Component, ElementRef, Renderer, ViewEncapsulation } from '@angular/core';
import { PlayerService } from '../player-service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  selector: 'player',
  styleUrls: ['player.scss'],
  template: `
    <div class="player-timeline">
      <audio-timeline
        [times]="player.times$ | async"
        (seek)="player.seek($event)"></audio-timeline>
    </div>

    <player-controls
      [currentTime]="player.currentTime$"
      [cursor]="player.cursor$ | async"
      [player]="player.player$ | async"
      [track]="player.track$ | async"
      (decreaseVolume)="player.decreaseVolume()"
      (increaseVolume)="player.increaseVolume()"
      (pause)="player.pause()"
      (play)="player.play()"
      (select)="player.select({trackId: $event})"></player-controls>
  `
})
export class PlayerComponent {
  constructor(public el: ElementRef, public player: PlayerService, public renderer: Renderer) {
    let sub = player.player$.subscribe(player => {
      if (player.isPlaying) {
        renderer.setElementClass(el.nativeElement, 'open', true);
        sub.unsubscribe();
      }
    });
  }
}

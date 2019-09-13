import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ITrack, ITracklistCursor } from 'app/tracklists';
import { IPlayerState } from '../state/player-state';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  selector: 'player-controls',
  styleUrls: ['player-controls.scss'],
  template: `
    <div class="player-controls" *ngIf="track">
      <div>
        <icon-button icon="skip-previous" (onClick)="previous()"></icon-button>
        <icon-button [icon]="player.isPlaying ? 'pause' : 'play'" (onClick)="player.isPlaying ? pause.emit() : play.emit()"></icon-button>
        <icon-button icon="skip-next" (onClick)="next()"></icon-button>
      </div>

      <div class="player-controls__time">{{currentTime | async | formatTime}} / {{track.duration | formatTime:'ms'}}</div>
      <div class="player-controls__title">{{track.title}}</div>

      <div class="player-controls__volume">
        <icon-button icon="remove" (onClick)="decreaseVolume.emit()"></icon-button>
        <span>{{player.volume | formatVolume}}</span>
        <icon-button icon="add" (onClick)="increaseVolume.emit()"></icon-button>
      </div>
    </div>
  `
})
export class PlayerControlsComponent {
  @Input() currentTime: Observable<number>;
  @Input() cursor: ITracklistCursor;
  @Input() player: IPlayerState;
  @Input() track: ITrack;

  @Output() decreaseVolume = new EventEmitter(false);
  @Output() increaseVolume = new EventEmitter(false);
  @Output() pause = new EventEmitter(false);
  @Output() play = new EventEmitter(false);
  @Output() select = new EventEmitter(false);

  next(): void {
    if (this.cursor.nextTrackId) {
      this.select.emit(this.cursor.nextTrackId);
    }
  }

  previous(): void {
    if (this.cursor.previousTrackId) {
      this.select.emit(this.cursor.previousTrackId);
    }
  }
}

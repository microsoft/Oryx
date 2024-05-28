import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ITimesState } from 'app/player';
import { ITrack } from '../models';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  selector: 'track-card',
  styleUrls: ['track-card.scss'],
  template: `
    <article class="track-card" [ngClass]="{'track-card--compact': compact, 'track-card--full': !compact}">
      <div class="track-card__image">
        <img [src]="track.artworkUrl">
      </div>

      <div class="track-card__main">
        <div class="track-card__timeline" *ngIf="compact">
          <audio-timeline
            *ngIf="isSelected"
            [times]="times | async"
            (seek)="seek.emit($event)"></audio-timeline>      
        </div>

        <a class="track-card__username" [routerLink]="['/users', track.userId, 'tracks']">{{track.username}}</a>
        <h1 class="track-card__title">{{track.title}}</h1>

        <waveform-timeline
          *ngIf="!compact"
          [isActive]="isSelected"
          [times]="times"
          [waveformUrl]="track.waveformUrl"
          (seek)="seek.emit($event)"></waveform-timeline>

        <div class="track-card__actions" *ngIf="track.streamable">
          <div class="cell">
            <icon-button
              [icon]="isPlaying ? 'pause' : 'play'"
              (onClick)="isPlaying ? pause.emit() : play.emit()"></icon-button>
            <span class="meta-duration">{{track.duration | formatTime:'ms'}}</span>
          </div>

          <div class="cell" *ngIf="!compact">
            <icon name="headset" className="icon--small"></icon>
            <span class="meta-playback-count">{{track.playbackCount | formatInteger}}</span>
          </div>

          <div class="cell" *ngIf="!compact">
            <icon name="favorite-border" className="icon--small"></icon>
            <span class="meta-likes-count">{{track.likesCount | formatInteger}}</span>
          </div>

          <div class="cell">
            <a [href]="track.userPermalinkUrl" target="_blank" rel="noopener noreferrer">
              <icon name="launch" className="icon--small"></icon>
            </a>
          </div>
        </div>
      </div>
    </article>
  `
})
export class TrackCardComponent {
  @Input() compact = false;
  @Input() isPlaying = false;
  @Input() isSelected = false;
  @Input() times: Observable<ITimesState>;
  @Input() track: ITrack;

  @Output() pause = new EventEmitter(false);
  @Output() play = new EventEmitter(false);
  @Output() seek = new EventEmitter(false);
}

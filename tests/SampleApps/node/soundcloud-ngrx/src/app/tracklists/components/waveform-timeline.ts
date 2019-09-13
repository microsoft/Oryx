import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ITimesState } from 'app/player';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'waveform-timeline',
  styleUrls: ['waveform-timeline.scss'],
  template: `
    <div class="waveform-timeline" [ngClass]="{'waveform-timeline--ready': ready}">
      <audio-timeline
        *ngIf="isActive"
        [times]="times | async"
        (seek)="seek.emit($event)"></audio-timeline>

      <waveform
        [src]="waveformUrl"
        (ready)="ready = $event"></waveform>
    </div>
  `
})
export class WaveformTimelineComponent {
  @Input() isActive: boolean = false;
  @Input() times: Observable<ITimesState>;
  @Input() waveformUrl: string;

  @Output() seek = new EventEmitter<any>(false);

  ready: boolean = false;
}

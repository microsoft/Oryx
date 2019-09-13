import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  ViewEncapsulation
} from '@angular/core';
import { ITimesState } from 'app/player';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  selector: 'audio-timeline',
  styleUrls: ['audio-timeline.scss'],
  template: `
    <div class="bar bar--buffered"
      [ngClass]="{'bar--animated': times?.bufferedTime !== 0}"
      [ngStyle]="{'width': times?.percentBuffered}"></div>

    <div class="bar bar--elapsed"
      [ngStyle]="{'width': times?.percentCompleted}"></div>
  `
})
export class AudioTimelineComponent {
  @Input() times: ITimesState;
  @Output() seek = new EventEmitter(false);

  @HostListener('click', ['$event'])
  onClick(event: any): void {
    const { currentTarget, pageX } = event;
    this.seek.emit(
      (pageX - currentTarget.getBoundingClientRect().left) / currentTarget.offsetWidth * this.times.duration
    );
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewEncapsulation
} from '@angular/core';
import { ApiService } from 'app/core';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  selector: 'waveform',
  styleUrls: ['waveform.scss'],
  template: ''
})
export class WaveformComponent implements OnInit {
  @Input() color: string = '#1d1e1f';
  @Input() src: string;
  @Output() ready = new EventEmitter<boolean>(false);

  constructor(public api: ApiService, public el: ElementRef) {}

  render(data: {height: number, width: number, samples: number[]}): void {
    let canvas = document.createElement('canvas');
    canvas.height = data.height / 2; // 70px
    canvas.width = data.width / 2;   // 900px

    let context = canvas.getContext('2d');
    context.fillStyle = this.color;

    let samples = data.samples,
        l = samples.length,
        i = 0,
        x = 0,
        v;

    for (; i < l; i += 2, x++) {
      v = samples[i] / 4;
      context.fillRect(x, 0, 1, 35 - v);
      context.fillRect(x, 35 + v, 1, 70);
    }

    this.el.nativeElement.appendChild(canvas);
    this.ready.emit(true);
  }

  ngOnInit(): void {
    this.api.fetch(this.src)
      .subscribe(data => this.render(data));
  }
}

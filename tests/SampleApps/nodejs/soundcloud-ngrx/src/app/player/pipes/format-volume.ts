import { Pipe, PipeTransform } from '@angular/core';


@Pipe({
  name: 'formatVolume',
  pure: true
})
export class FormatVolumePipe implements PipeTransform {
  transform(volume: number): string {
    if (!volume) return '0.0';
    volume /= 10;
    let precision = volume >= 1 ? 2 : 1;
    return volume.toPrecision(precision);
  }
}

import { Pipe, PipeTransform } from '@angular/core';


const REPLACER_PATTERN: RegExp = /(.)(?=(\d{3})+$)/g;


@Pipe({
  name: 'formatInteger',
  pure: true
})
export class FormatIntegerPipe implements PipeTransform {
  transform(int: number): number|string {
    if (int < 1000) return int;
    return String(int).replace(REPLACER_PATTERN, '$1,');
  }
}

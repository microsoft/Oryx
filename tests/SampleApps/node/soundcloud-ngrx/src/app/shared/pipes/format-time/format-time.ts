import { Pipe, PipeTransform } from '@angular/core';


@Pipe({
  name: 'formatTime',
  pure: true
})
export class FormatTimePipe implements PipeTransform {
  transform(time: number, unit?: string): string {
    if (typeof time !== 'number' || !time) {
      return '00:00';
    }

    // HTMLAudioElement provides time in seconds
    // SoundCloud provides time in milliseconds
    if (unit === 'ms') {
      time /= 1000; // convert milliseconds to seconds
    }

    let hours: number = Math.floor(time / 3600);
    let minutes: string = `0${Math.floor((time % 3600) / 60)}`.slice(-2);
    let seconds: string = `0${Math.floor((time % 60))}`.slice(-2);

    if (hours) {
      return `${hours}:${minutes}:${seconds}`;
    }
    else {
      return `${minutes}:${seconds}`;
    }
  }
}

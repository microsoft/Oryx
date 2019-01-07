export class AudioSource extends HTMLAudioElement {}


export function audioSourceFactory(): HTMLAudioElement {
  return new Audio();
}


export const AUDIO_SOURCE_PROVIDER = {
  provide: AudioSource,
  useFactory: audioSourceFactory
};

import {
  CLIENT_ID_PARAM,
  IMAGE_DEFAULT_SIZE,
  IMAGE_XLARGE_SIZE,
  WAVEFORM_IMAGE_HOST,
  WAVEFORM_JSON_HOST
} from 'app/app-config';
import { ITrackData } from './models';


const EN_DASH = String.fromCharCode(8211);


export function formatTrackTitle(title: string): string {
  if (!title) return '';
  return title.replace(/-/g, EN_DASH);
}

export function streamUrl(url: string): string {
  return `${url}?${CLIENT_ID_PARAM}`;
}

export function trackImageUrl(trackData: ITrackData, size: string = IMAGE_XLARGE_SIZE): string {
  let url = trackData.artwork_url || trackData.user.avatar_url;
  return url.replace(IMAGE_DEFAULT_SIZE, size);
}

export function waveformUrl(url: string): string {
  if (url.includes('.json')) return url;
  return url
    .replace(WAVEFORM_IMAGE_HOST, WAVEFORM_JSON_HOST)
    .replace('.png', '.json');
}

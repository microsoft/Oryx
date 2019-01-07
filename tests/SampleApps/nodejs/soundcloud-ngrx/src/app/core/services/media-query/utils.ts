import { IMediaQueryRule } from './interfaces';


export function em(value: number): number | string {
  if (typeof value !== 'number') {
    throw new TypeError('ERROR @ em() : expected param `value` to be a number');
  }

  return value === 0 ? value : `${value / 16}em`;
}


export function getMedia(rule: IMediaQueryRule): string {
  let media = rule.type || 'screen';

  if (rule.minWidth) media += ` and (min-width: ${em(rule.minWidth)})`;
  if (rule.maxWidth) media += ` and (max-width: ${em(rule.maxWidth)})`;
  if (rule.orientation) media += ` and (orientation: ${rule.orientation})`;

  return media;
}

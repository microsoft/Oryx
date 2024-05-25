import { MEDIA_QUERY_RULES, MediaQueryService } from './media-query-service';


export { MediaQueryService };
export { IMediaQueryResults } from './interfaces';


export const MEDIA_QUERY_PROVIDERS: any[] = [
  MediaQueryService,
  {provide: MEDIA_QUERY_RULES, useValue: [
    {
      id: 'large',
      minWidth: 980
    }
  ]}
];

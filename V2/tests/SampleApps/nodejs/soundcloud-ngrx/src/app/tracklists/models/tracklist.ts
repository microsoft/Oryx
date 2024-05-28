import { List, Map, Record } from 'immutable';


export interface ITracklist extends Map<string,any> {
  currentPage: number;
  hasNextPage: boolean;
  hasNextPageInStore: boolean;
  id: string;
  isNew: boolean;
  isPending: boolean;
  nextUrl: string;
  pageCount: number;
  trackIds: List<number>;
}

export const TracklistRecord = Record({
  currentPage: 0,
  hasNextPage: null,
  hasNextPageInStore: null,
  id: null,
  isNew: true,
  isPending: false,
  nextUrl: null,
  pageCount: 0,
  trackIds: List()
});

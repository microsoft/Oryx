import { Map, Record } from 'immutable';


export interface ISearchState extends Map<string,any> {
  query: string;
}

export const SearchStateRecord = Record({
  query: null
});

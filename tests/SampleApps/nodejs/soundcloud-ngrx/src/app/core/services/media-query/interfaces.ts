export interface IMediaQueryResults {
  [key: string]: boolean;
}

export interface IMediaQueryRule {
  id: string;
  maxWidth?: number;
  minWidth?: number;
  orientation?: string;
  type?: string;
}

export interface IMediaQueryUpdate {
  mql: MediaQueryList;
  rule: IMediaQueryRule;
}

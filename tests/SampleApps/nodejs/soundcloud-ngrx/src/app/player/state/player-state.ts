import { Map, Record } from 'immutable';


export interface IPlayerState extends Map<string,any> {
  isPlaying: boolean;
  trackId: number;
  tracklistId: string;
  volume: number;
}

export const PlayerStateRecord = Record({
  isPlaying: false,
  trackId: null,
  tracklistId: null,
  volume: null
});

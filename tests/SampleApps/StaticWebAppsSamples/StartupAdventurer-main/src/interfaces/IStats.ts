export interface IStat {
	category: string;
	name: string;
}

export interface IGradedStat extends IStat {
	level: number;
}

export interface IStatsState {
	selectedStats: IStat[];
	statPointsAvailable: number;
	gradedStats: IGradedStat[];
	totalStatPoints: number;
}

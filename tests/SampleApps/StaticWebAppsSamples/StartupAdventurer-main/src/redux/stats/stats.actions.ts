import { IAction } from "@/interfaces/IAction";
import { IStat, IGradedStat } from "@/interfaces/IStats";
import { ADD_STAT, RESET_STATS, ADD_STAT_POINT, REMOVE_STAT_POINT } from "./stats.types";

export const addStat = (stat: IStat): IAction => ({
	type: ADD_STAT,
	payload: { stat },
});

export const addStatPoint = (stat: IGradedStat): IAction => ({
	type: ADD_STAT_POINT,
	payload: { stat },
});

export const removeStatPoint = (stat: IGradedStat): IAction => ({
	type: REMOVE_STAT_POINT,
	payload: { stat },
});

export const resetStats = (): IAction => ({
	type: RESET_STATS,
	payload: {},
});

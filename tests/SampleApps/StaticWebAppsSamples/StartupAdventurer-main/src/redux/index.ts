import { combineReducers } from "redux";
import { character } from "./character";
import { info } from "./info";
import { ui } from "./ui";
import { stats } from "./stats";

export const reducers = () =>
	combineReducers({
		character,
		info,
		stats,
		ui,
	});

import { ICharacter } from "./ICharacter";
import { IBasicInfo } from "./IBasicInfo";
import { IStatsState } from "./IStats";
import { IUiState } from "./IUiState";

export interface IStoreState {
	character: ICharacter;
	info: IBasicInfo;
	stats: IStatsState;
	ui: IUiState;
}

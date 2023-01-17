export interface IUiState {
	currentStep: number;
	isEnd: boolean;
	isStart: boolean;
	isIdle: boolean;
	isCharacterDisplay: boolean;
	showGameOver: boolean;
	totalSteps: number;
	[key: string]: any;
}

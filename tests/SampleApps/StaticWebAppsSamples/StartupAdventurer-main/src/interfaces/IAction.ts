export interface IAction {
	type: string;
	payload: any;
	[key: string]: any;
}

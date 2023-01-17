export type Colors = string[];
export interface IColorSet {
	name: string;
	palette?: Colors;
}
export interface IColorOptions {
	[key: string]: Colors;
}

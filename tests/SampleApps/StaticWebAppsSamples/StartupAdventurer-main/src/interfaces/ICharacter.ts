import { IColorSet } from "./Colors"

export interface ICharacter {
	bottom?: IStyledSelection<BottomStyle>;
	tops?: {
		hoodie?: IColorSet;
		tshirt?: IColorSet;
		shirt?: IColorSet;
		jacket?: IColorSet;
	};
	shoes?: string; // This should probably be a type
	accessories?: string[];
	eyewear?: string;
	hair?: Partial<IStyledSelection<string>>;
	facialHair: Partial<IStyledSelection<string>>;
	skinColor: IColorSet;

	startedAt?: string;
	completedAt?: string;
	viewedOptionTabs: string[];
}

export type TopStyle = "hoodie" | "jacket" | "tshirt" | "shirt"
export type ColoredBottomStyle = "pants" | "maxiSkirt" | "miniSkirt"
export type OtherBottomStyle = "kilt" | "prosthetic" | "bathrobe" | "dress"
export type BottomStyle = ColoredBottomStyle | OtherBottomStyle


export interface IStyledSelection<Styles> {
	style: Styles,
	color?: IColorSet
}

export interface IColoredSelection {
	color?: IColorSet
}
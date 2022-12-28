import { Colors, IColorOptions } from "@/interfaces/Colors";
import isEqual from "lodash-es/isEqual";
import isEmpty from "lodash-es/isEmpty";

export interface IColorSet {
	name: string;
	palette?: Colors;
}

export const getColorSet = (colorPalette: Colors, colorOptions: IColorOptions): IColorSet => {
	if (!colorPalette || !colorOptions) return { name: "" };

	const name = Object.keys(colorOptions).filter(key => isEqual(colorOptions[key], colorPalette))[0] || "";

	return {
		name,
		palette: colorPalette,
	};
};

export const decideNextSet = (
	activeSet: IColorSet | undefined,
	newSet: IColorSet | undefined
): IColorSet | undefined => {
	return !isEmpty(activeSet) ? undefined : newSet;
};

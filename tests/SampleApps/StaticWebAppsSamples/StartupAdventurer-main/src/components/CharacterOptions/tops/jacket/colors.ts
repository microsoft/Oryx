import { IColorOptions } from "@/interfaces/Colors";

const jacketColors: IColorOptions = {
	"jacket-taurus": ["#343434", "#212121", "#000000"],
	"jacket-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"jacket-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"jacket-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"jacket-libra": ["#4C301D", "#382315", "#1E130B"],
	"jacket-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"jacket-virgo": ["#154387", "#0F3A6D", "#0B2E51"],
	"jacket-scorpio": ["#008272", "#006859", "#005E4E"],
};

export const defaultColor = jacketColors["jacket-taurus"];
export default jacketColors;

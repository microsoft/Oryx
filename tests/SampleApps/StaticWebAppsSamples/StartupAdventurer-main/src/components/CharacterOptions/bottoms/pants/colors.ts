import { IColorOptions } from "@/interfaces/Colors";

const pantsColors: IColorOptions = {
	"pants-taurus": ["#343434", "#212121", "#000000"],
	"pants-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"pants-cancer": ["#3D5172", "#30435E", "#253549"],
	"pants-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"pants-libra": ["#4C301D", "#382315", "#1E130B"],
	"pants-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"pants-virgo": ["#D4EDFF", "#B4D1E3", "#99BBCB"],
	"pants-scorpio": ["#008272", "#006859", "#005244"],
};

export const defaultColor = pantsColors["pants-cancer"];

export default pantsColors;

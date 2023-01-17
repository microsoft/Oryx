import { IColorOptions } from "@/interfaces/Colors";

const shirtColors: IColorOptions = {
	"shirt-taurus": ["#343434", "#212121", "#000000"],
	"shirt-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"shirt-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"shirt-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"shirt-libra": ["#68217A", "#581C6B", "#442359"],
	"shirt-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"shirt-virgo": ["#00188f", "#001972", "#021f49"],
	"shirt-scorpio": ["#D4EDFF", "#B4D1E3", "#99BBCB"],
};

export const defaultColor = shirtColors["shirt-scorpio"];
export default shirtColors;

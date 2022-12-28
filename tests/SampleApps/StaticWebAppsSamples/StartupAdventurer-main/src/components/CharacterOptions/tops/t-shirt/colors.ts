import { IColorOptions } from "@/interfaces/Colors";

const tshirtColors: IColorOptions = {
	"t-shirt-taurus": ["#343434", "#212121", "#000000"],
	"t-shirt-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"t-shirt-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"t-shirt-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"t-shirt-libra": ["#68217A", "#581C6B", "#442359"],
	"t-shirt-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"t-shirt-virgo": ["#00188f", "#001972", "#021f49"],
	"t-shirt-scorpio": ["#008272", "#006859", "#005E4E"],
};

export const defaultColor = tshirtColors["t-shirt-taurus"];
export default tshirtColors;

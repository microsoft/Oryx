import { IColorOptions } from "@/interfaces/Colors";

const miniSkirtColors: IColorOptions = {
	"mini-skirt-taurus": ["#343434", "#212121", "#000000"],
	"mini-skirt-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"mini-skirt-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"mini-skirt-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"mini-skirt-libra": ["#68217A", "#581C6B", "#442359"],
	"mini-skirt-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"mini-skirt-virgo": ["#00188f", "#001972", "#021f49"],
	"mini-skirt-scorpio": ["#D4EDFF", "#B4D1E3", "#99BBCB"],
};

export const defaultColor = miniSkirtColors["mini-skirt-taurus"];
export default miniSkirtColors;

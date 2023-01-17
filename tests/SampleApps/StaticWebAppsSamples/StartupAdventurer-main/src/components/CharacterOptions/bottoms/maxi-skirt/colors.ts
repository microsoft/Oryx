import { IColorOptions } from "@/interfaces/Colors";

const maxiSkirtColors: IColorOptions = {
	"maxi-skirt-taurus": ["#343434", "#212121", "#000000"],
	"maxi-skirt-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"maxi-skirt-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"maxi-skirt-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"maxi-skirt-libra": ["#68217A", "#581C6B", "#442359"],
	"maxi-skirt-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"maxi-skirt-virgo": ["#00188f", "#001972", "#021f49"],
	"maxi-skirt-scorpio": ["#D4EDFF", "#B4D1E3", "#99BBCB"],
};

export const defaultColor = maxiSkirtColors["maxi-skirt-scorpio"];
export default maxiSkirtColors;

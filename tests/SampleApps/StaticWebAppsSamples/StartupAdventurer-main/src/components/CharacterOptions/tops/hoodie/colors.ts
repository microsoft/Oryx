import { IColorOptions } from "@/interfaces/Colors";

const hoodieColors: IColorOptions = {
	"hoodie-taurus": ["#343434", "#212121", "#000000"],
	"hoodie-gemini": ["#FFFFFF", "#EAEAEA", "#D8D8D8"],
	"hoodie-cancer": ["#FFB900", "#EFA500", "#D68F00"],
	"hoodie-leo": ["#F472D0", "#D665BB", "#BA54AB"],
	"hoodie-libra": ["#68217A", "#581C6B", "#442359"],
	"hoodie-lisa": ["#BA141A", "#A3131D", "#8C101C"],
	"hoodie-virgo": ["#00188f", "#001972", "#021f49"],
	"hoodie-scorpio": ["#008272", "#006859", "#005E4E"],
};

export const defaultColor = hoodieColors["hoodie-lisa"];
export default hoodieColors;

import { IColorOptions } from "@/interfaces/Colors";

const facialHairColors: IColorOptions = {
	"facial-hair-taurus": ["#343434", "#212121", "#000000"],
	"facial-hair-gemini": ["#603218", "#4F2814", "#422011"],
	"facial-hair-cancer": ["#FCE1B1", "#E8C589", "#DBAC6E"],
	"facial-hair-virgo": ["#FFD521", "#EAC31C", "#D3A12C"],
	"facial-hair-scorpio": ["#EAEAEA", "#D8D8D8", "#C4C4C4"],
};

export const defaultColor = facialHairColors["facial-hair-gemini"];
export const defaultColorObject = { name: "facial-hair-gemini", palette: defaultColor };
export default facialHairColors;

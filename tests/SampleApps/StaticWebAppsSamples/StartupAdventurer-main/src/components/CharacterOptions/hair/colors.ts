import { Colors } from "@/interfaces/Colors";

interface IColorOptions {
	[key: string]: Colors;
}

const hairColors: IColorOptions = {
	"hair-gemini": ["#603218", "#4F2814", "#422011"],
	"hair-taurus": ["#343434", "#212121", "#000000"],
	"hair-cancer": ["#FCE1B1", "#E8C589", "#DBAC6E"],
	"hair-leo": ["#FF7B5C", "#E56E57", "#C1594C"],
	"hair-libra": ["#EF97DF", "#E281D5", "#C96FBC"],
	"hair-virgo": ["#4850E5", "#3442C6", "#283899"],
	"hair-scorpio": ["#EAEAEA", "#D8D8D8", "#C4C4C4"],
};

export const defaultColor = hairColors["hair-gemini"];
export const defaultColorObject = { name: "hair-gemini", palette: defaultColor };
export default hairColors;

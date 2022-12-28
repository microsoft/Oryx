import { IColorOptions } from "@/interfaces/Colors";

const skinColors: IColorOptions = {
	"skin-taurus": ["#FFEBE1", "#EFD9CE", "#E0CBC1", "#68605a"],
	"skin-gemini": ["#F6CCA7", "#DDBA9E", "#C4A385", "#533e2f"],
	"skin-cancer": ["#B78869", "#9E715A", "#7C5B45", "#3d2617"],
	"skin-leo": ["#A46135", "#894D2F", "#663D32", "#3d2617"],
	"skin-libra": ["#5B4234", "#412E22", "#2E1D17", "#000000"],
	"skin-lisa": ["#FFD521", "#EAC31C", "#D3A12C"],
};

export const defaultColor = skinColors["skin-cancer"];
export const defaultColorObject = { name: "skin-cancer", palette: defaultColor };
export default skinColors;

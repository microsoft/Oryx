import hair from "@/components/CharacterOptions/hair";
import hairColors from "@/components/CharacterOptions/hair/colors";
import facialHair from "@/components/CharacterOptions/facial-hair";
import facialHairColors from "@/components/CharacterOptions/facial-hair/colors";
import tops from "@/components/CharacterOptions/tops";
import bottoms from "@/components/CharacterOptions/bottoms";
import shoes from "@/components/CharacterOptions/shoes";
import accessories from "@/components/CharacterOptions/accessories";
import skinColors from "@/components/CharacterOptions/skin";
import eyewear from "@/components/CharacterOptions/eyewear";
import get from "lodash-es/get";

const { tshirt, shirt, jacket, hoodie } = tops;

const emptyLayer = () => null;

const looks: any[] = [
	{
		hair: { layer: hair.hair1.layer, color: get(hairColors, "hair-leo") },
		facialHair: { layer: facialHair.facialHair1.layer, color: get(facialHairColors, "facial-hair-taurus") },
		tshirt: { layer: tshirt.layer, color: get(tshirt.colors, "t-shirt-leo") },
		jacket: { layer: jacket.layer, color: get(jacket.colors, "jacket-scorpio") },
		bottom: { layer: bottoms.maxiSkirt.layer, color: get(bottoms, ["maxiSkirt", "colors", "maxi-skirt-taurus"]) },
		shoes: { layer: shoes[3].layer },
		accessories: [accessories.apple, accessories.briefcase],
		skin: get(skinColors, "skin-gemini"),
		eyewear: { layer: eyewear.eyewear6.layer },
	},
	{
		hair: { layer: hair.hair5.layer, color: get(hairColors, "hair-libra") },
		facialHair: { layer: emptyLayer, color: null },
		shirt: { layer: shirt.layer, color: get(shirt.colors, "shirt-leo") },
		hoodie: { layer: hoodie.layer, color: get(hoodie.colors, "hoodie-virgo") },
		bottom: { layer: bottoms.kilt.layer, color: null },
		shoes: { layer: shoes[5].layer },
		accessories: [accessories.keyboard],
		skin: get(skinColors, "skin-leo"),
		eyewear: { layer: emptyLayer },
	},
	{
		hair: { layer: emptyLayer, color: get(hairColors, "hair-leo") },
		facialHair: { layer: facialHair.facialHair3.layer, color: get(facialHairColors, "facial-hair-scorpio") },
		tshirt: { layer: tshirt.layer, color: get(tshirt.colors, "t-shirt-lisa") },
		jacket: { layer: jacket.layer, color: get(jacket.colors, "jacket-libra") },
		bottom: { layer: bottoms.pants.layer, color: get(bottoms, ["pants", "colors", "pants-gemini"]) },
		shoes: { layer: shoes[4].layer },
		accessories: [],
		skin: get(skinColors, "skin-cancer"),
		eyewear: { layer: eyewear.eyewear2 },
	},
	{
		hair: { layer: hair.hair6.layer, color: get(hairColors, "hair-gemini") },
		facialHair: { layer: facialHair.facialHair5.layer, color: get(facialHairColors, "facial-hair-virgo") },
		shirt: { layer: shirt.layer, color: get(shirt.colors, "shirt-cancer") },
		bottom: { layer: bottoms.miniSkirt.layer, color: get(bottoms, ["miniSkirt", "colors", "mini-skirt-libra"]) },
		shoes: { layer: shoes[0].layer },
		accessories: [],
		skin: get(skinColors, "skin-taurus"),
		eyewear: { layer: eyewear.eyewear4 },
	},
];

export default looks;

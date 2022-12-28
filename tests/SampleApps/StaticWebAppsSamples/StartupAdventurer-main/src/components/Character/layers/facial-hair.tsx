import React from "react";
import facialHair from "@/components/CharacterOptions/facial-hair";
import get from "lodash-es/get";
import camelCase from "lodash-es/camelCase";
import { ICharacter } from "@/interfaces/ICharacter";

interface IProps {
	selected?: ICharacter["facialHair"]
}

const FacialHairLayer = ({ selected }: IProps) => {
	const activeLayer = get(facialHair, [camelCase(selected?.style), "layer"]);

	return (
		<g id="character-facial-hair">
			{activeLayer ? activeLayer({ colors: get(selected?.color, ["palette"]) }) : null}
		</g>
	);
};

export default FacialHairLayer;

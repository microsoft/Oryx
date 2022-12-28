import React from "react";
import bottoms from "@/components/CharacterOptions/bottoms";
import get from "lodash-es/get";
import { ICharacter } from "@/interfaces/ICharacter";
interface IProps {
	selected?: ICharacter["bottom"]
}

const BottomLayer = ({ selected: selectedBottom }: IProps) => {
	const activeLayer = selectedBottom ? get(bottoms, [selectedBottom.style, "layer"]) : null;

	return (
		<g id="character-bottom">
			{activeLayer ? activeLayer({ colors: get(selectedBottom, ["color", "palette"]) }) : null}
		</g>
	);
};

export default BottomLayer;

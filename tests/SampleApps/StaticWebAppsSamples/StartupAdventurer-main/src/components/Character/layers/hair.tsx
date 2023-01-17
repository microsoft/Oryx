import React from "react";
import hair from "@/components/CharacterOptions/hair";
import get from "lodash-es/get";
import { ICharacter } from "@/interfaces/ICharacter";

interface IProps {
	selected?: ICharacter["hair"]
}

const HairLayer = ({ selected }: IProps) => {
	const activeLayer = selected?.style ? get(hair, [selected?.style, "layer"]) : null;

	return <g id="character-hair">{activeLayer ? activeLayer({ colors: get(selected?.color, ["palette"]) }) : null}</g>;
};

export default HairLayer;

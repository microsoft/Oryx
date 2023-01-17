import React, { Fragment } from "react";
import accessories from "@/components/CharacterOptions/accessories";

import get from "lodash-es/get";

interface IProps {
	selectedAccessories?: string[]
}

const CharacterAccessories = ({ selectedAccessories = [] }: IProps) => {

	const getLayer = (key: string, i: number) => {
		const layer = get(accessories, [key, "layer"]);

		if (typeof layer === "function") return <Fragment key={"layer" + key + i}>{layer()}</Fragment>;

		return null;
	};

	return <g id="character-accessories">{!!selectedAccessories && selectedAccessories.map(getLayer)}</g>;
};

export default CharacterAccessories;

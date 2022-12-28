import React from "react";
import eyewear from "@/components/CharacterOptions/eyewear";
import get from "lodash-es/get";
import camelCase from "lodash-es/camelCase";

const Eyewear = ({ selected: selectedEyewear = "" }) => {
	const activeLayer = get(eyewear, [camelCase(selectedEyewear), "layer"]);

	return <g id="character-eyewear">{activeLayer ? activeLayer() : null}</g>;
};

export default Eyewear;

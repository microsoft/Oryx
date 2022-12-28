import React from "react";
import shoes from "@/components/CharacterOptions/shoes";

const shoeLayers = shoes.map((shoe, i: number) => ({
	name: `shoe-${i + 1}`,
	component: shoe.layer,
}));

const ShoeLayer = ({ selectedShoes = "" }) => {

	const getShoeByName = (name: string) => shoeLayers.filter(shoe => shoe && shoe.name === name)[0];

	const getLayer = (active: string) => {
		const shoe = getShoeByName(active);
		return shoe ? shoe.component() : null;
	};

	return <g id="character-shoes">{getLayer(selectedShoes)}</g>;
};

export default ShoeLayer;

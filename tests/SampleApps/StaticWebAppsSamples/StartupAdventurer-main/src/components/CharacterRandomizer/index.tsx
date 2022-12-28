import React, { useState } from "react";
import CharacterBase from "@/components/Character/base";
import { Container } from "./styles";

import get from "lodash-es/get";
import shuffle from "lodash-es/shuffle";

import characterLooks from "./character-looks";
import useInterval from "@/hooks/use-interval";
import { Colors } from "@/interfaces/Colors";

const characters = shuffle(characterLooks);
const initialCharacter = characters[0];

const CharacterRandomizer = ({ running = true }) => {
	const [character, setCharacter] = useState(initialCharacter);
	const [options, setOptions] = useState(characters);

	const getLayer = (obj: { layer: Function; color: Colors | null }) => {
		const layer = get(obj, "layer");
		const color = get(obj, "color");
		return typeof layer === "function" ? layer({ colors: color ? color : undefined }) : null;
	};

	try {
		useInterval(
			() => {
				const newOptions = [...options];
				const newCharacter = newOptions.pop();
				newOptions.unshift(newCharacter);

				setOptions(newOptions);
				setCharacter(newCharacter);
			},
			running ? 1000 : null
		);

		return (
			<Container className="character-randomizer">
				<CharacterBase skinColors={get(character, "skin") || []}>
					{character.tshirt && <g id="c-tshirt">{getLayer(character.tshirt)}</g>}
					{character.shirt && <g id="c-shirt">{getLayer(character.shirt)}</g>}
					{character.jacket && <g id="c-jacket">{getLayer(character.jacket)}</g>}
					{character.hoodie && <g id="c-hoodie">{getLayer(character.hoodie)}</g>}
					{character.bottom && <g id="c-bottom">{getLayer(character.bottom)}</g>}
					{character.shoes && <g id="c-shoes">{getLayer(character.shoes)}</g>}
					{character.accessories &&
						character.accessories.map((accessory: any, i: number) => (
							<g key={"accessory$$" + i} data-id="accessory">
								{getLayer(accessory)}
							</g>
						))}
					{character.hair && <g id="c-hair">{getLayer(character.hair)}</g>}
					{character.facialHair && <g id="c-facial-hair">{getLayer(character.facialHair)}</g>}
					{character.eyewear && <g id="c-eyewear">{getLayer(character.eyewear)}</g>}
				</CharacterBase>
			</Container>
		);
	} catch (e) {
		console.error(e);
		return null;
	}
};

export default CharacterRandomizer;

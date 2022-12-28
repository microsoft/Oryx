import React from "react";
import { CharacterContainer } from "./styles";
import CharacterBase from "./base";

import Hair from "./layers/hair";
import Shoes from "./layers/shoes";
import Bottoms from "./layers/bottom";
import Tops from "./layers/top";
import FacialHair from "./layers/facial-hair";
import Eyewear from "./layers/eyewear";
import Accessories from "./layers/accessories";
import { useSelector } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";

const Character = () => {
	const { skinColor, bottom, tops, shoes, accessories, hair, eyewear, facialHair } = useSelector((store: IStoreState) => store.character);
	const hasProsthetic = !!bottom && bottom.style && bottom.style === "prosthetic";

	return (
		<CharacterContainer>
			<CharacterBase skinColors={skinColor && skinColor.palette} hasProsthetic={hasProsthetic}>
				<Tops selected={tops} />
				<Bottoms selected={bottom} />
				<Shoes selectedShoes={shoes} />
				<Accessories selectedAccessories={accessories} />
				<Hair selected={hair} />
				<Eyewear selected={eyewear} />
				<FacialHair selected={facialHair} />
			</CharacterBase>
		</CharacterContainer>
	);
};

export default Character;

import React from "react";
import tops from "@/components/CharacterOptions/tops";

import isEmpty from "lodash-es/isEmpty";
import get from "lodash-es/get";
import { ICharacter } from "@/interfaces/ICharacter";

interface IProps { selected?: ICharacter["tops"] }

const CharacterTop = ({ selected }: IProps) => {
	const { hoodie, jacket, shirt, tshirt } = tops;

	return (
		<g id="character-top">
			{!isEmpty(selected?.tshirt) && tshirt.layer({ colors: get(selected?.tshirt, "palette") })}
			{!isEmpty(selected?.shirt) && shirt.layer({ colors: get(selected?.shirt, "palette") })}
			{!isEmpty(selected?.jacket) && jacket.layer({ colors: get(selected?.jacket, "palette") })}
			{!isEmpty(selected?.hoodie) && hoodie.layer({ colors: get(selected?.hoodie, "palette") })}
		</g>
	);
};

export default CharacterTop;

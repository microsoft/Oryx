import React from "react";
import { OptionPanel } from "./styles";
import hair from "@/components/CharacterOptions/hair";
import hairColors from "@/components/CharacterOptions/hair/colors";
import skinColors from "@/components/CharacterOptions/skin";
import facialHair from "@/components/CharacterOptions/facial-hair";
import facialHairColors from "@/components/CharacterOptions/facial-hair/colors";
import eyewear from "@/components/CharacterOptions/eyewear";

import OptionStyleSelectable from "@/components/Selectables/option-style-selectable";
import OptionColorSelectable from "@/components/Selectables/option-color-selectable";

import toArray from "lodash-es/toArray";
import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";
import {
	setHairStyle,
	setHairColor,
	setSkinColor,
	setFacialHair,
	setFacialHairColor,
	setEyewear,
} from "@/redux/character/character.actions";

import { IStoreState } from "@/interfaces/IStoreState";
import clsx from "clsx";

const BodyPanel = () => {
	const dispatch: Dispatch = useDispatch();

	const {
		hair: selectedHair,
		facialHair: selectedFacialHair,
		skinColor,
		eyewear: selectedEyewear,
	} = useSelector((store: IStoreState) => store.character);

	const hairOptions = toArray(hair).map(({ name, thumb }) => ({
		value: name,
		thumb,
	}));

	const facialHairOptions = toArray(facialHair).map(({ name, thumb }) => ({
		value: name,
		thumb,
	}));

	const eyewearOptions = toArray(eyewear).map(({ name, thumb }) => ({
		value: name,
		thumb,
	}));

	return (
		<OptionPanel id="body-panel" aria-labelledby="body" className={clsx("body-options", !selectedFacialHair && "no-facial-hair", !selectedHair?.style && "no-hair")}>
			<OptionStyleSelectable
				thumbColors={selectedHair?.color && selectedHair?.color.palette}
				horizontal={true}
				title="Hair style"
				styles={hairOptions}
				onResetClicked={() => dispatch(setHairStyle(undefined))}
				onStyleClicked={style => dispatch(setHairStyle(style))}
				selectedStyle={selectedHair?.style}
				className="hair-style"
			/>
			<OptionColorSelectable
				horizontal={true}
				colors={hairColors}
				title="Hair Color"
				showTitle={false}
				onColorClicked={color => dispatch(setHairColor(color))}
				withHeader={false}
				withSwatchContainer={false}
				activeColor={selectedHair?.color}
				className="hair-color"
			/>

			<OptionColorSelectable
				title="Skin"
				horizontal={true}
				colors={skinColors}
				onColorClicked={color => dispatch(setSkinColor(color))}
				withSwatchContainer={false}
				activeColor={skinColor}
				className="skin-color"
				withResetButton={false}
			/>

			<OptionStyleSelectable
				thumbColors={selectedFacialHair?.color && selectedFacialHair.color.palette}
				horizontal={true}
				title="Facial hair"
				styles={facialHairOptions}
				onResetClicked={() => dispatch(setFacialHair(undefined))}
				onStyleClicked={style => dispatch(setFacialHair(style))}
				selectedStyle={selectedFacialHair?.style}
				className="facial-hair"
			/>
			<OptionColorSelectable
				title="Facial Hair Color"
				horizontal={true}
				colors={facialHairColors}
				onColorClicked={color => dispatch(setFacialHairColor(color))}
				onResetClicked={() => dispatch(setFacialHairColor(undefined))}
				withHeader={false}
				withSwatchContainer={false}
				activeColor={selectedFacialHair?.color}
				className="facial-hair-color"
			/>
			<OptionStyleSelectable
				horizontal={true}
				title="Glasses"
				styles={eyewearOptions}
				onResetClicked={() => dispatch(setEyewear(undefined))}
				onStyleClicked={style => dispatch(setEyewear(style))}
				selectedStyle={selectedEyewear}
				className="eyewear"
			/>
		</OptionPanel>
	);
};

export default BodyPanel;

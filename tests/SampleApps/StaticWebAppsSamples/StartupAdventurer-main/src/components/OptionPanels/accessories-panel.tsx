import React from "react";
import { OptionPanel } from "./styles";

import accessories from "@/components/CharacterOptions/accessories";
import OptionStyleSelectable from "@/components/Selectables/option-style-selectable";

import toArray from "lodash-es/toArray";
import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";

import { setAccessory } from "@/redux/character/character.actions";

import { IStoreState } from "@/interfaces/IStoreState";

const AccessoriesPanel = () => {
	const dispatch: Dispatch = useDispatch();

	const { accessories: selectedAccessories } = useSelector((store: IStoreState) => store.character);

	const accessoryOptions = toArray(accessories).map(({ name, thumb }) => ({
		value: name,
		thumb,
	}));

	return (
		<OptionPanel id="accessories-panel" aria-labelledby="accessories" className="accessory-options">
			<OptionStyleSelectable
				title="Accessories"
				styles={accessoryOptions}
				onResetClicked={() => dispatch(setAccessory(undefined))}
				onStyleClicked={style => dispatch(setAccessory(style))}
				selectedStyle={selectedAccessories}
				className="accessories"
			/>
		</OptionPanel>
	);
};

export default AccessoriesPanel;

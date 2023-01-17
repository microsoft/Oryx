import React from "react";
import { OptionPanel } from "./styles";
import OptionColorSelectable from "@/components/Selectables/option-color-selectable";
import OptionStyleSelectable from "@/components/Selectables/option-style-selectable";
import bottoms from "@/components/CharacterOptions/bottoms";
import shoes from "@/components/CharacterOptions/shoes";

import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";
import { setBottom, setShoes } from "@/redux/character/character.actions";
import { IStoreState } from "@/interfaces/IStoreState";
import { getColorSet, decideNextSet } from "@/utils/selection-utils";
import { BottomStyle } from "@/interfaces/ICharacter";

const BottomPanel = () => {
	const { pants, maxiSkirt, miniSkirt, prosthetic, kilt, bathrobe, dress } = bottoms;
	const other = [
		{
			value: "kilt",
			thumb: kilt.thumb,
		},
		{
			value: "prosthetic",
			thumb: prosthetic.thumb,
		},
		{
			value: "bathrobe",
			thumb: bathrobe.thumb,
		},
		{
			value: "dress",
			thumb: dress.thumb,
		},
	];

	const shoeOptions = shoes.map((shoe, i: number) => ({
		value: `shoe-${i + 1}`,
		thumb: shoe.thumb,
	}));

	const dispatch: Dispatch = useDispatch();
	const { bottom, shoes: selectedShoes } = useSelector((store: IStoreState) => store.character);

	const getActiveBottomColor = (type: string) => {
		if (bottom && bottom.style === type) {
			return bottom.color
		}
	}

	return (
		<OptionPanel id="bottom-panel" aria-labelledby="bottom" className="bottom-options">
			<OptionColorSelectable
				title="Pants"
				colors={pants.colors}
				onColorClicked={color => dispatch(setBottom("pants", color))}
				onResetClicked={() => dispatch(setBottom("pants", undefined))}
				thumbnail={pants.thumb}
				activeColor={getActiveBottomColor("pants")}
				onThumbClicked={() =>
					dispatch(
						setBottom(
							"pants",
							decideNextSet(getActiveBottomColor("pants"), getColorSet(pants.defaultColor, pants.colors))
						)
					)
				}
			/>
			<OptionColorSelectable
				title="Maxi-skirt"
				colors={maxiSkirt.colors}
				onColorClicked={color => dispatch(setBottom("maxiSkirt", color))}
				onResetClicked={() => dispatch(setBottom("maxiSkirt", undefined))}
				thumbnail={maxiSkirt.thumb}
				activeColor={getActiveBottomColor("maxiSkirt")}
				onThumbClicked={() =>
					dispatch(
						setBottom(
							"maxiSkirt",
							decideNextSet(
								getActiveBottomColor("maxiSkirt"),
								getColorSet(maxiSkirt.defaultColor, maxiSkirt.colors)
							)
						)
					)
				}
			/>
			<OptionColorSelectable
				title="Mini-skirt"
				colors={miniSkirt.colors}
				onColorClicked={color => dispatch(setBottom("miniSkirt", color))}
				onResetClicked={() => dispatch(setBottom("miniSkirt", undefined))}
				thumbnail={miniSkirt.thumb}
				activeColor={getActiveBottomColor("miniSkirt")}
				onThumbClicked={() =>
					dispatch(
						setBottom(
							"miniSkirt",
							decideNextSet(
								getActiveBottomColor("miniSkirt"),
								getColorSet(miniSkirt.defaultColor, miniSkirt.colors)
							)
						)
					)
				}
			/>
			<OptionStyleSelectable<BottomStyle>
				title="Other"
				styles={other}
				onResetClicked={(selected: BottomStyle | BottomStyle[]) =>
					dispatch(setBottom(Array.isArray(selected) ? selected[0] : selected, undefined))
				}
				onStyleClicked={style => dispatch(setBottom(style, { name: style }))}
				selectedStyle={bottom && bottom.style}
			/>
			<OptionStyleSelectable
				title="Shoes"
				styles={shoeOptions}
				onResetClicked={() => dispatch(setShoes(""))}
				onStyleClicked={style => dispatch(setShoes(style))}
				selectedStyle={selectedShoes}
				horizontal={true}
			/>
		</OptionPanel>
	);
};

export default BottomPanel;

import React from "react";
import { OptionPanel } from "./styles";
import OptionColorSelectable from "@/components/Selectables/option-color-selectable";
import tops from "@/components/CharacterOptions/tops";
import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";
import { setTop } from "@/redux/character/character.actions";
import { IStoreState } from "@/interfaces/IStoreState";
import { getColorSet, decideNextSet } from "@/utils/selection-utils";

const TopPanel = () => {
	const { tshirt, shirt, jacket, hoodie } = tops;
	const dispatch: Dispatch = useDispatch();
	const { tshirt: activeTShirt = undefined, shirt: activeShirt, jacket: activeJacket, hoodie: activeHoodie } = useSelector(
		(store: IStoreState) => store.character.tops
	) ?? {};

	return (
		<OptionPanel id="top-panel" aria-labelledby="top">
			<OptionColorSelectable
				activeColor={activeTShirt}
				title="T-shirt"
				colors={tshirt.colors}
				thumbnail={tshirt.thumb}
				onColorClicked={color => dispatch(setTop("tshirt", color))}
				onResetClicked={() => dispatch(setTop("tshirt", undefined))}
				onThumbClicked={() =>
					dispatch(
						setTop("tshirt", decideNextSet(activeTShirt, getColorSet(tshirt.defaultColor, tshirt.colors)))
					)
				}
			/>
			<OptionColorSelectable
				activeColor={activeShirt}
				title="Shirt"
				colors={shirt.colors}
				thumbnail={shirt.thumb}
				onColorClicked={color => dispatch(setTop("shirt", color))}
				onResetClicked={() => dispatch(setTop("shirt", undefined))}
				onThumbClicked={() =>
					dispatch(setTop("shirt", decideNextSet(activeShirt, getColorSet(shirt.defaultColor, shirt.colors))))
				}
			/>
			<OptionColorSelectable
				activeColor={activeJacket}
				title="Jacket"
				colors={jacket.colors}
				thumbnail={jacket.thumb}
				onColorClicked={color => dispatch(setTop("jacket", color))}
				onResetClicked={() => dispatch(setTop("jacket", undefined))}
				onThumbClicked={() =>
					dispatch(
						setTop("jacket", decideNextSet(activeJacket, getColorSet(jacket.defaultColor, jacket.colors)))
					)
				}
			/>
			<OptionColorSelectable
				activeColor={activeHoodie}
				title="Hoodie"
				colors={hoodie.colors}
				thumbnail={hoodie.thumb}
				onColorClicked={color => dispatch(setTop("hoodie", color))}
				onResetClicked={() => dispatch(setTop("hoodie", undefined))}
				onThumbClicked={() =>
					dispatch(
						setTop("hoodie", decideNextSet(activeHoodie, getColorSet(hoodie.defaultColor, hoodie.colors)))
					)
				}
			/>
		</OptionPanel>
	);
};

export default TopPanel;

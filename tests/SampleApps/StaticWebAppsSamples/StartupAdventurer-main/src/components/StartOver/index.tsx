import React from "react";
import { StartOverButton } from "./styles";
import trashcan from "@/icons/trashcan.svg";
import { Dispatch } from "redux";
import { useDispatch } from "react-redux";
import { uiActions } from "@/redux/ui";

const StartOver = () => {
	const dispatch: Dispatch = useDispatch();

	const startOver = () => {
		dispatch(uiActions.startOver());
	};

	return (
		<StartOverButton onClick={startOver}>
			Start over{" "}
			<span>
				<img src={trashcan} alt="trashcan icon" />
			</span>
		</StartOverButton>
	);
};

export default StartOver;

import React from "react";
import { Prev } from "./styles";
import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";
import { uiActions } from "@/redux/ui";
import { IStoreState } from "@/interfaces/IStoreState";

interface IProps {
	disabled?: boolean;
	[key: string]: any;
}

const PrevButton = ({ disabled = false, children, ...restProps }: IProps) => {
	const dispatch: Dispatch = useDispatch();
	const { totalSteps, currentStep } = useSelector((store: IStoreState) => store.ui);
	const navigatePrev = () => {
		/* if user is about to navigate to first screen, show "game over?" screen */
		if (totalSteps > 1 && currentStep === 1) {
			dispatch(uiActions.startOver());
		} else {
			dispatch(uiActions.navigatePrev());
		}
	};

	return (
		<Prev onClick={navigatePrev} disabled={disabled} {...restProps}>
			{!!children ? children : null}
		</Prev>
	);
};

export default PrevButton;

import React from "react";
import { Next } from "./styles";
import { Dispatch } from "redux";
import { useDispatch } from "react-redux";
import { uiActions } from "@/redux/ui";
import arrow from "./arrow-back.svg";

interface IProps {
	disabled?: boolean;
	[key: string]: any;
}

const NextButton = ({ disabled = false, children, beforeNext, ...restProps }: IProps) => {
	const dispatch: Dispatch = useDispatch();
	const navigateNext = async () => {
		if (beforeNext) {
			await beforeNext();
		}
		dispatch(uiActions.navigateNext());
	};

	return (
		<Next onClick={navigateNext} disabled={disabled} {...restProps}>
			{!!children ? children : null}
			<span className="icon">
				<img src={arrow} alt="->" />
			</span>
		</Next>
	);
};

export default NextButton;

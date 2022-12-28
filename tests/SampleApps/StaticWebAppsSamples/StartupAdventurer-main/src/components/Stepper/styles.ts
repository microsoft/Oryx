import styled from "styled-components";
import { colors, buttonShine } from "@/utils/style-utils";
import backArrow from "./arrow-back.svg";

export const StepperNav = styled.div.attrs(() => ({
	className: "stepper-nav",
}))`
	bottom: 232px;
	display: flex;
	justify-content: space-between;
	padding: 0 292px 0 224px;
	position: fixed;
	left: 0;
	width: 100%;
`;

const Button = styled.button`
	align-items: center;
	background-color: ${colors.white};
	border: 7px solid ${colors.lightGrey};
	border-radius: 0;
	box-shadow: 0px 7px 0px ${colors.darkBlue};
	font-size: 60px;
	display: flex;
	height: 176px;
	justify-content: space-between;
	margin: 0;
	padding: 0;

	&:focus {
		outline: 10px solid ${colors.green};
	}

	&:active {
		background-color: ${colors.lightGrey};
	}

	&:disabled {
		color: ${colors.black};
		opacity: 0.6;
		pointer-events: none;
	}
`;

export const Prev = styled(Button).attrs(() => ({
	className: "prev-button",
}))`
	background-image: url(${backArrow});
	background-position: center;
	background-repeat: no-repeat;
	background-size: 70px 49px;
	width: 176px;
`;

export const Next = styled(Button).attrs(() => ({
	className: "next-button",
}))`
	padding: 0 80px;

	.icon {
		display: inline-block;
		margin-left: 40px;
		transform: rotate(180deg);
		vertical-align: center;
		width: 70px;

		img {
			display: block;
			width: 100%;
		}
	}

	&:not([disabled]) {
		${buttonShine}
	}
`;

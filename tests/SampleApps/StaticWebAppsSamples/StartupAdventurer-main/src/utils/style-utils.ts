import { keyframes, css } from "styled-components";

export const colors = {
	base: "#272727",
	black: "#000000",
	darkBlue: "#00547D",
	darkGreen: "#008076",
	darkGrey: "#333333",
	green: "#00a99d",
	grey: "#D6D6D6",
	lightGrey: "#EBEBEB",
	offWhite: "#FAFAFA",
	pink: "#F472D0",
	white: "#FFFFFF",
};

export const responsiveSize = (min: number, max: number, minScreen: number = 992, maxScreen: number = 4500): string => {
	return `calc(${min}px + (${max} - ${min}) * ((100vw - ${minScreen}px) / (${maxScreen} - ${minScreen})))`;
};

const shine = keyframes`
	0% {
		opacity: 0;
		transform: skew(-20deg) translateX(-100%);
	}
	10% {
		opacity: 0.61;
		transform: skew(-20deg) translateX(-100%);
	}
	50% {
		opacity: 0.61;
		transform: skew(-20deg) translateX(calc(100% + 60px));
	}
	51%, 100% {
		opacity: 0;
		transform: skew(-20deg) translateX(100%);
	}
`;

export const buttonShine = css`
	overflow: hidden;
	position: relative;

	&::before {
		animation: ${shine} 4s linear infinite;
		background-image: linear-gradient(
			to right,
			rgba(255, 255, 255, 0) 0px,
			#99feec 180px,
			rgba(255, 255, 255, 0) 180px
		);
		content: "";
		display: block;
		height: 100%;
		opacity: 0.61;
		position: absolute;
		top: 0;
		left: 0;
		transform: skew(-20deg);
		width: 100%;
	}
`;

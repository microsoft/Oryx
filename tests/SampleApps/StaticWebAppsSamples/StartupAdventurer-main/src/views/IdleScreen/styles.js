import styled, { keyframes } from "styled-components";
import { colors } from "@/utils/style-utils";

export const IdleScreenWrapper = styled.div`
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;

	.lights {
		height: 110%;
		left: -50px;
		position: absolute;
		top: -100px;
		width: 120%;
		transform-style: flat;
		transform: none !important;
		overflow: visible;
		backface-visibility: visible;

		&.lights.lights {
			transform: none !important;
		}

		svg {
			height: 100%;
			left: 0;
			position: absolute;
			top: 0;
			width: 100%;

			&.lightbeam {
				opacity: 1;
				mix-blend-mode: soft-light;
			}
		}
	}
`;

export const Spotlight = styled.div`
	background-repeat: no-repeat;
	background-size: cover;
	height: 100%;
	left: -220px;
	position: absolute;
	top: 0;
	width: 100%;
	z-index: 1;
`;

export const IdleScreenContainer = styled.div`
	height: 100%;
	left: 0;
	position: absolute;
	top: 0;
	width: 100%;

	.character-randomizer {
		bottom: 524px;
		left: 383px;
		position: absolute;
		top: auto;
		width: 1468px;
		z-index: 2;
	}
`;

export const Title = styled.h2`
	color: ${colors.pink};
	font-size: 120px;
	font-weight: normal;
	margin: 0 0 100px;
	z-index: 9;
`;

export const LogoContainer = styled.div`
	right: 402px;
	position: absolute;
	text-align: center;
	top: 426px;
	width: 2697px;
	z-index: 9;

	.logo {
		position: relative;
		left: auto;
		right: auto;
		top: 0;
		width: 100%;
	}
`;

const blink = keyframes`
	50% {
		opacity: 0;
	}
`;

export const Quide = styled.p`
	animation: ${blink} 1s step-end infinite;
	color: ${colors.white};
	font-size: 60px;
	margin: 0;
	text-align: center;
`;

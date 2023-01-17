import styled from "styled-components";
import { colors } from "@/utils/style-utils";

export const Spotlight = styled.div`
	backface-visibility: visible;
	height: 112%;
	left: -95px;
	overflow: visible;
	position: absolute;
	top: -233px;
	transform-style: flat;
	transform: none !important;
	width: 133%;

	svg {
		height: 100%;
		left: 0;
		position: absolute;
		right: 0;
		top: 0;
		width: 100%;

		&.lightbeam {
			mix-blend-mode: soft-light;
		}
	}
`;

export const StatsWrapper = styled.div`
	color: ${colors.white};
	display: flex;
	flex-wrap: wrap;
	height: 100%;
	justify-content: space-between;
	left: 0;
	padding: 170px 292px 170px 224px;
	position: absolute;
	top: 0;
	width: 100%;
	z-index: 3;
`;

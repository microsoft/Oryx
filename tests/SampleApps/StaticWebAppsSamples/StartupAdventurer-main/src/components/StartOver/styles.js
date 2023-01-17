import styled from "styled-components";
import { colors } from "@/utils/style-utils";

export const StartOverButton = styled.button.attrs(() => ({
	className: "start-over-button hoverable",
}))`
	-webkit-appearance: none;
	-moz-appearance: none;
	background: rgba(0, 0, 0, 0);
	border: 0;
	color: ${colors.white};
	display: block;
	font-size: 60px;
	padding: 30px;
	position: fixed;
	top: 140px;
	right: 262px;
	z-index: 99;

	&:focus {
		outline: 10px solid ${colors.green};
	}

	span {
		display: inline-block;
		margin-left: 30px;
		vertical-align: middle;
		width: 63px;
	}

	img {
		display: block;
		width: 100%;
	}
`;

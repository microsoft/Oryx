import styled, { keyframes } from "styled-components";
import { colors } from "@/utils/style-utils";

const warn = keyframes`
	0%, 100% {
		transform: scale(1) translateY(0);
	}
	50% {
		transform: scale(1.05) translateY(-40px);
	}
`;

export const Remaining = styled.p`
	bottom: 80px;
	color: ${colors.black};
	font-size: 40px;
	background: ${colors.white};
	box-shadow: 0px 21px 0px ${colors.darkBlue};
	padding: 50px;
	text-align: center;
	position: fixed;
	right: 80px;
	z-index: 99999;

	&.critical {
		animation: ${warn} 500ms linear forwards;
		animation-iteration-count: 2;
	}
`;

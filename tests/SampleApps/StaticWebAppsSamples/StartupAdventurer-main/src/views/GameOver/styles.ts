import styled from "styled-components";
import { colors } from "@/utils/style-utils";
import bg from "@/graphics/background.svg";

export const GameOverContainer = styled.div`
	align-items: center;
	background-image: url(${bg});
	background-repeat: no-repeat;
	background-size: cover;
	display: flex;
	flex-direction: column;
	height: 100%;
	justify-content: center;
	left: 0;
	position: fixed;
	top: 0;
	width: 100%;
	z-index: 9999;
`;

export const TitleContainer = styled.div`
	margin: 0 0 296px;
`;

export const GameOverButtons = styled.div`
	align-items: center;
	display: flex;
	justify-content: center;
`;

interface IButtonProps {
	primary?: boolean;
}

export const Button = styled.button<IButtonProps>`
	align-items: center;
	background-color: ${props => (props.primary ? colors.green : colors.white)};
	border: 7px solid ${props => (props.primary ? colors.darkGreen : colors.lightGrey)};
	border-radius: 0;
	box-shadow: 0px 7px 0px ${colors.darkBlue};
	color: ${props => (props.primary ? colors.white : colors.black)};
	font-size: 60px;
	display: flex;
	height: 176px;
	justify-content: center;
	margin: 0 45px;
	outline: none;
	padding: 0 40px;
	width: 1090px;

	&:active {
		background-color: ${props => (props.primary ? colors.darkGreen : colors.lightGrey)};
	}
`;

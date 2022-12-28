import styled, { css } from "styled-components";
import { colors } from "@/utils/style-utils";
import DropdownAria  from "react-dropdown-aria"

interface IDropdownProps {
	readonly isOpen?: boolean;
	[key: string]: any;
}

export const Dropdown = styled.div.attrs(() => ({
	className: "dropdown",
}))<IDropdownProps>`
	position: relative;
	z-index: ${props => (props.isOpen ? 9 : 1)};
`;

export const StyledDropdown = styled(DropdownAria)`
	font-size: 60px;
	height: 176px;
`

export const Button = styled.button`
	align-items: center;
	border: 7px solid ${colors.lightGrey};
	border-radius: 0;
	display: flex;
	font-size: 60px;
	height: 176px;
	justify-content: space-between;
	line-height: 1;
	margin: 0;
	padding: 48px 73px 55px;
	position: relative;
	outline: 0;

	span {
		display: inline-block;

		&.arrow {
			position: absolute;
			right: 80px;
			top: 50%;
			transform: translateY(-50%);
			z-index: 9;
		}

		img {
			display: block;
			height: auto;
			margin-left: 50px;
			width: 70px;
		}
	}
`;

interface IToggleProps {
	hasSelection?: boolean;
}

export const DropdownToggle = styled(Button).attrs(() => ({
	className: "dropdown-toggle",
}))<IToggleProps>`
	background: ${props => (props.hasSelection ? colors.white : colors.darkGrey)};
	box-shadow: 0px 21px 0px ${colors.darkBlue};
	color: ${props => (props.hasSelection ? colors.black : colors.white)};
	padding-right: 300px;
	z-index: 2;
	white-space: nowrap;

	&::after {
		background: linear-gradient(
			to left,
			${props => (props.hasSelection ? colors.white : colors.darkGrey)} 70%,
			${props => (props.hasSelection ? "rgba(255,255,255, 0)" : "rgba(51, 51, 51, 0)")} 100%
		);
		content: "";
		height: 100%;
		pointer-events: none;
		position: absolute;
		right: 0;
		top: 0;
		width: 300px;
		z-index: 5;
	}

	${props =>
		props.hasSelection &&
		css`
			img {
				filter: invert(1);
			}
		`}
`;

export const DropdownContent = styled.div.attrs(() => ({
	className: "dropdown-content",
}))`
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	box-shadow: 0px 21px 0px ${colors.darkBlue};
	left: 0;
	max-height: 1160px;
	min-width: 100%;
	overflow-y: auto;
	overscroll-behavior: none;
	position: absolute;
	top: 0;
	white-space: nowrap;
	z-index: 3;

	--scrollbar-width: 40px;
	--scrollbar-padding: 30px;

	::-webkit-scrollbar {
		background-color: rgba(255, 255, 255, 0);
		width: calc((var(--scrollbar-padding) * 2) + var(--scrollbar-width));
	}

	::-webkit-scrollbar-track,
	::-webkit-scrollbar-thumb {
		background-clip: padding-box;
		border: 32px solid rgba(255, 255, 255, 0);
	}

	::-webkit-scrollbar-track {
		background-color: ${colors.lightGrey};
		background-image: linear-gradient(to right, ${colors.grey} 0px, ${colors.grey} 40px),
			linear-gradient(to bottom, ${colors.grey} 0px, ${colors.grey} 100%),
			linear-gradient(to bottom, ${colors.grey} 0px, ${colors.grey} 100%),
			linear-gradient(to right, ${colors.grey} 0px, ${colors.grey} 40px);
		background-size: 100% 7px, 7px 100%, 7px 100%, 100% 7px;
		background-position: 0 0, 0 0, 100% 0, 0 100%;
		background-repeat: no-repeat;
	}

	::-webkit-scrollbar-thumb {
		background-color: ${colors.black};
		width: calc(100% - 14px);
	}
`;

interface IOptionProps {
	readonly isActive?: boolean;
}

export const Option = styled(Button)<IOptionProps>`
	background: ${props => (props.isActive ? colors.darkBlue : colors.white)};
	border: 0;
	border-bottom: 7px solid ${colors.grey};
	color: ${props => (props.isActive ? colors.white : colors.black)};
	width: 100%;

	&:last-child {
		border-bottom: 0;
	}
`;

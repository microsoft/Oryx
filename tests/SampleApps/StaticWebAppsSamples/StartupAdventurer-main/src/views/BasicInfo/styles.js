import styled from "styled-components";
import { colors } from "@/utils/style-utils";

export const BasicInfoContainer = styled.div`
	left: 0;
	margin: auto;
	opacity: 0;
	position: absolute;
	right: 0;
	top: 599px;
	width: 3102px;
`;

export const dropdownStyle = {
	DropdownButton: (base, { internalSelectedOption, open }) => ({
		...base,
		fontSize: "60px",
		height: "176px",
		background: internalSelectedOption ? colors.white : colors.darkGrey,
		boxShadow: `0px 21px 0px ${colors.darkBlue}`,
		color: internalSelectedOption ? colors.black : colors.white,
		padding: "48px 73px 55px",
		border: open ? `7px solid ${colors.green}` : `7px solid ${colors.lightGrey}`,
		lineHeight: 1,
		margin: 0,
		'&:hover': {},
		'&:focus': {
			border: `7px solid ${colors.green}`
		},

		img: {
			filter: internalSelectedOption ? "invert(1)" : undefined
		}
	}),
	DisplayedValue: (base, { internalSelectedOption }) => ({
		...base,
		color: internalSelectedOption ? colors.black : colors.white,
		borderRight: undefined
	}),

	Arrow: (base, { open }) => ({
		filter: "invert(1)"
	}),

	OptionContainer: (base) => ({
		...base,
		maxHeight: "1160px",
		overflowY: "auto",

		overscrollBehavior: "none",
		'--scrollbar-width': "40px",
		'--scrollbar-padding': "30px",

		'::-webkit-scrollbar': {
			backgroundColor: "rgba(255, 255, 255, 0)",
			width: "calc((var(--scrollbar-padding) * 2) + var(--scrollbar-width))"
		},

		'::-webkit-scrollbar-track, ::-webkit-scrollbar-thumb': {
			backgroundClip: "padding-box",
			border: "32px solid rgba(255, 255, 255, 0)"
		},

		'::-webkit-scrollbar-track': {
			backgroundColor: colors.lightGrey,
			backgroundImage: `linear-gradient(to right, ${colors.grey} 0px, ${colors.grey} 40px),
			linear-gradient(to bottom, ${colors.grey} 0px, ${colors.grey} 100%),
			linear-gradient(to bottom, ${colors.grey} 0px, ${colors.grey} 100%),
			linear-gradient(to right, ${colors.grey} 0px, ${colors.grey} 40px)`,
			backgroundSize: "100% 7px, 7px 100%, 7px 100%, 100% 7px",
			backgroundPosition: "0 0, 0 0, 100% 0, 0 100%",
			backgroundRepeat: "no-repeat"
		},

		'::-webkit-scrollbar-thumb': {
			backgroundColor: colors.black,
			width: "calc(100% - 14px)"
		}
	}),

	OptionItem: (base, _state, extras) => ({
		...base,
		fontSize: "60px",
		height: "176px",
		padding: "48px 73px 55px",
		margin: 0,
		backgroundColor: (extras && extras.selected) ? colors.darkBlue : colors.white,
		border: 0,
		borderBottom: `7px solid ${colors.grey}`,
		color: (extras && extras.selected) ? colors.white : colors.black,
		width: "100%",

		'&:last-child': {
			borderBottom: 0
		}
	})


}

export const Title = styled.h1`
	color: ${colors.white};
	font-size: 190px;
	font-weight: normal;
	margin: 0 0 339px;
	text-align: center;
`;

export const InfoColumns = styled.div`
	display: flex;
	justify-content: space-between;
	margin-bottom: 319px;

	&:last-of-type {
		margin-bottom: 0;
	}
`;

export const InfoColumn = styled.div`
	display: flex;
	flex-direction: column;

	&:first-child {
		width: 1645px;
	}

	&:last-child {
		width: 1097px;
	}

	.dropdown-toggle {
		width: 100%;
	}
`;

export const FieldTitle = styled.h3`
	color: ${colors.white};
	font-size: 50px;
	line-height: 1;
	margin: 0 0 42px;
	text-transform: uppercase;
`;

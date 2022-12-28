import styled from "styled-components";
import { colors } from "@/utils/style-utils";
import clsx from "clsx";

export const OptionsContainer = styled.div`
	display: flex;
	margin-top: 236px;
`;

export const Tabs = styled.div`
	width: 713px;

	.tab-button {
		background: ${colors.black};
		border: 7px solid ${colors.darkGrey};
		color: ${colors.white};
		font-size: 60px;
		margin-bottom: 28px;
		opacity: 0;
		padding: 43px 63px;
		text-align: left;
		width: 100%;

		&--active {
			background: ${colors.white};
			border-color: ${colors.grey};
			color: ${colors.black};
		}

		&:focus {
			outline: 10px solid ${colors.green}
		}
	}
`;

export const OptionPanel = styled.div.attrs(props => ({
	className: clsx("option-panel", props.className),
	tabIndex: 0,
	role: "tabpanel"
}))`
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	color: ${colors.black};
	height: 1968px;
	opacity: 0; /* this will be set to 1 by anime */
	padding: 100px 0;
	width: 1504px;
	margin-left: -7px;
	z-index: 9;

	&.bottom-options {
		.options-wrapper {
			margin-bottom: 20px;
		}
	}

	&.body-options .options-wrapper {
		margin-bottom: 75px;
	}

	.hair-style + .hair-color,
	.facial-hair + .facial-hair-color {
		margin-top: -40px;
	}

	.options-wrapper.carry-on,
	.options-wrapper.in-hand,
	.options-wrapper.neck {
		.options-container::after {
			background: ${colors.offWhite};
			content: "";
			display: block;
			flex: 1 0 auto;
		}
	}

	/* disable color picking if no hair or facial hair is selected */
	&.no-facial-hair {
		.facial-hair-color {
			button {
				pointer-events: none;
			}
			.is-active-button {
				border-color: ${colors.lightGrey};
			}
		}
	}
	&.no-hair {
		.hair-color {
			button {
				pointer-events: none;
			}
			.is-active-button {
				border-color: ${colors.lightGrey};
			}
		}
	}

	&.accessory-options {
		.options-container {
			flex-wrap: wrap;

			.style-thumbnail {
				margin-bottom: 28px;
			}

			.style-thumbnail:nth-of-type(5n) {
				margin-right: 0;
			}
		}
	}
`;

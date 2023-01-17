import styled, { keyframes } from "styled-components";
import { colors } from "@/utils/style-utils";
import bg from "@/graphics/background.svg";

export const EndScreenWrapper = styled.div`
	align-items: center;
	display: flex;
	height: 100%;
	justify-content: center;
	left: 0;
	position: absolute;
	top: 0;
	width: 100%;

	.dimmer {
		background: rgba(0, 0, 0, 0.6);
		height: 100%;
		left: 0;
		pointer-events: none;
		position: fixed;
		top: 0;
		width: 100%;
		z-index: 50;
	}

	.stepper-nav {
		bottom: 273px;
		margin: auto;
		max-width: 3125px;
		padding-left: 13px;
		right: 0;

		.progress-bar {
			align-items: center;
			margin-left: 0;

			&::after {
				color: ${colors.white};
				content: "Game completed!";
				display: inline-block;
				font-size: 50px;
				margin-left: 90px;
				vertical-align: middle;
			}
		}

		.prev-button {
			position: fixed;
			opacity: 0.4;
			top: 0;
			left: 0;
		}
		.next-button {
			display: none;
		}
	}
`;

export const EndScreenContainer = styled.div`
	align-items: center;
	display: flex;
	justify-content: space-between;
	width: 3125px;
`;

export const CharacterCard = styled.div`
	background-color: ${colors.white};
	border: 7px solid ${colors.grey};
	height: 2048px;
	padding: 70px 70px 20px;
	width: 1504px;
`;

export const CharacterArea = styled.div`
	background-clip: content-box;
	background-image: url(${bg});
	background-repeat: no-repeat;
	background-size: 217%;
	border: 7px solid ${colors.grey};
	height: 1354px;
	margin-bottom: 85px;
	opacity: 0;
	overflow: hidden;
	position: relative;
	width: 100%;

	.lights {
		height: 125%;
		left: 0px;
		position: absolute;
		top: -104px;
		width: 100%;

		svg {
			height: 100%;
			left: 0;
			position: absolute;
			width: auto;
		}

		.lightbeam {
			mix-blend-mode: soft-light;
		}
	}

	.character-container {
		filter: none;
		height: 100%;
		left: 0;
		margin: auto;
		position: absolute;
		right: 0;
		top: 15px;
		width: 65%;
	}
`;

export const Stats = styled.div`
	display: flex;
	flex-wrap: wrap;
	justify-content: space-between;

	.skill-container {
		margin-bottom: 54px;
		opacity: 0;
		padding: 38px 0;
		position: relative;
		width: calc((100% - 54px) / 2);

		.grading-buttons {
			display: none;
		}

		.skill-icon {
			flex: 0 0 80px;
			height: 80px;
			width: 80px;
		}

		.skill-name {
			font-size: 50px;
			letter-spacing: -2px;
			line-height: 1;
		}

		.skill-points {
			background-color: ${colors.white};
			font-size: 60px;
			margin: 0;
			padding: 0 20px;
			position: absolute;
			top: -6px;
			right: 20px;
			transform: translateY(-50%);
		}
	}
`;

export const Stat = styled.div`
	border: 7px solid ${colors.grey};
	padding: 38px 36px;
`;

const bounce = keyframes`
	0%   { transform: translateY(0); }
	10%  { transform: translateY(-20px); }
	30%  { transform: translateY(100px); }
	50%  { transform: translateY(0); }
	57%  { transform: translateY(7px); }
	64%  { transform: translateY(0); }
	100% { transform: translateY(0); }
`;

export const CtaArea = styled.div.attrs(() => ({
	className: "cta-area",
}))`
	h1 {
		color: ${colors.white};
		font-size: 100px;
		font-weight: normal;
		margin-top: -180px;
		text-align: center;
	}

	p {
		color: ${colors.white};
		font-size: 60px;
		margin-bottom: 195px;
		text-align: center;

		&.cta-text {
			background: ${colors.white};
			color: ${colors.black};
			padding: 10px 25px;
		}
	}

	#arrow-down {
		animation: ${bounce} 2s cubic-bezier(0.28, 0.84, 0.42, 1) infinite;
		display: block;
		margin: 0 auto;
	}
`;

export const QRCodeContainer = styled.div`
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	margin: 141px auto 0;
	width: max-content;

	img {
		height: 739px;
		width: 739px;
	}
`;

export const EndButton = styled.button`
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	display: block;
	font-size: 60px;
	line-height: 1;
	margin: 80px auto 0;
	padding: 30px 30px 38px;
	width: 739px;
`;

const size = 30;
const spinnerColor = "#4ffcda";

const spin = keyframes`
	0% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	6.25% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	12.5% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	18.75% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	25% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	31.25% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	37.5% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
	43.75% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px transparent;
	}
	50% {
		box-shadow:
			0px ${-3 * size}px transparent,
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px ${spinnerColor},
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	56.25% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px transparent,
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px ${spinnerColor},
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	62.5% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px transparent,
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px ${spinnerColor},
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	68.75% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px transparent,
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px ${spinnerColor},
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	75% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px transparent,
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px ${spinnerColor},
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	81.25% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px transparent,
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px ${spinnerColor},
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	87.5% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px transparent,
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px ${spinnerColor},
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	93.75% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px transparent,
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px ${spinnerColor};
	}
	100% {
		box-shadow:
			0px ${-3 * size}px ${spinnerColor},
			${size}px ${-3 * size}px ${spinnerColor},
			${2 * size}px ${-2 * size}px ${spinnerColor},
			${3 * size}px ${-1 * size}px ${spinnerColor},
			${3 * size}px 0px ${spinnerColor},
			${3 * size}px ${size}px ${spinnerColor},
			${2 * size}px ${2 * size}px ${spinnerColor},
			${size}px ${3 * size}px ${spinnerColor},
			0px ${3 * size}px transparent,
			${-1 * size}px ${3 * size}px transparent,
			${-2 * size}px ${2 * size}px transparent,
			${-3 * size}px ${size}px transparent,
			${-3 * size}px 0px transparent,
			${-3 * size}px ${-1 * size}px transparent,
			${-2 * size}px ${-2 * size}px transparent,
			${-1 * size}px ${-3 * size}px transparent;
	}
`;

export const SpinnerContainer = styled.div.attrs(() => ({
	className: "spinner-container",
}))`
	align-items: center;
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	display: flex;
	height: 739px;
	justify-content: center;
	margin: 141px auto 0;
	position: relative;
	width: 739px;

	&::after {
		bottom: 45px;
		color: ${colors.black};
		content: "Generating QR code";
		display: block;
		font-size: 50px;
		position: absolute;
		text-align: center;
		width: 100%;
	}
`;

export const Spinner = styled.div`
	animation: ${spin} 1s linear infinite;
	height: ${size}px;
	margin-bottom: 80px;
	width: ${size}px;
`;

export const PublicUrl = styled.a`
	color: #ffffff;
	font-family: "Roboto", Arial, Helvetica, sans-serif;
	font-style: normal;
	font-weight: 500;
	font-size: 70px;
	line-height: 82px;
	margin: 120px auto -95px !important;
	text-align: center;
`;

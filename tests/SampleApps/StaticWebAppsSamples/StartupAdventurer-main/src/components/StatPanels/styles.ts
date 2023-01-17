import styled, { css, keyframes } from "styled-components";
import { colors } from "@/utils/style-utils";

export const StatsContainer = styled.div`
	display: flex;
	margin-top: 236px;
`;

export const Tabs = styled.div.attrs(() => ({
	className: "stat-panel-tabs",
}))`
	width: 713px;

	.tab-button {
		background: ${colors.black};
		border: 7px solid ${colors.darkGrey};
		color: ${colors.white};
		font-size: 60px;
		margin-bottom: 28px;
		opacity: 0;
		outline: 0;
		padding: 43px 63px;
		text-align: left;
		width: 100%;

		&--active {
			background: ${colors.white};
			border-color: ${colors.lightGrey};
			color: ${colors.black};
		}

		&[disabled] {
			background: ${colors.darkGrey};
			color: rgba(255, 255, 255, 0.2);
			pointer-events: none;
		}
	}
`;

interface IStatsPanelProps {
	className?: string;
}

export const StatsPanel = styled.div.attrs(({ className }) => ({
	className: "stats-panel" + (className ? " " + className : ""),
}))<IStatsPanelProps>`
	background: ${colors.white};
	border: 7px solid ${colors.grey};
	color: ${colors.black};
	height: 1968px;
	padding: 100px 0;
	position: relative;
	width: 1504px;
	margin-left: -7px;
	z-index: 9;

	:not(.stats-panel-distribute) {
		> *:not(.distribute-text) {
			opacity: 0;
		}
	}
`;

const bounce = keyframes`
	0%, 100% {
		transform: translate(0, 0);
	}
	50% {
		transform: translate(-30px, -30px);
	}
`;

export const Title = styled.h2`
	border: 0;
	background: transparent;
	color: ${colors.black};
	display: block;
	font-size: 60px;
	font-weight: normal;
	margin: 30px auto 92px;
	text-align: center;

	span.icon {
		animation: ${bounce} 350ms linear forwards;
		animation-iteration-count: 2;
		display: inline-block;
		margin-right: 30px;
		vertical-align: middle;
	}
`;

export const CategoryTabs = styled.div`
	display: flex;
	justify-content: space-between;
	padding: 0 92px 30px;
	position: relative;

	.tab-button {
		align-items: center;
		background: rgba(255, 255, 255, 0);
		border: 0;
		color: ${colors.black};
		display: flex;
		flex-direction: column;
		font-size: 45px;
		letter-spacing: -0.05em;
		outline: 0;
		opacity: 0.5;
		padding: 0;
		text-align: center;
		text-transform: uppercase;

		&--active {
			opacity: 1;
		}

		span.count {
			font-size: 60px;
			pointer-events: none;

			&.count-active {
				color: ${colors.green};
			}
		}
	}
`;

export const TabIndicator = styled.span`
	background: ${colors.black};
	bottom: 0;
	height: 7px;
	left: 0;
	opacity: 0;
	position: absolute;
	transition: opacity 150ms 150ms, transform 250ms;
	transform-origin: 0% 0%;
	width: 300px;
`;

export const OptionList = styled.div`
	border: 7px solid ${colors.grey};
	margin: 0 auto;
	max-height: 1210px;
	overflow-y: auto;
	width: 1306px;

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
	disabled?: boolean;
	selected?: boolean;
}

export const Option = styled.button<IOptionProps>`
	align-items: center;
	background-color: ${props => (props.selected ? colors.lightGrey : colors.white)};
	border-color: transparent;
	border-bottom: 7px solid ${colors.grey};
	display: flex;
	font-size: 50px;
	margin: 0;
	opacity: ${props => (props.disabled ? 0.5 : 1)};
	outline: 0;
	padding: 64px 50px 63px;
	pointer-events: ${props => (props.disabled ? "none" : "auto")};
	text-align: left;
	width: 100%;

	&:active {
		background-color: ${colors.lightGrey};
	}

	&:last-of-type {
		border-bottom: 0;
	}

	.check {
		height: 49px;
		margin-right: 24px;
		width: 70px;

		svg {
			width: 100%;
		}
	}

	${props =>
		!props.selected &&
		css`
			.check {
				filter: grayscale(1);
				opacity: 0.4;
			}
		`}
`;

export const Grading = styled.div`
	margin: 0 auto;
	width: 1306px;
`;

export const Remaining = styled.p`
	bottom: 45px;
	color: ${colors.black};
	font-size: 38px;
	left: 95px;
	margin: 0;
	position: absolute;
`;

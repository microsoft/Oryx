import styled from "styled-components";
import { colors } from "@/utils/style-utils";

export const GraderContainer = styled.div.attrs(() => ({
	className: "skill-container",
}))`
	align-items: center;
	border: 7px solid ${colors.lightGrey};
	display: flex;
	margin-bottom: 80px;
	width: 100%;
`;

export const GradingButtons = styled.div.attrs(() => ({
	className: "grading-buttons",
}))`
	display: flex;
	flex-direction: column;
	margin-left: auto;
`;

export const GradingButton = styled.button`
	align-items: center;
	background: ${colors.black};
	border: 0;
	color: ${colors.white};
	display: flex;
	height: 140px;
	justify-content: center;
	margin: 8px 8px 0px;
	outline: 0;
	padding: 0;
	position: relative;
	width: 135px;

	&:last-of-type {
		margin-bottom: 8px;
	}

	&[disabled] {
		opacity: 0.5;
		pointer-events: none;
	}

	&.plus {
		&::before,
		&::after {
			background: ${colors.white};
			content: "";
			display: block;
			height: 14px;
			left: 50%;
			position: absolute;
			top: 50%;
			transform: translate(-50%, -50%);
			width: 63px;
		}

		&::after {
			transform: translate(-50%, -50%) rotate(90deg);
		}
	}

	&.minus {
		&::before {
			background: ${colors.white};
			content: "";
			display: block;
			height: 14px;
			left: 50%;
			position: absolute;
			top: 50%;
			transform: translate(-50%, -50%);
			width: 63px;
		}
	}
`;

export const SkillIcon = styled.span`
	flex: 0 0 150px;
	height: 150px;
	margin-left: 39px;
	width: 150px;
`;

export const SkillName = styled.span.attrs(() => ({
	className: "skill-name",
}))`
	color: ${colors.black};
	display: block;
	flex-grow: 1;
	font-size: 60px;
	padding: 0 43px 0 43px;
`;

export const Points = styled.span.attrs(() => ({
	className: "skill-points",
}))`
	color: ${colors.black};
	display: inline-block;
	font-size: 120px;
	margin-left: auto;
	margin-right: 77px;
`;

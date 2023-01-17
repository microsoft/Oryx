import styled from "styled-components";
import { colors } from "~/src/utils/style-utils";

export const SliderContainer = styled.div`
	padding: 0 50px;
	position: relative;
	margin-top: 70px;

	span.spacer {
		display: block;
		height: 50px;
		position: absolute;
		top: 0;
		width: 105px;
		z-index: 1;

		&.blue {
			background-color: ${colors.darkBlue};
			left: 0;
			height: 52px;
			top: -1px;
		}
		&.white {
			background-color: ${colors.white};
			right: 0;
		}
	}

	--touch-extra: 70px;
	--track-height: 52px;
	--color: ${colors.darkBlue};

	.slider-touchable-track {
		background-clip: padding-box;
		background-image: linear-gradient(
			to bottom,
			rgba(0, 84, 125, 0) 0px,
			rgba(0, 84, 125, 0) var(--touch-extra, 40px),
			var(--color, ${colors.white}) var(--touch-extra, 40px),
			var(--color, ${colors.white}) calc(var(--touch-extra, 40px) + var(--track-height, 52px)),
			rgba(0, 84, 125, 0) calc(var(--touch-extra, 40px) + var(--track-height, 52px)),
			rgba(0, 84, 125, 0) calc(var(--touch-extra, 40px) * 2 + var(--track-height, 52px))
		);
		box-sizing: content-box;
		cursor: pointer;
		height: var(--track-height, 52px);
		padding-bottom: var(--touch-extra, 40px);
		padding-top: var(--touch-extra, 40px);
		position: absolute;
		top: 50%;
		transform: translateY(-50%);
		width: 100%;
	}

	.slider-rail {
		--color: ${colors.white};
	}

	.slider-track {
		--color: ${colors.darkBlue};
	}

	.slider-handles {
		position: relative;
		top: 50%;
		transform: translateY(-50%);
		z-index: 5;
	}

	.slider {
		margin: auto;
		height: 50px;
		position: relative;
		/* touch-action: none; */
		width: calc(100% - 100px);
		z-index: 3;

		.tick {
			background-color: rgb(200, 200, 200);
			height: 80px;
			position: absolute;
			top: -15px;
			transform: translateX(-50%);
			width: 14px;

			&::after {
				color: ${colors.white};
				display: block;
				font-size: 50px;
				left: 50%;
				position: absolute;
				top: calc(100% + 30px);
				transform: translateX(-50%);
			}

			&:nth-of-type(1)::after {
				content: "5";
			}
			&:nth-of-type(2)::after {
				content: "10";
			}
			&:nth-of-type(3)::after {
				content: "25";
			}
			&:nth-of-type(4)::after {
				content: "50";
			}
			&:nth-of-type(5)::after {
				content: "100";
			}
			&:nth-of-type(6)::after {
				content: "100+";
			}
		}

		.slider-ticks {
			position: relative;
			z-index: 3;
		}

		.is-touching .slider-handle .tooltip {
			display: block;
		}

		.slider-handle {
			&:focus {
				outline: 10px solid ${colors.green};
			}
			.tooltip {
				background: ${colors.white};
				border: 7px solid ${colors.lightGrey};
				color: ${colors.black};
				display: none;
				font-size: 60px;
				left: 50%;
				padding: 40px 45px;
				position: absolute;
				bottom: calc(100% + 50px);
				transform: translateX(-50%);
				white-space: nowrap;

				.tooltip-tip {
					top: calc(100% - 10px);
					display: inline-block;
					left: 50%;
					position: absolute;
					transform: translateX(-50%) rotate(180deg);
				}
			}
		}
	}
`;

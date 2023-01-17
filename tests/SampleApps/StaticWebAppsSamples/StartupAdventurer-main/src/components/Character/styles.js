import styled from "styled-components";
import clsx from "clsx";

export const CharacterContainer = styled.div.attrs(props => ({
	className: clsx("character-container"),
}))`
	filter: brightness(0);
	height: 2310px;
	margin-left: auto;
	position: relative;
	right: 40px;
	top: 110px;
	width: 1530px;

	.stats & {
		filter: brightness(1);
	}

	> svg {
		display: block;
		height: 100%;
		left: 0;
		top: 0;
		width: 100%;
	}

	#character-base {
		position: relative;
		z-index: 0;
	}
`;

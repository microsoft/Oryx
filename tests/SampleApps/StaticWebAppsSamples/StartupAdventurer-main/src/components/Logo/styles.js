import styled from "styled-components";

export const LogoContainer = styled.div`
	height: 0;
	padding-bottom: 56.25%;
	position: relative;

	svg {
		display: block;
		height: 100%;
		left: 0;
		position: absolute;
		top: 0;
		width: 100%;
	}
`;

export const LogoWrapper = styled.div.attrs(() => ({
	className: "logo",
}))`
	overflow: hidden;
`;

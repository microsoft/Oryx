import styled from "styled-components";
import { colors } from "@/utils/style-utils";

export const ProgressBar = styled.div`
	height: 89px;
	display: flex;
	margin-left: 300px;

	span {
		background: ${colors.white};
		display: inline-block;
		flex: 1 0 auto;
		margin-right: 25px;

		&:last-of-type {
			margin-right: 0;
		}
	}
`;

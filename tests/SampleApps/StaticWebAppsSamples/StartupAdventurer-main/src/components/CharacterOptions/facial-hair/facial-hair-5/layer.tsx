import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair5 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="facialHair_5" data-name="facialHair_5">
			<polygon
				fill={color1}
				points="450.94 230 450.94 240 450.94 250 460.94 250 460.94 240 460.94 230 450.94 230"
			/>
			<polygon
				fill={color1}
				points="440.94 240 430.94 240 420.94 240 410.94 240 400.94 240 400.94 250 390.94 250 390.94 260 400.94 260 410.94 260 420.94 260 430.94 260 440.94 260 450.94 260 450.94 250 440.94 250 440.94 240"
			/>
			<polygon
				fill={color1}
				points="390.94 230 380.94 230 380.94 240 380.94 250 390.94 250 390.94 240 390.94 230"
			/>
			<polygon
				fill={color2}
				points="430.95 240 420.95 240 420.95 250 430.95 250 440.95 250 440.95 240 430.95 240"
			/>
			<polygon
				fill={color3}
				points="420.95 250 420.95 240 410.95 240 400.95 240 400.95 250 390.95 250 390.95 260 400.95 260 410.95 260 420.95 260 430.95 260 430.95 250 420.95 250"
			/>
			<polygon
				fill={color3}
				points="390.95 230 380.95 230 380.95 240 380.95 250 390.95 250 390.95 240 390.95 230"
			/>
		</g>
	);
};

export default FacialHair5;

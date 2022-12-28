import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair2 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="facialHair_2" data-name="facialHair_2">
			<polygon
				fill={color1}
				points="430.94 260 420.94 260 410.94 260 400.94 260 400.94 270 400.94 280 410.94 280 410.94 290 420.94 290 420.94 300 430.94 300 430.94 290 440.94 290 440.94 280 450.94 280 450.94 270 450.94 260 440.94 260 430.94 260"
			/>
			<polygon
				fill={color1}
				points="440.94 240 430.94 240 420.94 240 410.94 240 400.94 240 400.94 250 410.94 250 420.94 250 430.94 250 440.94 250 450.94 250 450.94 240 440.94 240"
			/>
			<polygon
				fill={color2}
				points="400.95 270 400.95 280 410.95 280 410.95 270 410.95 260 400.95 260 400.95 270"
			/>
			<polygon
				fill={color2}
				points="410.95 240 400.95 240 400.95 250 410.95 250 420.95 250 420.95 240 410.95 240"
			/>
			<polygon
				fill={color3}
				points="420.95 260 410.95 260 410.95 270 410.95 280 410.95 290 420.95 290 420.95 300 430.95 300 430.95 290 430.95 280 440.95 280 440.95 270 440.95 260 430.95 260 420.95 260"
			/>
			<polygon
				fill={color3}
				points="430.95 240 420.95 240 420.95 250 430.95 250 440.95 250 440.95 240 430.95 240"
			/>
		</g>
	);
};

export default FacialHair2;

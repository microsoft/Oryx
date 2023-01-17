import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair1 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="facialHair_1" data-name="facialHair_1">
			<polygon
				fill={color1}
				points="440.94 240 430.94 240 420.94 240 410.94 240 400.94 240 400.94 250 410.94 250 420.94 250 430.94 250 440.94 250 450.94 250 450.94 240 440.94 240"
			/>
			<polygon
				fill={color2}
				points="430.95 240 420.95 240 420.95 250 430.95 250 440.95 250 440.95 240 430.95 240"
			/>
			<polygon
				fill={color3}
				points="410.95 240 400.95 240 400.95 250 410.95 250 420.95 250 420.95 240 410.95 240"
			/>
		</g>
	);
};

export default FacialHair1;

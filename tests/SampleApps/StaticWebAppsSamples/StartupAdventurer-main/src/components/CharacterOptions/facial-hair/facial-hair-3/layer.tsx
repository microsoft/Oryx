import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair3 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="facialHair_3" data-name="facialHair_3">
			<path
				fill={color1}
				d="M451,240H381V230H371V220H361V190H351v60h10v10h10v10h10v10h10v10h50V280h10V260h10V240Zm-10,20H401V250h40Z"
			/>
			<polygon
				fill={color2}
				points="430.95 240 420.95 240 420.95 250 430.95 250 440.95 250 440.95 240 430.95 240"
			/>
			<polygon
				fill={color2}
				points="420.95 260 410.95 260 410.95 270 410.95 280 410.95 290 420.95 290 430.95 290 430.95 280 440.95 280 440.95 270 440.95 260 430.95 260 420.95 260"
			/>
			<polygon
				fill={color2}
				points="380.95 240 380.95 230 370.95 230 370.95 220 360.95 220 360.95 230 360.95 240 370.95 240 370.95 250 380.95 250 380.95 260 390.95 260 400.95 260 400.95 250 400.95 240 390.95 240 380.95 240"
			/>
			<polygon
				fill={color3}
				points="410.95 240 400.95 240 400.95 250 410.95 250 420.95 250 420.95 240 410.95 240"
			/>
			<polygon
				fill={color3}
				points="390.95 260 380.95 260 380.95 250 370.95 250 370.95 240 360.95 240 360.95 230 360.95 220 360.95 210 360.95 200 360.95 190 350.95 190 350.95 200 350.95 210 350.95 220 350.95 230 350.95 240 350.95 250 360.95 250 360.95 260 370.95 260 370.95 270 380.95 270 380.95 280 390.95 280 390.95 290 400.95 290 410.95 290 410.95 280 410.95 270 410.95 260 400.95 260 390.95 260"
			/>
		</g>
	);
};

export default FacialHair3;

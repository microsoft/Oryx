import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair4 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="facialHair_4" data-name="facialHair_4">
			<path
				fill={color1}
				d="M450.94,240h-70V230h-10V220h-10V190h-10v60h10v80h10v20h10v20h20v10h40V370h10V360h10V330h-10V320h10V280h-10V270h10V240Zm-10,20h-40V250h40Z"
			/>
			<polygon
				fill={color2}
				points="430.95 240 420.95 240 420.95 250 430.95 250 440.95 250 440.95 240 430.95 240"
			/>
			<polygon
				fill={color2}
				points="420.95 310 420.95 320 420.95 330 430.95 330 430.95 320 440.95 320 440.95 310 440.95 300 430.95 300 430.95 310 420.95 310"
			/>
			<polygon
				fill={color2}
				points="420.95 260 410.95 260 410.95 270 420.95 270 430.95 270 430.95 280 440.95 280 440.95 270 440.95 260 430.95 260 420.95 260"
			/>
			<rect fill={color2} x="410.95" y="370" width="10" height="10" />
			<rect fill={color2} x="410.95" y="340" width="10" height="10" />
			<rect fill={color2} x="410.95" y="300" width="10" height="10" />
			<polygon
				fill={color2}
				points="410.95 280 410.95 290 420.95 290 420.95 300 430.95 300 430.95 290 430.95 280 420.95 280 410.95 280"
			/>
			<polygon
				fill={color2}
				points="400.95 360 400.95 370 410.95 370 410.95 360 410.95 350 400.95 350 400.95 360"
			/>
			<polygon
				fill={color2}
				points="390.95 340 400.95 340 400.95 330 400.95 320 390.95 320 390.95 330 390.95 340"
			/>
			<polygon
				fill={color2}
				points="400.95 300 400.95 290 390.95 290 390.95 300 390.95 310 400.95 310 400.95 300"
			/>
			<rect fill={color2} x="380.95" y="360" width="10" height="10" />
			<rect fill={color2} x="380.95" y="310" width="10" height="10" />
			<polygon
				fill={color2}
				points="380.95 250 380.95 260 380.95 270 390.95 270 390.95 260 400.95 260 400.95 250 390.95 250 390.95 240 380.95 240 380.95 230 370.95 230 370.95 220 360.95 220 360.95 230 360.95 240 370.95 240 370.95 250 380.95 250"
			/>
			<rect fill={color3} x="430.95" y="360" width="10" height="10" />
			<rect fill={color3} x="430.95" y="340" width="10" height="10" />
			<rect fill={color3} x="420.95" y="370" width="10" height="10" />
			<polygon
				fill={color3}
				points="420.95 350 410.95 350 410.95 360 410.95 370 420.95 370 420.95 360 420.95 350"
			/>
			<rect fill={color3} x="410.95" y="330" width="10" height="10" />
			<rect fill={color3} x="410.95" y="270" width="10" height="10" />
			<rect fill={color3} x="400.95" y="370" width="10" height="10" />
			<rect fill={color3} x="400.95" y="340" width="10" height="10" />
			<rect fill={color3} x="400.95" y="320" width="10" height="10" />
			<rect fill={color3} x="400.95" y="300" width="10" height="10" />
			<rect fill={color3} x="400.95" y="280" width="10" height="10" />
			<polygon
				fill={color3}
				points="410.95 250 420.95 250 420.95 240 410.95 240 400.95 240 390.95 240 390.95 250 400.95 250 410.95 250"
			/>
			<rect fill={color3} x="390.95" y="360" width="10" height="10" />
			<rect fill={color3} x="390.95" y="310" width="10" height="10" />
			<polygon
				fill={color3}
				points="390.95 340 390.95 330 390.95 320 380.95 320 380.95 310 390.95 310 390.95 300 390.95 290 390.95 280 400.95 280 400.95 270 410.95 270 410.95 260 400.95 260 390.95 260 390.95 270 380.95 270 380.95 260 380.95 250 370.95 250 370.95 240 360.95 240 360.95 230 360.95 220 360.95 210 360.95 200 360.95 190 350.95 190 350.95 200 350.95 210 350.95 220 350.95 230 350.95 240 350.95 250 360.95 250 360.95 260 360.95 270 360.95 280 360.95 290 360.95 300 360.95 310 360.95 320 360.95 330 370.95 330 370.95 340 370.95 350 380.95 350 380.95 360 390.95 360 390.95 350 390.95 340"
			/>
		</g>
	);
};

export default FacialHair4;

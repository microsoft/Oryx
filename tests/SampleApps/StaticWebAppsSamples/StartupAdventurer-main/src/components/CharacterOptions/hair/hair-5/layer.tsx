import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair5 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="hair_5" data-name="hair_5">
			<path
				fill={color1}
				d="M360.94,180V170h10V140h10V130h70v10h10V250h10V240h10V230h10V220h10V210h10V190h10V180h-10V170h10V140h-10V130h10V120h-10V100h-10V70h-10V60h-20V40h-10V50h-10V30h-30V20h-10V30h-10V20h-10V30h-10V20h-10V30h-30V40h-20V50h-10V60h-10V70h-10V90h-10v20h-10v20h-10v10h10v10h-10v10h10v20h10v10h-10v10h10v20h10v10h10v10h10v10h10v10h20v10h30V260h-10V240h-10V230h-10V190h20Z"
			/>
			<rect fill={color2} x="340.95" y="140" width="10" height="10" />
			<rect fill={color2} x="340.95" y="170" width="10" height="10" />
			<rect fill={color2} x="350.95" y="170" width="10" height="10" />
			<rect fill={color2} x="340.95" y="160" width="10" height="10" />
			<rect fill={color2} x="350.95" y="120" width="10" height="10" />
			<rect fill={color3} x="290.95" y="80" width="10" height="10" />
			<rect fill={color3} x="410.95" y="80" width="10" height="10" />
			<polygon
				fill={color3}
				points="380.95 40 380.95 30 380.95 20 370.95 20 370.95 30 370.95 40 360.95 40 360.95 30 350.95 30 350.95 40 340.95 40 330.95 40 330.95 50 330.95 60 340.95 60 340.95 50 350.95 50 350.95 60 340.95 60 340.95 70 350.95 70 360.95 70 360.95 80 360.95 90 370.95 90 370.95 100 360.95 100 360.95 90 350.95 90 350.95 80 340.95 80 340.95 70 330.95 70 330.95 60 320.95 60 310.95 60 300.95 60 300.95 70 300.95 80 310.95 80 310.95 70 320.95 70 320.95 80 330.95 80 330.95 90 340.95 90 340.95 100 350.95 100 350.95 110 340.95 110 330.95 110 330.95 120 340.95 120 340.95 130 350.95 130 350.95 120 360.95 120 360.95 110 370.95 110 370.95 120 360.95 120 360.95 130 350.95 130 350.95 140 350.95 150 340.95 150 340.95 160 350.95 160 350.95 170 360.95 170 370.95 170 370.95 160 370.95 150 370.95 140 380.95 140 380.95 130 390.95 130 400.95 130 400.95 120 390.95 120 390.95 110 400.95 110 400.95 100 390.95 100 390.95 90 400.95 90 400.95 80 390.95 80 390.95 70 400.95 70 400.95 60 390.95 60 390.95 50 380.95 50 380.95 40"
			/>
			<rect fill={color3} x="390.95" y="40" width="10" height="10" />
			<rect fill={color3} x="390.95" y="20" width="10" height="10" />
			<rect fill={color3} x="320.95" y="140" width="10" height="10" />
			<rect fill={color3} x="300.95" y="140" width="10" height="10" />
			<polygon fill={color3} points="440.95 30 430.95 30 430.95 40 440.95 40 450.95 40 450.95 30 440.95 30" />
			<polygon
				fill={color3}
				points="420.95 40 420.95 30 420.95 20 410.95 20 410.95 30 400.95 30 400.95 40 410.95 40 420.95 40"
			/>
			<rect fill={color3} x="310.95" y="170" width="10" height="10" />
			<rect fill={color3} x="330.95" y="160" width="10" height="10" />
			<rect fill={color3} x="330.95" y="130" width="10" height="10" />
			<rect fill={color3} x="310.95" y="130" width="10" height="10" />
			<rect fill={color3} x="300.95" y="100" width="10" height="10" />
			<rect fill={color3} x="280.95" y="100" width="10" height="10" />
			<polygon
				fill={color3}
				points="430.95 100 430.95 110 440.95 110 450.95 110 450.95 100 440.95 100 430.95 100"
			/>
			<rect fill={color3} x="410.95" y="100" width="10" height="10" />
			<polygon
				fill={color3}
				points="330.95 90 320.95 90 310.95 90 310.95 100 320.95 100 320.95 110 330.95 110 330.95 100 330.95 90"
			/>
			<rect fill={color3} x="420.95" y="60" width="10" height="10" />
			<rect fill={color3} x="300.95" y="120" width="10" height="10" />
			<rect fill={color3} x="270.95" y="120" width="10" height="10" />
			<rect fill={color2} x="340.95" y="180" width="10" height="10" />
			<rect fill={color2} x="350.95" y="180" width="10" height="10" />
			<rect fill={color2} x="340.95" y="50" width="10" height="10" />
			<path
				fill={color2}
				d="M361,250V240H351V230H341V170H331V160h10V140H331v10H321V140H311v10H301V140h10V130H301V120h10v10h10v10h10V130h10V120H331V110H321V100H311v10H301V100h10V90h20V80H321V70H311V80H301V90H281v10h10v10H271v10h10v10H261v10h10v10H261v10h10v20h10v10H271v10h10v20h10v10h10v10h10v10h10v10h20v10h30V260H361Zm-40-70H311V170h10Z"
			/>
			<polygon
				fill={color2}
				points="330.95 40 320.95 40 320.95 50 310.95 50 310.95 60 320.95 60 330.95 60 330.95 50 330.95 40"
			/>
			<rect fill={color2} x="360.95" y="110" width="10" height="10" />
			<rect fill={color2} x="340.95" y="30" width="10" height="10" />
			<rect fill={color2} x="360.95" y="30" width="10" height="10" />
			<rect fill={color2} x="290.95" y="70" width="10" height="10" />
			<polygon
				fill={color2}
				points="360.95 70 350.95 70 340.95 70 340.95 80 350.95 80 350.95 90 360.95 90 360.95 80 360.95 70"
			/>
			<rect fill={color2} x="340.95" y="130" width="10" height="10" />
			<polygon
				fill={color2}
				points="330.95 110 340.95 110 350.95 110 350.95 100 340.95 100 340.95 90 330.95 90 330.95 100 330.95 110"
			/>
			<rect fill={color2} x="360.95" y="90" width="10" height="10" />
			<rect fill={color2} x="330.95" y="60" width="10" height="10" />
		</g>
	);
};

export default Hair5;

import React from "react";
import { defaultColor } from "./colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const MaxiSkirt = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="maxiskirt_2" data-name="maxiskirt 2">
			<path
				d="m520.95 820v-10-10-10h-10.01v-10-10-10-10-10h-10v-10-10-10-10-10-10-10h-10v-10-10-10-10-10-10h-10v-10-10-10-10h-10-10-10-10v10h-10-10-10-10-10-10-10-10-10-10-10-20v10h10v30h-10v10h10v10h10v20h-10-20v-10h-20v10 10 10 10h-10v10 10 10 10 10 10h-10v10 10 10 10 10h-10v10 10 10h-10v10 10 10 10 10 10 10 10 10 10 10h10 10 10v10h10 10 10 10 10 10 10 10v-10h10 10 10v10h10 10 10 10 10 10 10 10v-10h10 10 10 10 10 10.01 10v-10-10-10-10-10-10-10-10-10-10-10-10z"
				fill={color1}
			/>
			<path d="m520.94 930h10v10h-10z" fill={color2} />
			<path d="m510.94 920h10v10h-10z" fill={color2} />
			<path
				d="m500.94 920h10v-10h-10v-10-10-10-10-10-10-10-10h-10v-10-10h-10v10 10 10 10 10 10 10h10v10 10 10 10 10 10h10 10v-10h-10z"
				fill={color2}
			/>
			<path d="m480.94 790h10v10h-10z" fill={color2} />
			<path d="m480.94 780v-10-10h-10v10 10 10h10z" fill={color2} />
			<path d="m480.94 580v-10h-10-10-10-10v10h10 10 10z" fill={color2} />
			<path
				d="m450.94 870v-10-10h-10v10 10 10 10h10v10 10 10 10 10 10h10v-10-10-10-10-10-10-10h-10z"
				fill={color2}
			/>
			<path d="m450.94 830v-10-10-10-10-10h-10v10 10 10 10 10 10h10z" fill={color2} />
			<path
				d="m420.94 580h-10-10-10-10-10-10-10-10-10v10 10 10 10 10h10v10 10h10v10h-10v10h-10v10 10 10 10 10 10h-10v10 10 10h-10v10 10 10h10 10v-10-10h10v-10-10-10h10v-10-10-10h10v-10h-10v-10h-10v-10h10v-10h10v-10-10h10v-10-10-10-10h-10v10h-10v-10h-10v-10h10v-10h10v10h10v-10h10 10 10 10 10 10 10v-10h-10z"
				fill={color2}
			/>
			<path d="m380.94 900v10h-10v10 10h-10v10h10 10v10h10v-10-10-10-10-10-10h-10z" fill={color2} />
			<path d="m380.94 820v10 10 10 10 10 10h10v-10-10-10-10-10-10-10h-10z" fill={color2} />
			<path d="m380.94 780v10 10h10v-10-10-10h-10z" fill={color2} />
			<path d="m350.94 680h10v10h-10z" fill={color2} />
			<path d="m350.94 600h10v10h-10z" fill={color2} />
			<path d="m340.94 860h-10v10 10 10 10 10 10 10 10h10v-10-10-10-10-10-10-10h10v-10-10h-10z" fill={color2} />
			<path d="m340.94 830v10h10v-10-10h-10z" fill={color2} />
			<path d="m310.94 800h10v10h-10z" fill={color2} />
			<path
				d="m300.94 820v10h-10v10 10 10 10 10 10 10 10 10 10 10h10 10v-10-10-10-10-10h-10v-10h10v-10-10-10-10-10-10-10h-10z"
				fill={color2}
			/>
			<path d="m300.94 790h10v10h-10z" fill={color2} />
			<path d="m310.94 720h10v10h-10z" fill={color3} />
			<path
				d="m300.94 730v-10h10v-10h10v10h10v-50h10v-10h10v-10h-10v-20h-10v-50h-20v10h10v30h-10v10h10v10h10v20h-30v-10h-20v30h10v10h-10v10h-10v60h10v10h-10v10h-10v20h-10v30h-10v110h20v-10h-10v-10h10v-10h10v10h10v10h-10v20h80v-10h-60v-110h10v-40h10v-30h10v-20h-10v-10zm-20 120h-10v10h-10v-10h-10v-10h10v-10h10v10h10z"
				fill={color3}
			/>
			<path d="m260.94 920h10v10h-10z" fill={color3} />
			<path d="m260.94 840h10v10h-10z" fill={color3} />
			<path d="m260.94 760h10v10h-10z" fill={color3} />
			<g fill={color1}>
				<path d="m510.94 840h10v10h-10z" />
				<path d="m470.94 880h10v10h-10z" />
				<path d="m470.94 800h10v10h-10z" />
				<path d="m470.94 720h10v10h-10z" />
				<path d="m470.94 640h10v10h-10z" />
				<path d="m430.94 920h10v10h-10z" />
				<path d="m430.94 840h10v10h-10z" />
				<path d="m430.94 760h10v10h-10z" />
				<path d="m430.94 680h10v10h-10z" />
				<path d="m430.94 600h10v10h-10z" />
				<path d="m390.94 880h10v10h-10z" />
				<path d="m390.94 800h10v10h-10z" />
				<path d="m390.94 720h10v10h-10z" />
				<path d="m390.94 640h10v10h-10z" />
				<path d="m350.94 920h10v10h-10z" />
				<path d="m350.94 840h10v10h-10z" />
				<path d="m350.94 760h10v10h-10z" />
				<path d="m340.94 930h10v10h-10z" />
				<path d="m310.94 900v10 10 10 10h10 10v-10-10-10-10-10h-10v10z" />
				<path d="m310.94 880h10v10h-10z" />
			</g>
			<g fill="#1e130b">
				<path d="m520.94 920h10v10h-10z" />
				<path d="m520.94 840h10v10h-10z" />
				<path d="m510.94 930h10v10h-10z" />
				<path d="m510.94 910h10v10h-10z" />
				<path d="m510.94 850h10v10h-10z" />
				<path d="m510.94 830h10v10h-10z" />
				<path d="m500.94 920h10v10h-10z" />
				<path d="m500.94 840h10v10h-10z" />
				<path d="m500.94 760h10v10h-10z" />
				<path d="m480.94 880h10v10h-10z" />
				<path d="m480.94 800h10v10h-10z" />
				<path d="m480.94 720h10v10h-10z" />
				<path d="m480.94 640h10v10h-10z" />
				<path d="m470.94 890h10v10h-10z" />
				<path d="m470.94 870h10v10h-10z" />
				<path d="m470.94 810h10v10h-10z" />
				<path d="m470.94 790h10v10h-10z" />
				<path d="m470.94 730h10v10h-10z" />
				<path d="m470.94 710h10v10h-10z" />
				<path d="m470.94 650h10v10h-10z" />
				<path d="m470.94 630h10v10h-10z" />
				<path d="m460.94 880h10v10h-10z" />
				<path d="m460.94 800h10v10h-10z" />
				<path d="m460.94 720h10v10h-10z" />
				<path d="m460.94 640h10v10h-10z" />
				<path d="m440.94 920h10v10h-10z" />
				<path d="m440.94 840h10v10h-10z" />
				<path d="m440.94 760h10v10h-10z" />
				<path d="m440.94 680h10v10h-10z" />
				<path d="m440.94 600h10v10h-10z" />
				<path d="m430.94 930h10v10h-10z" />
				<path d="m430.94 910h10v10h-10z" />
				<path d="m430.94 850h10v10h-10z" />
				<path d="m430.94 830h10v10h-10z" />
				<path d="m430.94 770h10v10h-10z" />
				<path d="m430.94 750h10v10h-10z" />
				<path d="m430.94 690h10v10h-10z" />
				<path d="m430.94 670h10v10h-10z" />
				<path d="m430.94 610h10v10h-10z" />
				<path d="m430.94 590h10v10h-10z" />
				<path d="m420.94 920h10v10h-10z" />
				<path d="m420.94 840h10v10h-10z" />
				<path d="m420.94 760h10v10h-10z" />
				<path d="m420.94 680h10v10h-10z" />
				<path d="m420.94 600h10v10h-10z" />
				<path d="m400.94 880h10v10h-10z" />
				<path d="m400.94 800h10v10h-10z" />
				<path d="m400.94 720h10v10h-10z" />
				<path d="m400.94 640h10v10h-10z" />
				<path d="m390.94 890h10v10h-10z" />
				<path d="m390.94 870h10v10h-10z" />
				<path d="m390.94 810h10v10h-10z" />
				<path d="m390.94 790h10v10h-10z" />
				<path d="m390.94 730h10v10h-10z" />
				<path d="m390.94 710h10v10h-10z" />
				<path d="m390.94 650h10v10h-10z" />
				<path d="m390.94 630h10v10h-10z" />
				<path d="m380.94 880h10v10h-10z" />
				<path d="m380.94 800h10v10h-10z" />
				<path d="m380.94 720h10v10h-10z" />
				<path d="m380.94 640h10v10h-10z" />
				<path d="m360.94 920h10v10h-10z" />
				<path d="m360.94 840h10v10h-10z" />
				<path d="m360.94 760h10v10h-10z" />
				<path d="m360.94 680h10v10h-10z" />
				<path d="m360.94 600h10v10h-10z" />
				<path d="m350.94 930h10v10h-10z" />
				<path d="m350.94 910h10v10h-10z" />
				<path d="m350.94 850h10v10h-10z" />
				<path d="m350.94 830h10v10h-10z" />
				<path d="m350.94 770h10v10h-10z" />
				<path d="m350.94 750h10v10h-10z" />
				<path d="m350.94 690h10v10h-10z" />
				<path d="m350.94 670h10v10h-10z" />
				<path d="m350.94 610h10v10h-10z" />
				<path d="m350.94 590h10v10h-10z" />
				<path d="m340.94 920h10v10h-10z" />
				<path d="m340.94 840h10v10h-10z" />
				<path d="m340.94 760h10v10h-10z" />
				<path d="m340.94 680h10v10h-10z" />
				<path d="m340.94 600h10v10h-10z" />
				<path d="m320.94 880h10v10h-10z" />
				<path d="m320.94 800h10v10h-10z" />
				<path d="m320.94 720h10v10h-10z" />
				<path d="m310.94 890h10v10h-10z" />
				<path d="m310.94 870h10v10h-10z" />
				<path d="m310.94 810h10v10h-10z" />
				<path d="m310.94 790h10v10h-10z" />
				<path d="m310.94 730h10v10h-10z" />
				<path d="m310.94 710h10v10h-10z" />
				<path d="m300.94 880h10v10h-10z" />
				<path d="m300.94 800h10v10h-10z" />
				<path d="m300.94 720h10v10h-10z" />
				<path d="m280.94 680h10v10h-10z" />
				<path d="m270.94 920h10v10h-10z" />
				<path d="m270.94 840h10v10h-10z" />
				<path d="m270.94 760h10v10h-10z" />
				<path d="m270.94 690h10v10h-10z" />
				<path d="m260.94 930h10v10h-10z" />
				<path d="m260.94 910h10v10h-10z" />
				<path d="m260.94 850h10v10h-10z" />
				<path d="m260.94 830h10v10h-10z" />
				<path d="m260.94 770h10v10h-10z" />
				<path d="m260.94 750h10v10h-10z" />
				<path d="m250.94 920h10v10h-10z" />
				<path d="m250.94 840h10v10h-10z" />
			</g>
		</g>
	);
};

export default MaxiSkirt;

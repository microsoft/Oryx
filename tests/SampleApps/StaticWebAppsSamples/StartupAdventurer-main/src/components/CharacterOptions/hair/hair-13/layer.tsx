import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair13 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="hair_13" data-name="hair_13">
			<path d="m370.9 100v-10h-10v10 10 10h10v-10h10 10v-10h-10z" fill="#d665bb" />
			<path d="m400.9 100h10v10h-10z" fill="#d665bb" />
			<path d="m440.9 100h10v10h-10z" fill="#d665bb" />
			<path d="m440.9 80h-10v10h-10v10 10h10v-10h10v-10z" fill="#d665bb" />
			<path d="m450.9 110h10v10h-10z" fill="#d665bb" />
			<path d="m450.9 70h-10v10 10 10h10v10h10v-10-10-10h.1v-10h-10" fill="#f472d0" />
			<path d="m380.9 80h-10v-10h-10-10v10h10v10h10v10h10 10v-10h-10z" fill="#f472d0" />
			<path d="m400.9 80v10 10h10v10h10v-10-10-10h-10z" fill="#f472d0" />
			<g fill="#ba54ab">
				<path d="m360.9 110v-10-10-10h-10v10 10 10 10 10h10 10v-10h-10z" />
				<path d="m370.9 110v10h10 10v-10h-10z" />
				<path d="m400.9 110v10h10 10v-10h-10z" />
				<path d="m440.9 110v-10h-10v10 10h10 10v-10z" />
				<path d="m400.9 90h-10v10 10h10v-10z" />
				<path d="m450.9 120h10v10h-10z" />
			</g>
			<path
				d="m350.9 130v-10h-10v10 10 10 10 10 10 10h10 10v-10-10h-10v-10-10h10v-10h10v-10h-10z"
				fill={color3}
			/>
			<path d="m390.9 70v-10-10-10h-10-10v10h-10v10 10h10v10h10v10h10 10v-10h-10z" fill={color3} />
			<path d="m390.9 120h-10-10v10h10 10 10v-10-10h-10z" fill={color3} />
			<path d="m420.9 110h10v10h-10z" fill={color3} />
			<path d="m360.9 150h-10v10 10h10 10v-10-10-10h-10z" fill={color2} />
			<path d="m370.9 130h10v10h-10z" fill={color2} />
			<path d="m410.9 70v-10-10-10h-10-10v10 10 10 10h10 10 10v-10z" fill={color2} />
			<path d="m420.9 80h10v10h-10z" fill={color2} />
			<path d="m450.9 130h10v10h-10z" fill={color1} />
			<path d="m420.9 80h10 10v-10-10-10h-10v-10h-10-10v10 10 10h10z" fill={color1} />
			<path d="m440.9 120h-10-10-10-10v10h10 10 10 10 10v-10z" fill={color1} />
		</g>
	);
};

export default Hair13;

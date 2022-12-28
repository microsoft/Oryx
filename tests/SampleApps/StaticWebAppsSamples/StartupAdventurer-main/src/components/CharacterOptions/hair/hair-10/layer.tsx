import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair10 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="hair_10" data-name="hair_10">
			<path
				d="m421 130h20v20h10v10h10v100h-10v10h-10v10h-20v10h50v-150h-10v-20h-10v-10h-10v-10h-80v10h-10v10h-10v20h-10v150h40v-20-10h-10v-20h-10v-10h-10v-40h30v-10h20v-10h10v-10h10v-10h10v-10h10v-10"
				fill={color1}
			/>
			<path d="m340.94 130h10v10h-10z" fill={color3} />
			<path d="m340.94 160h10v10h-10z" fill={color3} />
			<path d="m350.94 120h10v10h-10z" fill={color3} />
			<path d="m340.94 180h10v10h-10z" fill={color3} />
			<path d="m360.94 270v-10-10h-10-10v10 10 10h-10v10h10 10 10 10v-10-10z" fill={color2} />
			<path d="m340.94 150h10v10h-10z" fill={color3} />
			<path d="m350.94 130h10v10h-10z" fill={color2} />
			<path d="m380.94 130h-10v10h-10v10h-10v10 10h10v-10h10v-10h10 10v-10h10v-10h-10z" fill={color2} />
			<path d="m440.94 130h10v10h-10z" fill={color2} />
			<path d="m340.94 140h10v10h-10z" fill={color2} />
			<path d="m450.94 140h10v10h-10z" fill={color2} />
			<path d="m340.94 170h10v10h-10z" fill={color2} />
			<path d="m370.94 120h10 10v-10h-10-10-10v10 10h10z" fill={color2} />
			<path d="m350.94 180h10v10h-10z" fill={color2} />
			<path d="m450.94 270h-10v10 10h10 10v-10-10-10h-10z" fill={color2} />
			<g fill={color3}>
				<path d="m360.94 130h10v10h-10z" />
				<path d="m420.94 280v10h10 10v-10h-10z" />
				<path d="m360.94 160h10v10h-10z" />
				<path d="m330.94 150v10 10 10 10 10 10 10 10 10 10 10 10 10h10v-10-10-10-10-10-10-10-10-10-10-10-10-10-10h-10z" />
				<path d="m350.94 140h10v10h-10z" />
				<path d="m390.94 140h10v10h-10z" />
				<path d="m440.94 140h10v10h-10z" />
				<path d="m350.94 170h10v10h-10z" />
				<path d="m380.94 110v-10h-10-10v10h10z" />
				<path d="m340.94 120h10v10h-10z" />
				<path d="m380.94 120h-10v10h10 10 10v-10h-10z" />
				<path d="m430.94 120h10v10h-10z" />
				<path d="m360.94 260h10v10h-10z" />
				<path d="m350.94 110h10v10h-10z" />
				<path d="m370.94 150v10h10 10v-10h-10z" />
				<path d="m450.94 150h10v10h-10z" />
				<path d="m350.94 230h-10v10 10h10 10v-10h-10z" />
			</g>
		</g>
	);
};

export default Hair10;

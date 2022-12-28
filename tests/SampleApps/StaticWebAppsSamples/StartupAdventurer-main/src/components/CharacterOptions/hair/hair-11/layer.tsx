import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair11 = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<g id="hair_11" data-name="hair_11">
			<path
				d="m500.94 210v10h-10v-60h-10v-10h-10v-30h-10v-10h-20v-10h-70v10h-30v10h-10v10h-10v90h-20v30h20v10h-10v10h10v10h30v-10h20v-10h-10v-20h-10v-10h-10v-40h40v-10h20v-10h20v-10h10v-10h10v-10h10v10h10v110h-10v10h20v10h10v-10h10v-20h10v-10h10v-30z"
				fill={color1}
			/>
			<path d="m370.95 120h10v10h-10z" fill={color3} />
			<path d="m360.95 150h10v10h-10z" fill={color3} />
			<path d="m360.95 180h10v10h-10z" fill={color3} />
			<path d="m370.95 100h10v10h-10z" fill={color3} />
			<path
				d="m350.95 130v-10h10v-10h-10-10v10h-10v10h-10v10 10 10 10 10 10 10 10 10h10v10 10h10v10h10v10h-10-10v10 10h10 10v-10h10 10v-10h-10v-10-10h-10v-10h-10v-10-10-10-10h10v-10h10v-10-10h-10v10h-10v-10-10-10h10v10h10v-10h10v-10h-10z"
				fill={color3}
			/>
			<path d="m370.95 140h10v10h-10z" fill={color3} />
			<path d="m450.95 140h10v10h-10z" fill={color3} />
			<path d="m320.95 230h-10-10v10h10v10h10 10v-10h-10z" fill={color3} />
			<path d="m360.95 160h10v10h-10z" fill={color2} />
			<path d="m350.95 120h10v10h-10z" fill={color3} />
			<path d="m340.95 150h10v10h-10z" fill={color3} />
			<path d="m340.95 140h10v10h-10z" fill={color3} />
			<g fill={color2}>
				<path d="m330.95 220h-10-10-10v10h10 10v10h10v-10z" />
				<path d="m480.95 220h-10v-10h10v-10h-10v-10-10-10h-10v10 10 10 10 10 10 10 10 10h-10v10h10 10v-10-10-10h10v-10z" />
				<path d="m340.95 160h10v10h-10z" />
				<path d="m400.95 120h-10v-10h-10-10-10v10 10h10v-10h10v10h-10v10h10v10h-10v10 10h10v-10h10v-10h10v-10h10v-10h-10z" />
				<path d="m350.95 150h10v10h-10z" />
				<path d="m350.95 180h10v10h-10z" />
				<path d="m360.95 170h10v10h-10z" />
				<path d="m340.95 240h-10v10h-10v10h-10v10h10v10h10v-10-10h10 10v-10h-10z" />
				<path d="m300.95 240h10v10h-10z" />
				<path d="m480.95 240h10v10h-10z" />
				<path d="m360.95 140h10v10h-10z" />
			</g>
		</g>
	);
};

export default Hair11;

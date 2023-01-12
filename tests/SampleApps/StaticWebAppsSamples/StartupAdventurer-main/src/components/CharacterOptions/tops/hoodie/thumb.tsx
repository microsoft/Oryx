import React from "react";
import { Colors } from "@/interfaces/Colors";
import { defaultColor } from "./colors";

interface IProps {
	colors?: Colors;
}

const HoodieThumb = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;

	return (
		<svg version="1.1" data-id="clothes-top-thumb" x="0px" y="0px" viewBox="0 0 182 182">
			<path
				d="m173.7 93.8v-5.6h-5.5-5.5v5.6h-5.5v5.5h-5.5v5.5h-5.5-5.6v-5.5-5.5-5.6-5.5-5.5h-5.5v-5.5-5.5-5.5-5.6h-5.5v-5.5-5.5-5.5h-5.5v-5.5h-5.5v-5.5h-5.5-5.6v-5.5h-5.5v-5.6h-5.5v-5.5h-5.5v-5.5h-5.5-5.5-5.5v-5.5h-5.6-5.5-5.5-5.5-5.5-5.5-5.6v5.5h-5.5-5.5v5.5h-5.5v5.5 5.6 5.5h5.5v5.5h-5.5v5.5 5.5h-5.5v5.5 5.5h-5.5v.1 5.5 5.5 5.5 5.5h-5.5v5.5 5.5 5.6 5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5h5.5v5.5 5.5 5.6 5.5h5.5v5.5 5.5h5.5 5.5 5.5v-5.5h.3 5.3 5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5 5.5 5.5v-5.5h5.5 5.6 5.5v-5.6-5.5-5.5-5.5-5.5h-5.5v-5.5-5.5-5.6-5.5-5.5h5.5v5.5 5.5h5.5v5.6h5.5v5.5h5.5v5.5h5.5 5.6 5.5v-5.5h5.5v-5.5h5.5v-5.6h5.5v-5.5-5.5h5.5v-5.5h5.5v-5.5-5.5-5.5z"
				fill={color1}
			/>
			<path d="m173.7 93.8h5.5v5.5h-5.5z" fill={color2} />
			<path d="m96.5 27.6h-5.5v5.5 5.5h5.5v-5.5z" fill={color2} />
			<path d="m35.8 11v-5.5h-5.5-5.5v5.5h5.5z" fill={color2} />
			<path
				d="m13.8 99.3v5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5 5.5 5.5h5.5v-5.5-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5-5.5h5.5v-5.5h-5.5z"
				fill={color2}
			/>
			<path
				d="m168.2 104.8h-5.5-5.5v5.5 5.5 5.5h-5.5v5.5h-5.6-5.5v-5.5h-5.5v-5.5-5.5h-5.5v-5.5-5.5-5.5-5.6h-5.5v-5.5-5.5-5.5-5.5-5.5h-5.5v5.5 5.5 5.5 5.5 5.5 5.6h5.5-5.5v5.5 5.5 5.5h5.5v5.5h5.5v5.5 5.5h5.5v5.6h5.5v5.5h5.5 5.6v-5.5h5.5v-5.6h5.5v-5.5-5.5h5.5v-5.5h5.5v-5.5-5.5h-5.5z"
				fill={color2}
			/>
			<path d="m30.3 121.3v5.5 5.6h5.5v-5.6-5.5-5.5h-5.5z" fill={color2} />
			<path d="m30.3 16.5v5.6h5.5 5.6v-5.6-5.5h-5.6v5.5z" fill={color2} />
			<path d="m113.1 49.6h5.5v5.5h-5.5z" fill={color2} />
			<path d="m96.5 22.1v5.5 5.5h5.5v-5.5h5.5v-5.5h-5.5z" fill={color2} />
			<path d="m80 38.6h5.5v5.5h-5.5z" fill={color2} />
			<path
				d="m74.5 171v-5.5h-5.6v-5.6h-5.5v-5.5-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5-5.5h-5.5v-5.5-5.5-5.6h-5.5v-5.5-5.5-5.5-5.5h-5.5v5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5h-5.5v5.5 5.5 5.6h5.5v5.5h5.5v5.5h5.5 5.5 5.5 5.6 5.5 5.5v-5.5h-5.5z"
				fill={color2}
			/>
			<path d="m63.4 44.1h5.5v-5.5h-5.5-5.5v5.5z" fill={color2} />
			<path d="m35.8 77.2h5.5v5.5h-5.5z" fill={color2} />
			<path
				d="m30.3 55.2v-5.6h5.5v-5.5-5.5h5.6 5.5 5.5 5.5v-5.5h-5.5v-5.5h-5.5v-5.5h-5.5v5.5h-5.6-5.5v5.5h-5.5v5.5h-5.5v5.5 5.5 5.6h-5.5v5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5h5.5 5.5v-5.5-5.6-5.5-5.5-5.5-5.5-5.5h5.5z"
				fill={color2}
			/>
			<g fill={color3}>
				<path d="m13.8 49.6v5.6h5.5v-5.6-5.5h-5.5z" />
				<path d="m157.2 93.8h5.5v5.5h-5.5z" />
				<path d="m30.3 27.6h5.5 5.6v-5.5h-5.6-5.5v-5.6-5.5h-5.5-5.5v5.5 5.6 5.5h5.5v5.5h5.5z" />
				<path d="m140.6 137.9v-5.5h-5.5v-5.6h-5.5v-5.5-5.5h-5.5v-5.5h-5.5v-5.5-5.5h-5.5v5.5 5.5 5.5h5.5v5.5 5.5h5.5v5.6h5.5v5.5h5.5v5.5h5.5 5.6 5.5v-5.5h-5.5z" />
				<path d="m13.8 154.4v-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5-5.5-5.5h-5.5v-5.5-5.6-5.5-5.5h-5.5v5.5 5.5 5.6 5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5h5.5v5.5 5.5 5.6 5.5h5.5v5.5 5.5h5.5v-5.5-5.5-5.5-5.6h-5.5z" />
				<path d="m173.7 99.3v5.5 5.5h5.5v-5.5-5.5z" />
				<path d="m151.7 99.3h5.5v5.5h-5.5z" />
				<path d="m157.2 126.8h5.5v5.5h-5.5z" />
				<path d="m168.2 110.3h5.5v5.5h-5.5z" />
				<path d="m146.2 110.3h5.5v-5.5h-5.5-5.6v5.5 5.5h5.6z" />
				<path d="m19.3 33.1h5.5v5.5h-5.5z" />
				<path d="m162.7 88.2h5.5v5.5h-5.5z" />
				<path d="m162.7 121.3v5.5h5.5v-5.5-5.5h-5.5z" />
				<path d="m151.7 132.4h5.5v5.5h-5.5z" />
				<path d="m41.4 16.5h5.5v5.5h-5.5z" />
				<path d="m118.6 93.8v-5.6-5.5-5.5-5.5-5.5-5.5-5.5h-5.5v5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5h5.5z" />
				<path d="m85.5 38.6v5.5 5.5 5.6 5.5 5.5 5.5 5.5h5.5v-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5h-5.5z" />
				<path d="m57.9 33.1h5.5v5.5h-5.5z" />
				<path d="m52.4 27.6h5.5v5.5h-5.5z" />
				<path d="m46.9 22.1h5.5v5.5h-5.5z" />
				<path d="m46.9 165.5h-5.5v-5.6-5.5-5.5h5.5v-5.5-5.5-5.5-5.6-5.5-5.5-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5-5.5-5.5h-5.5v5.5 5.5 5.5 5.5h-5.5v5.5 5.6 5.5 5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5 5.5 5.5h-.1v-5.5-5.5-5.5-5.5h-5.5v5.5 5.5 5.5 5.5 5.5 5.6 5.5 5.5h5.6 5.5 5.5 5.5v-5.5h-5.5z" />
				<path d="m8.3 60.7v5.5 5.5 5.5 5.5 5.5 5.6 5.5h5.5v-5.5-5.6-5.5-5.5-5.5-5.5-5.5-5.5h-5.5z" />
				<path d="m91 16.5v-5.5-5.5h-5.5-5.5-5.5v-5.5h-5.6-5.5-5.5-5.5v5.5 5.5h5.5v5.5 5.6h5.5 5.5v5.5h5.6v5.5h-5.6v5.5 5.5 5.5 5.6 5.5 5.5 5.5 5.5h5.6v-5.5-5.5-5.5-5.5-5.6-5.5-5.5-5.5h5.5v-5.5h5.5v-5.5h5.5z" />
			</g>
		</svg>
	);
};

export default HoodieThumb;
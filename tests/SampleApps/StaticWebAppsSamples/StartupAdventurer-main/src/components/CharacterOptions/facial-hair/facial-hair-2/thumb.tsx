import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair2Thumb = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<svg version="1.1" x="0px" y="0px" viewBox="0 0 182 182">
			<path
				d="m136.5 18.2v-9.1h-9.1v-9.1h-9.1-9.1-9.1-9.1-9.1-9.1v9.1h9.1 9.1v9.1 9.1 9.1 9.1 9.1 9.1h9.1 9.1v9.1 9.1h9.1v9.1 9.1 9.1 9.1h9.1v9.1h-9.1-9.1v9.1h9.1 9.1v9.1h-9.1-9.1-9.1-9.1v-9.1-9.1h9.1v-9.1-9.1-9.1h-9.1-9.1-9.1v9.1 9.1h9.1v9.1 9.1 9.1 9.1h9.1 9.1v9.1h9.1 9.1 9.1v-9.1h9.1v-9.1h9.1v-9.1-9.1-9.1-9.1-9.1-9.1-9.1-9.1h-9.1v9.1 9.1 9.1h-9.1v-9.1-9.1-9.1-9.1h9.1 9.1v-9.1-9.1-9.1-9.1-9.1z"
				fill="#eaeaea"
			/>
			<path d="m127.4 91v9.1h9.1v-9.1-9.1h-9.1z" fill="#b5b5b5" />
			<path d="m127.4 72.8h9.1v9.1h-9.1z" fill="#d8d8d8" />
			<path d="m127.4 63.7v9.1h9.1 9.1v-9.1h-9.1z" fill="#b5b5b5" />
			<path d="m109.2 100.1v9.1 9.1h-9.1v9.1h9.1 9.1 9.1v-9.1h-9.1v-9.1-9.1-9.1-9.1h-9.1v9.1z" fill="#b5b5b5" />
			<path d="m100.1 81.9v9.1 9.1 9.1 9.1h9.1v-9.1-9.1-9.1-9.1-9.1h-9.1z" fill="#d8d8d8" />
			<path d="m91 154.7h-9.1v9.1h9.1 9.1v-9.1z" fill="#d8d8d8" />
			<path d="m91 136.5v9.1h9.1 9.1 9.1 9.1v-9.1h-9.1-9.1-9.1z" fill="#b5b5b5" />
			<path d="m91 127.4v9.1h9.1 9.1v-9.1h-9.1z" fill="#d8d8d8" />
			<path d="m100.1 81.9h-9.1v9.1 9.1h9.1v-9.1z" fill="#b5b5b5" />
			<path d="m100.1 72.8h-9.1-9.1v9.1h9.1 9.1z" fill="#eaeaea" />
			<path d="m81.9 72.8h9.1 9.1 9.1v-9.1h-9.1-9.1-9.1z" fill="#b5b5b5" />
			<path d="m72.8 154.7h9.1v9.1h-9.1z" fill="#b5b5b5" />
			<path
				d="m81.9 145.6v-9.1-9.1-9.1h-9.1v-9.1-9.1h9.1 9.1v-9.1-9.1h-9.1v-9.1-9.1h9.1v-9.1-9.1-9.1-9.1-9.1-9.1h-9.1-9.1v9.1h-9.1v9.1 9.1h-9.1v9.1 9.1 9.1 9.1 9.1h9.1v9.1 9.1 9.1 9.1 9.1 9.1h9.1v9.1 9.1h9.1z"
				fill="#d8d8d8"
			/>
			<path
				d="m72.8 136.5h-9.1v-9.1-9.1-9.1-9.1-9.1-9.1h-9.1v-9.1-9.1-9.1-9.1-9.1h9.1v-9.1-9.1h9.1v-9.1-9.1h-9.1v9.1h-9.1v9.1h-9.1v9.1h-9.1v9.1 9.1 9.1 9.1 9.1 9.1h9.1v9.1 9.1 9.1 9.1 9.1h9.1v9.1 9.1h9.1v9.1h9.1v-9.1z"
				fill="#b5b5b5"
			/>
			<path d="m45.5 100.1h-9.1v9.1 9.1h9.1v-9.1z" fill="#d8d8d8" />
			<path d="m45.5 91v-9.1h-9.1v9.1 9.1h9.1z" fill="#eaeaea" />
			<path
				d="m118.3 145.6h-9.1-9.1-9.1v9.1 9.1h9.1v9.1h9.1v9.1h9.1v-9.1h9.1v-9.1h9.1v-9.1-9.1h-9.1z"
				fill={color1}
			/>
			<path d="m127.4 127.4h-9.1-9.1-9.1-9.1v9.1h9.1 9.1 9.1 9.1 9.1v-9.1z" fill={color1} />
			<path d="m127.4 145.6h-9.1-9.1-9.1v9.1 9.1 9.1h9.1v9.1h9.1v-9.1-9.1h9.1v-9.1z" fill={color2} />
			<path d="m118.3 127.4h-9.1v9.1h9.1 9.1v-9.1z" fill={color2} />
			<g fill={color3}>
				<path d="m100.1 145.6h-9.1v9.1 9.1h9.1v-9.1z" />
				<path d="m100.1 127.4h-9.1v9.1h9.1 9.1v-9.1z" />
			</g>
		</svg>
	);
};

export default FacialHair2Thumb;

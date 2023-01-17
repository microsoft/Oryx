import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const FacialHair4Thumb = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<svg version="1.1" x="0px" y="0px" viewBox="0 0 182 182">
			<path
				d="m123.5 13v-6.5h-6.5v-6.5h-6.5-6.5-6.5-6.5-6.5-6.5v6.5h6.5 6.5v6.5 6.5 6.5 6.5 6.5 6.5h6.5 6.5v6.5 6.5h6.5v6.5 6.5 6.5 6.5h6.5v6.5h-6.5-6.5v6.5h6.5 6.5v6.5h-6.5-6.5-6.5-6.5v-6.5-6.5h6.5v-6.5-6.5-6.5h-6.5-6.5-6.5v6.5 6.5h6.5v6.5 6.5 6.5 6.5h6.5 6.5v6.5h6.5 6.5 6.5v-6.5h6.5v-6.5h6.5v-6.5-6.5-6.5-6.5-6.5-6.5-6.5-6.5h-6.5v6.5 6.5 6.5h-6.5v-6.5-6.5-6.5-6.5h6.5 6.5v-6.5-6.5-6.5-6.5-6.5z"
				fill="#eaeaea"
			/>
			<path d="m117 65v6.5h6.5v-6.5-6.5h-6.5z" fill="#b5b5b5" />
			<path d="m117 52h6.5v6.5h-6.5z" fill="#d8d8d8" />
			<path d="m117 45.5v6.5h6.5 6.5v-6.5h-6.5z" fill="#b5b5b5" />
			<path d="m104 71.5v6.5 6.5h-6.5v6.5h6.5 6.5 6.5v-6.5h-6.5v-6.5-6.5-6.5-6.5h-6.5v6.5z" fill="#b5b5b5" />
			<path d="m97.5 58.5v6.5 6.5 6.5 6.5h6.5v-6.5-6.5-6.5-6.5-6.5h-6.5z" fill="#d8d8d8" />
			<path d="m91 110.5h-6.5v6.5h6.5 6.5v-6.5z" fill="#d8d8d8" />
			<path d="m91 97.5v6.5h6.5 6.5 6.5 6.5v-6.5h-6.5-6.5-6.5z" fill="#b5b5b5" />
			<path d="m91 91v6.5h6.5 6.5v-6.5h-6.5z" fill="#d8d8d8" />
			<path d="m97.5 58.5h-6.5v6.5 6.5h6.5v-6.5z" fill="#b5b5b5" />
			<path d="m97.5 52h-6.5-6.5v6.5h6.5 6.5z" fill="#eaeaea" />
			<path d="m84.5 52h6.5 6.5 6.5v-6.5h-6.5-6.5-6.5z" fill="#b5b5b5" />
			<path d="m78 110.5h6.5v6.5h-6.5z" fill="#b5b5b5" />
			<path
				d="m84.5 104v-6.5-6.5-6.5h-6.5v-6.5-6.5h6.5 6.5v-6.5-6.5h-6.5v-6.5-6.5h6.5v-6.5-6.5-6.5-6.5-6.5-6.5h-6.5-6.5v6.5h-6.5v6.5 6.5h-6.5v6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5h6.5z"
				fill="#d8d8d8"
			/>
			<path
				d="m78 97.5h-6.5v-6.5-6.5-6.5-6.5-6.5-6.5h-6.5v-6.5-6.5-6.5-6.5-6.5h6.5v-6.5-6.5h6.5v-6.5-6.5h-6.5v6.5h-6.5v6.5h-6.5v6.5h-6.5v6.5 6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5h6.5v6.5h6.5v-6.5z"
				fill="#b5b5b5"
			/>
			<path d="m58.5 71.5h-6.5v6.5 6.5h6.5v-6.5z" fill="#d8d8d8" />
			<path d="m58.5 65v-6.5h-6.5v6.5 6.5h6.5z" fill="#eaeaea" />
			<path
				d="m123.5 91h-6.5-6.5-6.5-6.5-6.5-6.5-6.5v-6.5h-6.5v-6.5h-6.5v-6.5-6.5-6.5h-6.5v6.5 6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5 6.5 6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5h6.5v6.5 6.5h6.5 6.5v6.5h6.5 6.5 6.5 6.5v-6.5h6.5v-6.5h6.5v-6.5-6.5-6.5h-6.5v-6.5h6.5v-6.5-6.5-6.5-6.5h-6.5v-6.5h6.5v-6.5-6.5-6.5zm-6.5 13h-26v-6.5h26z"
				fill={color1}
			/>
			<path d="m104 130h6.5v6.5h-6.5z" fill="none" />
			<path d="m97.5 117v6.5h6.5v6.5h6.5v-6.5-6.5h-6.5z" fill={color2} />
			<path d="m78 136.5h6.5v6.5h-6.5z" fill={color2} />
			<path d="m104 136.5v6.5 6.5h6.5v-6.5h6.5v-6.5-6.5h-6.5v6.5z" fill={color2} />
			<path d="m110.5 91h-6.5v6.5h6.5 6.5v-6.5z" fill={color2} />
			<path d="m91 130v-6.5h-6.5v6.5 6.5h6.5z" fill={color2} />
			<path d="m97.5 130h6.5v6.5h-6.5z" fill={color2} />
			<path d="m91 169v6.5h6.5v-6.5-6.5h-6.5z" fill={color2} />
			<path d="m78 169h6.5v6.5h-6.5z" fill={color2} />
			<path d="m84.5 156h6.5v-6.5-6.5h-6.5v6.5z" fill={color2} />
			<path
				d="m78 97.5v6.5 6.5h6.5v-6.5h6.5v-6.5h-6.5v-6.5h-6.5v-6.5h-6.5v-6.5h-6.5v-6.5-6.5-6.5h-6.5v6.5 6.5 6.5 6.5 6.5h6.5 6.5v6.5z"
				fill={color2}
			/>
			<path d="m104 104h-6.5v6.5h6.5 6.5v6.5h6.5v-6.5-6.5h-6.5z" fill={color2} />
			<path d="m97.5 156h6.5v6.5h-6.5z" fill={color2} />
			<path d="m97.5 175.5h6.5v6.5h-6.5z" fill={color2} />
			<path d="m91 110.5h6.5v6.5h-6.5z" fill="none" />
			<path d="m78 136.5h6.5v6.5h-6.5z" fill="none" />
			<path d="m97.5 162.5h-6.5v-6.5h6.5v-6.5h-6.5v-6.5h-6.5v6.5 6.5 6.5 6.5h6.5v6.5h6.5v-6.5z" fill="none" />
			<g fill={color3}>
				<path d="m91 117h6.5v6.5h-6.5z" />
				<path d="m84.5 136.5h6.5v6.5h-6.5z" />
				<path d="m84.5 156v-6.5-6.5h-6.5v-6.5h6.5v-6.5-6.5-6.5h6.5v-6.5h6.5v-6.5h-6.5-6.5v6.5h-6.5v-6.5-6.5h-6.5v-6.5h-6.5-6.5v6.5h6.5v6.5 6.5 6.5 6.5 6.5 6.5 6.5 6.5h6.5v6.5 6.5h6.5v6.5h6.5v-6.5z" />
				<path d="m104 162.5h-6.5v6.5 6.5h6.5v-6.5z" />
				<path d="m91 143h6.5v6.5h-6.5z" />
				<path d="m97.5 110.5h6.5v6.5h-6.5z" />
				<path d="m97.5 97.5h6.5v-6.5h-6.5-6.5-6.5v6.5h6.5z" />
				<path d="m91 130h6.5v6.5h-6.5z" />
				<path d="m84.5 169h6.5v6.5h-6.5z" />
				<path d="m110.5 169h6.5v6.5h-6.5z" />
				<path d="m97.5 149.5h6.5v6.5h-6.5z" />
				<path d="m91 156h6.5v6.5h-6.5z" />
				<path d="m110.5 156h6.5v6.5h-6.5z" />
				<path d="m91 175.5h6.5v6.5h-6.5z" />
				<path d="m104 175.5h6.5v6.5h-6.5z" />
			</g>
		</svg>
	);
};

export default FacialHair4Thumb;

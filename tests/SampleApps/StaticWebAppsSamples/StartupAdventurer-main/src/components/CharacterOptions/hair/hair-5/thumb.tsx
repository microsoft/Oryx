import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair5Thumb = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;
	return (
		<svg version="1.1" id="hair-5-thumb" x="0px" y="0px" viewBox="0 0 182 182">
			<path
				d="m133 70v-7h-7v-7h-7-7-7-7-7-7v7h7 7v7 7 7 7 7 7h7 7v7 7h7v7 7 7 7h7v7h-7-7v7h7 7v7h-7-7-7-7v-7-7h7v-7-7-7h-7-7-7v7 7h7v7 7 7 7h7 7v7h7 7 7v-7h7v-7h7v-7-7-7-7-7-7-7-7h-7v7 7 7h-7v-7-7-7-7h7 7v-7-7-7-7-7z"
				fill="#eaeaea"
			/>
			<path d="m126 126v7h7v-7-7h-7z" fill="#b5b5b5" />
			<path d="m126 112h7v7h-7z" fill="#d8d8d8" />
			<path d="m126 105v7h7 7v-7h-7z" fill="#b5b5b5" />
			<path d="m112 133v7 7h-7v7h7 7 7v-7h-7v-7-7-7-7h-7v7z" fill="#b5b5b5" />
			<path d="m105 119v7 7 7 7h7v-7-7-7-7-7h-7z" fill="#d8d8d8" />
			<path d="m98 175h-7v7h7 7v-7z" fill="#d8d8d8" />
			<path d="m98 161v7h7 7 7 7v-7h-7-7-7z" fill="#b5b5b5" />
			<path d="m98 154v7h7 7v-7h-7z" fill="#d8d8d8" />
			<path d="m105 119h-7v7 7h7v-7z" fill="#b5b5b5" />
			<path d="m105 112h-7-7v7h7 7z" fill="#eaeaea" />
			<path d="m91 112h7 7 7v-7h-7-7-7z" fill="#b5b5b5" />
			<path d="m84 175h7v7h-7z" fill="#b5b5b5" />
			<path
				d="m91 168v-7-7-7h-7v-7-7h7 7v-7-7h-7v-7-7h7v-7-7-7-7-7-7h-7-7v7h-7v7 7h-7v7 7 7 7 7h7v7 7 7 7 7 7h7v7 7h7z"
				fill="#d8d8d8"
			/>
			<path
				d="m84 161h-7v-7-7-7-7-7-7h-7v-7-7-7-7-7h7v-7-7h7v-7-7h-7v7h-7v7h-7v7h-7v7 7 7 7 7 7h7v7 7 7 7 7h7v7 7h7v7h7v-7z"
				fill="#b5b5b5"
			/>
			<path d="m63 133h-7v7 7h7v-7z" fill="#d8d8d8" />
			<path d="m63 126v-7h-7v7 7h7z" fill="#eaeaea" />
			<path
				d="m70 112v-7h7v-7-7-7h7v-7h7 7 7 7 7 7 7v7h7v7 7 7 7 7 7 7 7 7 7 7h7v-7h7v-7h7v-7h7v-7h7v-7-7h7v-7h-7v-7h7v-7-7-7h-7v-7h7v-7h-7v-7-7h-7v-7-7-7h-7v-7h-7-7v-7-7h-7v7h-7v-7-7h-7-7-7v-7h-7v7h-7v-7h-7v7h-7v-7h-7v7h-7-7-7v7h-7-7v7h-7v7h-7v7h-7v7 7h-7v7 7h-7v7 7h-7v7h7v7h-7v7h7v7 7h7v7h-7v7h7v7 7h7v7h7v7h7v7h7v7h7 7v7h7 7 7v-7h-7v-7-7h-7v-7h-7v-7-7-7-7h14z"
				fill={color1}
			/>
			<g fill={color2}>
				<path d="m21 42h7v7h-7z" />
				<path d="m105 42h7v7h-7z" />
				<path d="m84 14v-7-7h-7v7 7h-7v-7h-7v7h-7-7v7 7h7v-7h7v7h-7v7h7 7v7 7h7v7h-7v-7h-7v-7h-7v-7h-7v-7h-7-7-7v7 7h7v-7h7v7h7v7h7v7h7v7h-7-7v7h7v7h7v-7h7v-7h7v7h-7v7h-7v7 7h-7v7h7v7h7 7v-7-7-7h7v-7h7 7v-7h-7v-7h7v-7h-7v-7h7v-7h-7v-7h7v-7h-7v-7h-7z" />
				<path d="m91 14h7v7h-7z" />
				<path d="m91 0h7v7h-7z" />
				<path d="m42 84h7v7h-7z" />
				<path d="m28 84h7v7h-7z" />
				<path d="m126 7h-7v7h7 7v-7z" />
				<path d="m112 14v-7-7h-7v7h-7v7h7z" />
				<path d="m35 105h7v7h-7z" />
				<path d="m49 98h7v7h-7z" />
				<path d="m49 77h7v7h-7z" />
				<path d="m35 77h7v7h-7z" />
				<path d="m28 56h7v7h-7z" />
				<path d="m14 56h7v7h-7z" />
				<path d="m119 56v7h7 7v-7h-7z" />
				<path d="m105 56h7v7h-7z" />
				<path d="m49 49h-7-7v7h7v7h7v-7z" />
				<path d="m112 28h7v7h-7z" />
				<path d="m28 70h7v7h-7z" />
				<path d="m7 70h7v7h-7z" />
			</g>
			<path d="m35 77h7v7h-7z" fill="none" />
			<path d="m28 56h7v7h-7z" fill="none" />
			<path d="m49 77h7v7h-7z" fill="none" />
			<path d="m49 98h7v7h-7z" fill="none" />
			<path d="m28 70h7v7h-7z" fill="none" />
			<path d="m42 84h7v7h-7z" fill="none" />
			<path d="m35 105h7v7h-7z" fill="none" />
			<path d="m28 84h7v7h-7z" fill="none" />
			<path d="m35 49v7h7v7h7v-7-7h-7z" fill="none" />
			<path d="m63 70h7v7h-7z" fill={color3} />
			<path d="m56 21h7v7h-7z" fill={color3} />
			<path d="m49 14h-7v7h-7v7h7 7v-7z" fill={color3} />
			<path d="m70 63h7v7h-7z" fill={color3} />
			<path d="m56 7h7v7h-7z" fill={color3} />
			<path d="m70 7h7v7h-7z" fill={color3} />
			<path d="m21 35h7v7h-7z" fill={color3} />
			<path d="m70 35h-7-7v7h7v7h7v-7z" fill={color3} />
			<path
				d="m56 84h-7v7h-7v-7h-7v7h-7v-7h7v-7h-7v-7h7v7h7v7h7v-7h7v-7h-7v-7h-7v-7h-7v7h-7v-7h7v-7h7 7v-7h-7v-7h-7v7h-7v7h-7-7v7h7v7h-7-7v7h7v7h-7-7v7h7v7h-7v7h7v7 7h7v7h-7v7h7v7 7h7v7h7v7h7v7h7v7h7 7v7h7 7 7v-7h-7v-7-7h-7v-7h-7v-7-7-7-7h7 7v-7-7h-7v-7h-7v7h-7v-7h7v-7h7v-7-7h-7zm-14 28h-7v-7h7z"
				fill={color3}
			/>
			<path d="m49 63h7 7v-7h-7v-7h-7v7z" fill={color3} />
			<path d="m70 49h7v7h-7z" fill={color3} />
			<path d="m49 28h7v7h-7z" fill={color3} />
		</svg>
	);
};

export default Hair5Thumb;

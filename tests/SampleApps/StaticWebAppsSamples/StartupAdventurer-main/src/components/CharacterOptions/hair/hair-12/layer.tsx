import React from "react";
import { defaultColor } from "../colors";
import { Colors } from "@/interfaces/Colors";

interface IProps {
	colors?: Colors;
}

const Hair12 = ({ colors = defaultColor }: IProps = {}) => {
	return (
		<g id="hair_12" data-name="hair_12">
			<path
				d="m470.94 150v-10h-10v-10-10h-10v-10h-10v-10h-10v-10-10h-10v-10h-10-10-10v10h-10v10 10h-10-10v10h-10v10h-10v10 10h-10v10 10 10 10 10 10 10 10h10v-10-10-10h10 10v-10h10v-10-10h10v-10h10v-10h10v-10h10 10v10h10 10v10h10v10h10v10 10 10 10h10v-10-10-10-10z"
				fill="#005244"
			/>
			<g fill="#006859">
				<path d="m400.94 80h-10v10 10h10v10h10v-10-10h-10z" />
				<path d="m350.94 160v10 10 10h10v-10-10-10-10h-10z" />
				<path d="m360.94 120v10 10h10v-10h10v-10h-10z" />
				<path d="m420.94 120h10v10h-10z" />
			</g>
			<path
				d="m470.94 150v-10h-10v-10-10h-10v-10h-10v-10h-10v-10-10h-10v-10h-10v10h-10v10h10v10 10h10 10v10 10h10v10h10v10h10v10 10 10 10 10h10v-10-10-10-10z"
				fill="#008272"
			/>
			<path
				d="m390.94 120h-10v10h-10v10h-10v10 10 10 10h10v-10-10h10v-10h10v-10h10v-10h10v-10h-10z"
				fill="#008272"
			/>
		</g>
	);
};

export default Hair12;

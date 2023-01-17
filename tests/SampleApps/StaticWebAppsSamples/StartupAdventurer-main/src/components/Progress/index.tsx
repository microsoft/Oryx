import React from "react";
import { ProgressBar } from "./styles";

const Progress = ({ step = 1 }) => {
	return (
		<ProgressBar className="progress-bar">
			<svg fill="none" height="87" viewBox="0 0 532 87" width="532" xmlns="http://www.w3.org/2000/svg">
				<g fill="#fff">
					<path d="m0 14h7v59h-7z" />
					<path d="m7 7h7v7h-7z" />
					<path d="m7 73h7v7h-7z" />
					<path d="m0 0h7v59h-7z" transform="matrix(-1 0 0 1 532 14)" />
					<path d="m0 0h7v7h-7z" transform="matrix(-1 0 0 1 525 7)" />
					<path d="m0 0h7v7h-7z" transform="matrix(-1 0 0 1 525 73)" />
					<path d="m14 0h504v7h-504z" />
					<path d="m14 80h504v7h-504z" />
					<path d="m28 28h100v31h-100z" opacity={step >= 1 ? 1 : 0.3} />
					<path d="m153.25 28h100v31h-100z" opacity={step >= 2 ? 1 : 0.3} />
					<path d="m278.5 28h100v31h-100z" opacity={step >= 3 ? 1 : 0.3} />
					<path d="m403.75 28h100v31h-100z" opacity={step >= 4 ? 1 : 0.3} />
				</g>
			</svg>
		</ProgressBar>
	);
};

export default Progress;

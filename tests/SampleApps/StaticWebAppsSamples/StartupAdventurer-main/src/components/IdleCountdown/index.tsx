import React, { useState } from "react";
import useInterval from "@/hooks/use-interval";
import { Remaining } from "./styles";
import noop from "lodash-es/noop";

const IdleCountdown = ({ afterComplete = noop, timeout = 30 }) => {
	const [remaining, setRemaining] = useState(timeout);
	const [running, setRunning] = useState(true);
	const [isCritical, setCritical] = useState(false);

	useInterval(
		() => {
			if (remaining === 0) {
				setRunning(false);
				afterComplete();
			} else {
				if (remaining <= 6) {
					setCritical(true);
				}
				setRemaining(s => s - 1);
			}
		},
		running ? 1000 : null
	);

	return (
		<Remaining className={isCritical ? "critical" : ""}>
			Returning to idle screen in {remaining}
			<small>s</small>
			<br />
			Touch screen to continue
		</Remaining>
	);
};

export default IdleCountdown;

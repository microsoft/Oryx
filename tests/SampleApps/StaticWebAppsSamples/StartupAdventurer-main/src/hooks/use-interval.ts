import { useRef, useEffect } from "react";
import noop from "lodash-es/noop";

const useInterval = (callback: typeof noop, delay: number | null) => {
	const savedCallback = useRef(noop);

	// Remember the latest function.
	useEffect(() => {
		savedCallback.current = callback;
	}, [callback]);

	// Set up the interval.
	useEffect(() => {
		function tick() {
			savedCallback.current();
		}
		if (delay !== null) {
			let id = setInterval(tick, delay);
			return () => clearInterval(id);
		}
	}, [delay]);
};

export default useInterval;

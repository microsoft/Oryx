import { useEffect, RefObject } from "react";

const useOnClickOutside = (ref: RefObject<HTMLElement>, handler: (e?: MouseEvent | TouchEvent) => void) => {
	useEffect(() => {
		const listener = (event: MouseEvent | TouchEvent) => {
			const target = event.target as HTMLElement;
			// Do nothing if clicking ref's element or descendent elements
			if (!ref.current || ref.current.contains(target)) {
				return;
			}

			handler(event);
		};

		document.addEventListener("mousedown", listener);
		document.addEventListener("touchstart", listener);

		return () => {
			document.removeEventListener("mousedown", listener);
			document.removeEventListener("touchstart", listener);
		};
	}, [ref, handler]);
};

export default useOnClickOutside;

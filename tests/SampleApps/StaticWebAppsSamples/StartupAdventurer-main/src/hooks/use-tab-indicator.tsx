import { useEffect, RefObject, useCallback } from "react";

function useTabIndicator(buttonContainerRef: RefObject<HTMLElement>, indicatorRef: RefObject<HTMLElement>) {
	const setIndicatorPosition = useCallback(() => {
		const { current: container } = buttonContainerRef;
		const { current: indicator } = indicatorRef;
		const activeButton = container ? (container.querySelector(".tab-button--active") as HTMLElement) : null;

		if (activeButton && indicator) {
			const buttonLeft = activeButton.offsetLeft;
			const buttonWidth = activeButton.offsetWidth;
			const indicatorWidth = indicator.offsetWidth;
			const deltaWidth = buttonWidth / indicatorWidth;

			indicator.style.transform = `translateX(${buttonLeft}px) scaleX(${deltaWidth})`;
		}
	}, [buttonContainerRef, indicatorRef]);

	const onContainerClick = useCallback(
		({ target }: any) => {
			requestAnimationFrame(() => {
				if (target.className.indexOf("tab-button") !== -1) {
					setIndicatorPosition();
				}
			});
		},
		[setIndicatorPosition]
	);

	useEffect(() => {
		let initTimeout: number;

		const indicator = indicatorRef.current;

		if (indicator) {
			initTimeout = window.setTimeout(() => {
				setIndicatorPosition();
				if (indicator) indicator.style.opacity = "1";
			}, 300);

			return () => {
				clearTimeout(initTimeout);
				if (indicator) indicator.style.opacity = "0";
			};
		}
	}, [indicatorRef, setIndicatorPosition]);

	useEffect(() => {
		const buttonContainer = buttonContainerRef.current;
		if (buttonContainer) {
			buttonContainer.style.position = "relative";
			buttonContainer.addEventListener("click", onContainerClick);

			return () => {
				if (buttonContainer) buttonContainer.removeEventListener("click", onContainerClick);
			};
		}
	}, [buttonContainerRef, onContainerClick]);
}

export default useTabIndicator;

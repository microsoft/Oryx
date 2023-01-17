import React, { useEffect, useCallback } from "react";
import { ConfiguratorWrapper, Spotlight } from "./styles";
import StartOver from "@/components/StartOver";
import OptionPanels from "@/components/OptionPanels";
import Character from "@/components/Character";
import StepperFooter from "@/components/Stepper/StepperFooter";
import anime from "animejs";
import EventEmitter from "@/utils/event-emitter";
import Lightbeam from "@/graphics/lightbeam";
import Floorlight from "@/graphics/floorlight";
import { Dispatch } from "redux";
import { useDispatch, useSelector } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";
import { characterActions } from "@/redux/character";

const Configurator = () => {
	const dispatch: Dispatch = useDispatch();
	const { viewedOptionTabs } = useSelector((store: IStoreState) => store.character);

	const animateOptions = useCallback(() => {
		anime.set(".option-panel", { opacity: 1 });
		anime({
			targets: ".options-wrapper button, .options-wrapper .style-thumbnail, .options-wrapper h2",
			opacity: [0, 1],
			translateY: [70, 0],
			delay: anime.stagger(10),
			duration: 160,
			easing: "easeOutExpo",
			complete: () => animateCharacter(),
		});
	}, []);

	const animateCharacter = () => {
		anime({
			targets: ".character-container",
			filter: "brightness(1)",
			duration: 1000,
			easing: "linear",
		});
		anime({
			targets: ".spotlight svg",
			opacity: 1,
			duration: 1000,
			easing: "linear",
		});
	};

	const animateInitial = useCallback(() => {
		anime.set(".option-panel", { opacity: 0, translateY: 50, willChange: "transform, opacity" });
		anime.set(".option-tabs .tab-button", { translateX: "100%" });
		anime.set(".options-wrapper button, .options-wrapper .style-thumbnail, .options-wrapper h2", {
			opacity: 0,
			translateY: 70,
		});
		const tl = anime.timeline({
			easing: "easeInOutExpo",
		});
		tl.add({
			targets: ".option-panel",
			translateY: 0,
			opacity: 1,
			complete: () => animateOptions(),
		});
		tl.add({
			targets: ".option-tabs .tab-button",
			opacity: 1,
			translateX: 0,
			delay: anime.stagger(100),
		});
		anime({
			targets: ".view-quide-text",
			opacity: [0, 1],
			translateY: [30, 0],
			easing: "linear",
			duration: 500,
		});
	}, [animateOptions]);

	const beforeNext = () =>
		new Promise<void>((resolve) => {
			anime({
				targets: ".option-panel > *",
				opacity: [1, 0],
				duration: 350,
				easing: "linear",
			});
			anime({
				targets: ".view-quide-text",
				opacity: [1, 0],
				translateY: [0, -30],
				easing: "linear",
				duration: 350,
			});
			anime({
				targets: ".option-tabs .tab-button",
				opacity: [1, 0],
				translateX: [0, "100%"],
				easing: "linear",
				delay: anime.stagger(50),
				duration: (el: any, i: number) => i * 150 + 150,
				complete: () => resolve(),
			});
		});

	const hasViewedSomeOtherOptionTab = ["top", "bottom", "accessories"].some((key) => viewedOptionTabs.includes(key));

	useEffect(() => {
		animateInitial();
		dispatch(characterActions.setViewedTab("body"));

		EventEmitter.subscribe("tabChange", ({ id, tab }: { id: string; tab: string }) => {
			if (id === "option-panels") {
				dispatch(characterActions.setViewedTab(tab));
				requestAnimationFrame(animateOptions);
			}
		});

		return () => {
			anime.set(".character-container", { filter: "brightness(1)" });
			anime.set(".spotlight svg", { opacity: 1 });
		};
	}, [animateInitial, animateOptions, dispatch]);

	return (
		<div>
			<p className="view-quide-text">Explore and configure!</p>
			<Spotlight className="spotlight">
				<Lightbeam className="lightbeam" />
				<Floorlight className="floorlight" />
			</Spotlight>
			<ConfiguratorWrapper>
				<StartOver />
				<Character />
				<OptionPanels />
			</ConfiguratorWrapper>
			<StepperFooter
				nextHtml="Ready? Add some stats"
				beforeNext={beforeNext}
				nextDisabled={!hasViewedSomeOtherOptionTab}
			/>
		</div>
	);
};

export default Configurator;

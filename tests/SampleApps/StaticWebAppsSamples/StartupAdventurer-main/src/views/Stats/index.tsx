import React, { Fragment, useState, useEffect, useCallback } from "react";
import { StatsWrapper, Spotlight } from "./styles";
import StartOver from "@/components/StartOver";
import Character from "@/components/Character";
import StatPanels from "@/components/StatPanels";
import StepperFooter from "@/components/Stepper/StepperFooter";
import { useSelector, useDispatch } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";
import EventEmitter from "@/utils/event-emitter";
import anime from "animejs";
import Lightbeam from "@/graphics/lightbeam";
import Floorlight from "@/graphics/floorlight";
import { Dispatch } from "redux";
import { characterActions } from "@/redux/character";

const Stats = () => {
	const { statPointsAvailable, selectedStats, gradedStats } = useSelector((store: IStoreState) => store.stats);
	const [isDistributeViewed, setDistributeViewed] = useState(false);
	const dispatch: Dispatch = useDispatch();

	const isDone =
		statPointsAvailable === 0 && selectedStats.length === 4 && gradedStats.length === 4 && isDistributeViewed;

	const animateInitial = () => {
		const tl = anime.timeline({ easing: "easeInOutExpo" });
		tl.add({
			targets: ".stats-panel > *",
			opacity: 1,
			translateY: 0,
			delay: anime.stagger(100),
			duration: 400,
		});
		tl.add({
			targets: ".stat-panel-tabs .tab-button",
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
	};

	const animateOptions = useCallback(() => {
		anime.set(".stats-panel > *", { opacity: 1 });
		anime({
			targets: ".option-list button",
			opacity: [0, 1],
			translateY: [50, 0],
			delay: anime.stagger(90),
			duration: 350,
			easing: "easeOutExpo",
		});
	}, []);

	const animateTabs = useCallback(() => {
		anime.set(".option-list button", { opacity: 0 });
		animateOptions();
	}, [animateOptions]);

	const animateStatBoxes = () => {
		anime({
			targets: ".skill-container",
			opacity: [0, 1],
			translateY: [70, 0],
			delay: anime.stagger(100),
			duration: 350,
			easing: "easeOutExpo",
		});
	};

	const beforeNext = async () => {
		const time = new Date().toJSON();
		dispatch(characterActions.setEndTime(time));

		anime.set("span.flash", {
			background: "#FFFFFF",
			height: "100%",
			left: 0,
			opacity: 0,
			position: "absolute",
			top: 0,
			width: "100%",
			zIndex: 10000,
		});

		await anime({
			targets: ".flash",
			opacity: [0, 1, 0.6, 1],
			easing: "easeOutExpo",
			duration: 350,
		}).finished;
	};

	useEffect(() => {
		let rendering = true;
		anime.set(".character-container", { filter: "brightness(1)" });
		anime.set(".spotlight", { opacity: 1 });
		anime.set(".stat-panel-tabs .tab-button", { opacity: 0, translateX: "100%" });
		anime.set(".stats-panel > *", { opacity: 0, translateY: 40 });

		requestAnimationFrame(animateInitial);

		EventEmitter.subscribe("tabChange", ({ id, tab }: { id: string; tab: string }) => {
			if (id === "stats") {
				if (tab === "choose") requestAnimationFrame(animateTabs);
				if (tab === "distribute") {
					if (rendering) setDistributeViewed(true);
					requestAnimationFrame(animateStatBoxes);
				}
			}

			if (id === "stat-categories") {
				requestAnimationFrame(animateOptions);
			}
		});

		return () => {
			rendering = false;
		};
	}, [animateTabs, animateOptions]);

	return (
		<Fragment>
			<p className="view-quide-text">What are your strengths?</p>
			<Spotlight className="spotlight">
				<Lightbeam className="lightbeam" />
				<Floorlight className="floorlight" />
			</Spotlight>
			<StartOver />
			<StatsWrapper id="stats-wrapper">
				<Character />
				<StatPanels />
			</StatsWrapper>
			<StepperFooter nextHtml="All done!" nextDisabled={!isDone} beforeNext={beforeNext} />
		</Fragment>
	);
};

export default Stats;

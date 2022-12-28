import React from "react";
import { Slider, Handles, Tracks, Rail, Ticks, SliderItem, GetTrackProps } from "react-compound-slider";
import { colors } from "@/utils/style-utils";
import { SliderContainer } from "./styles";
import noop from "lodash-es/noop";
import clsx from "clsx";

interface IHandleProps {
	handle: {
		id: string;
		value: string | number;
		percent?: number;
	};
	getHandleProps: Function;
	[key: string]: any;
}

const valueMap: { [key: string]: string } = {
	"0": "1-5",
	"20": "6-10",
	"40": "11-25",
	"60": "26-50",
	"80": "51-100",
	"100": "100+",
};

const Handle = ({ handle: { id, value, percent }, getHandleProps }: IHandleProps) => {
	return (
		<div
			style={{
				backgroundColor: colors.darkBlue,
				border: `7px solid ${colors.white}`,
				color: "#333",
				cursor: "pointer",
				height: 90,
				left: `${percent}%`,
				position: "absolute",
				textAlign: "center",
				top: "50%",
				transform: "translate(-50%, -50%)",
				width: 90,
				zIndex: 2,
			}}
			className="slider-handle"
			tabIndex={0}
			{...getHandleProps(id)}
		>
			<span className="tooltip">
				{valueMap[`${value}`]}
				<span className="tooltip-tip">
					<svg width="49" height="35" viewBox="0 0 49 35" fill="none" xmlns="http://www.w3.org/2000/svg">
						<rect x="49" y="21" width="7" height="49" transform="rotate(90 49 21)" fill="white" />
						<rect x="42" y="14" width="7" height="35" transform="rotate(90 42 14)" fill="white" />
						<rect x="35" y="7" width="7" height="21" transform="rotate(90 35 7)" fill="white" />
						<rect x="49" y="28" width="7" height="49" transform="rotate(90 49 28)" fill="white" />
						<rect x="7" y="21" width="7" height="7" transform="rotate(90 7 21)" fill="#D6D6D6" />
						<rect x="49" y="21" width="7" height="7" transform="rotate(90 49 21)" fill="#D6D6D6" />
						<rect x="14" y="14" width="7" height="7" transform="rotate(90 14 14)" fill="#D6D6D6" />
						<rect x="42" y="14" width="7" height="7" transform="rotate(90 42 14)" fill="#D6D6D6" />
						<rect x="21" y="7" width="7" height="7" transform="rotate(90 21 7)" fill="#D6D6D6" />
						<rect x="35" y="7" width="7" height="7" transform="rotate(90 35 7)" fill="#D6D6D6" />
						<rect x="28" width="7" height="7" transform="rotate(90 28 0)" fill="#D6D6D6" />
					</svg>
				</span>
			</span>
		</div>
	);
};

const Track = ({
	source,
	target,
	getTrackProps,
}: {
	source: SliderItem;
	target: SliderItem;
	getTrackProps: GetTrackProps;
}) => {
	return (
		<div
			className="slider-touchable-track slider-track"
			style={{
				left: `${source.percent}%`,
				width: `${target.percent - source.percent}%`,
			}}
			{...getTrackProps()}
		/>
	);
};

interface ITick {
	tick: { id: string; value: number; percent: number };
}

const Tick = ({ tick }: ITick) => {
	return (
		<span
			className="tick"
			data-tick={tick.id}
			style={{
				left: `${tick.percent}%`,
			}}
		/>
	);
};

const CompanySizeRange = ({ onChange = noop }: { onChange: typeof noop }) => {
	const onValueChange = (e: any) => {
		const [value] = e;
		const returnValue = valueMap[`${value}`] || null;
		onChange(returnValue);
	};

	return (
		<div>
			<SliderContainer>
				<span className="spacer blue"></span>
				<Slider values={[40]} step={20} domain={[0, 100]} className="slider" onChange={onValueChange}>
					<Rail>
						{({ getRailProps }) => (
							<div className="slider-touchable-track slider-rail" {...getRailProps()} />
						)}
					</Rail>

					<Handles>
						{({ handles, getHandleProps, activeHandleID }) => (
							<div className={clsx(activeHandleID && "is-touching", "slider-handles")}>
								{handles.map(handle => (
									<Handle key={handle.id} handle={handle} getHandleProps={getHandleProps} />
								))}
							</div>
						)}
					</Handles>
					<Tracks right={false}>
						{({ tracks, getTrackProps }) => (
							<div className="slider-tracks">
								{tracks.map(({ id, source, target }) => (
									<Track key={id} source={source} target={target} getTrackProps={getTrackProps} />
								))}
							</div>
						)}
					</Tracks>
					<Ticks values={[0, 20, 40, 60, 80, 100]}>
						{({ ticks }) => (
							<div className="slider-ticks" style={{ pointerEvents: "none" }}>
								{ticks.map(tick => (
									<Tick key={tick.id} tick={tick} />
								))}
							</div>
						)}
					</Ticks>
				</Slider>
				<span className="spacer white"></span>
			</SliderContainer>
		</div>
	);
};

export default CompanySizeRange;

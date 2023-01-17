import React, { Fragment, useState } from "react";
import { Thumbnail, Container, Swatches, ColorSwatch, Color, Wrapper, Header, Title, Deselector } from "./styles";
import noop from "lodash-es/noop";
import HorizontalScroller from "@/components/HorizontalScroller";
import { IColorOptions, IColorSet } from "@/interfaces/Colors";
import clsx from "clsx";

interface IProps {
	activeColor?: IColorSet;
	onColorClicked: (color: IColorSet) => void;
	onResetClicked?: () => void;
	onThumbClicked?: () => void;
	thumbnail?: Function;
	colors: IColorOptions;
	horizontal?: boolean;
	title: string;
	showTitle?: boolean;
	withHeader?: boolean;
	withSwatchContainer?: boolean;
	withResetButton?: boolean;
	className?: string;
}

const OptionColorSelectable = (props: IProps) => {
	const {
		title,
		onColorClicked = noop,
		thumbnail,
		onResetClicked = noop,
		onThumbClicked = noop,
		activeColor = null,
		colors = {},
		horizontal = false,
		withHeader = true,
		showTitle = false,
		withSwatchContainer = true,
		withResetButton = true,
		className,
	} = props;
	const isActive = (name: string): boolean => !!activeColor && activeColor.name === name;
	const isActiveFromColors = !!activeColor && !!colors[activeColor.name];

	const ContainerElement = horizontal ? HorizontalScroller : Container;
	const SwatchContainer = withSwatchContainer ? Swatches : Fragment;

	let buttons: HTMLElement[] = [];
	const setButtonRef = (button: HTMLButtonElement) => buttons.push(button);

	const [focusedIndex, setFocusedIndex] = useState<number | undefined>(undefined);

	const setInitialFocus = (e: React.FocusEvent<HTMLDivElement>) => {
		if (focusedIndex !== undefined) {
			return;
		}
		let selectedIndex = activeColor ? Object.keys(colors).indexOf(activeColor.name) : 0;
		if (selectedIndex > -1) {
			buttons[selectedIndex].focus();
			setFocusedIndex(selectedIndex);
		}
	};

	const keyPress = (event: React.KeyboardEvent<HTMLElement>, index: number) => {
		switch (event.key) {
			case "ArrowRight":
				event.preventDefault();
				if (index < buttons.length - 1) {
					const nextButton = buttons[index + 1];
					nextButton.focus();
					setFocusedIndex(index + 1);
				}
				break;
			case "ArrowLeft":
				event.preventDefault();
				if (index > 0) {
					const prevButton = buttons[index - 1];
					prevButton.focus();
					setFocusedIndex(index - 1);
				}
				break;
			case "Home":
				event.preventDefault();
				const firstButton = buttons[0];
				setFocusedIndex(0);
				firstButton.focus();
				break;
			case "End":
				event.preventDefault();
				const lastButton = buttons[buttons.length - 1];
				setFocusedIndex(buttons.length - 1);
				lastButton.focus();

				break;
			default:
				break;
		}
	};
	const onBlur = (e: React.FocusEvent<HTMLDivElement>) => {
		const newFocus: (EventTarget & HTMLElement) | null = e.relatedTarget as EventTarget & HTMLElement;
		if (e.relatedTarget && buttons.indexOf(newFocus) < 0) {
			setFocusedIndex(undefined);
		}
	};

	return (
		<Wrapper
			hasThumbnail={!!thumbnail}
			isHorizontal={horizontal}
			className={className}
			tabIndex={focusedIndex ? -1 : 0}
			onFocus={setInitialFocus}
			onBlur={onBlur}
		>
			{withHeader && (
				<Header>
					{showTitle && <Title>{title}</Title>}
					{withResetButton && !!onResetClicked && (
						<Deselector disabled={!isActiveFromColors} onClick={onResetClicked}>
							Deselect
						</Deselector>
					)}
				</Header>
			)}
			<ContainerElement>
				{!!thumbnail && (
					<Thumbnail active={isActiveFromColors} onClick={onThumbClicked}>
						{thumbnail(!!activeColor ? { colors: activeColor.palette } : undefined)}
					</Thumbnail>
				)}
				<SwatchContainer>
					{Object.keys(colors).map((key: string, index: number) => (
						<ColorSwatch
							tabIndex={-1}
							ref={setButtonRef}
							onKeyDown={(e) => keyPress(e, index)}
							key={key}
							onClick={() => onColorClicked({ name: key, palette: colors[key] })}
							active={isActive(key)}
							className={clsx(isActive(key) && "is-active-button")}
						>
							{colors[key].slice(0, 3).map((color: string | undefined) => (
								<Color color={color} key={`swtch_${key}_${color}`} />
							))}
						</ColorSwatch>
					))}
				</SwatchContainer>
			</ContainerElement>
		</Wrapper>
	);
};

export default OptionColorSelectable;

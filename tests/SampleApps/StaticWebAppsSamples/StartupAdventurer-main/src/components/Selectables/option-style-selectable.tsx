import React, { useState } from "react";
import { Thumbnail, Wrapper, Header, Title, Deselector, Container } from "./styles";
import isArray from "lodash-es/isArray";
import noop from "lodash-es/noop";
import HorizontalScroller from "@/components/HorizontalScroller";
import { Colors } from "@/interfaces/Colors";

interface IStyle {
  value: string;
  thumb: Function;
}

interface IProps<T = string> {
	onResetClicked: (style: T | T[]) => void;
	onStyleClicked: (style: T) => void;
	selectedStyle?: T | T[];
	styles: IStyle[];
	title: string;
	showTitle?: boolean;
	horizontal?: boolean;
	className?: string;
	thumbColors?: Colors;
	[key: string]: any;
}

const OptionStyleSelectable = <T extends string>(props: IProps<T>) => {
	const {
		styles,
		title,
		onResetClicked = noop,
		onStyleClicked = noop,
		selectedStyle = "",
		horizontal = false,
		showTitle = true,
		className,
		thumbColors,
	} = props;
	const isActive = (value: T) =>
		!!selectedStyle &&
		(!Array.isArray(selectedStyle) ? selectedStyle === value : selectedStyle.indexOf(value) !== -1);
	const isActiveFromStyles =
		!!selectedStyle &&
		(Array.isArray(selectedStyle)
			? selectedStyle.length > 0
			: styles.some((style) => style.value === selectedStyle));

  const ContainerElement = horizontal ? HorizontalScroller : Container;

  const [focusedIndex, setFocusedIndex] = useState<number | undefined>(undefined);
  // const nextOption = (index: number) =>
  const keyPressed = (event: React.KeyboardEvent<HTMLElement>, index: number) => {
    switch (event.key) {
      case "ArrowRight":
        event.preventDefault();
        if (index < styles.length - 1) {
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
        firstButton.focus();
        break;
      case "End":
        event.preventDefault();
        const lastButton = buttons[buttons.length - 1];
        lastButton.focus();

        break;
      default:
        break;
    }
  };

  let buttons: HTMLButtonElement[] = [];
  const setButtonRef = (ref: HTMLButtonElement) => buttons.push(ref);

  const onBlur = (e: React.FocusEvent<HTMLDivElement>) => {
    if (e.relatedTarget) {
      setFocusedIndex(undefined);
    }
  };

  const onClick = (value: string, index: number) => {
    setFocusedIndex(index);
    onStyleClicked(value);
  };

	return (
		<Wrapper
			isHorizontal={horizontal}
			className={className}
			aria-label={title}
			aria-activedescendant={`${className}-${focusedIndex}`}
			tabIndex={0}
			onBlur={onBlur}
			// onFocus={setInitialFocus}
			role="listbox"
			aria-labelledby={`${className}-title`}
			aria-orientation="horizontal"
		>
			<Header>
				{showTitle && <Title id={`${className}-title`}>{title}</Title>}
				<Deselector tabIndex={-1} disabled={!isActiveFromStyles} onClick={() => onResetClicked(selectedStyle)}>
					Deselect
				</Deselector>
			</Header>
			<ContainerElement>
				{styles &&
					isArray(styles) &&
					styles
						.filter((style) => style && typeof style.thumb === "function")
						.map(({ value, thumb }, index) => (
							<Thumbnail
								role="option"
								as="button"
								ref={setButtonRef}
								onClick={() => onClick(value, index)}
								key={value}
								active={isActive(value as T)}
								aria-selected={isActive(value as T)}
								tabIndex={-1}
								onKeyDown={(e: React.KeyboardEvent<HTMLElement>) => keyPressed(e, index)}
								id={`${className}-${index}`}
							>
								{thumb(!!thumbColors ? { colors: thumbColors } : {})}
							</Thumbnail>
						))}
			</ContainerElement>
		</Wrapper>
	);
};

export default OptionStyleSelectable;

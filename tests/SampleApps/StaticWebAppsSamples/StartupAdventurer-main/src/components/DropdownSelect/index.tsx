import React, { RefObject, useRef, useState } from "react";
import noop from "lodash-es/noop";
import kebabCase from "lodash-es/kebabCase";
import useOnClickOutside from "@/hooks/use-click-outside";
import { Dropdown, DropdownToggle, DropdownContent, Option } from "./styles";
import arrow from "./arrow-down.svg";

interface IProps {
	onSelect: (item: string) => void;
	items: string[];
	selectedItems?: string[];
	buttonText?: string | undefined;
	[key: string]: any;
}

const DropdownSelect = ({
	onSelect = noop,
	items = [],
	buttonText = "Select",
	selectedItems = [],
	...restProps
}: IProps) => {
	const [isOpen, setOpen] = useState(false);
	const containerRef: RefObject<HTMLDivElement> = useRef(null);
	const toggle = () => setOpen(state => !state);

	useOnClickOutside(containerRef, () => setOpen(false));

	const selectItem = (item: string) => {
		onSelect(item);
		setOpen(false);
	};

	const isActive = (item: string) => selectedItems.indexOf(item) !== -1;
	const hasSelection = !!selectedItems && selectedItems.length > 0;

	return (
		<Dropdown {...restProps} ref={containerRef} isOpen={isOpen}>
			<DropdownToggle onClick={toggle} hasSelection={hasSelection}>
				<span>{buttonText}</span>
				<span className="arrow">
					<img src={arrow} alt="v" />
				</span>
			</DropdownToggle>
			{isOpen && (
				<DropdownContent>
					{items.map((item: string, i: number) => (
						<Option key={kebabCase(item) + i} onClick={() => selectItem(item)} isActive={isActive(item)}>
							{item}
						</Option>
					))}
				</DropdownContent>
			)}
		</Dropdown>
	);
};

export default DropdownSelect;

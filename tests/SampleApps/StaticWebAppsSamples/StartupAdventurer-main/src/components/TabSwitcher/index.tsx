import React, { memo, useState, createContext, useContext, useRef, useEffect, RefObject } from "react";
import noop from "lodash-es/noop";
import clsx from "clsx";
import EventEmitter from "@/utils/event-emitter";

interface ITabSwitcherContext {
	activeTab: string | number;
	changeTab: typeof noop;
	prevTab: typeof noop;
	nextTab: typeof noop;
	firstTab: typeof noop;
	lastTab: typeof noop;
}

const initialContext: ITabSwitcherContext = {
	activeTab: 1,
	changeTab: noop,
	prevTab: noop,
	nextTab: noop,
	firstTab: noop,
	lastTab: noop
};

const context = createContext(initialContext);

const { Provider } = context;

interface ITabProps {
	id: number | string;
	children: any;
	[key: string]: any;
}

const Tab = (props: ITabProps) => {
	const { id, children, className, ...rest } = props;
	const { changeTab, activeTab, prevTab, nextTab, firstTab, lastTab } = useContext(context);
	const tabRef: RefObject<HTMLButtonElement> = useRef(null);

	const keyPressed = (event: React.KeyboardEvent<HTMLButtonElement>) => {
		switch (event.key) {
			case "ArrowUp":
				prevTab();
				break;
			case "ArrowDown":
				nextTab();
				break;
			case "Home":
				firstTab();
				break;
			case "End":
				lastTab();
				break;
			default:
				break;
		}
	}

	useEffect(() => {
		if (activeTab === id) {
			if (tabRef && tabRef.current) {
				tabRef.current.focus();
			}
		}
	}, [id, activeTab])

	return (
		<button
			ref={tabRef}
			onKeyDown={keyPressed}
			className={clsx(
				"tab-button",
				activeTab === id && " tab-button--active",
				activeTab !== id && " tab-button--inactive",
				"hoverable",
				className
			)}
			role="tab"
			tabIndex={activeTab === id ? 0 : -1}
			aria-selected={activeTab === id}
			aria-controls={`${id}-panel`}
			onClick={({ target }) => changeTab(id, target)}
			{...rest}
		>
			{children}
		</button>
	);
};

interface ITabPanelProps {
	whenActive: number | string;
	children: any;
	[key: string]: any;
}

const TabPanel = ({ whenActive, children, ...rest }: ITabPanelProps) => {
	const { activeTab } = useContext(context);
	return activeTab === whenActive ? children : null;
};

interface ITabSwitcherProps {
	children: any;
	initialTab?: number | string;
	emitChanges?: boolean;
	id?: string;
}

const TabSwitcher = ({ children, initialTab = 1, emitChanges = false, id }: ITabSwitcherProps) => {
	const [activeTab, setActiveTab] = useState(initialTab || 1);

	const tabIDs: string[] = ["body", "top", "bottom", "accessories"]

	const currentTabIndex = () => tabIDs.indexOf(activeTab as string) || 0;
	const prevTab = () => {
		const prevTabIndex: number = currentTabIndex() > 0 ? currentTabIndex() - 1 : tabIDs.length - 1;
		changeTab(tabIDs[prevTabIndex]);
	}

	const nextTab = () => {
		const nextTabIndex: number = currentTabIndex() < tabIDs.length - 1 ? currentTabIndex() + 1 : 0;
		changeTab(tabIDs[nextTabIndex]);
	}

	const firstTab = () => changeTab(tabIDs[0])

	const lastTab = () => changeTab(tabIDs[tabIDs.length - 1])

	const changeTab = (newTab: number | string) => {
		setActiveTab(newTab);

		if (emitChanges) {
			EventEmitter.emit("tabChange", { tab: newTab, id });
		}
	};

	return (
		<Provider
			value={{
				activeTab,
				changeTab,
				prevTab,
				nextTab,
				firstTab,
				lastTab
			}}
		>
			{children}
		</Provider>
	);
};
export default memo(TabSwitcher);
export { Tab, TabPanel, context };

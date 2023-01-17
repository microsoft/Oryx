import React, { useRef } from "react";
import { StatsPanel, CategoryTabs, Title, TabIndicator, OptionList, Option } from "./styles";
import TabSwitcher, { Tab, TabPanel } from "@/components/TabSwitcher";
import statOptions from "./stat-options";
import useTabIndicator from "@/hooks/use-tab-indicator";
import { useSelector, useDispatch } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";
import { Dispatch } from "redux";
import { statsActions } from "@/redux/stats";
import clsx from "clsx";
import Checkmark from "./checkmark";
import EvaluateArrow from "./evaluate-arrow";

const ChooseStats = () => {
	const tabContainer = useRef(null);
	const tabIndicator = useRef(null);
	const { selectedStats } = useSelector((store: IStoreState) => store.stats);
	const dispatch: Dispatch = useDispatch();

	const addStat = (name: string, category: string) => {
		dispatch(statsActions.addStat({ name, category }));
	};

	const statsSelected = selectedStats.length === 4;

	const isOptionActive = (option: string) =>
		!!selectedStats && selectedStats.length > 0 && !!selectedStats.find(stat => !!stat && stat.name === option);

	const selectedStatsInCategory = (category: string): number => {
		if (!selectedStats || selectedStats.length === 0) return 0;
		return selectedStats.filter(stat => !!stat && stat.category === category).length;
	};

	useTabIndicator(tabContainer, tabIndicator);

	return (
		<StatsPanel>
			<Title>Choose 4 skills</Title>
			<TabSwitcher initialTab={statOptions[0].category} emitChanges={true} id="stat-categories">
				<CategoryTabs ref={tabContainer}>
					{statOptions.map(({ category }, index: number) => {
						const statCount = selectedStatsInCategory(category);
						return (
							<Tab id={category} key={category + "$$" + index}>
								{category}{" "}
								<span className={clsx("count", statCount > 0 && "count-active")}>({statCount})</span>
							</Tab>
						);
					})}
					<TabIndicator ref={tabIndicator} />
				</CategoryTabs>
				{statOptions.map(({ category, options }, index: number) => (
					<TabPanel whenActive={category} key={"panel$$" + category + "$$" + index}>
						<OptionList className="option-list">
							{options.map((option: string, i: number) => (
								<Option
									key={option + "$$" + i}
									disabled={statsSelected && !isOptionActive(option)}
									selected={isOptionActive(option)}
									onClick={() => addStat(option, category)}
								>
									<span className="check">
										<Checkmark />
									</span>
									{option}
								</Option>
							))}
						</OptionList>
					</TabPanel>
				))}
			</TabSwitcher>
			{statsSelected && (
				<Title as={Tab} id="distribute" className="distribute-text" style={{ marginTop: 85 }}>
					<span className="icon">
						<EvaluateArrow />
					</span>
					Next, distribute points
				</Title>
			)}
		</StatsPanel>
	);
};

export default ChooseStats;

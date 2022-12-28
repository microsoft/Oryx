import React from "react";
import TabSwitcher, { Tab, TabPanel } from "@/components/TabSwitcher";
import { Tabs, StatsContainer } from "./styles";
import ChoosePanel from "./choose-stats";
import DistributePanel from "./distribute-points";
import { useSelector } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";

const OptionPanels = () => {
	const { selectedStats } = useSelector((store: IStoreState) => store.stats);

	return (
		<TabSwitcher initialTab="choose" emitChanges={true} id="stats">
			<StatsContainer>
				<Tabs>
					<Tab id="choose">Select your skills</Tab>
					<Tab id="distribute" disabled={selectedStats.length !== 4} className="distribute-points">
						Distribute points
					</Tab>
				</Tabs>

				<TabPanel whenActive="choose">
					<ChoosePanel />
				</TabPanel>
				<TabPanel whenActive="distribute">
					<DistributePanel />
				</TabPanel>
			</StatsContainer>
		</TabSwitcher>
	);
};

export default OptionPanels;

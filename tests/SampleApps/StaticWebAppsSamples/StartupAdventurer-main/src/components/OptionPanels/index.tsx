import React from "react";
import TabSwitcher, { Tab, TabPanel } from "@/components/TabSwitcher";
import { Tabs, OptionsContainer } from "./styles";
import BodyPanel from "./body-panel";
import TopPanel from "./top-panel";
import BottomPanel from "./bottom-panel";
import AccessoriesPanel from "./accessories-panel";

const OptionPanels = () => {
	return (
		<TabSwitcher initialTab="body" emitChanges={true} id="option-panels">
			<OptionsContainer>
				<Tabs className="option-tabs" role="tablist">
					<Tab id="body">Body</Tab>
					<Tab id="top">Top style</Tab>
					<Tab id="bottom">Bottom style</Tab>
					<Tab id="accessories">Accessories</Tab>
				</Tabs>

				<TabPanel whenActive="body">
					<BodyPanel />
				</TabPanel>
				<TabPanel whenActive="top">
					<TopPanel />
				</TabPanel>
				<TabPanel whenActive="bottom">
					<BottomPanel />
				</TabPanel>
				<TabPanel whenActive="accessories">
					<AccessoriesPanel />
				</TabPanel>
			</OptionsContainer>
		</TabSwitcher>
	);
};

export default OptionPanels;

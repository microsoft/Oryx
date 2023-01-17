import React from "react";
import { StatsPanel, Title, Grading, Remaining } from "./styles";
import StatGrader from "@/components/StatGrader";
import { useSelector } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";

const DistributePoints = () => {
	const { gradedStats, statPointsAvailable, totalStatPoints } = useSelector((store: IStoreState) => store.stats);

	return (
		<StatsPanel className="stats-panel-distribute">
			<Title>Distribute {totalStatPoints} points</Title>
			<Grading>
				{gradedStats.map(({ name, category, level }, index: number) => (
					<StatGrader
						key={name + category + index}
						stat={{ name, category, level }}
						canIncrease={!!statPointsAvailable}
					/>
				))}
			</Grading>
			{!!statPointsAvailable && <Remaining>Points remaining: {statPointsAvailable}</Remaining>}
		</StatsPanel>
	);
};

export default DistributePoints;

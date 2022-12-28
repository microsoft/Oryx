import React from "react";
import { StepperNav } from "./styles";
import PrevButton from "./PrevButton";
import NextButton from "./NextButton";
import ProgressBar from "@/components/Progress";
import { useSelector } from "react-redux";
import { IStoreState } from "@/interfaces/IStoreState";

interface IProps {
	nextDisabled?: boolean;
	prevDisabled?: boolean;
	nextHtml?: any;
	beforeNext?: any;
	beforePrev?: any;
}

const StepperFooter = ({
	nextDisabled = false,
	prevDisabled = false,
	nextHtml = "Next",
	beforeNext = undefined,
	beforePrev = undefined,
}: IProps) => {
	const { currentStep } = useSelector((store: IStoreState) => store.ui);

	return (
		<StepperNav>
			<div style={{ display: "flex", alignItems: "center" }}>
				<PrevButton disabled={prevDisabled} beforePrev={beforePrev} title="Back" />
				<ProgressBar step={currentStep} />
			</div>
			<NextButton disabled={nextDisabled} beforeNext={beforeNext}>
				{nextHtml}
			</NextButton>
		</StepperNav>
	);
};

export default StepperFooter;

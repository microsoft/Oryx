import React, { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import { Dispatch } from "redux";
import { uiActions } from "@/redux/ui";
import clsx from "clsx";
import { IStoreState } from "@/interfaces/IStoreState";

interface IStep {
  component: React.ComponentClass<any> | any;
  name?: string;
  nextButtonText?: string;
}

interface IProps {
  steps: IStep[];
}

const Stepper = ({ steps }: IProps) => {
  const { currentStep } = useSelector((store: IStoreState) => store.ui);
  const dispatch: Dispatch = useDispatch();

  useEffect(() => {
    dispatch(uiActions.setTotalSteps(steps.length));
  }, [steps.length, dispatch]);

  const { component: Component, name } = steps[currentStep];

  return (
    <div className={clsx("stepper", name)}>
      <div className="stepper-step" key={"step-" + currentStep}>
        <Component />
      </div>
    </div>
  );
};

export default Stepper;

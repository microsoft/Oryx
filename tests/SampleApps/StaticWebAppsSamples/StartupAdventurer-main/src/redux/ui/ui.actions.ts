import {
  NAVIGATE_NEXT,
  NAVIGATE_PREV,
  NAVIGATE_TO,
  SET_TOTAL_STEPS,
  RESET_UI,
  START_OVER,
  CONTINUE_BUILD,
  SET_IDLE,
  DISPLAY_CHARACTER,
} from "./ui.types";
import { IAction } from "@/interfaces/IAction";

export const navigateNext = (): IAction => ({
  type: NAVIGATE_NEXT,
  payload: {},
});

export const navigateTo = (step: number): IAction => ({
  type: NAVIGATE_TO,
  payload: { step },
});

export const navigatePrev = (): IAction => ({
  type: NAVIGATE_PREV,
  payload: {},
});

export const setTotalSteps = (steps: number): IAction => ({
  type: SET_TOTAL_STEPS,
  payload: { steps },
});

export const resetUi = (): IAction => ({
  type: RESET_UI,
  payload: {},
});

export const startOver = (): IAction => ({
  type: START_OVER,
  payload: {},
});

export const continueBuild = (): IAction => ({
  type: CONTINUE_BUILD,
  payload: {},
});

export const setIdle = (idle: boolean): IAction => ({
  type: SET_IDLE,
  payload: { idle },
});

export const displayCharacter = (): IAction => ({
  type: DISPLAY_CHARACTER,
  payload: {},
});

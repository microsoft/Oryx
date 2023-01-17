import {
  NAVIGATE_NEXT,
  NAVIGATE_PREV,
  RESET_UI,
  SET_TOTAL_STEPS,
  START_OVER,
  CONTINUE_BUILD,
  SET_IDLE,
  NAVIGATE_TO,
  DISPLAY_CHARACTER,
} from "./ui.types";
import { IUiState } from "@/interfaces/IUiState";

const initialState: IUiState = {
  currentStep: 0,
  isEnd: false,
  isStart: true,
  isIdle: true,
  isCharacterDisplay: false,
  showGameOver: false,
  totalSteps: 5,
};

const reducer = (state: IUiState = initialState, { type, payload }: { type: string; payload: any }) => {
  switch (type) {
    case RESET_UI:
      return initialState;

    case SET_IDLE:
      return {
        ...state,
        isIdle: payload.idle,
      };

    case NAVIGATE_NEXT: {
      const { currentStep, totalSteps } = state;
      const nextStep = currentStep + 1;

      return {
        ...state,
        currentStep: nextStep < totalSteps ? nextStep : totalSteps - 1,
        isStart: currentStep === 0,
        isEnd: nextStep === totalSteps - 1,
      };
    }

    case NAVIGATE_TO: {
      const { totalSteps } = state;
      const { step } = payload;

      return {
        ...state,
        currentStep: step,
        isStart: step === 0,
        isEnd: step === totalSteps - 1,
      };
    }

    case NAVIGATE_PREV: {
      const { currentStep, totalSteps } = state;
      const prevStep = currentStep - 1;

      return {
        ...state,
        currentStep: prevStep > 0 ? prevStep : 0,
        isStart: prevStep === 0,
        isEnd: prevStep === totalSteps - 1,
      };
    }

    case SET_TOTAL_STEPS:
      return {
        ...state,
        totalSteps: payload.steps,
      };

    case START_OVER:
      return {
        ...state,
        showGameOver: true,
      };

    case CONTINUE_BUILD:
      return {
        ...state,
        showGameOver: false,
      };

    case DISPLAY_CHARACTER: {
      return {
        ...state,
        isCharacterDisplay: true,
        isIdle: false,
        isStart: false,
        currentStep: 0,
        isEnd: false,
      };
    }

    default:
      return state;
  }
};

export default reducer;

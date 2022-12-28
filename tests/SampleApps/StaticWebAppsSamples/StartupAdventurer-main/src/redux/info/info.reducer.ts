import { SET_BUSINESS_CATEGORY, SET_COMPANY_SIZE, SET_ROLE, SET_FUNDING, RESET_INFO } from "./info.types";

import { IAction } from "@/interfaces/IAction";
import { IBasicInfo } from "@/interfaces/IBasicInfo";

const initialState: IBasicInfo = {
  businessCategory: undefined,
  companySize: "11-25",
  funding: undefined,
  role: undefined,
};

const reducer = (state: IBasicInfo = initialState, { type, payload }: IAction) => {
  switch (type) {
    case SET_BUSINESS_CATEGORY:
      return {
        ...state,
        businessCategory: payload.category,
      };

    case SET_COMPANY_SIZE:
      return {
        ...state,
        companySize: payload.size,
      };

    case SET_ROLE:
      return {
        ...state,
        role: payload.role,
      };

    case SET_FUNDING:
      return {
        ...state,
        funding: payload.funding,
      };

    case RESET_INFO:
      return initialState;

    default:
      return state;
  }
};

export default reducer;

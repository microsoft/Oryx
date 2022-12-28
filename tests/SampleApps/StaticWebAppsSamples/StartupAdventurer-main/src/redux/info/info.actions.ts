import { SET_BUSINESS_CATEGORY, SET_COMPANY_SIZE, SET_ROLE, SET_FUNDING, RESET_INFO } from "./info.types";

import { IAction } from "@/interfaces/IAction";

export const setBusinessCategory = (category: string | undefined): IAction => ({
	type: SET_BUSINESS_CATEGORY,
	payload: { category },
});

export const setCompanySize = (size: string | undefined): IAction => ({
	type: SET_COMPANY_SIZE,
	payload: { size },
});

export const setRole = (role: string | undefined): IAction => ({
	type: SET_ROLE,
	payload: { role },
});

export const setFunding = (funding: string | undefined): IAction => ({
	type: SET_FUNDING,
	payload: { funding },
});

export const resetInfo = (): IAction => ({
	type: RESET_INFO,
	payload: {},
});

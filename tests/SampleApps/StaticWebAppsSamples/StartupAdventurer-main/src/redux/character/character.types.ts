import { Action } from "@/redux/createAction";
import { ICharacter, IColoredSelection, IStyledSelection, TopStyle } from "@/interfaces/ICharacter";


export const SET_VIEWED_TAB: string = "character/SET_VIEWED_TAB";

export const RESET_CHARACTER: string = "character/RESET_CHARACTER";

export enum CharacterActionType {
	SET_HAIRSTYLE = "character/SET_HAIRSTYLE",
	SET_HAIR_COLOR = "character/SET_HAIR_COLOR",
	SET_SKIN_COLOR = "character/SET_SKIN_COLOR",
	SET_FACIAL_HAIR = "character/SET_FACIAL_HAIR",
	SET_FACIAL_HAIR_COLOR = "character/SET_FACIAL_HAIR_COLOR",
	SET_EYEWEAR = "character/SET_EYEWEAR",
	SET_TOP = "character/SET_TOP",
	SET_BOTTOM = "character/SET_BOTTOM",
	SET_SHOES = "character/SET_SHOES",
	SET_ACCESSORY = "character/SET_ACCESSORY",
	SET_START_TIME = "character/SET_START_TIME",
	SET_END_TIME = "character/SET_END_TIME",
	SET_VIEWED_TAB = "character/SET_VIEWED_TAB",
	RESET_CHARACTER = "character/RESET_CHARACTER",
}

type SetBottomAction = Action<typeof CharacterActionType.SET_BOTTOM, ICharacter['bottom']>;
type SetTopAction = Action<typeof CharacterActionType.SET_TOP, IStyledSelection<TopStyle>>;
type SetShoesAction = Action<typeof CharacterActionType.SET_SHOES, IStyledSelection<string>>;
type SetAccessoryAction = Action<typeof CharacterActionType.SET_ACCESSORY, IStyledSelection<string | undefined>>;
type SetHairStyleAction = Action<typeof CharacterActionType.SET_HAIRSTYLE, Partial<IStyledSelection<string>>>;
type SetHairColorAction = Action<typeof CharacterActionType.SET_HAIR_COLOR, IColoredSelection>;
type SetEyewearAction = Action<typeof CharacterActionType.SET_EYEWEAR, IStyledSelection<string | undefined>>;
type SetFacialHairStyleAction = Action<typeof CharacterActionType.SET_FACIAL_HAIR, Partial<IStyledSelection<string>>>;
type SetFacialHairColorAction = Action<typeof CharacterActionType.SET_FACIAL_HAIR_COLOR, IColoredSelection>;
type SetSkinColorAction = Action<typeof CharacterActionType.SET_SKIN_COLOR, Required<IColoredSelection>>;
type SetStartTimeAction = Action<typeof CharacterActionType.SET_START_TIME, { time: string }>;
type SetEndTimeAction = Action<typeof CharacterActionType.SET_END_TIME, { time: string }>;
type ResetCharacterAction = Action<typeof CharacterActionType.RESET_CHARACTER, {}>;
type SetViewedTabAction = Action<typeof CharacterActionType.SET_VIEWED_TAB, { tab: string }>;


export type CharacterAction =
	SetBottomAction | SetTopAction | SetShoesAction | SetAccessoryAction | SetHairStyleAction | SetHairColorAction | SetEyewearAction | SetFacialHairStyleAction | SetFacialHairColorAction | SetSkinColorAction | SetStartTimeAction | SetEndTimeAction | SetViewedTabAction | ResetCharacterAction;
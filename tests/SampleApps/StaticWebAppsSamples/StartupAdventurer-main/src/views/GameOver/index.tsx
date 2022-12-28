import React from "react";
import { GameOverContainer, TitleContainer, GameOverButtons, Button } from "./styles";
import Title from "./title";
import { Dispatch } from "redux";
import { useDispatch } from "react-redux";
import { characterActions } from "@/redux/character";
import { infoActions } from "@/redux/info";
import { uiActions } from "@/redux/ui";
import { statsActions } from "@/redux/stats";

const GameOver = () => {
	const dispatch: Dispatch = useDispatch();

	const setGameOver = () => {
		dispatch(characterActions.resetCharacter());
		dispatch(infoActions.resetInfo());
		dispatch(uiActions.resetUi());
		dispatch(statsActions.resetStats());
	};

	const continueBuild = () => {
		dispatch(uiActions.continueBuild());
	};

	return (
		<GameOverContainer>
			<TitleContainer>
				<Title />
			</TitleContainer>
			<GameOverButtons>
				<Button onClick={continueBuild}>No, continue experience</Button>
				<Button primary onClick={setGameOver}>
					Yes, stop this!
				</Button>
			</GameOverButtons>
		</GameOverContainer>
	);
};

export default GameOver;

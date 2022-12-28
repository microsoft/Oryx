import React, { useEffect, useState } from "react";
import { StartContainer, StartButton, StartChapters } from "./styles";
import { Dispatch } from "redux";
import { useDispatch } from "react-redux";
import { uiActions } from "@/redux/ui";
import anime from "animejs";
import clsx from "clsx";
import { characterActions } from "@/redux/character";
import { /* useUserInfo, */ StaticWebAuthLogins } from "@aaronpowell/react-static-web-apps-auth";

const StartScreen = () => {
  const dispatch: Dispatch = useDispatch();
  const [starting, setStarting] = useState(false);

  const start = () => {
    setStarting(true);
    const time = new Date().toJSON();
    dispatch(characterActions.setStartTime(time));

    anime({
      targets: ".start-container",
      opacity: [1, 0],
      duration: 600,
      complete: () => dispatch(uiActions.navigateNext()),
    });
  };

  const paragraphs = [
    "It is the year 2021. Hustlers and hackers roam the Earth. Your time is now, adventurer.",
    "Design and equip your 16bit character to survive the wilds of the startup ecosystem. Choose your look, your accessories, and your skills and download your avatar.",
  ];

  const getWords = (paragraph: string) =>
    paragraph.split(" ").map((word: string, i: number, arr: string[]) => (
      <span key={"wrd-$$" + i + word} data-index={i}>
        {word}
        {i !== arr.length - 1 ? " " : ""}
      </span>
    ));

  const getParagraph = (paragraph: string, index: number) => (
    <p className={clsx("paragraph", `paragraph-${index + 1}`)} key={"prgrph" + index}>
      {getWords(paragraph)}
    </p>
  );

  useEffect(() => {
    anime.set(".start-container", { opacity: 1 });
    anime.set(".paragraph span", { opacity: 0 });
    anime.set(".start-button", { opacity: 0 });
    const staggerTime = 150;

    const tl = anime.timeline({
      easing: "linear",
      duration: 50,
    });

    tl.add({
      targets: ".paragraph-1 span",
      opacity: 1,
      endDelay: 1000,
      delay: anime.stagger(staggerTime),
    });

    tl.add({
      targets: ".paragraph-2 span",
      opacity: 1,
      delay: anime.stagger(staggerTime),
      endDelay: 1000,
    });

    tl.add({
      targets: ".start-button",
      opacity: [0, 1],
      translateY: ["60px", 0],
      duration: 500,
      easing: "easeInOutExpo",
    });
  }, []);

  // we're not using an authenticated experience, but here's how you could enable it
  // const userInfo = useUserInfo();
  // const isAuthenticated = userInfo.userId !== undefined;
  const isAuthenticated = true;

  return (
    <StartContainer className="start-container">
      <StartChapters>{paragraphs.map(getParagraph)}</StartChapters>
      {isAuthenticated ? (
        <StartButton className="start-button" disabled={starting} onClick={start}>
          Start adventure
        </StartButton>
      ) : (
        <>
          <StartButton className="start-button" disabled={starting}>
            <StaticWebAuthLogins
              azureAD={false}
              facebook={false}
              github={true}
              google={false}
              twitter={false}
              postLoginRedirect={"/logged-in"}
            />
          </StartButton>
        </>
      )}
    </StartContainer>
  );
};

export default StartScreen;

import React, { useEffect } from "react";
import CharacterRandomizer from "@/components/CharacterRandomizer";
import { IdleScreenWrapper, IdleScreenContainer, Title, LogoContainer, Quide } from "./styles";
import Logo from "@/components/Logo";
import anime from "animejs";
import noop from "lodash-es/noop";
import Floorlight from "@/graphics/floorlight";
import Lightbeam from "@/graphics/lightbeam";

const IdleScreen = ({ onExit = noop }) => {
  const [isRunning, setRunning] = React.useState(true);

  const containerRef = React.createRef<HTMLDivElement>();
  const doExit = () => {
    setRunning(false);
    anime.set(".character-randomizer", { willChange: "transform", translateX: 0 });
    anime.set(".logo", { willChange: "transform", translateX: 0, translateY: 0, scale: 1 });

    (async () => {
      anime({
        targets: "#idle-title, #idle-quide",
        opacity: [1, 0],
        easing: "linear",
        duration: 500,
      });
      anime({
        targets: ".lightbeam, .floorlight",
        opacity: [1, 0],
        easing: "linear",
        duration: 500,
      });
      await anime({
        targets: ".character-randomizer",
        filter: ["brightness(1)", "brightness(0)"],
        easing: "linear",
        duration: 500,
        endDelay: 200,
      }).finished;
      anime({
        targets: ".character-randomizer",
        opacity: [1, 0],
        duration: 500,
        easing: "linear",
      });
      anime({
        targets: "#idle-title, #idle-quote",
        height: 0,
        margin: 0,
        duration: 500,
        easing: "linear",
      });
      anime({
        targets: "#idle-logo-container",
        left: 103,
        top: 124,
        width: 815,
        duration: 500,
        easing: "linear",
        endDelay: 500,
        complete: () => onExit(),
      });
    })();
  };

  useEffect(() => {
    if (containerRef.current) {
      containerRef.current.focus();
    }
  });

  return (
    <IdleScreenWrapper className="idle-screen-wrapper" onKeyPress={doExit}>
      <div className="lights" id="idle-spotlight">
        <Lightbeam className="lightbeam" />
        <Floorlight className="floorlight" />
      </div>
      <IdleScreenContainer
        ref={containerRef}
        className="idle-screen-container"
        tabIndex={0}
        onClick={doExit}
        onKeyDown={doExit}
      >
        <CharacterRandomizer running={isRunning} />
        <LogoContainer id="idle-logo-container">
          <Title id="idle-title">Create your own</Title>
          <Logo />
          <Quide id="idle-quide">Touch the screen to start your adventure</Quide>
        </LogoContainer>
      </IdleScreenContainer>
    </IdleScreenWrapper>
  );
};

export default IdleScreen;
